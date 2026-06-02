#pragma once
#include <string>
#include <vector>
#include <cstdint>

// Cross-platform NIC enumeration. Intentionally NDI-agnostic (always compiled)
// so it can also back a generic "preferred network interface" setting. The
// load-bearing value for NDI is the chosen adapter's IPv4 (NDI's machine config
// pins to the NIC by IP, not by name).
struct NetAdapterInfo {
    std::string name;          // Win: GUID (\Device\... / {GUID}); mac: BSD name en0
    std::string friendlyLabel; // Win: FriendlyName (UTF-8); mac: "Wi-Fi (en0)"
    enum class Kind { WiFi, Ethernet, Other } kind = Kind::Other;
    std::string ipv4;          // dotted, first preferred non-APIPA unicast
    std::string subnetMask;    // dotted (e.g. 255.255.255.0)
    bool isUp = false;
};

// Enumerate currently-up adapters that have a usable IPv4. Relatively
// expensive on Windows (GetAdaptersAddresses traverses NDIS) — call
// on-demand (panel open / Refresh), NOT per frame; cache the result.
std::vector<NetAdapterInfo> EnumerateNetworkAdapters();

// Helper: classify kind -> short tag ("Wi-Fi" / "Ethernet" / "Net").
const char* NetAdapterKindTag(NetAdapterInfo::Kind k);

// Helper: (a & mask) == (b & mask) over dotted IPv4 strings. Returns
// false if either string fails to parse. Used for same-subnet checks.
bool SameSubnet(const std::string& ipA, const std::string& ipB, const std::string& maskDotted);
