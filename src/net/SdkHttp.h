#pragma once
#include <string>
#include <utility>
#include <vector>

// Minimal blocking JSON-over-HTTP POST for the Easel agent SDK control path.
// Unlike WHEPSource's httpRequest() this carries custom headers (the SDK key,
// X-Easel-House-Act) and reports the HTTP status, so callers can tell a 401
// from an envelope error from a transport failure. Always call it off the
// render/UI thread.
//   macOS:   NSURLSession   (SdkHttp_mac.mm)
//   Windows: WinHTTP        (SdkHttp_win.cpp)
struct SdkHttpResponse {
    int status = 0;          // HTTP status; 0 = no HTTP response at all
    std::string body;
    std::string error;       // transport error text when status == 0
    bool timedOut = false;   // request may still have been delivered
};

SdkHttpResponse sdkHttpPostJson(const std::string& url,
                                const std::string& jsonBody,
                                const std::vector<std::pair<std::string, std::string>>& headers,
                                double timeoutSec);
