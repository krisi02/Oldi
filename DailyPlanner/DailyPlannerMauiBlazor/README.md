# DailyPlannerMauiBlazor

This is a .NET MAUI Blazor Hybrid application that provides a modern cross-platform UI for the DailyPlanner application. It is intended to replace the existing Xamarin.Forms XAML UI with MAUI Blazor Razor components.

## Project Structure

- **Components/Pages/**: Blazor pages
  - `Login.razor`: Login page with username/password authentication
  - `Main.razor`: Main page with role-based content (placeholder for future features)
  - `Home.razor`: Root redirect to login
- **Services/**: Custom services
  - `IStorageService`: Storage abstraction interface
  - `MauiStorageService`: MAUI SecureStorage implementation (replaces Akavache)
  - `MauiRepository`: Repository adapter that uses MAUI storage instead of Akavache
- **Shared/**: Linked files from the existing DailyPlanner project
  - Models, Remote API, Repository interfaces, Settings, and Utilities

## Features

### Milestone 1: Login → Main Flow

✅ Implemented:
- Login page with username and password inputs
- Error handling and busy overlay
- Integration with existing Remote API and Repository
- MAUI SecureStorage for token/session persistence
- Role-based navigation (AUTISTA, NEL, IMPIANTISTA)
- Italian culture (it-IT) support
- Main page placeholder with logout functionality

⏳ Future Milestones:
- Driver notifications view
- NEL planning management
- Complete feature parity with Xamarin.Forms app

## Technical Details

### Dependencies

- **.NET 9** (targeting net9.0-android, with iOS support when building on macOS)
- **Microsoft.Maui.Controls** 9.0.120
- **Microsoft.AspNetCore.Components.WebView.Maui** 9.0.120
- **Newtonsoft.Json** 13.0.3 (for API serialization)

### Dependency Injection

The app is configured in `MauiProgram.cs` with the following services:
- `IStorageService` → `MauiStorageService` (Singleton)
- `IRemoteAPI` → `DailyPlannerAPI` (Singleton)
- `IRepository` → `MauiRepository` (Singleton)

### Storage

The app uses MAUI's `SecureStorage` API instead of Akavache for storing user authentication tokens and session data. This provides:
- Platform-native secure storage (Keychain on iOS, KeyStore on Android)
- Async/await support
- JSON serialization via System.Text.Json

### Authentication Flow

1. User enters credentials on `/login` page
2. `MauiRepository.LoginAsync()` calls `DailyPlannerAPI.LoginAsync()`
3. On success, user data is stored in SecureStorage
4. User is redirected to `/main` with role-specific content
5. Authentication token is automatically included in subsequent API requests

## Building

### Prerequisites

- .NET 9 SDK or later
- MAUI workloads installed:
  ```bash
  dotnet workload install maui-android
  ```
- For iOS: macOS with Xcode

### Build Commands

```bash
# Build for Android
dotnet build -f net9.0-android

# Build for iOS (macOS only)
dotnet build -f net9.0-ios

# Build for Windows (Windows only)
dotnet build -f net9.0-windows10.0.19041.0
```

## Notes

### iOS Support
iOS target (`net9.0-ios`) can be added to the project when building on macOS. The current repository builds on Linux, so only Android target is included by default.

### Prism Dependencies
The new MAUI Blazor app does not use Prism for navigation or dependency injection. Navigation is handled by Blazor's `NavigationManager`, and DI is handled by the built-in .NET DI container.

### Shiny Push Notifications
Push notification support (Shiny.Push.FirebaseMessaging) has been stubbed for Milestone 1. Integration will be completed in future milestones as needed.

### File Linking
The project uses file linking (via `<Compile Include="...">` in the .csproj) to share Models, Remote API code, and Repository interfaces with the existing Xamarin.Forms project. This avoids code duplication while the migration is in progress.

## Migration Strategy

This is part of a phased migration from Xamarin.Forms to .NET MAUI:

1. **Milestone 1** (Current): Login → Main flow
2. **Milestone 2** (Future): Driver notifications and planning views
3. **Milestone 3** (Future): NEL admin features
4. **Milestone 4** (Future): Complete feature parity and Xamarin.Forms deprecation

## Related Projects

- **DailyPlanner**: Xamarin.Forms netstandard2.0 shared library
- **DailyPlanner.Android**: Xamarin.Forms Android app
- **DailyPlanner.iOS**: Xamarin.Forms iOS app
