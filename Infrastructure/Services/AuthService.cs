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
        private readonly SemaphoreSlim _initSemaphore = new(1, 1);

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
            await _initSemaphore.WaitAsync();
            try
            {
                if (_isInitialized)
                {
                    Console.WriteLine("⚠️ AuthService: Already initialized");
                    return _isAuthenticated;
                }

                Console.WriteLine("🚀 AuthService: Initializing...");

                // Verificar si podemos hacer llamadas a JS (no estamos en pre-rendering)
                if (!CanUseJavaScript())
                {
                    Console.WriteLine("⚠️ AuthService: Cannot use JS during pre-rendering, skipping initialization");
                    return false;
                }

                // Pequeño delay para asegurar que JS esté completamente listo
                await Task.Delay(50);

                try
                {
                    // Intentar obtener datos de autenticación
                    var authDataJson = await _jsRuntime.InvokeAsync<string>("eval",
                        "JSON.stringify(window.authHelper ? window.authHelper.getAuthData() : null)",
                        TimeSpan.FromSeconds(5)); // Timeout de 5 segundos

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
                        Console.WriteLine("ℹ️ AuthService: No auth data found in sessionStorage");
                        ResetAuthState();
                    }
                }
                catch (JSException jsEx)
                {
                    Console.WriteLine($"⚠️ AuthService: JS error during initialization: {jsEx.Message}");
                    ResetAuthState();
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("⚠️ AuthService: Initialization timed out");
                    ResetAuthState();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ AuthService: Initialization error: {ex.Message}");
                    ResetAuthState();
                }

                _isInitialized = true;
                NotifyStateChanged();

                return _isAuthenticated;
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        public async Task<bool> CheckAuthenticationAsync()
        {
            if (!CanUseJavaScript())
            {
                Console.WriteLine("⚠️ AuthService: Cannot check auth during pre-rendering");
                return false;
            }

            try
            {
                Console.WriteLine("🔍 AuthService: Checking authentication...");

                var authDataJson = await _jsRuntime.InvokeAsync<string>("eval",
                    "JSON.stringify(window.authHelper ? window.authHelper.getAuthData() : null)",
                    TimeSpan.FromSeconds(3));

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
            if (!CanUseJavaScript())
            {
                Console.WriteLine("⚠️ AuthService: Cannot login during pre-rendering");
                return false;
            }

            try
            {
                Console.WriteLine($"🔐 AuthService: Logging in {userName}...");

                var success = await _jsRuntime.InvokeAsync<bool>("eval",
                    $"window.authHelper ? window.authHelper.saveAuthData('{userId}', '{userName}', '{userRole}') : false",
                    TimeSpan.FromSeconds(3));

                if (success)
                {
                    _isAuthenticated = true;
                    _userId = userId;
                    _userName = userName;
                    _isAdmin = userRole == "Admin";
                    _isInitialized = true;

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
            if (!CanUseJavaScript())
            {
                Console.WriteLine("⚠️ AuthService: Cannot logout during pre-rendering");
                return false;
            }

            try
            {
                Console.WriteLine("🔓 AuthService: Logging out...");

                await _jsRuntime.InvokeAsync<bool>("eval",
                    "window.authHelper ? window.authHelper.clearAuthData() : false",
                    TimeSpan.FromSeconds(3));

                ResetAuthState();
                _isInitialized = true;
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

        private bool CanUseJavaScript()
        {
            // Verificar si podemos usar JS (no estamos en pre-rendering)
            // IJSInProcessRuntime indica que estamos en el cliente
            // IJSUnmarshalledRuntime también indica cliente
            // Si es solo IJSRuntime, probablemente estamos en el servidor

            try
            {
                return _jsRuntime is not null;
            }
            catch
            {
                return false;
            }
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
            try
            {
                Console.WriteLine($"📢 Notifying auth state change - IsAuth: {_isAuthenticated}");
                OnAuthStateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notifying state change: {ex.Message}");
            }
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