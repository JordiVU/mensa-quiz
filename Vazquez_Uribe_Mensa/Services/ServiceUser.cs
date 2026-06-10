using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vazquez_Uribe_Mensa.Models;

namespace Vazquez_Uribe_Mensa.Services
{
    /// <summary>
    /// Proporciona servicios de acceso a datos para la entidad <see cref="User"/>,
    /// implementando operaciones CRUD sobre la base de datos.
    /// </summary>
    public class ServiceUser : IDisposable
    {
        #region Variables_Interfaz
        bool disposed;

        /// <summary>
        /// Inicializa una nueva instancia del servicio <see cref="ServiceUser"/>.
        /// </summary>
        public ServiceUser()
        {
            disposed = false;
        }
        #endregion

        #region CRUD
        // METODOS CRUD

        /// <summary>
        /// Obtiene la lista completa de usuarios registrados en el sistema.
        /// </summary>
        // LISTAR
        public async Task<List<User>> Listar()
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.Users
                    .AsNoTracking()
                    .OrderBy(u => u.Id)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Obtiene un usuario específico a partir de su identificador.
        /// </summary>
        // LISTAR ID
        public async Task<User?> Listar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
            }
        }

        /// <summary>
        /// Inserta un nuevo usuario en la base de datos.
        /// </summary>
        // INSERTAR
        public async Task<User> Insertar(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var _context = new MensaQuizDbContext())
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        // ACTUALIZAR
        public async Task<bool> Actualizar(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var _context = new MensaQuizDbContext())
            {
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (existing is null) return false;

                // Actualiza campos permitidos
                existing.Username = user.Username;
                existing.Password = user.Password;
                existing.Role = user.Role;
                existing.IsActive = user.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
        }

        /// <summary>
        /// Elimina un usuario de la base de datos a partir de su identificador.
        /// </summary>
        // BORRAR
        public async Task<bool> Borrar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (entity is null) return false;

                _context.Users.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        #endregion

        #region LiberacionRecursos
        // MÉTODOS DE LIBERACIÓN DE RECURSOS ----------

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing) { }
            disposed = true;
        }

        ~ServiceUser()
        {
            Dispose(false);
        }
        #endregion
    }
}