# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changes

- Use SDK provided 0Harmony.dll instead of HarmonyX. [`#19`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/19)

## [0.1.0] - 2025-12-09

### Added

- Easy way to make user friendly patch by `YesPatchBase` and export it by `ExportYesPatch` assembly attribute.
- Easy to find a location to storage your patch settings by `PatchSettingsHelper`.
- Patch Name, Description and Category.
- Custom Patch Settings UI.
- Locate, Manage and Apply `YesPatchBase` Patch.
- Patch Management Ui
  - Allow user broswer patch by category.
  - Allow user to enable or disable patch.
  - Allow user check why patch or unpatch failed.
  - Allow user modify patch settings (if patch provided).
- Logging system
  - More Log Level: Trace, Debug, Info, Warning, Error, Critical.
  - Logging View Window.
  - Allow control min log level send to Unity Console.

### Changes from the [0.1.0-beta.1]

#### Added

- Allow user disable patch when patch error from gui. [`#7`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/7)
- Show Windows Menu Items in both `Tools/Yes! Patch Frameowrk` and `Window/Yes! Patch Framework` in Editor Toolbar. [`#10`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/10)

#### Changed

- \[BREAKING CHANGE\] All Patches are enabled by default. [`#7`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/7)
- \[BREAKING CHANGE\] Patch failed won't disable patch now. [`#7`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/7)
- Use GUID instead of Path to load UXML asset. [`#12`](https://github.com/project-vrcz/yet-another-sdk-patch/pull/12)

## [0.1.0-beta.1] - 2025-12-06

### Added

- Easy way to make user friendly patch by `YesPatchBase` and export it by `ExportYesPatch` assembly attribute.
- Easy to find a location to storage your patch settings by `PatchSettingsHelper`.
- Patch Name, Description and Category.
- Custom Patch Settings UI.
- Locate, Manage and Apply `YesPatchBase` Patch.
- Patch Management Ui
  - Allow user broswer patch by category.
  - Allow user to enable or disable patch.
  - Allow user check why patch or unpatch failed.
  - Allow user modify patch settings (if patch provided).

[unreleased]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/framework-v0.1.0...HEAD
[0.1.0]: https://github.com/project-vrcz/yet-another-sdk-patch/compare/framework-v0.1.0-beta.1...framework-v0.1.0
[0.1.0-beta.1]: https://github.com/project-vrcz/yet-another-sdk-patch/releases/tag/framework-v0.1.0-beta.1