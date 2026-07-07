#ifdef HAS_OPENCV
#include "scanning/WebcamSource.h"
#include <opencv2/imgproc.hpp>
#include <chrono>
#include <iostream>

WebcamSource::~WebcamSource() {
    close();
}

bool WebcamSource::open(int cameraIndex) {
    close();
    m_cameraIndex = cameraIndex;

    if (!m_capture.open(cameraIndex, cv::CAP_ANY)) {
        std::cerr << "WebcamSource: failed to open camera " << cameraIndex << std::endl;
        return false;
    }

    // Try to set a reasonable resolution
    m_capture.set(cv::CAP_PROP_FRAME_WIDTH, 1280);
    m_capture.set(cv::CAP_PROP_FRAME_HEIGHT, 720);

    m_camWidth = (int)m_capture.get(cv::CAP_PROP_FRAME_WIDTH);
    m_camHeight = (int)m_capture.get(cv::CAP_PROP_FRAME_HEIGHT);

    m_rgbaBuffer.resize(m_camWidth * m_camHeight * 4);
    m_texture.createEmpty(m_camWidth, m_camHeight);
    m_open = true;

    m_running = true;
    m_captureThread = std::thread(&WebcamSource::captureLoop, this);

    std::cout << "WebcamSource: opened camera " << cameraIndex
              << " (" << m_camWidth << "x" << m_camHeight << ")" << std::endl;
    return true;
}

void WebcamSource::close() {
    m_running = false;
    m_frameCv.notify_all(); // release any captureFrame() waiter promptly
    if (m_captureThread.joinable()) {
        m_captureThread.join();
    }
    if (m_capture.isOpened()) {
        m_capture.release();
    }
    {
        std::lock_guard<std::mutex> lock(m_frameMutex);
        m_latestBGR.release();
        m_frameFresh = false;
    }
    m_open = false;
}

bool WebcamSource::isOpen() const {
    // m_capture is owned by the worker thread while running — don't poke it
    // from other threads here.
    return m_open;
}

void WebcamSource::suspend() {
    if (!m_open) return;
    close();
    m_suspended = true;
}

void WebcamSource::captureLoop() {
    while (m_running) {
        cv::Mat frame;
        // read() blocks at camera rate — that's the pacing of this loop.
        if (!m_capture.read(frame) || frame.empty()) {
            std::this_thread::sleep_for(std::chrono::milliseconds(5));
            continue;
        }
        {
            std::lock_guard<std::mutex> lock(m_frameMutex);
            m_latestBGR = std::move(frame);
            m_frameFresh = true;
            m_frameSeq++;
        }
        m_frameCv.notify_all();
    }
}

void WebcamSource::update() {
    if (!m_open) {
        // Lazy resume after suspend(): the source is back in the live stack.
        if (m_suspended) {
            m_suspended = false; // one attempt — camera may be gone
            open(m_cameraIndex);
        }
        return;
    }

    cv::Mat frame;
    {
        std::lock_guard<std::mutex> lock(m_frameMutex);
        if (!m_frameFresh) return; // no new frame since last upload
        frame = m_latestBGR.clone();
        m_frameFresh = false;
    }

    // BGR -> RGBA
    cv::Mat rgba;
    cv::cvtColor(frame, rgba, cv::COLOR_BGR2RGBA);

    // Flip vertically for OpenGL (bottom-up)
    cv::flip(rgba, rgba, 0);

    m_texture.updateData(rgba.data, rgba.cols, rgba.rows, GL_RGBA, GL_UNSIGNED_BYTE);
}

cv::Mat WebcamSource::captureFrame() {
    if (!m_open) return cv::Mat();

    // Wait for a frame grabbed AFTER this call — the scanner flips a
    // projected pattern and then captures, so handing back the latest
    // buffered frame could return one exposed before the flip (or the same
    // frame twice for consecutive patterns).
    std::unique_lock<std::mutex> lock(m_frameMutex);
    const uint64_t startSeq = m_frameSeq;
    m_frameCv.wait_for(lock, std::chrono::milliseconds(500),
                       [&] { return m_frameSeq > startSeq || !m_running; });
    return m_latestBGR.clone(); // BGR
}

#endif // HAS_OPENCV
