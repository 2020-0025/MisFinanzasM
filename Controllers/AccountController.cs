using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Services;

namespace MisFinanzas.Controllers
{
    [Route("")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(
           SignInManager<ApplicationUser> signInManager,
           UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("external-login-challenge")]
        public IActionResult ExternalLoginChallenge(string provider, string returnUrl = "/")
        {
            // Configurar el redirect URI para cuando Google retorne
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

            // Configurar propiedades de autenticación
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            Console.WriteLine($"🚀 Starting external login challenge with {provider}");
            Console.WriteLine($"📍 Redirect URL: {redirectUrl}");

            // Redirigir a Google
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                Console.WriteLine($"❌ Error from external provider: {remoteError}");
                return Redirect($"/login?error=Error desde Google: {remoteError}");
            }

            Console.WriteLine("✅ External login callback received");

            try
            {
                // Obtener información del login externo
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    Console.WriteLine("❌ No external login info found");
                    return Redirect("/login?error=No se pudo obtener la información de Google");
                }

                Console.WriteLine($"🔐 External login info received from {info.LoginProvider}");

                // Obtener email del usuario de Google
                var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("❌ No email found in claims");
                    return Redirect("/login?error=No se pudo obtener tu email de Google");
                }

                Console.WriteLine($"📧 Email from Google: {email}");

                // Buscar si ya existe un usuario con este email
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // NUEVO USUARIO - Crear cuenta
                    Console.WriteLine("📝 Creating new user...");

                    var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email.Split('@')[0];

                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = name,
                        UserRole = "User",
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(user);

                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"❌ Error creating user: {errors}");
                        return Redirect($"/login?error=Error al crear tu cuenta: {errors}");
                    }

                    Console.WriteLine($"✅ New user created: {user.Email}");
                }
                else
                {
                    Console.WriteLine($"✅ Existing user found: {user.Email}");
                }

                // Verificar si ya tiene vinculado este login externo
                var existingLogin = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (existingLogin == null)
                {
                    // Vincular el login de Google a este usuario
                    Console.WriteLine("🔗 Linking Google login to user...");

                    var addLoginResult = await _userManager.AddLoginAsync(user, info);

                    if (!addLoginResult.Succeeded)
                    {
                        var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"❌ Error linking login: {errors}");
                        return Redirect($"/login?error=Error al vincular con Google: {errors}");
                    }

                    Console.WriteLine($"✅ Google login linked to user: {user.Email}");
                }

                // 🔥 Hacer sign-in con ASP.NET Identity (crea la cookie de autenticación)
                Console.WriteLine("🔐 Signing in user with ASP.NET Identity...");

                await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                Console.WriteLine($"✅ User signed in with Identity cookie");

                // Redirigir a página Blazor que sincronizará con SessionStorage
                return Redirect("/external-login-success?userId=" + user.Id + "&userName=" + Uri.EscapeDataString(user.UserName ?? user.Email) + "&userRole=" + Uri.EscapeDataString(user.UserRole ?? "User"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ExternalLoginCallback: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Redirect($"/login?error=Ocurrió un error inesperado: {ex.Message}");
            }
        }
    }
}