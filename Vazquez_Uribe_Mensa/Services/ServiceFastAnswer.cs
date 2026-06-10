using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vazquez_Uribe_Mensa.Models;

namespace Vazquez_Uribe_Mensa.Services
{
    public class ServiceFastAnswer : IDisposable
    {
        #region Variables_Interfaz
        bool disposed;

        public ServiceFastAnswer()
        {
            disposed = false;
        }
        #endregion

        #region CRUD
        // NOTA: No se implementa Actualizar porque las entradas del podio son inmutables.
        // Una vez registrada la respuesta de un grupo no tiene sentido modificarla.

        // LISTAR POR SESIÓN (Para mostrar el podio en tiempo real)
        public async Task<List<FastAnswerEntry>> ListarPorSesion(int sessionId)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.FastAnswerEntries
                    .Where(fa => fa.QuizSessionId == sessionId)
                    .AsNoTracking()
                    .OrderBy(fa => fa.ResponseTimeSeconds) // Ordenar por los más rápidos
                    .ToListAsync();
            }
        }

        // INSERTAR
        public async Task<FastAnswerEntry> Insertar(FastAnswerEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            using (var _context = new MensaQuizDbContext())
            {
                entry.Timestamp = DateTime.Now;
                await _context.FastAnswerEntries.AddAsync(entry);
                await _context.SaveChangesAsync();
                return entry;
            }
        }

        // BORRAR (Limpiar entradas del podio)
        public async Task<bool> Borrar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                var entity = await _context.FastAnswerEntries.FirstOrDefaultAsync(fa => fa.Id == id);
                if (entity is null) return false;

                _context.FastAnswerEntries.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
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

        ~ServiceFastAnswer()
        {
            Dispose(false);
        }
        #endregion
    }
}