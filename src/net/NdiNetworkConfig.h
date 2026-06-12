#pragma once
#ifdef HAS_NDI
#include <string>
#include <vector>
#include <utility>

// Holds the user's machine-wide NDI network selection. The load-bearing value
// is interfaceIp: NDI's machine config (ndi-config.v1.json -> ndi.adapters.allowed)
// pins all NDI send/recv/discovery to a NIC by its IPv4, NOT by name. The SDK
// send_create_t / recv_create_v3_t structs have no NIC field, so this file (read
// only at NDIlib_initialize() time) is the only lever.
struct NdiNetworkSettings {
    bool        enabled = false;       // false = Auto (don't write adapters.allowed)
    std::string interfaceName;         // adapter GUID/BSD name (for re-match on load)
    std::string interfaceIp;           // NIC IPv4 — the load-bearing value for adapters.allowed
    std::string extraIps;              // comma-separated remote peer IPs for cross-subnet discovery
    bool        useDiscoveryServer = false;
    std::string discoveryServer;       // "ip" or "ip,ip2" (TCP 5959 registry)
};

struct NdiPeerStatus {
    std::string name;        // NDI source name
    std::string url;         // advertised ip:port (host:port from p_url_address)
    std::string ip;          // parsed host part
    bool sameSubnet = false; // vs the selected/active NIC
    bool reachable = false;  // TCP-connect to ip:5960 succeeded (or :5959 if server)
};

namespace NdiNetworkConfig {
    // Returns the app-owned config folder Easel writes ndi-config.v1.json into
    // (Win: %LOCALAPPDATA%\Easel\ndi ; mac: ~/Library/Application Support/Easel/ndi ;
    // Linux: $XDG_CONFIG_HOME|$HOME/.config/easel/ndi). Created if missing.
    std::string configDir();

    // Write ndi-config.v1.json into configDir() from the given settings and set
    // the NDI_CONFIG_DIR env var to that folder. MUST be called BEFORE
    // NDIlib_initialize(). Returns true on success. When s.enabled is false it
    // still writes a minimal file WITHOUT adapters.allowed (Auto = all NICs).
    bool applyToEnv(const NdiNetworkSettings& s);

    // True when the two settings differ enough to require an NDI re-init (the
    // SDK reads adapters.allowed / networks only at initialize() time).
    bool needsReinit(const NdiNetworkSettings& oldS, const NdiNetworkSettings& newS);

    // p_extra_ips string for find_create_v2 — currently == s.extraIps, but
    // centralised so the finder and config file stay in sync. Empty -> caller
    // passes nullptr.
    std::string findExtraIps(const NdiNetworkSettings& s);

    // Reachability: TCP connect (timeout ms) to host:port. host may be "ip" or
    // "ip:port" (port arg used only when host has none). Cross-platform.
    bool tcpProbe(const std::string& host, int port, int timeoutMs = 250);

    // Classify a list of discovered NDI sources against the active NIC: parse
    // each url's host:port, compute same-subnet (needs activeIp+activeMask),
    // and tcpProbe host:5960. Cheap-on-demand only (probes block).
    std::vector<NdiPeerStatus> classifyPeers(
        const std::vector<std::pair<std::string, std::string>>& nameUrlPairs,
        const std::string& activeIp, const std::string& activeMask);
}
#endif // HAS_NDI
