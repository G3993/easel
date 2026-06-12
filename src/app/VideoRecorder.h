#pragma once
#ifdef HAS_FFMPEG

#include <glad/glad.h>
#include <string>
#include <vector>
#include <deque>
#include <thread>
#include <mutex>
#include <atomic>
#include <condition_variable>
#include <cstdint>

struct AVFormatContext;
struct AVCodecContext;
struct AVStream;
struct AVFrame;
struct AVPacket;
struct SwsContext;
struct SwrContext;

struct RecAudioDevice {
    std::string name;
    std::string id;       // WASAPI endpoint ID
    bool isCapture;       // true = microphone, false = output (loopback)
};

class VideoRecorder {
public:
    ~VideoRecorder();

    static std::vector<RecAudioDevice> enumerateAudioDevices();

    void setAudioDevice(int index) { m_selectedAudioDevice = index; }
    int audioDevice() const { return m_selectedAudioDevice; }

    // fps is only a hint now: it feeds gop_size + the framerate metadata. Actual
    // frame timing is wall-clock VFR, so recording tracks the real render rate.
    bool start(const std::string& path, int width, int height, int fps = 60);
    void stop();
    bool isActive() const { return m_active; }

    void sendFrame(GLuint texture, int w, int h);

    double uptimeSeconds() const;
    const std::string& filePath() const { return m_path; }

private:
    // One captured frame handed from the GL thread to the encode thread. Carries
    // its own size (so a mid-recording zone resize is rescaled, not dropped) and
    // its wall-clock capture time (so PTS reflects real elapsed time → correct speed).
    struct RecFrame {
        std::vector<uint8_t> pixels;
        int w = 0;
        int h = 0;
        int64_t captureUs = 0;
    };

    bool initEncoder(const std::string& path, int width, int height, int fps);
    bool initAudioCapture();
    void encodeThread();
    void encodeVideoFrame(const RecFrame& f);
    void drainAudio();
    void encodeAudioSamples(const float* data, int numSamples, int channels);
    void cleanup();
    void cleanupAudio();

    std::string m_path;
    std::atomic<bool> m_active{false};
    std::atomic<bool> m_stopRequested{false};
    int m_selectedAudioDevice = -1; // -1 = default output loopback

    // Video
    AVFormatContext* m_fmtCtx = nullptr;
    AVCodecContext* m_videoCodecCtx = nullptr;
    AVStream* m_videoStream = nullptr;
    AVFrame* m_videoFrame = nullptr;
    AVPacket* m_packet = nullptr;
    SwsContext* m_swsCtx = nullptr;
    int64_t m_videoFrameIndex = 0;
    int m_width = 0, m_height = 0;          // fixed encoder output size (pinned at start)
    int m_srcW = 0, m_srcH = 0;             // current sws input size (differs after a zone/canvas resize)
    double m_startTime = 0;
    int64_t m_startPtsUs = 0;               // wall-clock anchor (av_gettime_relative) of the first encoded frame
    int64_t m_lastVideoPts = -1;            // strictly-monotonic guard for VFR pts (in codec time_base ticks)

    // Audio
    AVCodecContext* m_audioCodecCtx = nullptr;
    AVStream* m_audioStream = nullptr;
    AVFrame* m_audioFrame = nullptr;
    SwrContext* m_swrCtx = nullptr;
    int64_t m_audioSamplesWritten = 0;
    int64_t m_audioStartUs = 0;     // wall-clock anchor for loopback silence-fill
    int m_audioFrameSize = 0;
    std::vector<float> m_audioAccum;

    void* m_audioClient = nullptr;
    void* m_captureClient = nullptr;
    void* m_audioDevice = nullptr;
    int m_wasapiSampleRate = 0;
    int m_wasapiChannels = 0;

    // PBO async readback (double-buffered)
    GLuint m_pbo[2] = {0, 0};
    int m_pboIndex = 0;
    bool m_pboReady = false;
    int m_prevW = 0, m_prevH = 0;           // size of the frame whose readback was last issued
    size_t m_pboBytes = 0;                  // current PBO byte size (detects a zone/canvas resize)

    // Frame handoff — bounded queue of timestamped RGBA frames (true VFR), with a
    // small pool of recycled pixel buffers to avoid per-frame multi-MB allocations.
    std::deque<RecFrame> m_frameQueue;
    std::vector<std::vector<uint8_t>> m_bufPool;
    size_t m_maxQueue = 8;                  // cap; drop the OLDEST frame under sustained overload
    std::mutex m_mutex;
    std::condition_variable m_cv;
    std::thread m_thread;
};

#endif
