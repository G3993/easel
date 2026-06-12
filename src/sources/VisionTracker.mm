#import "sources/VisionTracker.h"

#import <AVFoundation/AVFoundation.h>
#import <Vision/Vision.h>
#import <CoreVideo/CoreVideo.h>
#import <simd/simd.h>

#include <cmath>

// ---------------------------------------------------------------------
// Objective-C capture delegate. Owns the AVCaptureSession, runs the
// enabled Vision requests on each frame, normalizes the results, and
// hands them back to the C++ VisionTracker via publish().
// ---------------------------------------------------------------------
@interface EaselVisionDelegate : NSObject <AVCaptureVideoDataOutputSampleBufferDelegate>
@property (nonatomic, assign) VisionTracker* owner;
@property (nonatomic, strong) AVCaptureSession* session;
@property (nonatomic, strong) dispatch_queue_t queue;
@end

@implementation EaselVisionDelegate

- (void)captureOutput:(AVCaptureOutput*)output
didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer
       fromConnection:(AVCaptureConnection*)connection {
    if (!self.owner) return;
    CVPixelBufferRef pixelBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
    if (!pixelBuffer) return;

    const bool wantHand = self.owner->handEnabled();
    const bool wantFace = self.owner->faceEnabled();
    const bool wantPose = self.owner->poseEnabled();
    if (!wantHand && !wantFace && !wantPose) return;

    NSMutableArray<VNRequest*>* requests = [NSMutableArray array];

    VNDetectHumanHandPoseRequest* handReq = nil;
    VNDetectHumanBodyPoseRequest* poseReq = nil;
    VNDetectFaceLandmarksRequest* faceReq = nil;

    if (wantHand) {
        handReq = [[VNDetectHumanHandPoseRequest alloc] init];
        handReq.maximumHandCount = 2;
        [requests addObject:handReq];
    }
    if (wantPose) {
        poseReq = [[VNDetectHumanBodyPoseRequest alloc] init];
        [requests addObject:poseReq];
    }
    if (wantFace) {
        faceReq = [[VNDetectFaceLandmarksRequest alloc] init];
        [requests addObject:faceReq];
    }
    if (requests.count == 0) return;

    // The front camera is mirrored; Vision's coordinate origin is the
    // bottom-left. We flip Y on read so signals match shader-UV / SC3.
    VNImageRequestHandler* handler =
        [[VNImageRequestHandler alloc] initWithCVPixelBuffer:pixelBuffer
                                                 orientation:kCGImagePropertyOrientationUpMirrored
                                                     options:@{}];
    NSError* err = nil;
    if (![handler performRequests:requests error:&err]) {
        return;  // transient frame failure — skip, next frame retries
    }

    VisionTracker::Signals sig;

    // ── Hands ──────────────────────────────────────────────────────
    if (handReq) {
        NSArray<VNHumanHandPoseObservation*>* obs = handReq.results;
        sig.handCount = (float)obs.count;
        // First two hands → left/right slots (Vision doesn't label
        // chirality reliably without a separate request, so we just take
        // observation order; good enough for "hand 1 / hand 2" controls).
        for (NSUInteger i = 0; i < obs.count && i < 2; i++) {
            VNHumanHandPoseObservation* h = obs[i];
            NSError* e2 = nil;
            VNRecognizedPoint* wrist =
                [h recognizedPointForJointName:VNHumanHandPoseObservationJointNameWrist error:&e2];
            VNRecognizedPoint* thumbTip =
                [h recognizedPointForJointName:VNHumanHandPoseObservationJointNameThumbTip error:&e2];
            VNRecognizedPoint* indexTip =
                [h recognizedPointForJointName:VNHumanHandPoseObservationJointNameIndexTip error:&e2];
            float hx = 0.5f, hy = 0.5f;
            if (wrist && wrist.confidence > 0.2f) {
                hx = (float)wrist.location.x;
                hy = 1.0f - (float)wrist.location.y;
            }
            if (i == 0) { sig.leftHandX = hx; sig.leftHandY = hy; }
            else        { sig.rightHandX = hx; sig.rightHandY = hy; }
            // Pinch from the first hand: thumb-tip ↔ index-tip distance,
            // mapped so touching ≈ 1, spread ≈ 0.
            if (i == 0 && thumbTip && indexTip &&
                thumbTip.confidence > 0.2f && indexTip.confidence > 0.2f) {
                float dx = (float)(thumbTip.location.x - indexTip.location.x);
                float dy = (float)(thumbTip.location.y - indexTip.location.y);
                float d = sqrtf(dx*dx + dy*dy);
                // ~0.30 normalized distance = fully open; 0.02 = touching.
                float pinch = 1.0f - (d - 0.02f) / (0.30f - 0.02f);
                sig.pinch = fmaxf(0.0f, fminf(1.0f, pinch));
            }
        }
    }

    // ── Pose ───────────────────────────────────────────────────────
    if (poseReq) {
        NSArray<VNHumanBodyPoseObservation*>* obs = poseReq.results;
        if (obs.count > 0) {
            VNHumanBodyPoseObservation* p = obs[0];
            NSError* e2 = nil;
            VNRecognizedPoint* nose =
                [p recognizedPointForJointName:VNHumanBodyPoseObservationJointNameNose error:&e2];
            if (nose && nose.confidence > 0.1f) {
                sig.headX = (float)nose.location.x;
                sig.headY = 1.0f - (float)nose.location.y;
                sig.poseConfidence = (float)nose.confidence;
            }
        }
    }

    // ── Face ───────────────────────────────────────────────────────
    if (faceReq) {
        NSArray<VNFaceObservation*>* obs = faceReq.results;
        sig.faceDetected = obs.count > 0 ? 1.0f : 0.0f;
        if (obs.count > 0) {
            VNFaceObservation* f = obs[0];
            VNFaceLandmarks2D* lm = f.landmarks;
            // Crude smile proxy: outer-lip width ÷ height. A smile widens
            // and flattens the mouth, raising the ratio. Normalized into a
            // rough [0,1]; good enough to drive a reactive parameter.
            if (lm.outerLips && lm.outerLips.pointCount > 0) {
                const CGPoint* pts = lm.outerLips.normalizedPoints;
                NSUInteger n = lm.outerLips.pointCount;
                float minX = 1e9f, maxX = -1e9f, minY = 1e9f, maxY = -1e9f;
                for (NSUInteger i = 0; i < n; i++) {
                    minX = fminf(minX, (float)pts[i].x);
                    maxX = fmaxf(maxX, (float)pts[i].x);
                    minY = fminf(minY, (float)pts[i].y);
                    maxY = fmaxf(maxY, (float)pts[i].y);
                }
                float w = maxX - minX, h = maxY - minY;
                if (h > 1e-4f) {
                    float ratio = w / h;             // ~2 neutral, ~4+ smile
                    float smile = (ratio - 2.0f) / 2.5f;
                    sig.smile = fmaxf(0.0f, fminf(1.0f, smile));
                }
            }
        }
    }

    self.owner->publish(sig);
}

@end

// ---------------------------------------------------------------------
// C++ side
// ---------------------------------------------------------------------

VisionTracker::~VisionTracker() {
    stop();
}

bool VisionTracker::start() {
    if (m_running.load()) return true;

    AVAuthorizationStatus status =
        [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
    if (status == AVAuthorizationStatusDenied ||
        status == AVAuthorizationStatusRestricted) {
        NSLog(@"[VisionTracker] camera permission denied/restricted");
        return false;
    }
    if (status == AVAuthorizationStatusNotDetermined) {
        // Fire the prompt; the user grants it asynchronously. We proceed
        // to build the session — frames simply won't flow until granted,
        // and the user can re-toggle once they allow.
        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo
                                 completionHandler:^(BOOL granted){
            (void)granted;
        }];
    }

    AVCaptureSession* session = [[AVCaptureSession alloc] init];
    session.sessionPreset = AVCaptureSessionPreset640x480;  // plenty for tracking

    AVCaptureDevice* device =
        [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
    if (!device) {
        NSLog(@"[VisionTracker] no video capture device");
        return false;
    }
    NSError* err = nil;
    AVCaptureDeviceInput* input =
        [AVCaptureDeviceInput deviceInputWithDevice:device error:&err];
    if (!input || err) {
        NSLog(@"[VisionTracker] device input failed: %@", err);
        return false;
    }
    if ([session canAddInput:input]) [session addInput:input];

    EaselVisionDelegate* delegate = [[EaselVisionDelegate alloc] init];
    delegate.owner = this;
    delegate.queue = dispatch_queue_create("com.easel.vision", DISPATCH_QUEUE_SERIAL);

    AVCaptureVideoDataOutput* output = [[AVCaptureVideoDataOutput alloc] init];
    output.videoSettings = @{
        (id)kCVPixelBufferPixelFormatTypeKey :
            @(kCVPixelFormatType_32BGRA)
    };
    // Drop late frames — tracking should run on the freshest frame, not
    // back up a queue.
    output.alwaysDiscardsLateVideoFrames = YES;
    [output setSampleBufferDelegate:delegate queue:delegate.queue];
    if ([session canAddOutput:output]) [session addOutput:output];

    delegate.session = session;
    [session startRunning];

    // Retain the delegate (which retains the session) past this scope.
    m_impl = (void*)CFBridgingRetain(delegate);
    m_running.store(true);
    NSLog(@"[VisionTracker] started");
    return true;
}

void VisionTracker::stop() {
    if (!m_running.load()) return;
    if (m_impl) {
        EaselVisionDelegate* delegate =
            (EaselVisionDelegate*)CFBridgingRelease(m_impl);
        m_impl = nullptr;
        [delegate.session stopRunning];
        delegate.owner = nullptr;
        delegate.session = nil;
    }
    m_running.store(false);
    NSLog(@"[VisionTracker] stopped");
}

void VisionTracker::publish(const Signals& s) {
    std::lock_guard<std::mutex> lock(m_mutex);
    m_signals = s;
}

VisionTracker::Signals VisionTracker::signals() const {
    std::lock_guard<std::mutex> lock(m_mutex);
    return m_signals;
}
