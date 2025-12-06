# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[unreleased]: https://github.com/olivierlacan/keep-a-changelog/compare/v0.1.0-beta.1...HEAD
[0.1.0-beta.1]: https://github.com/olivierlacan/keep-a-changelog/releases/tag/v0.1.0-beta.1