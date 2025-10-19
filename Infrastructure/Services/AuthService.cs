using Microsoft.JSInterop;
using System.Text.Json;

namespace MisFinanzas.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isAuthenticated = false;
        private bool _isAdmin = false;
        private string _userName = "Usuario";
        private string? _userId = null;
        private bool _isInitialized = false;

        public event Action? OnAuthStateChanged;

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public bool IsAuthenticated => _isAuthenticated;
        public bool IsAdmin => _isAdmin;
        public string UserName => _userName;
        public string? UserId => _userId;
        public bool IsInitialized => _isInitialized;

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized)
            {
                Console.WriteLine("⚠️ AuthService: Already initialized");
                return _isAuthenticated;
            }

            Console.WriteLine("🚀 AuthService: Initializing...");

            // Wait for JS to be ready
            await Task.Delay(100);

            try
            {
                // Use the JavaScript helper to get auth data
                var authDataJson = await _jsRuntime.InvokeAsync<string>("eval",
                    "JSON.stringify(window.authHelper ? window.authHelper.getAuthData() : null)");

                if (!string.IsNullOrEmpty(authDataJson) && authDataJson != "null")
                {
                    var authData = JsonSerializer.Deserialize<AuthData>(authDataJson);

                    if (authData != null && !string.IsNullOrEmpty(authData.userId))
                    {
                        _isAuthenticated = true;
                        _userId = authData.userId;
                        _userName = authData.userName ?? "Usuario";
                        _isAdmin = authData.userRole == "Admin";

                        Console.WriteLine($"✅ AuthService: User authenticated - {_userName} (Admin: {_isAdmin})");
                    }
                    else
                    {
                        ResetAuthState();
                    }
                }
                else
                {
                    ResetAuthState();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AuthService initialization error: {ex.Message}");
                ResetAuthState();
            }

            _isInitialized = true;
            NotifyStateChanged();

            return _isAuthenticated;
        }

        public async Task<bool> CheckAuthenticationAsync()
        {
            try
            {
                Console.WriteLine("🔍 AuthService: Checking authentication...");

                // Use JavaScript helper
                var authDataJson = await _jsRuntime.InvokeAsync<string>("eval",
                    "JSON.stringify(window.authHelper ? window.authHelper.getAuthData() : null)");

                if (!string.IsNullOrEmpty(authDataJson) && authDataJson != "null")
                {
                    var authData = JsonSerializer.Deserialize<AuthData>(authDataJson);

                    if (authData != null && !string.IsNullOrEmpty(authData.userId))
                    {
                        _isAuthenticated = true;
                        _userId = authData.userId;
                        _userName = authData.userName ?? "Usuario";
                        _isAdmin = authData.userRole == "Admin";

                        Console.WriteLine($"✅ Found authenticated user: {_userName}");
                        NotifyStateChanged();
                        return true;
                    }
                }

                ResetAuthState();
                NotifyStateChanged();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CheckAuthentication error: {ex.Message}");
                ResetAuthState();
                NotifyStateChanged();
                return false;
            }
        }

        public async Task<bool> LoginAsync(string userId, string userName, string userRole)
        {
            try
            {
                Console.WriteLine($"🔐 AuthService: Logging in {userName}...");

                // Use JavaScript helper to save
                var success = await _jsRuntime.InvokeAsync<bool>("eval",
                    $"window.authHelper ? window.authHelper.saveAuthData('{userId}', '{userName}', '{userRole}') : false");

                if (success)
                {
                    _isAuthenticated = true;
                    _userId = userId;
                    _userName = userName;
                    _isAdmin = userRole == "Admin";

                    Console.WriteLine($"✅ Login successful for {userName}");
                    NotifyStateChanged();
                    return true;
                }

                Console.WriteLine("❌ Failed to save auth data");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                Console.WriteLine("🔓 AuthService: Logging out...");

                // Use JavaScript helper to clear
                await _jsRuntime.InvokeAsync<bool>("eval",
                    "window.authHelper ? window.authHelper.clearAuthData() : false");

                ResetAuthState();
                NotifyStateChanged();

                Console.WriteLine("✅ Logout successful");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Logout error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ForceRefreshAsync()
        {
            Console.WriteLine("🔄 Forcing auth refresh...");
            _isInitialized = false;
            return await InitializeAsync();
        }

        private void ResetAuthState()
        {
            _isAuthenticated = false;
            _userId = null;
            _userName = "Usuario";
            _isAdmin = false;
            Console.WriteLine("🔄 Auth state reset");
        }

        private void NotifyStateChanged()
        {
            Console.WriteLine($"📢 Notifying auth state change - IsAuth: {_isAuthenticated}");
            OnAuthStateChanged?.Invoke();
        }

        // Helper class for JSON deserialization
        private class AuthData
        {
            public string? userId { get; set; }
            public string? userName { get; set; }
            public string? userRole { get; set; }
        }
    }
}