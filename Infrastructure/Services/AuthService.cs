using Microsoft.JSInterop;

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

        public async Task InitializeAsync()
        {
            if (_isInitialized)
            {
                Console.WriteLine("⚠️ AuthService: Ya inicializado, evitando doble inicialización");
                return;
            }

            await CheckAuthenticationAsync();
            _isInitialized = true;
        }

        public async Task CheckAuthenticationAsync()
        {
            try
            {
                Console.WriteLine("🔍 AuthService: Verificando autenticación...");

                // Usar un pequeño delay para asegurar que sessionStorage esté disponible
                await Task.Delay(50);

                var userId = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "userId");
                var userName = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "userName");
                var userRole = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "userRole");

                Console.WriteLine($"AuthService - userId: '{userId}'");
                Console.WriteLine($"AuthService - userName: '{userName}'");
                Console.WriteLine($"AuthService - userRole: '{userRole}'");

                if (!string.IsNullOrEmpty(userId))
                {
                    _isAuthenticated = true;
                    _userId = userId;
                    _userName = userName ?? "Usuario";
                    _isAdmin = userRole == "Admin";

                    Console.WriteLine($"✅ AuthService: Usuario autenticado - {_userName} (Admin: {_isAdmin})");
                }
                else
                {
                    _isAuthenticated = false;
                    _userId = null;
                    _userName = "Usuario";
                    _isAdmin = false;

                    Console.WriteLine("❌ AuthService: No autenticado");
                }

                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AuthService Error: {ex.Message}");
                _isAuthenticated = false;
                NotifyStateChanged();
            }
        }

        public async Task LoginAsync(string userId, string userName, string userRole)
        {
            try
            {
                Console.WriteLine($"🔐 AuthService: Iniciando login para {userName}...");

                // Guardar en sessionStorage
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userId", userId);
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userName", userName);
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userRole", userRole);

                // Actualizar estado local
                _isAuthenticated = true;
                _userId = userId;
                _userName = userName;
                _isAdmin = userRole == "Admin";

                Console.WriteLine($"✅ AuthService: Login exitoso - {userName} (Admin: {_isAdmin})");

                // Notificar cambio de estado
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AuthService Login Error: {ex.Message}");
                throw; // Re-lanzar para que el componente pueda manejar el error
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                Console.WriteLine("🔓 AuthService: Cerrando sesión...");

                // Limpiar sessionStorage
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userId");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userName");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userRole");

                // Resetear estado local
                _isAuthenticated = false;
                _userId = null;
                _userName = "Usuario";
                _isAdmin = false;

                Console.WriteLine("✅ AuthService: Logout exitoso");

                // Notificar cambio de estado
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AuthService Logout Error: {ex.Message}");
                throw;
            }
        }

        public async Task RefreshAuthenticationAsync()
        {
            await CheckAuthenticationAsync();
        }

        private void NotifyStateChanged()
        {
            Console.WriteLine($"📢 AuthService: Notificando cambio de estado - IsAuth: {_isAuthenticated}");
            OnAuthStateChanged?.Invoke();
        }
    }
}