// WinHTTP backend for sdkHttpPostJson. On non-Apple POSIX builds there is no
// backend yet; the call reports an error instead of failing to link.
#include "net/SdkHttp.h"

#if defined(_WIN32)
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#include <winhttp.h>
#pragma comment(lib, "winhttp.lib")

static std::wstring widen(const std::string& s) {
    if (s.empty()) return {};
    int n = MultiByteToWideChar(CP_UTF8, 0, s.data(), (int)s.size(), nullptr, 0);
    std::wstring w(n, 0);
    MultiByteToWideChar(CP_UTF8, 0, s.data(), (int)s.size(), &w[0], n);
    return w;
}

SdkHttpResponse sdkHttpPostJson(const std::string& url,
                                const std::string& jsonBody,
                                const std::vector<std::pair<std::string, std::string>>& headers,
                                double timeoutSec) {
    SdkHttpResponse out;
    std::wstring wurl = widen(url);
    URL_COMPONENTS uc{};
    uc.dwStructSize = sizeof(uc);
    wchar_t host[256] = {0}, path[2048] = {0};
    uc.lpszHostName = host; uc.dwHostNameLength = 255;
    uc.lpszUrlPath = path;  uc.dwUrlPathLength = 2047;
    if (!WinHttpCrackUrl(wurl.c_str(), 0, 0, &uc)) { out.error = "invalid URL"; return out; }
    std::wstring fullPath = path;

    HINTERNET ses = WinHttpOpen(L"Easel/1.0", WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
                                WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
    if (!ses) { out.error = "WinHttpOpen failed"; return out; }
    HINTERNET con = WinHttpConnect(ses, host, uc.nPort, 0);
    HINTERNET req = con ? WinHttpOpenRequest(con, L"POST", fullPath.c_str(), nullptr,
                                             WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES,
                                             uc.nScheme == INTERNET_SCHEME_HTTPS ? WINHTTP_FLAG_SECURE : 0)
                        : nullptr;
    auto cleanup = [&] {
        if (req) WinHttpCloseHandle(req);
        if (con) WinHttpCloseHandle(con);
        WinHttpCloseHandle(ses);
    };
    if (!req) { out.error = "connect failed"; cleanup(); return out; }

    int ms = (int)(timeoutSec * 1000.0);
    WinHttpSetTimeouts(req, ms, ms, ms, ms);

    std::wstring hdr = L"Content-Type: application/json\r\n";
    for (const auto& h : headers) hdr += widen(h.first) + L": " + widen(h.second) + L"\r\n";

    BOOL ok = WinHttpSendRequest(req, hdr.c_str(), (DWORD)-1L,
                                 jsonBody.empty() ? WINHTTP_NO_REQUEST_DATA : (LPVOID)jsonBody.data(),
                                 (DWORD)jsonBody.size(), (DWORD)jsonBody.size(), 0);
    if (!ok || !WinHttpReceiveResponse(req, nullptr)) {
        DWORD e = GetLastError();
        out.timedOut = (e == ERROR_WINHTTP_TIMEOUT);
        out.error = out.timedOut ? "timed out" : ("network error " + std::to_string(e));
        cleanup();
        return out;
    }
    DWORD status = 0, sz = sizeof(status);
    WinHttpQueryHeaders(req, WINHTTP_QUERY_STATUS_CODE | WINHTTP_QUERY_FLAG_NUMBER,
                        WINHTTP_HEADER_NAME_BY_INDEX, &status, &sz, WINHTTP_NO_HEADER_INDEX);
    out.status = (int)status;
    DWORD avail = 0;
    while (WinHttpQueryDataAvailable(req, &avail) && avail > 0) {
        std::string buf(avail, '\0');
        DWORD got = 0;
        if (!WinHttpReadData(req, &buf[0], avail, &got) || got == 0) break;
        out.body.append(buf.data(), got);
    }
    cleanup();
    return out;
}

#elif !defined(__APPLE__)
SdkHttpResponse sdkHttpPostJson(const std::string&, const std::string&,
                                const std::vector<std::pair<std::string, std::string>>&, double) {
    SdkHttpResponse out;
    out.error = "SDK HTTP not implemented on this platform";
    return out;
}
#endif
