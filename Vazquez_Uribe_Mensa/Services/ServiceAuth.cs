using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Vazquez_Uribe_Mensa.Models;

namespace Vazquez_Uribe_Mensa.Services
{
    public class ServiceAuth : IDisposable
    {
        #region Variables_Interfaz
        bool disposed;

        // Propiedad estática para mantener el usuario logueado en toda la app 
        public static User? UsuarioActivo { get; private set; }

        public ServiceAuth()
        {
            disposed = false;
        }
        #endregion

        #region METODOS AUTH
        /// <summary>
        /// Comprueba las credenciales del usuario y lo establece como activo si son correctas.
        /// </summary>
        public async Task<User?> Login(string username, string passwordPlano)
        {
            using (var _context = new MensaQuizDbContext())
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (user != null && BCrypt.Net.BCrypt.Verify(passwordPlano, user.Password))
                {
                    UsuarioActivo = user;
                    return user;
                }

                return null;
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario actual.
        /// </summary>
        public void Logout()
        {
            UsuarioActivo = null;
        }
        #endregion

        #region LiberacionRecursos
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;
        }

        ~ServiceAuth()
        {
            Dispose(false);
        }
        #endregion
    }
}