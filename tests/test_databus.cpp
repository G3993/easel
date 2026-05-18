#define CATCH_CONFIG_MAIN
#include <catch2/catch_all.hpp>
#include "app/DataBus.h"
#include <thread>
#include <atomic>
#include <chrono>

TEST_CASE("DataBus string set/get round-trip", "[databus][string]") {
    DataBus bus;
    bus.set("foo", "hello");
    REQUIRE(bus.get("foo") == "hello");
    bus.set("foo", "world");
    REQUIRE(bus.get("foo") == "world");
    REQUIRE(bus.get("missing").empty());
}

TEST_CASE("DataBus numeric set/get round-trip", "[databus][numeric]") {
    DataBus bus;

    SECTION("Initial state has no number") {
        REQUIRE_FALSE(bus.hasNum("vision.pose.head.x"));
        REQUIRE(bus.getNum("vision.pose.head.x") == 0.0f);
    }

    SECTION("Custom default returned when unset") {
        REQUIRE(bus.getNum("missing", -1.0f) == -1.0f);
        REQUIRE(bus.getNum("missing", 3.14f) == 3.14f);
    }

    SECTION("setNum then getNum returns the value") {
        bus.setNum("vision.pose.head.x", 0.42f);
        REQUIRE(bus.hasNum("vision.pose.head.x"));
        REQUIRE(bus.getNum("vision.pose.head.x") == Catch::Approx(0.42f));
    }

    SECTION("setNum overwrites existing value") {
        bus.setNum("k", 1.0f);
        bus.setNum("k", 2.0f);
        REQUIRE(bus.getNum("k") == Catch::Approx(2.0f));
    }

    SECTION("Numeric and string maps are independent") {
        bus.set("k", "stringval");
        bus.setNum("k", 9.0f);
        REQUIRE(bus.get("k") == "stringval");
        REQUIRE(bus.getNum("k") == Catch::Approx(9.0f));
    }
}

TEST_CASE("DataBus bindings are unaffected by numeric API", "[databus][bindings]") {
    DataBus bus;
    bus.bind(7, "intensity", "vision.pose.head.x");
    REQUIRE(bus.binding(7, "intensity") == "vision.pose.head.x");
    bus.bind(7, "intensity", "");
    REQUIRE(bus.binding(7, "intensity").empty());
}

TEST_CASE("DataBus availableNumericKeys lists vision channels", "[databus][catalogue]") {
    auto keys = DataBus::availableNumericKeys();
    REQUIRE(keys.size() >= 3);

    bool sawHead = false, sawHand = false, sawFace = false;
    for (auto& k : keys) {
        if (k.key == "vision.pose.head.x") sawHead = true;
        if (k.key == "vision.hand.count")  sawHand = true;
        if (k.key == "vision.face.smile")  sawFace = true;
    }
    REQUIRE(sawHead);
    REQUIRE(sawHand);
    REQUIRE(sawFace);
}

TEST_CASE("DataBus numeric API is thread-safe under concurrent writers", "[databus][concurrency]") {
    DataBus bus;
    std::atomic<bool> stop{false};

    auto writer = [&](const std::string& key, float base) {
        for (int i = 0; i < 10000 && !stop.load(); i++) {
            bus.setNum(key, base + (float)i);
        }
    };

    std::thread t1(writer, "a", 1000.0f);
    std::thread t2(writer, "b", 2000.0f);
    std::thread reader([&]() {
        // Reads should never crash and should always return either 0
        // (default) or a value within the writer's range.
        for (int i = 0; i < 20000 && !stop.load(); i++) {
            float a = bus.getNum("a", -1.0f);
            float b = bus.getNum("b", -1.0f);
            REQUIRE((a == -1.0f || (a >= 1000.0f && a < 11000.0f)));
            REQUIRE((b == -1.0f || (b >= 2000.0f && b < 12000.0f)));
        }
    });

    t1.join();
    t2.join();
    stop.store(true);
    reader.join();

    // Final values must be the last write
    REQUIRE(bus.getNum("a") >= 1000.0f);
    REQUIRE(bus.getNum("b") >= 2000.0f);
}
