# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Allow request Cloudflare `/cdn-cgi/trace` endpoint in network resilience settings ui. [`#21`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/21)
  - You can check your IP address use to send request to VRChat Services.
  - And all debug information inculde in Cloudflare trace endpoint. (e.g User-Agent, TLS version, Edge location)

### Changes

- Use SDK provided 0Harmony.dll instead of HarmonyX. [`#19`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/19)

## [0.2.0-beta.2] - 2025-12-12

### Added

- Allow request Cloudflare `/cdn-cgi/trace` endpoint in network resilience settings ui. [`#21`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/21)
  - You can check your IP address use to send request to VRChat Services.
  - And all debug information inculde in Cloudflare trace endpoint. (e.g User-Agent, TLS version, Edge location)

### Changes

- Use SDK provided 0Harmony.dll instead of HarmonyX. [`#19`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/19)

## [0.2.0-beta.1] - 2025-12-11

### Changes

- Use SDK provided 0Harmony.dll instead of HarmonyX. [`#19`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/19)

## [0.1.0] - 2025-12-09

### Added

- Network Resilience Patch
  - Add Retry Strategy for VRChat SDK.
  - Logging Http Request (for HttpClient used by VRChat SDK).
  - Allow user to custom proxy settings for VRChat SDK or just use System Proxy.
- No Telemetry Patch
  - Disables all telemetry and analytics in the VRChat SDK.
- Randomize Device Id Patch
  - Randomizes the device ID sent with API requests.
- Remote Config Cache Patch
  - Reduce domain reload times by cache VRChat Api Config.
- Always Agree Copyright Agreement Patch
  - Automatically agrees to the copyright agreement when uploading content.

### Changes from [0.1.0-beta.2]

#### Changed

- Migrate to Yes! Patch Framework Logging system.
- Use GUID instead of Path to load UXML asset. [`#12`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/12)
- Rename to `Yet Another Patch for VRChat SDK - Base`. [`#14`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/14)

## [0.1.0-beta.2] - 2025-12-07

### Added

- Network Resilience Patch
  - Add Retry Strategy for VRChat SDK.
  - Logging Http Request (for HttpClient used by VRChat SDK).
  - Allow user to custom proxy settings for VRChat SDK or just use System Proxy.
- No Telemetry Patch
  - Disables all telemetry and analytics in the VRChat SDK.
- Randomize Device Id Patch
  - Randomizes the device ID sent with API requests.
- Remote Config Cache Patch
  - Reduce domain reload times by cache VRChat Api Config.
- Always Agree Copyright Agreement Patch
  - Automatically agrees to the copyright agreement when uploading content.

## [0.1.0-beta.1] - 2025-12-06

### Added

- Network Resilience Patch
  - Add Retry Strategy for VRChat SDK.
  - Logging Http Request (for HttpClient used by VRChat SDK).
  - Allow user to custom proxy settings for VRChat SDK or just use System Proxy.
- No Telemetry Patch
  - Disables all telemetry and analytics in the VRChat SDK.
- Randomize Device Id Patch
  - Randomizes the device ID sent with API requests.
- Remote Config Cache Patch
  - Reduce domain reload times by cache VRChat Api Config.
- Always Agree Copyright Agreement Patch
  - Automatically agrees to the copyright agreement when uploading content.

[unreleased]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/sdk-patch-v0.2.0-beta.2...HEAD
[0.2.0-beta.2]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/sdk-patch-v0.2.0-beta.1...sdk-patch-v0.2.0-beta.2
[0.2.0-beta.1]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/sdk-patch-v0.1.0...sdk-patch-v0.2.0-beta.1
[0.1.0]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/sdk-patch-v0.1.0-beta.2...sdk-patch-v0.1.0
[0.1.0-beta.2]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/sdk-patch-v0.1.0-beta.1...sdk-patch-v0.1.0-beta.2
[0.1.0-beta.1]: https://github.com/project-vrcz/yet-another-sdk-patch/releases/tag/sdk-patch-v0.1.0-beta.1