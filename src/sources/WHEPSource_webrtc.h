#pragma once
// macOS WHEP receiver on Google's libwebrtc (WebRTC.xcframework, the engine
// Chrome and the Etherea iPhone app use). Included from WHEPSource.h when
// EASEL_WEBRTC_MAC is set; same public surface as the libdatachannel class
// so project load ("WHEP" + URL), the menus and the Sources panel are shared.
//
// What libwebrtc brings over the hand-made path: a real jitter buffer with
// NACK/RTX, clock-drift handling, VideoToolbox decode and paced delivery.
// Frames arrive as NV12 CVPixelBuffers; update() binds their IOSurface planes
// as GL textures (no copy) and converts NV12 -> RGB in a shader.
#include "sources/ContentSource.h"
#include <string>
#include <memory>

// Shared HTTP utility (NSURLSession here; WinHTTP / sockets elsewhere)
std::string httpRequest(const std::string& method, const std::string& url,
                        const std::string& body, const std::string& contentType,
                        std::string* outLocationHeader = nullptr);

class WHEPSource : public ContentSource {
public:
    WHEPSource();
    ~WHEPSource() override;

    // Start receiving (asynchronous: signaling, ICE and the first frame all
    // happen in the background; reconnects forever with 1/2/5 s backoff).
    // Returns false only for a URL that is not http(s).
    bool connect(const std::string& whepUrl);
    void disconnect();
    bool isConnected() const;
    bool isFailed() const;
    std::string statusText() const override;

    // Auto-discover the Scope WHEP URL from the Etherea backend (localhost)
    static std::string discoverUrl(const std::string& baseUrl = "http://localhost:7860");

    void update() override;
    void suspend() override;
    GLuint textureId() const override;
    int width() const override;
    int height() const override;
    std::string typeName() const override { return "WHEP"; }
    std::string sourcePath() const override;
    bool isVideo() const override { return true; }
    bool isFlippedV() const override { return true; }   // row 0 = image top, like VideoSource / NDI
    bool isPlaying() const override;

    struct Impl;
private:
    std::shared_ptr<Impl> m;
};
