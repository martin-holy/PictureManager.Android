# Picture Manager – Android

Native Android frontend for [Picture Manager](https://github.com/martin-holy/PictureManager), sharing its application logic and data model with the WPF version through `PictureManager.Common`.

The Android application is written in C# on .NET for Android. Its interface is built directly with native Android Views and the custom `MH.UI.Android` framework. It does not use MAUI, Xamarin.Forms or XML layout files.

## Features

Picture Manager organizes and browses photo and video collections using a shared application model.

- **Media library** – browse folders and media items, with thumbnail and detailed views.
- **Viewers** – organize media into configurable collections using included and excluded folders, category groups and excluded keywords.
- **People** – browse people, view their associated media segments and manage person-related information.
- **Segments** – display and edit square image regions associated with people and other application data.
- **Image viewing** – view full-resolution images with zooming and panning.
- **Video playback** – preview and play videos using Android media playback.
- **Import and file operations** – import media, monitor progress, cancel imports and handle file-operation collisions.
- **Metadata and organization** – use the application functionality provided by `PictureManager.Common`, including media metadata, keywords and location information.
- **Sharing** – share selected media through Android's system sharing mechanism.
- **Configurable behavior** – adjust thumbnail scale, sorting, JPEG quality, GeoNames access, segment grouping and viewer behavior.

The Android frontend reuses the application layer, but its available commands are not identical to the WPF version. Some operations are intentionally not exposed on Android, including batch image compression, image-to-video creation, batch image resizing, thumbnail rebuilding, viewing modified items and exporting segments.

## Android implementation

The application targets `net9.0-android` and supports Android 5.0 (API 21) and later.

The UI is implemented in C# using native Android Views. It does not use MAUI, Xamarin.Forms or XML layout files. XML is used only for Android platform resources and manifests.

Android-specific integration includes:

- **Media access** – requests image and video access permissions and integrates with Android's media storage.
- **Thumbnails** – uses Android MediaStore thumbnails where suitable, with custom thumbnail generation and an application-managed cache as a fallback.
- **Image orientation** – applies media orientation when loading full images and generating thumbnails.
- **Video playback** – provides an Android media-player implementation for the shared application interfaces.
- **File operations** – integrates deletion, file sharing and media rescanning with Android.
- **Storage** – keeps application data and cached files in the app-specific external storage directory, falling back to internal app storage when necessary.
- **Lifecycle** – initializes the application asynchronously, displays initialization progress and backs up the database when the activity stops.

## Architecture

The Android project is a platform-specific frontend. Application behavior and domain logic are shared with the WPF application through `PictureManager.Common`.

```text
PictureManager.Android
  Native Android activity, views, dialogs and platform integration
            |
            v
PictureManager.Common
  Shared application logic, models, services and view models
            |
            +--------------------+
            |                    |
            v                    v
        MH.UI                 MH.Utils
  Shared UI abstractions,   Bindings, utilities,
  controls and view models  imaging and infrastructure
            ^
            |
      MH.UI.Android
  Native Android controls, hosts,
  bindings and platform utilities
```

The Android frontend adapts the shared view models to native Android controls. For example:

- `MainActivity` initializes the application, handles Android permissions and manages the activity lifecycle.
- `CoreUI` implements the platform integration contract (`ICoreP`), configures Android-specific services and connects shared application commands to Android functionality.
- `MainWindowV` composes the application interface using the slide-panel layout.
- Views in `Views/` implement media browsing, viewing, people, segments, settings and dialogs.
- `MediaItemVM` loads full images and thumbnails, combining MediaStore, custom generation and a local cache.

The UI uses `MH.UI.Android` controls and hosts, while shared application state and behavior remain in `PictureManager.Common`.

## Project structure

```text
src/
  PictureManager.Android/
    CoreUI.cs
    MainActivity.cs
    ViewModels/
    Views/
      Dialogs/
      Entities/
      Layout/
      Sections/
    Utils/
    Resources/
```

- **CoreUI.cs** – Android implementation of the shared platform integration interface.
- **MainActivity.cs** – application entry point, permission handling and lifecycle integration.
- **ViewModels/** – Android-specific media loading helpers.
- **Views/Dialogs/** – native Android dialogs for application operations.
- **Views/Entities/** – views for media items, people, segments, ratings and viewers.
- **Views/Layout/** – main window, tabs, toolbar, status bar and category navigation.
- **Views/Sections/** – media browsing, media viewer, people, segments, video detail and settings.
- **Utils/** – Android-specific dialog and imaging helpers.
- **Resources/** – Android drawable, string, color and XML resources.

## Build

### Requirements

- .NET 9 SDK with the .NET for Android workload.
- Android SDK and the required Android build tools.
- The `PictureManager.Common` repository.
- The MH framework repositories when building in Debug configuration.

### Debug

The Debug configuration uses project references to the local source repositories for:

- `PictureManager.Common`
- `MH.UI`
- `MH.UI.Android`
- `MH.Utils`

The repository layout is expected to place these repositories alongside `PictureManager.Android`, matching the relative project-reference paths in the project file.

Build the project with:

```bash
dotnet build src/PictureManager.Android/PictureManager.Android.csproj -c Debug
```

### Release

The Release configuration references the published NuGet packages:

- `MH.UI`
- `MH.UI.Android`
- `MH.Utils`

`PictureManager.Common` remains a project reference.

The Release build produces a signed APK. The signing keystore and passwords must be supplied through the MSBuild signing properties; the keystore itself is not included in the repository.

```bash
dotnet build src/PictureManager.Android/PictureManager.Android.csproj -c Release
```

## Related repositories

- [PictureManager (WPF)](https://github.com/martin-holy/PictureManager) – Windows desktop frontend.
- [PictureManager.Common](https://github.com/martin-holy/PictureManager.Common) – shared application logic and data model.
- [MH.UI](https://github.com/martin-holy/MH.UI) – shared UI framework.
- [MH.UI.Android](https://github.com/martin-holy/MH.UI.Android) – native Android UI implementation.
- [MH.Utils](https://github.com/martin-holy/MH.Utils) – utilities, bindings and infrastructure.

## License

See [LICENSE](LICENSE).
