#pragma once
#ifdef HAS_OPENCV

#include "sources/ContentSource.h"
#include "render/Texture.h"

#include <opencv2/core.hpp>
#include <opencv2/videoio.hpp>

#include <string>
#include <mutex>
#include <thread>
#include <atomic>
#include <condition_variable>

class WebcamSource : public ContentSource {
public:
    WebcamSource() = default;
    ~WebcamSource() override;

    bool open(int cameraIndex = 0);
    void close();
    bool isOpen() const;

    // Release the camera while keeping the index; update() lazily reopens
    // on the next live frame.
    void suspend() override;

    // ContentSource interface
    void update() override;
    GLuint textureId() const override { return m_texture.id(); }
    int width() const override { return m_texture.width(); }
    int height() const override { return m_texture.height(); }
    std::string typeName() const override { return "Webcam"; }

    // Scanner access — BLOCKS until the worker grabs a frame newer than the
    // moment of the call (so a scan capture can't see a frame exposed before
    // the pattern flip), then returns it (BGR). Times out after ~500ms and
    // returns whatever is latest.
    cv::Mat captureFrame();

    // Camera properties
    int cameraWidth() const { return m_camWidth; }
    int cameraHeight() const { return m_camHeight; }

private:
    // The worker thread owns ALL reads from m_capture (cv::VideoCapture::read
    // blocks at camera rate — ~33ms at 30fps — and used to block the render
    // thread, capping the whole compositor at camera rate). update() and
    // captureFrame() only touch m_latestBGR under the mutex.
    void captureLoop();

    cv::VideoCapture m_capture;
    Texture m_texture;
    std::vector<uint8_t> m_rgbaBuffer;
    int m_camWidth = 0;
    int m_camHeight = 0;
    bool m_open = false;

    std::thread m_captureThread;
    std::atomic<bool> m_running{false};
    std::mutex m_frameMutex;
    std::condition_variable m_frameCv;
    cv::Mat m_latestBGR;
    bool m_frameFresh = false;
    uint64_t m_frameSeq = 0; // bumps on every worker grab (for captureFrame)

    int m_cameraIndex = 0;
    bool m_suspended = false;
};

#endif // HAS_OPENCV
