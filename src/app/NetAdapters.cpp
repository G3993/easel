#include "app/NetAdapters.h"

#include <cstring>

#ifdef _WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <ws2tcpip.h>   // InetNtopA
#include <iphlpapi.h>   // GetAdaptersAddresses, ConvertLengthToIpv4Mask
#include <ipifcons.h>   // IF_TYPE_IEEE80211 (71), IF_TYPE_ETHERNET_CSMACD (6)
#pragma comment(lib, "iphlpapi.lib")  // auto-link for all targets (app + NDI tests)
#pragma comment(lib, "ws2_32.lib")
#elif defined(__APPLE__)
#include <ifaddrs.h>
#include <net/if.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <map>
#include <SystemConfiguration/SystemConfiguration.h>
#else
#include <ifaddrs.h>
#include <net/if.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#endif

const char* NetAdapterKindTag(NetAdapterInfo::Kind k) {
    switch (k) {
        case NetAdapterInfo::Kind::WiFi:     return "Wi-Fi";
        case NetAdapterInfo::Kind::Ethernet: return "Ethernet";
        default:                             return "Net";
    }
}

bool SameSubnet(const std::string& ipA, const std::string& ipB, const std::string& maskDotted) {
    if (ipA.empty() || ipB.empty() || maskDotted.empty()) return false;
#ifdef _WIN32
    IN_ADDR a{}, b{}, m{};
    if (InetPtonA(AF_INET, ipA.c_str(), &a) != 1) return false;
    if (InetPtonA(AF_INET, ipB.c_str(), &b) != 1) return false;
    if (InetPtonA(AF_INET, maskDotted.c_str(), &m) != 1) return false;
    return (a.S_un.S_addr & m.S_un.S_addr) == (b.S_un.S_addr & m.S_un.S_addr);
#else
    struct in_addr a{}, b{}, m{};
    if (inet_pton(AF_INET, ipA.c_str(), &a) != 1) return false;
    if (inet_pton(AF_INET, ipB.c_str(), &b) != 1) return false;
    if (inet_pton(AF_INET, maskDotted.c_str(), &m) != 1) return false;
    return (a.s_addr & m.s_addr) == (b.s_addr & m.s_addr);
#endif
}

#ifdef _WIN32

std::vector<NetAdapterInfo> EnumerateNetworkAdapters() {
    std::vector<NetAdapterInfo> out;
    ULONG flags = GAA_FLAG_SKIP_ANYCAST | GAA_FLAG_SKIP_MULTICAST | GAA_FLAG_SKIP_DNS_SERVER;
    ULONG size = 15000;
    std::vector<char> buf;
    PIP_ADAPTER_ADDRESSES aa = nullptr;
    ULONG ret = 0;
    for (int i = 0; i < 3; ++i) {
        buf.resize(size);
        aa = reinterpret_cast<PIP_ADAPTER_ADDRESSES>(buf.data());
        ret = GetAdaptersAddresses(AF_INET, flags, nullptr, aa, &size);
        if (ret != ERROR_BUFFER_OVERFLOW) break;
    }
    if (ret != NO_ERROR) return out;

    // Belt-and-suspenders: drop obvious virtual adapters that still report a
    // physical IfType (Hyper-V / VM / Docker / WSL bridges).
    static const char* nameBlock[] = {
        "Virtual", "VMware", "VirtualBox", "Hyper-V", "vEthernet", "Loopback", "Pseudo"
    };

    for (auto* p = aa; p; p = p->Next) {
        if (p->OperStatus != IfOperStatusUp) continue;
        if (p->IfType == IF_TYPE_SOFTWARE_LOOPBACK ||
            p->IfType == IF_TYPE_TUNNEL ||
            p->IfType == IF_TYPE_PPP) continue;

        NetAdapterInfo::Kind kind = NetAdapterInfo::Kind::Other;
        if (p->IfType == IF_TYPE_IEEE80211) {
            kind = NetAdapterInfo::Kind::WiFi;
        } else if (p->IfType == IF_TYPE_ETHERNET_CSMACD ||
                   p->IfType == IF_TYPE_GIGABITETHERNET ||
                   p->IfType == IF_TYPE_FASTETHER) {
            kind = NetAdapterInfo::Kind::Ethernet;
        }

        // FriendlyName (UTF-16 -> UTF-8) for blocklist + UI label.
        std::string fname;
        if (p->FriendlyName) {
            int n = WideCharToMultiByte(CP_UTF8, 0, p->FriendlyName, -1, nullptr, 0, nullptr, nullptr);
            if (n > 1) {
                fname.resize(n - 1);
                WideCharToMultiByte(CP_UTF8, 0, p->FriendlyName, -1, fname.data(), n, nullptr, nullptr);
            }
        }
        bool blocked = false;
        for (auto* tok : nameBlock) {
            if (fname.find(tok) != std::string::npos) { blocked = true; break; }
        }
        if (blocked) continue;

        // First preferred, non-APIPA IPv4 unicast address + its mask.
        std::string ip, mask;
        for (auto* u = p->FirstUnicastAddress; u; u = u->Next) {
            auto* sa = u->Address.lpSockaddr;
            if (!sa || sa->sa_family != AF_INET) continue;
            if (u->DadState != IpDadStatePreferred) continue;
            char ipbuf[INET_ADDRSTRLEN] = {};
            InetNtopA(AF_INET, &reinterpret_cast<sockaddr_in*>(sa)->sin_addr, ipbuf, sizeof(ipbuf));
            // Skip link-local APIPA (169.254.x.x).
            if (std::strncmp(ipbuf, "169.254.", 8) == 0) continue;
            ip = ipbuf;
            ULONG mnet = 0;
            ConvertLengthToIpv4Mask(u->OnLinkPrefixLength, &mnet);
            char mbuf[INET_ADDRSTRLEN] = {};
            InetNtopA(AF_INET, &mnet, mbuf, sizeof(mbuf));
            mask = mbuf;
            break;
        }
        if (ip.empty()) continue;

        NetAdapterInfo info;
        info.name = p->AdapterName ? p->AdapterName : "";  // GUID device name
        info.friendlyLabel = fname.empty() ? info.name : fname;
        info.kind = kind;
        info.ipv4 = ip;
        info.subnetMask = mask;
        info.isUp = true;
        out.push_back(std::move(info));
    }
    return out;
}

#elif defined(__APPLE__)

// Build BSD-name -> Kind map from SystemConfiguration. getifaddrs alone cannot
// reliably distinguish Wi-Fi from Ethernet; SCNetworkInterface can.
static std::map<std::string, NetAdapterInfo::Kind> BuildTypeMap() {
    std::map<std::string, NetAdapterInfo::Kind> m;
    CFArrayRef ifs = SCNetworkInterfaceCopyAll();
    if (!ifs) return m;
    for (CFIndex i = 0, n = CFArrayGetCount(ifs); i < n; ++i) {
        auto si = (SCNetworkInterfaceRef)CFArrayGetValueAtIndex(ifs, i);
        CFStringRef bsd = SCNetworkInterfaceGetBSDName(si);
        CFStringRef ty  = SCNetworkInterfaceGetInterfaceType(si);
        if (!bsd || !ty) continue;
        char nm[64] = {};
        if (!CFStringGetCString(bsd, nm, sizeof(nm), kCFStringEncodingUTF8)) continue;
        NetAdapterInfo::Kind k = NetAdapterInfo::Kind::Other;
        if (CFEqual(ty, kSCNetworkInterfaceTypeIEEE80211)) k = NetAdapterInfo::Kind::WiFi;
        else if (CFEqual(ty, kSCNetworkInterfaceTypeEthernet)) k = NetAdapterInfo::Kind::Ethernet;
        m[nm] = k;
    }
    CFRelease(ifs);
    return m;
}

std::vector<NetAdapterInfo> EnumerateNetworkAdapters() {
    std::vector<NetAdapterInfo> out;
    auto typeMap = BuildTypeMap();
    struct ifaddrs* head = nullptr;
    if (getifaddrs(&head) != 0) return out;
    static const char* skip[] = {
        "lo", "gif", "stf", "awdl", "llw", "utun", "vmnet", "bridge", "ap", "p2p"
    };
    for (auto* ifa = head; ifa; ifa = ifa->ifa_next) {
        if (!ifa->ifa_addr || ifa->ifa_addr->sa_family != AF_INET) continue;
        if (!(ifa->ifa_flags & IFF_UP) || !(ifa->ifa_flags & IFF_RUNNING)) continue;
        if (ifa->ifa_flags & (IFF_LOOPBACK | IFF_POINTOPOINT)) continue;
        std::string nm = ifa->ifa_name ? ifa->ifa_name : "";
        bool drop = false;
        for (auto* s : skip) {
            if (nm.rfind(s, 0) == 0) { drop = true; break; }
        }
        if (drop) continue;

        char ip[INET_ADDRSTRLEN] = {}, mask[INET_ADDRSTRLEN] = {};
        auto* sin = (struct sockaddr_in*)ifa->ifa_addr;
        inet_ntop(AF_INET, &sin->sin_addr, ip, sizeof(ip));
        if (std::strncmp(ip, "169.254.", 8) == 0) continue;  // skip APIPA
        if (ifa->ifa_netmask) {
            auto* mn = (struct sockaddr_in*)ifa->ifa_netmask;
            inet_ntop(AF_INET, &mn->sin_addr, mask, sizeof(mask));
        }

        NetAdapterInfo::Kind kind = NetAdapterInfo::Kind::Other;
        auto it = typeMap.find(nm);
        if (it != typeMap.end()) kind = it->second;

        std::string label = std::string(NetAdapterKindTag(kind)) + " (" + nm + ")";
        NetAdapterInfo info;
        info.name = nm;
        info.friendlyLabel = label;
        info.kind = kind;
        info.ipv4 = ip;
        info.subnetMask = mask;
        info.isUp = true;
        out.push_back(std::move(info));
    }
    freeifaddrs(head);
    return out;
}

#else  // Linux / other POSIX: no SystemConfiguration typing (kind=Other).

std::vector<NetAdapterInfo> EnumerateNetworkAdapters() {
    std::vector<NetAdapterInfo> out;
    struct ifaddrs* head = nullptr;
    if (getifaddrs(&head) != 0) return out;
    static const char* skip[] = {
        "lo", "gif", "stf", "awdl", "llw", "utun", "vmnet", "bridge", "ap", "p2p", "docker", "veth"
    };
    for (auto* ifa = head; ifa; ifa = ifa->ifa_next) {
        if (!ifa->ifa_addr || ifa->ifa_addr->sa_family != AF_INET) continue;
        if (!(ifa->ifa_flags & IFF_UP) || !(ifa->ifa_flags & IFF_RUNNING)) continue;
        if (ifa->ifa_flags & (IFF_LOOPBACK | IFF_POINTOPOINT)) continue;
        std::string nm = ifa->ifa_name ? ifa->ifa_name : "";
        bool drop = false;
        for (auto* s : skip) {
            if (nm.rfind(s, 0) == 0) { drop = true; break; }
        }
        if (drop) continue;

        char ip[INET_ADDRSTRLEN] = {}, mask[INET_ADDRSTRLEN] = {};
        auto* sin = (struct sockaddr_in*)ifa->ifa_addr;
        inet_ntop(AF_INET, &sin->sin_addr, ip, sizeof(ip));
        if (std::strncmp(ip, "169.254.", 8) == 0) continue;
        if (ifa->ifa_netmask) {
            auto* mn = (struct sockaddr_in*)ifa->ifa_netmask;
            inet_ntop(AF_INET, &mn->sin_addr, mask, sizeof(mask));
        }

        NetAdapterInfo info;
        info.name = nm;
        info.friendlyLabel = nm;
        info.kind = NetAdapterInfo::Kind::Other;
        info.ipv4 = ip;
        info.subnetMask = mask;
        info.isUp = true;
        out.push_back(std::move(info));
    }
    freeifaddrs(head);
    return out;
}

#endif
