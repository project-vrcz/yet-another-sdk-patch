# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Fix parameter driver editor always spawn animator window patch. [`#40`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/40)
  - If you exit play mode when avatar parameter driver editor visible, it will keep spawn animator window when domain reload in some case.
  - This patch will do nothing if VRCFury installed.

## [0.3.1-beta.2] - 2026-04-15

### Added

- Fix parameter driver editor always spawn animator window patch. [`#40`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/40)
  - If you exit play mode when avatar parameter driver editor visible, it will keep spawn animator window when domain reload in some case.
  - This patch will do nothing if VRCFury installed.

## [0.3.1-beta.1] - 2026-04-13

### Added

- Fix parameter driver editor always spawn animator window patch. [`#40`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/40)
  - If you exit play mode when avatar parameter driver editor visible, it will keep spawn animator window when domain reload in some case.

## [0.3.0] - 2026-03-12

### Changed

- **BREAKING CHANGE** Bump to Yes! Patch Framework v0.3.0

## [0.2.1] - 2025-12-25

### Changed

- Rename to `Yet Another SDK Patch - Avatars Pack`. [`#28`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/29)

## [0.2.0] - 2025-12-18

### Changed

- Use Framework provided 0Harmony.dll instead of HarmonyX. [`#22`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/22) [`#19`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/19)

## [0.2.0-beta.1] - 2025-12-11

### Changed

- Use SDK provided 0Harmony.dll instead of HarmonyX. [`#19`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/19)

## [0.1.0] - 2025-12-09

### Changed

- Fix .sb Forget to Crop Thumbnail Patch
  - Updated to only run on SDK versions 3.9.0 to 3.10.0, as VRChat fixed the issue in SDK 3.10.1.

### Added

- Fix .sb Forget to Crop Thumbnail Patch
  - Fix the issue that VRChat SDK forgets to crop the avatar thumbnail when creating new avatar after 3.9.0.

## [0.1.0-beta.1] - 2025-12-06

### Added

- Fix .sb Forget to Crop Thumbnail Patch
  - Fix the issue that VRChat SDK forgets to crop the avatar thumbnail when creating new avatar after 3.9.0.

[unreleased]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.3.1-beta.2...HEAD
[0.3.1-beta.2]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.3.1-beta.1...avatars-sdk-patch-v0.3.1-beta.2
[0.3.1-beta.1]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.3.0...avatars-sdk-patch-v0.3.1-beta.1
[0.3.0]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.2.1...avatars-sdk-patch-v0.3.0
[0.2.1]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.2.0...avatars-sdk-patch-v0.2.1
[0.2.0]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.2.0-beta.1...avatars-sdk-patch-v0.2.0
[0.2.0-beta.1]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.1.0...avatars-sdk-patch-v0.2.0-beta.1
[0.1.0]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/avatars-sdk-patch-v0.1.0-beta.1...avatars-sdk-patch-v0.1.0
[0.1.0-beta.1]: https://github.com/project-vrcz/yet-another-sdk-patch/releases/tag/avatars-sdk-patch-v0.1.0-beta.1