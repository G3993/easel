// macOS WHEP receiver on libwebrtc — see WHEPSource_webrtc.h.
// Compiled with -fobjc-arc.
#if defined(__APPLE__) && defined(EASEL_WEBRTC_MAC)
#include "sources/WHEPSource.h"
#include "render/ShaderProgram.h"
#include "render/Framebuffer.h"
#include "render/Mesh.h"

#import <Foundation/Foundation.h>
#import <WebRTC/WebRTC.h>
#include <CoreVideo/CoreVideo.h>
#include <IOSurface/IOSurface.h>
#include <OpenGL/OpenGL.h>
#include <OpenGL/CGLIOSurface.h>
#include <glm/glm.hpp>

#include <algorithm>
#include <atomic>
#include <chrono>
#include <cstdio>
#include <ctime>
#include <iostream>
#include <mutex>
#include <string>
#include <csignal>
#include <sys/stat.h>
#include <unistd.h>
#include <vector>

namespace {

double nowSec() {
    using namespace std::chrono;
    return duration<double>(steady_clock::now().time_since_epoch()).count();
}

// One factory for the process. Field trials must be set before ANY other
// WebRTC call: WebRTC-ForcePlayoutDelay puts a floor under the jitter
// buffer's target delay (the ObjC API has no SetJitterBufferMinimumDelay).
// A projection feed wants smoothness over latency: 250 ms by default,
// EASEL_WHEP_MIN_DELAY_MS overrides (0 = libwebrtc's own adaptive minimum).
RTCPeerConnectionFactory* sharedFactory() {
    static RTCPeerConnectionFactory* f = nil;
    static dispatch_once_t once;
    dispatch_once(&once, ^{
        int minMs = 250;
        if (const char* e = getenv("EASEL_WHEP_MIN_DELAY_MS")) minMs = std::max(0, atoi(e));
        if (minMs > 0) {
            // max_ms must be generous: with max_ms 1500 libwebrtc stalled
            // ~230 ms once per keyframe interval (measured on a clean
            // localhost feed: 16 freezes / 170 s); 10000 = 0 freezes.
            int maxMs = 10000;
            if (const char* e = getenv("EASEL_WHEP_MAX_DELAY_MS")) maxMs = std::max(minMs, atoi(e));
            NSString* v = [NSString stringWithFormat:@"min_ms:%d,max_ms:%d", minMs, maxMs];
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
            RTCInitFieldTrialDictionary(@{@"WebRTC-ForcePlayoutDelay": v});
#pragma clang diagnostic pop
        }
        RTCSetMinDebugLogLevel(RTCLoggingSeverityWarning);
        f = [[RTCPeerConnectionFactory alloc]
                initWithEncoderFactory:[[RTCDefaultVideoEncoderFactory alloc] init]
                        decoderFactory:[[RTCDefaultVideoDecoderFactory alloc] init]];
        std::cerr << "[WHEP] libwebrtc factory up (jitter floor " << minMs << " ms)" << std::endl;
    });
    return f;
}

std::string toStd(NSString* s) { return s ? std::string([s UTF8String]) : std::string(); }

// Synchronous HTTP with a hard timeout. Never call it on the main thread
// for a remote host — signaling uses the async variant below.
NSHTTPURLResponse* syncRequest(NSMutableURLRequest* req, NSData** outBody, double timeoutSec) {
    __block NSData* body = nil;
    __block NSHTTPURLResponse* resp = nil;
    dispatch_semaphore_t sem = dispatch_semaphore_create(0);
    req.timeoutInterval = timeoutSec;
    NSURLSessionDataTask* t = [[NSURLSession sharedSession] dataTaskWithRequest:req
        completionHandler:^(NSData* d, NSURLResponse* r, NSError* err) {
            body = d;
            if ([r isKindOfClass:[NSHTTPURLResponse class]]) resp = (NSHTTPURLResponse*)r;
            dispatch_semaphore_signal(sem);
        }];
    [t resume];
    if (dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, (int64_t)((timeoutSec + 0.5) * NSEC_PER_SEC))) != 0) {
        [t cancel];
        return nil;
    }
    if (outBody) *outBody = body;
    return resp;
}

const char* kVS =
    "#version 330 core\n"
    "layout(location = 0) in vec2 aPos;\n"
    "layout(location = 1) in vec2 aTexCoord;\n"
    "out vec2 vUV;\n"
    "void main() { gl_Position = vec4(aPos, 0.0, 1.0); vUV = aTexCoord; }\n";

// NV12 (Y + interleaved CbCr) or I420 (Y + Cb + Cr) rectangle textures ->
// RGB. FBO row 0 samples surface row 0 (the image top), so the result is
// top-down like every other video source (isFlippedV).
const char* kFS =
    "#version 330 core\n"
    "in vec2 vUV;\n"
    "out vec4 fragColor;\n"
    "uniform sampler2DRect uY;\n"
    "uniform sampler2DRect uC1;\n"
    "uniform sampler2DRect uC2;\n"
    "uniform bool uPlanar;\n"
    "uniform bool uFullRange;\n"
    "uniform vec2 uSize;\n"
    "uniform mat3 uYuvToRgb;\n"
    "void main() {\n"
    "    vec2 p = vUV * uSize;\n"
    "    vec2 c = p * 0.5;\n"
    "    float y = texture(uY, p).r;\n"
    "    vec2 uv = uPlanar ? vec2(texture(uC1, c).r, texture(uC2, c).r) : texture(uC1, c).rg;\n"
    "    if (!uFullRange) { y = (y - 16.0/255.0) * (255.0/219.0); uv = (uv - 128.0/255.0) * (255.0/224.0); }\n"
    "    else uv -= 0.5;\n"
    "    vec3 rgb = uYuvToRgb * vec3(y, uv);\n"
    "    fragColor = vec4(clamp(rgb, 0.0, 1.0), 1.0);\n"
    "}\n";

// column-major (glm) Y'CbCr -> R'G'B'
glm::mat3 yuvMatrix(bool bt601) {
    float kr = bt601 ? 0.299f : 0.2126f, kb = bt601 ? 0.114f : 0.0722f, kg = 1.0f - kr - kb;
    glm::mat3 m(1.0f);
    m[0] = glm::vec3(1.0f, 1.0f, 1.0f);                                       // Y
    m[1] = glm::vec3(0.0f, -2.0f * (1.0f - kb) * kb / kg, 2.0f * (1.0f - kb)); // Cb
    m[2] = glm::vec3(2.0f * (1.0f - kr), -2.0f * (1.0f - kr) * kr / kg, 0.0f); // Cr
    return m;
}

} // namespace

// ─── Impl ────────────────────────────────────────────────────────────

@class EaselWHEPPeer;

struct WHEPSource::Impl : std::enable_shared_from_this<WHEPSource::Impl> {
    // config / lifecycle (state below is touched only on `q`)
    std::string url;
    dispatch_queue_t q = nil;
    dispatch_source_t timer = nil;
    bool wantRunning = false;
    bool suspended = false;
    int gen = 0;                       // session generation; stale callbacks compare
    enum class St { Idle, Signaling, Connecting, Playing, Backoff } st = St::Idle;
    double sessionStart = 0.0;
    int backoffIdx = 0;
    RTCPeerConnection* pc = nil;
    RTCVideoTrack* track = nil;
    EaselWHEPPeer* peer = nil;
    std::string location;              // WHEP session resource (DELETE on teardown)
    // Etherea live room links (https://host/live/<room>#<token>, the
    // agent server's player page): same WebRTC, the room's own signaling —
    // GET config (TURN servers), POST offer -> JSON {sdp, sessionID},
    // GET state (presence, ~1 Hz), POST leave. Bearer = the #token.
    bool room = false;
    std::string roomBase, token, liveSession;
    NSArray<RTCIceServer*>* iceServers = nil;
    double gatherStart = 0.0;
    bool statePollBusy = false;
    int sessions = 0, reconnects = 0;

    // shared with the main thread
    mutable std::mutex statusMx;
    std::string status = "idle";
    std::atomic<bool> playing{false};
    std::atomic<int> frameW{0}, frameH{0};

    // latest-frame slot (decoder thread -> main thread)
    std::mutex frameMx;
    CVPixelBufferRef latestPB = nullptr;
    id<RTCI420Buffer> latestI420 = nil;
    uint64_t frameSeq = 0;
    std::atomic<double> lastFrameT{0.0};
    std::atomic<uint64_t> delivered{0};

    // main thread / GL
    uint64_t presentedSeq = 0;
    CVPixelBufferRef boundPB = nullptr;    // kept alive while its IOSurface is bound
    GLuint texY = 0, texC1 = 0, texC2 = 0;
    Framebuffer fbo;
    ShaderProgram prog;
    Mesh quad;
    bool glReady = false, glFailed = false;
    bool loggedPath = false;
    std::atomic<uint64_t> presented{0}, skipped{0};
    double lastPresentT = 0.0;
    std::atomic<double> maxPresentGap{0.0};   // worst gap between new frames on screen, per stats second

    // stats log
    FILE* statsLog = nullptr;
    uint64_t lastDelivered = 0, lastPresented = 0, lastSkipped = 0;
    int statsTick = 0;

    void setStatus(const std::string& s) {
        std::lock_guard<std::mutex> l(statusMx);
        if (s != status) std::cerr << "[WHEP] " << displayUrl() << ": " << s << std::endl;
        status = s;
    }
    // the URL without its #token (logs, stats, names never carry the secret)
    std::string displayUrl() const { return url.substr(0, url.find('#')); }
    void configureUrl(const std::string& u);
    NSMutableURLRequest* roomRequest(const std::string& op, NSString* method, const std::string& sessionId);
    void createPeer(int g);
    void leaveRoom(const std::string& sessionId, bool wait);

    // ── session (all on q) ──
    void start();
    void stop();
    void startSession();
    void teardown(bool waitForDelete);
    void fail(const std::string& why);
    void tick();
    void postOffer(int g);
    void onFrame(RTCVideoFrame* f, int g);
    void onConnState(RTCPeerConnectionState s, int g);
    void logStats(RTCStatisticsReport* r, double t);
};

// Delegate + renderer for one session. Holds the Impl weakly so a session
// that outlives its source just goes quiet.
@interface EaselWHEPPeer : NSObject <RTCPeerConnectionDelegate, RTCVideoRenderer>
- (instancetype)initWithImpl:(std::weak_ptr<WHEPSource::Impl>)impl gen:(int)gen;
@end

@implementation EaselWHEPPeer {
    std::weak_ptr<WHEPSource::Impl> _impl;
    int _gen;
}
- (instancetype)initWithImpl:(std::weak_ptr<WHEPSource::Impl>)impl gen:(int)gen {
    if ((self = [super init])) { _impl = impl; _gen = gen; }
    return self;
}
- (void)setSize:(CGSize)size {}
- (void)renderFrame:(RTCVideoFrame*)frame {
    if (!frame) return;
    if (auto m = _impl.lock()) m->onFrame(frame, _gen);
}
- (void)peerConnection:(RTCPeerConnection*)pc didChangeConnectionState:(RTCPeerConnectionState)s {
    if (auto m = _impl.lock()) {
        int g = _gen;
        std::weak_ptr<WHEPSource::Impl> w = m;
        dispatch_async(m->q, ^{ if (auto mm = w.lock()) mm->onConnState(s, g); });
    }
}
- (void)peerConnection:(RTCPeerConnection*)pc didChangeSignalingState:(RTCSignalingState)s {}
- (void)peerConnection:(RTCPeerConnection*)pc didAddStream:(RTCMediaStream*)stream {}
- (void)peerConnection:(RTCPeerConnection*)pc didRemoveStream:(RTCMediaStream*)stream {}
- (void)peerConnectionShouldNegotiate:(RTCPeerConnection*)pc {}
- (void)peerConnection:(RTCPeerConnection*)pc didChangeIceConnectionState:(RTCIceConnectionState)s {}
- (void)peerConnection:(RTCPeerConnection*)pc didChangeIceGatheringState:(RTCIceGatheringState)s {}
- (void)peerConnection:(RTCPeerConnection*)pc didGenerateIceCandidate:(RTCIceCandidate*)c {}
- (void)peerConnection:(RTCPeerConnection*)pc didRemoveIceCandidates:(NSArray<RTCIceCandidate*>*)c {}
- (void)peerConnection:(RTCPeerConnection*)pc didOpenDataChannel:(RTCDataChannel*)dc {}
@end

namespace {
NSDictionary* jsonObject(NSData* d) {
    if (!d) return nil;
    id o = [NSJSONSerialization JSONObjectWithData:d options:0 error:nil];
    return [o isKindOfClass:[NSDictionary class]] ? (NSDictionary*)o : nil;
}
// {ok, data, error:{code, message}} -> data, or the error text
NSDictionary* roomData(NSData* d, std::string& err, std::string& code) {
    NSDictionary* j = jsonObject(d);
    if (!j) { err = "bad reply"; return nil; }
    if (![j[@"ok"] boolValue]) {
        NSDictionary* e = [j[@"error"] isKindOfClass:[NSDictionary class]] ? j[@"error"] : nil;
        err = e[@"message"] ? toStd(e[@"message"]) : "refused";
        code = e[@"code"] ? toStd(e[@"code"]) : "";
        return nil;
    }
    return [j[@"data"] isKindOfClass:[NSDictionary class]] ? j[@"data"] : @{};
}
} // namespace

void WHEPSource::Impl::configureUrl(const std::string& u) {
    url = u;
    room = false; roomBase.clear(); token.clear(); liveSession.clear(); iceServers = nil;
    size_t hash = u.find('#');
    size_t scheme = u.find("://");
    if (hash == std::string::npos || scheme == std::string::npos) return;
    size_t pathStart = u.find('/', scheme + 3);
    size_t live = pathStart == std::string::npos ? std::string::npos : u.find("/live/", pathStart);
    if (live == std::string::npos || live > hash) return;
    size_t idStart = live + 6;
    size_t idEnd = u.find_first_of("/?#", idStart);
    std::string roomId = u.substr(idStart, idEnd - idStart);
    if (roomId.empty()) return;
    room = true;
    token = u.substr(hash + 1);
    roomBase = u.substr(0, pathStart) + "/api/live/rooms/" + roomId + "/";
}

NSMutableURLRequest* WHEPSource::Impl::roomRequest(const std::string& op, NSString* method, const std::string& sessionId) {
    NSURL* ru = [NSURL URLWithString:[NSString stringWithUTF8String:(roomBase + op).c_str()]];
    NSMutableURLRequest* req = [NSMutableURLRequest requestWithURL:ru];
    req.HTTPMethod = method;
    req.cachePolicy = NSURLRequestReloadIgnoringLocalCacheData;
    [req setValue:[NSString stringWithFormat:@"Bearer %s", token.c_str()] forHTTPHeaderField:@"Authorization"];
    [req setValue:[NSString stringWithUTF8String:sessionId.c_str()] forHTTPHeaderField:@"X-Live-Session"];
    [req setValue:@"" forHTTPHeaderField:@"X-Feed-Lease"];
    return req;
}

// End our place in the room (the WHEP DELETE of a live room).
void WHEPSource::Impl::leaveRoom(const std::string& sessionId, bool wait) {
    if (sessionId.empty() || roomBase.empty()) return;
    NSMutableURLRequest* req = roomRequest("leave", @"POST", sessionId);
    [req setValue:@"application/json" forHTTPHeaderField:@"Content-Type"];
    req.HTTPBody = [@"{}" dataUsingEncoding:NSUTF8StringEncoding];
    std::string shortId = sessionId.substr(0, 8);
    if (wait) {
        NSHTTPURLResponse* r = syncRequest(req, nullptr, 1.5);
        std::cerr << "[WHEP] room leave " << shortId << " -> " << (r ? (int)r.statusCode : 0) << std::endl;
    } else {
        req.timeoutInterval = 4.0;
        [[[NSURLSession sharedSession] dataTaskWithRequest:req
            completionHandler:^(NSData*, NSURLResponse* r, NSError*) {
                int code = [r isKindOfClass:[NSHTTPURLResponse class]] ? (int)((NSHTTPURLResponse*)r).statusCode : 0;
                std::cerr << "[WHEP] room leave " << shortId << " -> " << code << std::endl;
            }] resume];
    }
}

void WHEPSource::Impl::start() {
    wantRunning = true;
    backoffIdx = 0;
    if (!timer) {
        timer = dispatch_source_create(DISPATCH_SOURCE_TYPE_TIMER, 0, 0, q);
        dispatch_source_set_timer(timer, dispatch_time(DISPATCH_TIME_NOW, 0), 250 * NSEC_PER_MSEC, 20 * NSEC_PER_MSEC);
        std::weak_ptr<Impl> w = shared_from_this();
        dispatch_source_set_event_handler(timer, ^{ if (auto m = w.lock()) m->tick(); });
        dispatch_resume(timer);
    }
    startSession();
}

void WHEPSource::Impl::stop() {
    wantRunning = false;
    gen++;
    teardown(true);
    st = St::Idle;
    if (timer) { dispatch_source_cancel(timer); timer = nil; }
    setStatus("stopped");
}

void WHEPSource::Impl::startSession() {
    if (!wantRunning) return;
    teardown(false);                 // never two sessions from one source
    int g = ++gen;
    st = St::Signaling;
    sessionStart = nowSec();
    sessions++;
    setStatus(sessions > 1 ? "reconnecting" : "connecting");
    if (!room) { createPeer(g); return; }

    // live room: its TURN servers first (media relays across the NATs)
    std::weak_ptr<Impl> w = shared_from_this();
    NSMutableURLRequest* req = roomRequest("config", @"GET", "");
    req.timeoutInterval = 8.0;
    [[[NSURLSession sharedSession] dataTaskWithRequest:req
        completionHandler:^(NSData* d, NSURLResponse*, NSError* err) {
            std::string why, code;
            NSDictionary* data = err ? nil : roomData(d, why, code);
            if (err) why = toStd(err.localizedDescription);
            NSMutableArray<RTCIceServer*>* servers = [NSMutableArray array];
            for (NSDictionary* sv in ([data[@"iceServers"] isKindOfClass:[NSArray class]] ? data[@"iceServers"] : @[])) {
                if (![sv isKindOfClass:[NSDictionary class]]) continue;
                id urls = sv[@"urls"] ?: sv[@"url"];
                NSArray* list = [urls isKindOfClass:[NSArray class]] ? urls : (urls ? @[urls] : @[]);
                if (!list.count) continue;
                [servers addObject:[[RTCIceServer alloc] initWithURLStrings:list
                                                                   username:sv[@"username"]
                                                                 credential:sv[@"credential"]]];
            }
            dispatch_async(q, ^{
                auto m = w.lock();
                if (!m || m->gen != g) return;
                if (!data) { m->fail(code == "room_offline" ? "host offline" : "room: " + why); return; }
                m->iceServers = servers;
                m->createPeer(g);
            });
        }] resume];
}

void WHEPSource::Impl::createPeer(int g) {
    if (gen != g || !wantRunning) return;
    gatherStart = nowSec();
    RTCConfiguration* cfg = [[RTCConfiguration alloc] init];
    cfg.sdpSemantics = RTCSdpSemanticsUnifiedPlan;
    cfg.bundlePolicy = RTCBundlePolicyMaxBundle;
    cfg.rtcpMuxPolicy = RTCRtcpMuxPolicyRequire;
    cfg.continualGatheringPolicy = RTCContinualGatheringPolicyGatherOnce;
    if (room) {
        // the room's STUN/TURN; a direct route still wins when there is one
        cfg.iceServers = iceServers ?: @[];
        cfg.iceTransportPolicy = RTCIceTransportPolicyAll;
    } else {
        cfg.iceServers = @[];        // LAN: host candidates only
        // UDP only: MediaMTX serves ICE on UDP 8189, and an ICE-TCP pair
        // would turn every lost packet into a head-of-line stall
        cfg.tcpCandidatePolicy = RTCTcpCandidatePolicyDisabled;
    }
    RTCMediaConstraints* cons = [[RTCMediaConstraints alloc] initWithMandatoryConstraints:nil optionalConstraints:nil];

    peer = [[EaselWHEPPeer alloc] initWithImpl:shared_from_this() gen:g];
    pc = [sharedFactory() peerConnectionWithConfiguration:cfg constraints:cons delegate:peer];
    if (!pc) { fail("could not create peer connection"); return; }

    RTCRtpTransceiverInit* ti = [[RTCRtpTransceiverInit alloc] init];
    ti.direction = RTCRtpTransceiverDirectionRecvOnly;
    RTCRtpTransceiver* tr = [pc addTransceiverOfType:RTCRtpMediaTypeVideo init:ti];
    if (tr && [tr.receiver.track isKindOfClass:[RTCVideoTrack class]]) {
        track = (RTCVideoTrack*)tr.receiver.track;
        [track addRenderer:peer];
    }

    std::weak_ptr<Impl> w = shared_from_this();
    RTCPeerConnection* p = pc;
    [pc offerForConstraints:cons completionHandler:^(RTCSessionDescription* offer, NSError* err) {
        dispatch_async(q, ^{
            auto m = w.lock();
            if (!m || m->gen != g) return;
            if (!offer || err) { m->fail("offer: " + toStd(err.localizedDescription)); return; }
            [p setLocalDescription:offer completionHandler:^(NSError* e2) {
                dispatch_async(q, ^{
                    auto m2 = w.lock();
                    if (!m2 || m2->gen != g) return;
                    if (e2) { m2->fail("local description: " + toStd(e2.localizedDescription)); return; }
                    m2->postOffer(g);   // waits for ICE gathering (capped) first
                });
            }];
        });
    }];
}

// POST the offer once ICE gathering completes (or after ~2 s, whichever is
// first) — WHEP is non-trickle here, the candidates ride in the SDP.
void WHEPSource::Impl::postOffer(int g) {
    if (gen != g || !pc) return;
    // TURN allocations take longer than host candidates: give a room 4 s
    if (pc.iceGatheringState != RTCIceGatheringStateComplete && nowSec() - gatherStart < (room ? 4.0 : 2.0)) {
        std::weak_ptr<Impl> w = shared_from_this();
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, 40 * NSEC_PER_MSEC), q, ^{
            if (auto m = w.lock()) m->postOffer(g);
        });
        return;
    }
    NSString* sdp = pc.localDescription.sdp;
    if (!sdp) { fail("no local description"); return; }

    NSURL* u = [NSURL URLWithString:[NSString stringWithUTF8String:url.c_str()]];
    NSMutableURLRequest* req = room ? roomRequest("offer", @"POST", "") : [NSMutableURLRequest requestWithURL:u];
    req.HTTPMethod = @"POST";
    [req setValue:@"application/sdp" forHTTPHeaderField:@"Content-Type"];
    req.HTTPBody = [sdp dataUsingEncoding:NSUTF8StringEncoding];
    // Long on purpose: a slow server still hands back its Location, so a
    // session the watchdog already gave up on is DELETEd instead of orphaned
    // (the 8 s watchdog starts the next attempt meanwhile).
    req.timeoutInterval = 15.0;
    setStatus("signaling");

    std::weak_ptr<Impl> w = shared_from_this();
    RTCPeerConnection* p = pc;
    [[[NSURLSession sharedSession] dataTaskWithRequest:req
        completionHandler:^(NSData* d, NSURLResponse* r, NSError* err) {
            NSHTTPURLResponse* hr = [r isKindOfClass:[NSHTTPURLResponse class]] ? (NSHTTPURLResponse*)r : nil;
            NSString* answer = d ? [[NSString alloc] initWithData:d encoding:NSUTF8StringEncoding] : nil;
            NSString* loc = hr ? [hr valueForHTTPHeaderField:@"Location"] : nil;
            std::string roomErr, roomCode, roomSession;
            bool isRoom = false;
            if (auto m0 = w.lock()) isRoom = m0->room;
            if (isRoom) {
                // live room: JSON {ok, data:{sdp, sessionID, participantID}}
                loc = nil;
                NSDictionary* data = err ? nil : roomData(d, roomErr, roomCode);
                answer = [data[@"sdp"] isKindOfClass:[NSString class]] ? data[@"sdp"] : nil;
                roomSession = [data[@"sessionID"] isKindOfClass:[NSString class]] ? toStd(data[@"sessionID"]) : "";
            }
            dispatch_async(q, ^{
                auto m = w.lock();
                if (!m) {
                    // (a room session left behind here expires with the
                    // room's presence timeout; nothing to call without m)
                    // source gone mid-POST: still free the session on the server
                    if (loc) {
                        NSURL* lu = [NSURL URLWithString:loc relativeToURL:u];
                        NSMutableURLRequest* del = [NSMutableURLRequest requestWithURL:lu];
                        del.HTTPMethod = @"DELETE";
                        [[[NSURLSession sharedSession] dataTaskWithRequest:del] resume];
                    }
                    return;
                }
                if (loc) m->location = toStd([[NSURL URLWithString:loc relativeToURL:u] absoluteString]);
                if (m->room) {
                    if (m->gen != g) { m->leaveRoom(roomSession, false); return; }   // a late answer: leave it at once
                    m->liveSession = roomSession;
                    if (!answer.length) {
                        m->fail(roomCode == "room_offline" ? "host offline"
                              : "room: " + (err ? toStd(err.localizedDescription) : roomErr));
                        return;
                    }
                } else if (m->gen != g) { m->teardown(false); return; }
                if (!m->room && (err || !hr || (hr.statusCode != 201 && hr.statusCode != 200) || answer.length == 0)) {
                    std::string why = err ? toStd(err.localizedDescription)
                                          : ("HTTP " + std::to_string(hr ? (int)hr.statusCode : 0));
                    m->fail("signaling: " + why);
                    return;
                }
                RTCSessionDescription* ans = [[RTCSessionDescription alloc] initWithType:RTCSdpTypeAnswer sdp:answer];
                [p setRemoteDescription:ans completionHandler:^(NSError* e3) {
                    dispatch_async(q, ^{
                        auto m3 = w.lock();
                        if (!m3 || m3->gen != g) return;
                        if (e3) { m3->fail("answer: " + toStd(e3.localizedDescription)); return; }
                        m3->st = St::Connecting;
                        m3->setStatus("ICE connecting");
                    });
                }];
            });
        }] resume];
}

// Close the peer connection and DELETE the WHEP session. waitForDelete
// blocks (<= 1.5 s) so a quitting app / removed layer leaves nothing on
// the server.
void WHEPSource::Impl::teardown(bool waitForDelete) {
    playing = false;
    if (track && peer) [track removeRenderer:peer];
    track = nil;
    // DELETE / leave first, then close: closing first makes the server end
    // the session on its own and the DELETE then 404s.
    if (!liveSession.empty()) {
        leaveRoom(liveSession, waitForDelete);
        liveSession.clear();
    }
    if (!location.empty()) {
        NSURL* lu = [NSURL URLWithString:[NSString stringWithUTF8String:location.c_str()]];
        NSMutableURLRequest* del = [NSMutableURLRequest requestWithURL:lu];
        del.HTTPMethod = @"DELETE";
        std::string what = location;
        location.clear();
        if (waitForDelete) {
            NSHTTPURLResponse* r = syncRequest(del, nullptr, 1.5);
            std::cerr << "[WHEP] DELETE " << what << " -> " << (r ? (int)r.statusCode : 0) << std::endl;
        } else {
            del.timeoutInterval = 3.0;
            [[[NSURLSession sharedSession] dataTaskWithRequest:del
                completionHandler:^(NSData*, NSURLResponse* r, NSError*) {
                    int code = [r isKindOfClass:[NSHTTPURLResponse class]] ? (int)((NSHTTPURLResponse*)r).statusCode : 0;
                    std::cerr << "[WHEP] DELETE " << what << " -> " << code << std::endl;
                }] resume];
        }
    }
    if (pc) { [pc close]; pc = nil; }
    peer = nil;
}

void WHEPSource::Impl::fail(const std::string& why) {
    gen++;                               // silence the dead session's callbacks
    teardown(false);
    if (!wantRunning) { st = St::Idle; return; }
    static const int kBackoff[] = {1, 2, 5};
    int secs = kBackoff[std::min(backoffIdx, 2)];
    backoffIdx++;
    reconnects++;
    st = St::Backoff;
    setStatus("retry in " + std::to_string(secs) + "s: " + why);
    int g = gen;
    std::weak_ptr<Impl> w = shared_from_this();
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)secs * NSEC_PER_SEC), q, ^{
        auto m = w.lock();
        if (m && m->wantRunning && m->gen == g && m->st == St::Backoff) m->startSession();
    });
}

void WHEPSource::Impl::onConnState(RTCPeerConnectionState s, int g) {
    if (g != gen) return;
    if (s == RTCPeerConnectionStateFailed) fail("connection failed");
    else if (s == RTCPeerConnectionStateDisconnected) fail("connection lost");
}

// Watchdog (4 Hz) + stats (1 Hz), on q.
void WHEPSource::Impl::tick() {
    double t = nowSec();
    if (st == St::Signaling || st == St::Connecting) {
        if (t - sessionStart > 8.0) fail("no media after 8s");
    } else if (st == St::Playing) {
        if (t - lastFrameT.load() > 3.0) fail("no frames for 3s");
    }
    if (room && !liveSession.empty() && !statePollBusy && statsTick % 4 == 2 &&
        (st == St::Playing || st == St::Connecting)) {
        // presence, like the player page's poll: still joined, host still live
        statePollBusy = true;
        std::weak_ptr<Impl> w = shared_from_this();
        int g = gen;
        NSMutableURLRequest* req = roomRequest("state", @"GET", liveSession);
        req.timeoutInterval = 5.0;
        [[[NSURLSession sharedSession] dataTaskWithRequest:req
            completionHandler:^(NSData* d, NSURLResponse*, NSError* err) {
                std::string why, code;
                NSDictionary* data = err ? nil : roomData(d, why, code);
                dispatch_async(q, ^{
                    auto m = w.lock();
                    if (!m) return;
                    m->statePollBusy = false;
                    if (m->gen != g || !data) return;     // a missed poll is not a failure
                    if ([data[@"broadcasting"] isKindOfClass:[NSNumber class]] && ![data[@"broadcasting"] boolValue])
                        m->fail("host offline");
                    else if (m->st == St::Playing && [data[@"joined"] isKindOfClass:[NSNumber class]] && ![data[@"joined"] boolValue])
                        m->fail("dropped from the room");
                });
            }] resume];
    }
    if (++statsTick % 4 == 0 && pc && (st == St::Playing || st == St::Connecting)) {
        std::weak_ptr<Impl> w = shared_from_this();
        int g = gen;
        [pc statisticsWithCompletionHandler:^(RTCStatisticsReport* r) {
            dispatch_async(q, ^{
                auto m = w.lock();
                if (m && m->gen == g) m->logStats(r, nowSec());
            });
        }];
    }
}

// Decoder thread. Keep only the newest frame; update() shows each once.
void WHEPSource::Impl::onFrame(RTCVideoFrame* f, int g) {
    if (g != gen) return;
    id<RTCVideoFrameBuffer> buf = f.buffer;
    CVPixelBufferRef pb = nullptr;
    id<RTCI420Buffer> i420 = nil;
    if ([buf isKindOfClass:[RTCCVPixelBuffer class]]) {
        pb = ((RTCCVPixelBuffer*)buf).pixelBuffer;
        OSType fmt = CVPixelBufferGetPixelFormatType(pb);
        if (fmt == kCVPixelFormatType_420YpCbCr8BiPlanarFullRange || fmt == kCVPixelFormatType_420YpCbCr8BiPlanarVideoRange)
            CVPixelBufferRetain(pb);
        else { pb = nullptr; i420 = [buf toI420]; }
    } else {
        i420 = [buf toI420];
    }
    {
        std::lock_guard<std::mutex> l(frameMx);
        if (latestPB) CVPixelBufferRelease(latestPB);
        latestPB = pb;
        latestI420 = i420;
        frameSeq++;
    }
    frameW = f.width; frameH = f.height;
    lastFrameT = nowSec();
    delivered++;
    if (!playing.exchange(true)) {
        std::weak_ptr<Impl> w = shared_from_this();
        int fw = f.width, fh = f.height;
        dispatch_async(q, ^{
            auto m = w.lock();
            if (!m || m->gen != g) return;
            m->st = St::Playing;
            m->backoffIdx = 0;
            m->setStatus("live " + std::to_string(fw) + "x" + std::to_string(fh));
        });
    }
}

// One JSON line per second: libwebrtc's inbound-rtp video stats + what
// Easel did with the frames. ~/.easel/logs/whep_<stamp>.jsonl
void WHEPSource::Impl::logStats(RTCStatisticsReport* r, double t) {
    if (!statsLog) {
        const char* home = getenv("HOME");
        std::string dir = std::string(home ? home : "/tmp") + "/.easel/logs";
        mkdir((std::string(home ? home : "/tmp") + "/.easel").c_str(), 0755);
        mkdir(dir.c_str(), 0755);
        char stamp[32]; time_t now = time(nullptr);
        strftime(stamp, sizeof(stamp), "%Y%m%d-%H%M%S", localtime(&now));
        std::string path = dir + "/whep_" + stamp + "_" + std::to_string(getpid()) + "_" + std::to_string((uintptr_t)this % 1000) + ".jsonl";
        statsLog = fopen(path.c_str(), "a");
        std::cerr << "[WHEP] stats -> " << path << std::endl;
    }
    if (!statsLog) return;
    NSDictionary<NSString*, NSObject*>* in = nil;
    NSDictionary<NSString*, NSObject*>* pair = nil;
    for (RTCStatistics* s in r.statistics.allValues) {
        if ([s.type isEqualToString:@"inbound-rtp"] && [(NSString*)s.values[@"kind"] isEqualToString:@"video"]) in = s.values;
        if ([s.type isEqualToString:@"candidate-pair"] && [(NSNumber*)s.values[@"nominated"] boolValue]
            && [(NSString*)s.values[@"state"] isEqualToString:@"succeeded"]) pair = s.values;
    }
    auto num = [](NSDictionary<NSString*, NSObject*>* d, NSString* k) -> double {
        id v = d[k];
        return [v isKindOfClass:[NSNumber class]] ? [(NSNumber*)v doubleValue] : -1.0;
    };
    uint64_t dl = delivered.load(), pr = presented.load(), sk = skipped.load();
    double gap = maxPresentGap.exchange(0.0);
    fprintf(statsLog,
        "{\"t\":%.3f,\"wall\":%ld,\"url\":\"%s\",\"state\":%d,\"w\":%d,\"h\":%d,"
        "\"fps\":%.1f,\"framesReceived\":%.0f,\"framesDecoded\":%.0f,\"framesDropped\":%.0f,"
        "\"freezeCount\":%.0f,\"totalFreezesDuration\":%.3f,\"pauseCount\":%.0f,"
        "\"packetsReceived\":%.0f,\"packetsLost\":%.0f,\"nackCount\":%.0f,\"pliCount\":%.0f,\"firCount\":%.0f,"
        "\"jitter\":%.4f,\"jitterBufferDelay\":%.3f,\"jitterBufferTargetDelay\":%.3f,\"jitterBufferMinimumDelay\":%.3f,\"jitterBufferEmittedCount\":%.0f,"
        "\"keyFramesDecoded\":%.0f,\"totalDecodeTime\":%.3f,\"bytesReceived\":%.0f,\"rttMs\":%.1f,"
        "\"delivered\":%llu,\"presented\":%llu,\"skipped\":%llu,"
        "\"deliveredPerSec\":%llu,\"presentedPerSec\":%llu,\"skippedPerSec\":%llu,\"maxPresentGapMs\":%.1f,"
        "\"sessions\":%d,\"reconnects\":%d}\n",
        t, (long)time(nullptr), displayUrl().c_str(), (int)st, frameW.load(), frameH.load(),
        num(in, @"framesPerSecond"), num(in, @"framesReceived"), num(in, @"framesDecoded"), num(in, @"framesDropped"),
        num(in, @"freezeCount"), num(in, @"totalFreezesDuration"), num(in, @"pauseCount"),
        num(in, @"packetsReceived"), num(in, @"packetsLost"), num(in, @"nackCount"), num(in, @"pliCount"), num(in, @"firCount"),
        num(in, @"jitter"), num(in, @"jitterBufferDelay"), num(in, @"jitterBufferTargetDelay"), num(in, @"jitterBufferMinimumDelay"), num(in, @"jitterBufferEmittedCount"),
        num(in, @"keyFramesDecoded"), num(in, @"totalDecodeTime"), num(in, @"bytesReceived"),
        pair ? num(pair, @"currentRoundTripTime") * 1000.0 : -1.0,
        (unsigned long long)dl, (unsigned long long)pr, (unsigned long long)sk,
        (unsigned long long)(dl - lastDelivered), (unsigned long long)(pr - lastPresented), (unsigned long long)(sk - lastSkipped),
        gap * 1000.0, sessions, reconnects);
    fflush(statsLog);
    lastDelivered = dl; lastPresented = pr; lastSkipped = sk;
}

// ─── process exit: never leave a session on the server ───────────────
// Every live session costs the sender 8-15 Mbps of upload until its server
// times it out, so a quit (atexit) or a kill (SIGTERM / SIGINT / SIGHUP)
// DELETEs them all first; signals are then re-raised with the default
// action, so the process still dies the way it would have.
namespace {
std::mutex gLiveMx;
std::vector<std::weak_ptr<WHEPSource::Impl>> gLive;

void shutdownAllSessions() {
    std::vector<std::shared_ptr<WHEPSource::Impl>> live;
    {
        std::lock_guard<std::mutex> l(gLiveMx);
        for (auto& w : gLive) if (auto m = w.lock()) live.push_back(m);
        gLive.clear();
    }
    for (auto& m : live) {
        auto mm = m;
        dispatch_sync(m->q, ^{ if (mm->wantRunning || !mm->location.empty()) mm->stop(); });
    }
}

void installExitHooks() {
    static dispatch_once_t once;
    dispatch_once(&once, ^{
        atexit(shutdownAllSessions);
        static std::vector<dispatch_source_t> sources;
        for (int sig : {SIGTERM, SIGINT, SIGHUP}) {
            signal(sig, SIG_IGN);
            dispatch_source_t src = dispatch_source_create(DISPATCH_SOURCE_TYPE_SIGNAL, (uintptr_t)sig, 0,
                                                           dispatch_get_global_queue(QOS_CLASS_USER_INITIATED, 0));
            dispatch_source_set_event_handler(src, ^{
                std::cerr << "[WHEP] signal " << sig << ": closing WHEP sessions" << std::endl;
                shutdownAllSessions();
                signal(sig, SIG_DFL);
                raise(sig);
            });
            dispatch_resume(src);
            sources.push_back(src);
        }
    });
}

void registerLive(const std::shared_ptr<WHEPSource::Impl>& m) {
    installExitHooks();
    std::lock_guard<std::mutex> l(gLiveMx);
    gLive.erase(std::remove_if(gLive.begin(), gLive.end(), [](auto& w) { return w.expired(); }), gLive.end());
    gLive.push_back(m);
}
} // namespace

// ─── WHEPSource ──────────────────────────────────────────────────────

WHEPSource::WHEPSource() : m(std::make_shared<Impl>()) {
    m->q = dispatch_queue_create("easel.whep", DISPATCH_QUEUE_SERIAL);
    registerLive(m);
}

WHEPSource::~WHEPSource() {
    auto mm = m;
    dispatch_sync(m->q, ^{ mm->stop(); });
    {
        std::lock_guard<std::mutex> l(m->frameMx);
        if (m->latestPB) { CVPixelBufferRelease(m->latestPB); m->latestPB = nullptr; }
        m->latestI420 = nil;
    }
    if (m->boundPB) { CVPixelBufferRelease(m->boundPB); m->boundPB = nullptr; }
    if (m->texY) glDeleteTextures(1, &m->texY);
    if (m->texC1) glDeleteTextures(1, &m->texC1);
    if (m->texC2) glDeleteTextures(1, &m->texC2);
    m->fbo.destroy();
    if (m->statsLog) { fclose(m->statsLog); m->statsLog = nullptr; }
}

bool WHEPSource::connect(const std::string& whepUrl) {
    if (whepUrl.rfind("http://", 0) != 0 && whepUrl.rfind("https://", 0) != 0) {
        m->setStatus("not a WHEP url: " + whepUrl);
        return false;
    }
    sharedFactory();
    auto mm = m;
    std::string u = whepUrl;
    dispatch_async(m->q, ^{
        if (mm->wantRunning) mm->stop();
        mm->configureUrl(u);
        mm->suspended = false;
        mm->start();
    });
    {
        std::lock_guard<std::mutex> l(m->statusMx);
        m->url = whepUrl;   // sourcePath() right away (the queue sets it too)
    }
    return true;
}

void WHEPSource::disconnect() {
    auto mm = m;
    dispatch_sync(m->q, ^{ mm->stop(); });
}

void WHEPSource::suspend() {
    // Only reachable from undo/redo snapshots: drop the session (and the
    // server's upload for it); update() brings it back if it goes live again.
    auto mm = m;
    dispatch_sync(m->q, ^{
        if (!mm->wantRunning) return;
        mm->stop();
        mm->suspended = true;
        mm->setStatus("suspended");
    });
}

bool WHEPSource::isConnected() const { return m->playing.load(); }
bool WHEPSource::isFailed() const { return false; }   // it retries forever; statusText says why
bool WHEPSource::isPlaying() const { return m->playing.load(); }
std::string WHEPSource::statusText() const { std::lock_guard<std::mutex> l(m->statusMx); return m->status; }
std::string WHEPSource::sourcePath() const { std::lock_guard<std::mutex> l(m->statusMx); return m->url; }
GLuint WHEPSource::textureId() const { return m->fbo.textureId(); }
int WHEPSource::width() const { return m->fbo.width() > 0 ? m->fbo.width() : 16; }
int WHEPSource::height() const { return m->fbo.height() > 0 ? m->fbo.height() : 9; }

void WHEPSource::update() {
    Impl& s = *m;
    if (s.suspended) {
        auto mm = m;
        dispatch_async(m->q, ^{ if (mm->suspended) { mm->suspended = false; mm->start(); } });
        s.suspended = false;
    }

    CVPixelBufferRef pb = nullptr;
    id<RTCI420Buffer> i420 = nil;
    uint64_t seq;
    {
        std::lock_guard<std::mutex> l(s.frameMx);
        seq = s.frameSeq;
        if (seq == s.presentedSeq) return;          // nothing new: show the last frame again
        pb = s.latestPB; s.latestPB = nullptr;      // take ownership
        i420 = s.latestI420; s.latestI420 = nil;
    }
    if (s.presentedSeq > 0 && seq > s.presentedSeq + 1) s.skipped += seq - s.presentedSeq - 1;
    s.presentedSeq = seq;
    if (!pb && !i420) return;

    if (!s.glReady && !s.glFailed) {
        if (s.prog.loadFromSource(kVS, kFS)) {
            s.quad.createQuad();
            GLuint t[3]; glGenTextures(3, t);
            s.texY = t[0]; s.texC1 = t[1]; s.texC2 = t[2];
            for (GLuint id : t) {
                glBindTexture(GL_TEXTURE_RECTANGLE, id);
                glTexParameteri(GL_TEXTURE_RECTANGLE, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_RECTANGLE, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_RECTANGLE, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
                glTexParameteri(GL_TEXTURE_RECTANGLE, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
            }
            glBindTexture(GL_TEXTURE_RECTANGLE, 0);
            s.glReady = true;
        } else {
            s.glFailed = true;
            std::cerr << "[WHEP] NV12 shader failed to compile" << std::endl;
        }
    }
    if (!s.glReady) { if (pb) CVPixelBufferRelease(pb); return; }

    int w = 0, h = 0;
    bool planar = false, fullRange = true, bt601 = false;
    if (pb) {
        w = (int)CVPixelBufferGetWidth(pb);
        h = (int)CVPixelBufferGetHeight(pb);
        fullRange = CVPixelBufferGetPixelFormatType(pb) == kCVPixelFormatType_420YpCbCr8BiPlanarFullRange;
        CFTypeRef mat = CVBufferCopyAttachment(pb, kCVImageBufferYCbCrMatrixKey, nullptr);
        if (mat) { bt601 = CFEqual(mat, kCVImageBufferYCbCrMatrix_ITU_R_601_4); CFRelease(mat); }
        IOSurfaceRef surf = CVPixelBufferGetIOSurface(pb);
        bool zeroCopy = false;
        if (surf) {
            CGLContextObj ctx = CGLGetCurrentContext();
            glBindTexture(GL_TEXTURE_RECTANGLE, s.texY);
            CGLError e1 = CGLTexImageIOSurface2D(ctx, GL_TEXTURE_RECTANGLE, GL_R8, w, h, GL_RED, GL_UNSIGNED_BYTE, surf, 0);
            glBindTexture(GL_TEXTURE_RECTANGLE, s.texC1);
            CGLError e2 = CGLTexImageIOSurface2D(ctx, GL_TEXTURE_RECTANGLE, GL_RG8, w / 2, h / 2, GL_RG, GL_UNSIGNED_BYTE, surf, 1);
            zeroCopy = (e1 == kCGLNoError && e2 == kCGLNoError);
        }
        if (!zeroCopy) {
            CVPixelBufferLockBaseAddress(pb, kCVPixelBufferLock_ReadOnly);
            glBindTexture(GL_TEXTURE_RECTANGLE, s.texY);
            glPixelStorei(GL_UNPACK_ROW_LENGTH, (GLint)CVPixelBufferGetBytesPerRowOfPlane(pb, 0));
            glTexImage2D(GL_TEXTURE_RECTANGLE, 0, GL_R8, w, h, 0, GL_RED, GL_UNSIGNED_BYTE, CVPixelBufferGetBaseAddressOfPlane(pb, 0));
            glBindTexture(GL_TEXTURE_RECTANGLE, s.texC1);
            glPixelStorei(GL_UNPACK_ROW_LENGTH, (GLint)CVPixelBufferGetBytesPerRowOfPlane(pb, 1) / 2);
            glTexImage2D(GL_TEXTURE_RECTANGLE, 0, GL_RG8, w / 2, h / 2, 0, GL_RG, GL_UNSIGNED_BYTE, CVPixelBufferGetBaseAddressOfPlane(pb, 1));
            glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
            CVPixelBufferUnlockBaseAddress(pb, kCVPixelBufferLock_ReadOnly);
        }
        if (!s.loggedPath) {
            s.loggedPath = true;
            std::cerr << "[WHEP] first frame " << w << "x" << h << " NV12 " << (fullRange ? "full" : "video")
                      << " range, " << (bt601 ? "BT.601" : "BT.709") << ", "
                      << (zeroCopy ? "IOSurface zero-copy" : "CPU upload") << std::endl;
        }
    } else {
        planar = true;
        w = i420.width; h = i420.height;
        auto up = [&](GLuint tex, const uint8_t* data, int stride, int pw, int ph) {
            glBindTexture(GL_TEXTURE_RECTANGLE, tex);
            glPixelStorei(GL_UNPACK_ROW_LENGTH, stride);
            glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
            glTexImage2D(GL_TEXTURE_RECTANGLE, 0, GL_R8, pw, ph, 0, GL_RED, GL_UNSIGNED_BYTE, data);
        };
        up(s.texY, i420.dataY, i420.strideY, w, h);
        up(s.texC1, i420.dataU, i420.strideU, i420.chromaWidth, i420.chromaHeight);
        up(s.texC2, i420.dataV, i420.strideV, i420.chromaWidth, i420.chromaHeight);
        glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
        glPixelStorei(GL_UNPACK_ALIGNMENT, 4);
        fullRange = false;   // software decode hands out limited range
        if (!s.loggedPath) { s.loggedPath = true; std::cerr << "[WHEP] first frame " << w << "x" << h << " I420 (software path)" << std::endl; }
    }
    glBindTexture(GL_TEXTURE_RECTANGLE, 0);
    if (w <= 0 || h <= 0) { if (pb) CVPixelBufferRelease(pb); return; }

    if (s.fbo.width() != w || s.fbo.height() != h) {
        s.fbo.destroy();
        s.fbo.create(w, h);
    }
    GLint vp[4]; glGetIntegerv(GL_VIEWPORT, vp);
    GLint prevFbo = 0; glGetIntegerv(GL_FRAMEBUFFER_BINDING, &prevFbo);
    GLboolean blend = glIsEnabled(GL_BLEND);
    glDisable(GL_BLEND);
    s.fbo.bind();
    glViewport(0, 0, w, h);
    s.prog.use();
    s.prog.setInt("uY", 0);
    s.prog.setInt("uC1", 1);
    s.prog.setInt("uC2", 2);
    s.prog.setBool("uPlanar", planar);
    s.prog.setBool("uFullRange", fullRange);
    s.prog.setVec2("uSize", glm::vec2((float)w, (float)h));
    s.prog.setMat3("uYuvToRgb", yuvMatrix(bt601));
    glActiveTexture(GL_TEXTURE0); glBindTexture(GL_TEXTURE_RECTANGLE, s.texY);
    glActiveTexture(GL_TEXTURE1); glBindTexture(GL_TEXTURE_RECTANGLE, s.texC1);
    glActiveTexture(GL_TEXTURE2); glBindTexture(GL_TEXTURE_RECTANGLE, s.texC2);
    s.quad.draw();
    glActiveTexture(GL_TEXTURE2); glBindTexture(GL_TEXTURE_RECTANGLE, 0);
    glActiveTexture(GL_TEXTURE1); glBindTexture(GL_TEXTURE_RECTANGLE, 0);
    glActiveTexture(GL_TEXTURE0); glBindTexture(GL_TEXTURE_RECTANGLE, 0);
    glBindFramebuffer(GL_FRAMEBUFFER, (GLuint)prevFbo);
    glViewport(vp[0], vp[1], vp[2], vp[3]);
    if (blend) glEnable(GL_BLEND);

    // the surface just bound stays alive until the next frame replaces it
    if (s.boundPB) CVPixelBufferRelease(s.boundPB);
    s.boundPB = pb;

    s.presented++;
    double t = nowSec();
    if (s.lastPresentT > 0.0) {
        double gap = t - s.lastPresentT, prev = s.maxPresentGap.load();
        if (gap > prev) s.maxPresentGap = gap;
    }
    s.lastPresentT = t;
}

// ─── helpers shared with the rest of the app ─────────────────────────

std::string WHEPSource::discoverUrl(const std::string& baseUrl) {
    std::string statusJson = httpRequest("GET", baseUrl + "/api/scope/status", "", "");
    size_t pos = statusJson.find("\"rtmp_url\"");
    if (pos != std::string::npos) {
        size_t a = statusJson.find('"', pos + 10);
        size_t b = a == std::string::npos ? a : statusJson.find('"', a + 1);
        if (b != std::string::npos) {
            std::string rtmpUrl = statusJson.substr(a + 1, b - a - 1);
            httpRequest("POST", "http://localhost:9997/v3/config/paths/add/longlive",
                        "{\"source\": \"" + rtmpUrl + "\"}", "application/json");
        }
    }
    return "http://localhost:8889/longlive/whep";
}

std::string httpRequest(const std::string& method, const std::string& url,
                        const std::string& body, const std::string& contentType,
                        std::string* outLocationHeader) {
    @autoreleasepool {
        NSURL* u = [NSURL URLWithString:[NSString stringWithUTF8String:url.c_str()]];
        if (!u) return {};
        NSMutableURLRequest* req = [NSMutableURLRequest requestWithURL:u];
        req.HTTPMethod = [NSString stringWithUTF8String:method.c_str()];
        if (!contentType.empty()) [req setValue:[NSString stringWithUTF8String:contentType.c_str()] forHTTPHeaderField:@"Content-Type"];
        if (!body.empty()) req.HTTPBody = [NSData dataWithBytes:body.data() length:body.size()];
        bool local = url.find("localhost") != std::string::npos || url.find("127.0.0.1") != std::string::npos;
        NSData* d = nil;
        NSHTTPURLResponse* r = syncRequest(req, &d, local ? 1.0 : 5.0);
        if (!r) return {};
        if (outLocationHeader) *outLocationHeader = toStd([r valueForHTTPHeaderField:@"Location"]);
        if (!d) return {};
        return std::string((const char*)d.bytes, d.length);
    }
}

#endif // __APPLE__ && EASEL_WEBRTC_MAC
