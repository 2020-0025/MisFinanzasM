using Microsoft.AspNetCore.Identity;
using MisFinanzas.Domain.Entities;

namespace MisFinanzas.Infrastructure.Security
{
    /// <summary>
    ///  SOLO PARA PROPÓSITOS ACADÉMICOS
    /// Password hasher que puede trabajar con o sin encriptación
    /// Para cambiar entre modos, modificar appsettings.json
    /// </summary>
    public class PlainTextPasswordHasher : IPasswordHasher<ApplicationUser>
    {
        private readonly bool _useEncryption;

        public PlainTextPasswordHasher(bool useEncryption = false)
        {
            _useEncryption = useEncryption;
        }

        /// <summary>
        /// "Hashea" la contraseña. Si useEncryption es false, devuelve texto plano.
        /// </summary>
        public string HashPassword(ApplicationUser user, string password)
        {
            if (_useEncryption)
            {
                // Modo seguro: usar el hasher por defecto de Identity
                var defaultHasher = new PasswordHasher<ApplicationUser>();
                return defaultHasher.HashPassword(user, password);
            }

            // ⚠️ Modo académico: devolver contraseña en texto plano
            return password;
        }

        /// <summary>
        /// Verifica si la contraseña proporcionada coincide con el hash almacenado
        /// </summary>
        public PasswordVerificationResult VerifyHashedPassword(
            ApplicationUser user,
            string hashedPassword,
            string providedPassword)
        {
            if (_useEncryption)
            {
                // Modo seguro: usar el hasher por defecto de Identity
                var defaultHasher = new PasswordHasher<ApplicationUser>();
                return defaultHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            }

            // Modo académico: puede manejar AMBOS formatos
            // 1. Intentar comparación directa (texto plano legacy)
            if (hashedPassword == providedPassword)
            {
                return PasswordVerificationResult.Success;
            }

            // 2. Si no coincide, intentar verificar como hash
            //    (para permitir transición desde hash a texto plano)
            try
            {
                var defaultHasher = new PasswordHasher<ApplicationUser>();
                var result = defaultHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

                if (result == PasswordVerificationResult.Success)
                {
                    return PasswordVerificationResult.Success;
                }
            }
            catch
            {
                // No es un hash válido, continuar
            }

            return PasswordVerificationResult.Failed;
        }
    }
}