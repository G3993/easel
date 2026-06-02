# NDI Wi-Fi / Ethernet selector + device reachability

Design + implementation plan produced by the `easel-ndi-network-selector` workflow
(run `wf_05fe256d-dc7`, 2026-06-01). The 4 new source modules already exist on disk
(untracked); the *integration edits* below were reverted by a concurrent `git merge`
and must be re-applied onto the current post-merge `Application.cpp`.

## The core finding (why it's built this way)

The NDI SDK structs have **no per-stream "bind to NIC" field** — `send_create_t` /
`recv_create_v3_t` only expose `p_groups`; `find_create_t` also exposes `p_extra_ips`.
The only way to pin NDI to one adapter (Wi-Fi vs Ethernet) is the machine config file
**`ndi-config.v1.json`**, key `ndi.adapters.allowed = ["<NIC IPv4>"]`, which NDI reads
**only at `NDIlib_initialize()` time**. Easel calls `NDIRuntime::init()` early in
startup (before any project loads), so:
- First init always uses defaults (Auto = all NICs).
- A loaded project or a UI change rewrites the JSON and **re-inits** the NDI subsystem
  (`shutdown()` + recreate finder/output) so the new binding takes effect.

Cross-device reachability is layered, strongest last:
1. **mDNS** (UDP 5353) — link-local only; fails across subnets/VLANs.
2. **adapters.allowed** — pins announce/send/recv to one routable NIC.
3. **networks.ips / `find_create` `p_extra_ips`** — query specific remote IPs over TCP 5960 (fixes cross-subnet discovery for *known* peers).
4. **Discovery Server** (`networks.discovery`, TCP 5959) — central registration that replaces mDNS and works across all subnets. The only option that *guarantees* mutual reach.

## New modules (already on disk, untracked)

- `src/app/NetAdapters.{h,cpp}` — NDI-agnostic, always compiled. `EnumerateNetworkAdapters()`
  (Win `GetAdaptersAddresses`+IfType; mac `getifaddrs`+`SCNetworkInterfaceCopyAll`; Linux
  fallback), `NetAdapterKindTag()`, `SameSubnet()`. Returns `{name, friendlyLabel,
  kind(WiFi/Ethernet/Other), ipv4, subnetMask, isUp}`.
- `src/net/NdiNetworkConfig.{h,cpp}` — `#ifdef HAS_NDI`. `NdiNetworkSettings` +
  `NdiPeerStatus`; `configDir()`, `applyToEnv()` (writes JSON + sets `NDI_CONFIG_DIR`
  before init), `needsReinit()`, `findExtraIps()`, `tcpProbe()` (non-blocking
  connect+select, 250ms), `classifyPeers()` (SameSubnet + probe :5960).

Config folder (app-owned, avoids admin/Access-Manager contention):
Win `%LOCALAPPDATA%\Easel\ndi`, mac `~/Library/Application Support/Easel/ndi`,
Linux `$XDG_CONFIG_HOME|~/.config/easel/ndi`.

## Integration edits to RE-APPLY (re-anchor to current line numbers)

1. **`src/sources/NDIRuntime.h`** — add static stash:
   `static void setPendingNetworkSettings(const NdiNetworkSettings&);`
   `static const NdiNetworkSettings& pendingNetworkSettings();`
   `bool reinitWithSettings(const NdiNetworkSettings&);` (shutdown → m_initialized=false → stash → init).
   `#include "net/NdiNetworkConfig.h"`. Back with a static member in .cpp.
2. **`src/sources/NDIRuntime.cpp`** — at top of `init()` (after the early-return guard,
   BEFORE `m_api->initialize()`): `NdiNetworkConfig::applyToEnv(pendingNetworkSettings());`
3. **`src/sources/NDISource.cpp`** — in `NDIFinder::create()`, before `find_create_v2`:
   `std::string extra = NdiNetworkConfig::findExtraIps(NDIRuntime::pendingNetworkSettings());`
   `findCreate.p_extra_ips = extra.empty() ? nullptr : extra.c_str();` (keep `extra` alive across the call).
4. **`src/app/Application.h`** — `#include "app/NetAdapters.h"` (unconditional) +
   `#include "net/NdiNetworkConfig.h"` (HAS_NDI); members `NdiNetworkSettings m_ndiNetwork;`
   `std::vector<NetAdapterInfo> m_netAdapters;` `std::vector<NdiPeerStatus> m_ndiPeerStatus;`
   `double m_ndiPeerStatusLastRefresh = 0.0;`; decls `void applyNdiNetworkSettings(bool reinit);`
   `void refreshNdiPeerStatus();`. (Add the throttle members from verifier fix #2 too:
   `double m_ndiServerUpLastRefresh = 0.0; bool m_ndiServerUp = false;`)
5. **`src/app/Application.cpp`**
   - includes: `#include "app/NetAdapters.h"` + `#include "net/NdiNetworkConfig.h"`.
   - startup: BEFORE the `NDIRuntime::instance().init()` call, add
     `NDIRuntime::setPendingNetworkSettings(m_ndiNetwork);` (defaults → Auto).
   - implement `applyNdiNetworkSettings(bool reinit)`: set pending; if reinit && available:
     destroy `m_ndiOutput` + `m_ndiFinder` BEFORE `reinitWithSettings`, then recreate
     finder/sources/output. Else just `applyToEnv` (next-launch). Wrap in `#ifdef HAS_NDI`.
   - implement `refreshNdiPeerStatus()`: re-enumerate adapters, pick active NIC ip/mask,
     `classifyPeers(pairs, activeIp, activeMask)`. `#ifdef HAS_NDI`.
   - UI: new `if (flatSection("NDI Network")) {...}` between "NDI Sources" and "NDI Broadcast",
     inside the `isAvailable()` gate. Controls: adapter dropdown (Auto + "Wi-Fi - <ip>" /
     "Ethernet - <ip>"), Refresh, status line + amber Wi-Fi warning; Device Reachability
     list (green reachable / red unreachable / "reachable (cross-subnet)" / "no NDI port
     (firewall?)") with one-click **Add IP** remediation; manual PEER IPS InputText
     (commit on `IsItemDeactivatedAfterEdit`); **Use NDI Discovery Server** checkbox + SERVER
     input with throttled :5959 status.
   - saveProject: additive `j["ndiNetwork"] = {...}` (`#ifdef HAS_NDI`, no version bump).
   - loadProject: guarded `if (j.contains("ndiNetwork")) {...; applyNdiNetworkSettings(true);}`.
6. **`CMakeLists.txt`** — Win main-target libs: add `iphlpapi` (alongside `ws2_32`).
   mac frameworks: add `-framework SystemConfiguration`. New sources picked up by
   `GLOB_RECURSE` → **a cmake re-configure is REQUIRED**. NDI test exes that include
   `NDIRuntime.cpp`/`NDISource.cpp` must also list the two new .cpp + link the libs.

## default.easel additive key

```json
"ndiNetwork": {
  "enabled": false, "interface": "", "interfaceIp": "",
  "extraIps": "", "useDiscoveryServer": false, "discoveryServer": ""
}
```
Persist the stable IPv4 (+name fallback), never a list index.

## Verifier fixes to fold in while re-applying

- **major:** Discovery-server `tcpProbe(:5959)` was called *every frame* — throttle it
  (≥5s cache: `m_ndiServerUp` / `m_ndiServerUpLastRefresh`), like peer status.
- **minor:** trim whitespace around comma tokens in `extraIps` (Add-IP dedup + before
  handing to `find_create` / JSON).
- **nit:** guard `#define WIN32_LEAN_AND_MEAN` with `#ifndef` in both new .cpp (CMake also
  defines it → C4005).

## Known limitations (surface in UI)

- Machine/process-global: cannot bind one sender to Wi-Fi and another to Ethernet in one
  Easel process — all senders share `adapters.allowed`.
- Live NIC change re-inits NDI (brief glitch; connected receivers momentarily drop).
- `adapters.allowed` is lightly documented for NDI v6 — validate on the shipped runtime
  (pin NIC, pull cable, confirm source disappears). `p_extra_ips` + Discovery Server are
  first-class fallbacks if the key is ignored.
- DHCP can reassign the pinned IP → recommend a static/reserved IP in the UI hint.

## Multi-device test plan (abridged)

Re-configure cmake → build (needs OpenSSL or WHEP disabled — pre-existing). Verify:
enumeration (unplug Ethernet → disappears on Refresh); Auto default writes no
`adapters.allowed`; pin Ethernet on machine A, pull cable → A's source vanishes on B
(proves the pin); cross-subnet peer added via PEER IPS appears in the list; Discovery
Server makes A/B/C mutually discoverable regardless of subnet; firewall-block TCP 5960 →
"no NDI port" tag while ICMP still pings (proves we probe the right port).
