# Token Implementation Details

## Token Flow

### 1. Login Process (Components/Pages/Login.razor)

When user logs in, the `LoginAsync()` method is called:

```csharp
var response = await Repository.LoginAsync(Username, Password);

if (response.Success)
{
    // Token is automatically saved in MauiRepository
    NavigateToMainPage();
}
```

### 2. Token Storage (Services/MauiRepository.cs)

The `LoginAsync` method in MauiRepository handles token storage:

```csharp
public async Task<Response<UserModel>> LoginAsync(string username, string password)
{
    var response = await _remoteAPI.LoginAsync(username, password, "");
    if (response.Success)
    {
        // 1. Save entire user object (including token) to SecureStorage
        await _storageService.SetAsync(LoggedUserKey, response.Item);
        
        // 2. Update Settings for app-wide access
        DailyPlanner.Settings.Settings.Default.LoggedUserId = response.Item.UserId;
        DailyPlanner.Settings.Settings.Default.LoggedUserFullName = response.Item.Username;
        DailyPlanner.Settings.Settings.Default.LoggedUserRole = response.Item.Role;
        DailyPlanner.Settings.Settings.Default.PlantCode = response.Item.PlantCode;
        DailyPlanner.Settings.Settings.Default.PlantName = response.Item.PlantDescription;
        
        // 3. CRITICAL: Save the token for API authentication
        DailyPlanner.Settings.Settings.Default.AuthorizationToken = response.Item.Token;
        
        _isLogged = true;
    }
    return response;
}
```

### 3. Token Usage in API Calls (Shared/Remote/DailyPlannerAPI.cs)

The token is automatically included in all subsequent API requests:

```csharp
public HttpClient HttpClient
{
    get
    {
        if (_httpClient == null)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.BaseAddress = new Uri(apiBaseURL);
        }
        
        // Token is retrieved from Settings and added to headers
        if (Settings.Settings.Default.AuthorizationToken != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    ExtendedHTTPHeaders.XA_TOKEN_HEADER, 
                    System.Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(
                            Settings.Settings.Default.AuthorizationToken)));
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        
        return _httpClient;
    }
}
```

### 4. Session Persistence

On app restart, the token is automatically loaded:

```csharp
public async Task<bool> IsUserLoggedAsync()
{
    if (_isLogged == null)
    {
        // Load user data (including token) from SecureStorage
        var loggedUser = await _storageService.GetAsync<UserModel>(LoggedUserKey);
        _isLogged = loggedUser != null;
        
        if (_isLogged.Value && loggedUser != null)
        {
            // Restore all user data including token
            DailyPlanner.Settings.Settings.Default.LoggedUserId = loggedUser.UserId;
            DailyPlanner.Settings.Settings.Default.LoggedUserFullName = loggedUser.Username;
            DailyPlanner.Settings.Settings.Default.LoggedUserRole = loggedUser.Role;
            DailyPlanner.Settings.Settings.Default.PlantCode = loggedUser.PlantCode;
            DailyPlanner.Settings.Settings.Default.PlantName = loggedUser.PlantDescription;
            
            // CRITICAL: Restore the token
            DailyPlanner.Settings.Settings.Default.AuthorizationToken = loggedUser.Token;
        }
    }
    return _isLogged.Value;
}
```

## Storage Implementation

### MauiStorageService (Services/MauiStorageService.cs)

Uses MAUI's SecureStorage API:

```csharp
public async Task SetAsync<T>(string key, T value)
{
    var json = JsonSerializer.Serialize(value);
    await SecureStorage.SetAsync(key, json);  // Platform-native secure storage
}

public async Task<T?> GetAsync<T>(string key)
{
    var json = await SecureStorage.GetAsync(key);
    if (string.IsNullOrEmpty(json))
        return default;
    
    return JsonSerializer.Deserialize<T>(json);
}
```

This provides:
- **iOS**: Keychain storage
- **Android**: KeyStore storage
- **Encrypted**: Platform-native encryption
- **Persistent**: Survives app restarts

## Verification

All functionality matches the original Xamarin.Forms implementation:

✅ Token grabbed from API response
✅ Token saved to secure storage
✅ Token saved to Settings for API headers
✅ Token automatically included in all API requests
✅ Token persists across app restarts
✅ Token cleared on logout

## App Hosting

The app is properly hosted in MainPage.xaml:

```xml
<ContentPage>
    <BlazorWebView x:Name="blazorWebView" HostPage="wwwroot/index.html">
        <BlazorWebView.RootComponents>
            <RootComponent Selector="#app" ComponentType="{x:Type components:Routes}" />
        </BlazorWebView.RootComponents>
    </BlazorWebView>
</ContentPage>
```

Routes.razor handles all navigation:

```razor
<Router AppAssembly="typeof(MauiProgram).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

## Pages Structure

```
Components/Pages/
├── Home.razor       - Redirects to /login
├── Login.razor      - Full login implementation
├── Main.razor       - Role-based main page
└── NotFound.razor   - 404 page
```

Template pages (Counter, Weather) have been removed as they're not relevant to the app.
