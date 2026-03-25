#ifdef HAS_NDI
#include "sources/NDIRuntime.h"
#include <iostream>
#include <chrono>
#include <thread>

int main() {
    NDIRuntime::instance().init();
    if (!NDIRuntime::instance().isAvailable()) {
        std::cerr << "NDI runtime not available" << std::endl;
        return 1;
    }

    auto* api = NDIRuntime::instance().api();

    NDIlib_find_create_t findCreate = {};
    findCreate.show_local_sources = true;
    auto finder = api->find_create_v2(&findCreate);

    std::cout << "Searching for NDI sources (polling every 2s for 10s)..." << std::endl;

    for (int round = 0; round < 5; round++) {
        api->find_wait_for_sources(finder, 2000);

        uint32_t count = 0;
        const NDIlib_source_t* sources = api->find_get_current_sources(finder, &count);

        std::cout << "\n--- Round " << (round + 1) << ": " << count << " sources ---" << std::endl;
        for (uint32_t i = 0; i < count; i++) {
            std::cout << "  [" << i << "] name: " << (sources[i].p_ndi_name ? sources[i].p_ndi_name : "(null)")
                      << "  url: " << (sources[i].p_url_address ? sources[i].p_url_address : "(null)") << std::endl;
        }
    }

    api->find_destroy(finder);
    return 0;
}
#else
#include <iostream>
int main() { std::cerr << "NDI not available" << std::endl; return 1; }
#endif
