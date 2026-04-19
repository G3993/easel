#ifdef HAS_NDI
#include "sources/NDIRuntime.h"
#include <iostream>
#include <fstream>
#include <chrono>
#include <thread>
#include <cstring>
#include <vector>

// Write raw BGRA as BMP
static void writeBMP(const char* path, const uint8_t* data, int w, int h) {
    int rowBytes = w * 4;
    int imageSize = rowBytes * h;
    int fileSize = 54 + imageSize;

    uint8_t header[54] = {};
    header[0] = 'B'; header[1] = 'M';
    memcpy(header + 2, &fileSize, 4);
    int offset = 54; memcpy(header + 10, &offset, 4);
    int infoSize = 40; memcpy(header + 14, &infoSize, 4);
    memcpy(header + 18, &w, 4);
    memcpy(header + 22, &h, 4);
    short planes = 1; memcpy(header + 26, &planes, 2);
    short bpp = 32; memcpy(header + 28, &bpp, 2);
    memcpy(header + 34, &imageSize, 4);

    std::ofstream f(path, std::ios::binary);
    f.write((char*)header, 54);
    // BMP is bottom-up, NDI is top-down — flip
    for (int y = h - 1; y >= 0; y--) {
        f.write((const char*)(data + y * rowBytes), rowBytes);
    }
}

int main() {
    auto& rt = NDIRuntime::instance();
    if (!rt.init()) { std::cerr << "NDI init failed\n"; return 1; }

    // Find source
    auto* finder = rt.api()->find_create_v2(nullptr);
    if (!finder) { std::cerr << "Finder failed\n"; return 1; }

    std::cout << "Searching for NDI sources...\n";
    const NDIlib_source_t* sources = nullptr;
    uint32_t count = 0;
    for (int i = 0; i < 40; i++) {
        rt.api()->find_wait_for_sources(finder, 200);
        sources = rt.api()->find_get_current_sources(finder, &count);
        if (count > 0) break;
    }
    if (count == 0) { std::cerr << "No sources found\n"; return 1; }

    std::cout << "Connecting to: " << sources[0].p_ndi_name << "\n";
    NDIlib_recv_create_v3_t recvCreate = {};
    recvCreate.source_to_connect_to = sources[0];
    recvCreate.color_format = NDIlib_recv_color_format_BGRX_BGRA;
    auto* recv = rt.api()->recv_create_v3(&recvCreate);
    rt.api()->find_destroy(finder);
    if (!recv) { std::cerr << "Recv failed\n"; return 1; }

    // Wait for connection
    std::this_thread::sleep_for(std::chrono::seconds(1));

    // Capture 10 consecutive frames
    std::cout << "Capturing 10 consecutive frames...\n";
    int captured = 0;
    int attempts = 0;
    while (captured < 10 && attempts < 100) {
        NDIlib_video_frame_v2_t video = {};
        auto type = rt.api()->recv_capture_v2(recv, &video, nullptr, nullptr, 100);
        if (type == NDIlib_frame_type_video) {
            char path[256];
            snprintf(path, sizeof(path), "/tmp/ndi_frame_%02d.bmp", captured);
            writeBMP(path, video.p_data, video.xres, video.yres);

            // Also compute average brightness to detect black frames
            long long sum = 0;
            int pixels = video.xres * video.yres;
            for (int i = 0; i < pixels; i++) {
                int idx = i * 4;
                sum += video.p_data[idx] + video.p_data[idx+1] + video.p_data[idx+2];
            }
            double avg = (double)sum / (pixels * 3.0);

            std::cout << "  Frame " << captured << ": " << video.xres << "x" << video.yres
                      << " avg_brightness=" << avg << "\n";

            rt.api()->recv_free_video_v2(recv, &video);
            captured++;
        }
        attempts++;
    }

    rt.api()->recv_destroy(recv);
    std::cout << "Saved " << captured << " frames to /tmp/ndi_frame_*.bmp\n";
    return 0;
}
#else
#include <iostream>
int main() { std::cerr << "NDI not available\n"; return 1; }
#endif
