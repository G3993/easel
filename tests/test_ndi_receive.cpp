#ifdef HAS_NDI
#include "sources/NDIRuntime.h"
#include "sources/NDISource.h"
#include <iostream>
#include <chrono>
#include <thread>
#include <cstring>

int main() {
    NDIRuntime::instance().init();
    if (!NDIRuntime::instance().isAvailable()) {
        std::cerr << "NDI runtime not available" << std::endl;
        return 1;
    }

    auto* api = NDIRuntime::instance().api();

    // Find sources
    NDIlib_find_create_t findCreate = {};
    findCreate.show_local_sources = true;
    auto finder = api->find_create_v2(&findCreate);

    std::cout << "Searching for NDI sources (8 seconds)..." << std::endl;
    for (int i = 0; i < 4; i++) api->find_wait_for_sources(finder, 2000);

    uint32_t count = 0;
    const NDIlib_source_t* sources = api->find_get_current_sources(finder, &count);

    std::cout << "Found " << count << " sources:" << std::endl;
    int scIdx = -1;
    for (uint32_t i = 0; i < count; i++) {
        std::cout << "  [" << i << "] " << sources[i].p_ndi_name << std::endl;
        std::string name = sources[i].p_ndi_name ? sources[i].p_ndi_name : "";
        // Case-insensitive search for shaderclaw
        std::string lower = name;
        for (auto& c : lower) c = tolower(c);
        if (lower.find("shaderclaw") != std::string::npos ||
            lower.find("shader-claw") != std::string::npos ||
            lower.find("shader_claw") != std::string::npos) {
            scIdx = i;
        }
    }

    if (scIdx < 0) {
        // Try any non-Easel source
        for (uint32_t i = 0; i < count; i++) {
            std::string name = sources[i].p_ndi_name ? sources[i].p_ndi_name : "";
            if (name.find("(Easel)") == std::string::npos) {
                scIdx = i;
                std::cout << "Using non-Easel source: " << name << std::endl;
                break;
            }
        }
    }
    if (scIdx < 0 && count > 0) {
        std::cout << "Only Easel found, using first source" << std::endl;
        scIdx = 0;
    }
    if (scIdx < 0) {
        std::cerr << "No NDI sources found at all" << std::endl;
        api->find_destroy(finder);
        return 1;
    }

    std::cout << "\nConnecting to: " << sources[scIdx].p_ndi_name << std::endl;

    // Create receiver
    NDIlib_recv_create_v3_t recvCreate = {};
    recvCreate.color_format = NDIlib_recv_color_format_RGBX_RGBA;
    recvCreate.bandwidth = NDIlib_recv_bandwidth_highest;
    recvCreate.allow_video_fields = true;
    recvCreate.p_ndi_recv_name = "Easel-Test";

    auto recv = api->recv_create_v3(&recvCreate);
    if (!recv) {
        std::cerr << "Failed to create receiver" << std::endl;
        api->find_destroy(finder);
        return 1;
    }

    api->recv_connect(recv, &sources[scIdx]);
    api->find_destroy(finder);

    std::cout << "Receiving frames for 5 seconds..." << std::endl;
    std::cout << "---" << std::endl;

    auto start = std::chrono::steady_clock::now();
    int videoFrames = 0;
    int audioFrames = 0;
    int metaFrames = 0;
    int emptyPolls = 0;
    int lastW = 0, lastH = 0;

    auto lastPrint = start;
    int intervalFrames = 0;

    while (true) {
        auto now = std::chrono::steady_clock::now();
        double elapsed = std::chrono::duration<double>(now - start).count();
        if (elapsed > 5.0) break;

        NDIlib_video_frame_v2_t video = {};
        NDIlib_audio_frame_v3_t audio = {};
        NDIlib_metadata_frame_t meta = {};

        NDIlib_frame_type_e type = api->recv_capture_v3(recv, &video, &audio, &meta, 100);

        if (type == NDIlib_frame_type_video) {
            videoFrames++;
            intervalFrames++;
            lastW = video.xres;
            lastH = video.yres;
            api->recv_free_video_v2(recv, &video);
        } else if (type == NDIlib_frame_type_audio) {
            audioFrames++;
            api->recv_free_audio_v3(recv, &audio);
        } else if (type == NDIlib_frame_type_metadata) {
            metaFrames++;
            api->recv_free_metadata(recv, &meta);
        } else if (type == NDIlib_frame_type_none) {
            emptyPolls++;
        }

        // Print stats every second
        double sincePrint = std::chrono::duration<double>(now - lastPrint).count();
        if (sincePrint >= 1.0) {
            std::cout << "  " << intervalFrames << " video fps | "
                      << lastW << "x" << lastH << " | "
                      << emptyPolls << " empty polls" << std::endl;
            intervalFrames = 0;
            emptyPolls = 0;
            lastPrint = now;
        }
    }

    double totalSec = std::chrono::duration<double>(
        std::chrono::steady_clock::now() - start).count();

    std::cout << "---" << std::endl;
    std::cout << "Total: " << videoFrames << " video, "
              << audioFrames << " audio, " << metaFrames << " meta in "
              << totalSec << "s" << std::endl;
    std::cout << "Average: " << (videoFrames / totalSec) << " video fps" << std::endl;
    std::cout << "Resolution: " << lastW << "x" << lastH << std::endl;

    api->recv_destroy(recv);
    return 0;
}

#else
#include <iostream>
int main() {
    std::cerr << "NDI not available in this build" << std::endl;
    return 1;
}
#endif
