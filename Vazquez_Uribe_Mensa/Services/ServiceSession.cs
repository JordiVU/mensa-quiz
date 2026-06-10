using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vazquez_Uribe_Mensa.Models;

namespace Vazquez_Uribe_Mensa.Services
{
    public class ServiceSession : IDisposable
    {
        #region Variables_Interfaz
        bool disposed;

        public ServiceSession()
        {
            disposed = false;
        }
        #endregion

        #region CRUD
        // LISTAR
        public async Task<List<QuizSession>> Listar()
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.QuizSessions
                    .Include(qs => qs.Quiz)
                    .AsNoTracking()
                    .OrderByDescending(qs => qs.StartedAt)
                    .ToListAsync();
            }
        }

        // LISTAR ID
        public async Task<QuizSession?> Listar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.QuizSessions
                .Include(qs => qs.Quiz)
                .Include(qs => qs.FastAnswerEntries)
                .AsNoTracking()
                .FirstOrDefaultAsync(qs => qs.Id == id);
            }
        }

        // LISTA POR USUARIO
        public async Task<List<QuizSession>> ListarPorUsuario(int userId)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.QuizSessions
                    .Include(qs => qs.Quiz)
                    .Where(qs => qs.Quiz.UserId == userId)
                    .AsNoTracking()
                    .OrderByDescending(qs => qs.StartedAt)
                    .ToListAsync();
            }
        }

        // INSERTAR (Registra el inicio de la sesión)
        public async Task<QuizSession> Insertar(QuizSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            using (var _context = new MensaQuizDbContext())
            {
                await _context.QuizSessions.AddAsync(session);
                await _context.SaveChangesAsync();
                return session;
            }
        }

        // ACTUALIZAR (Normalmente usado para guardar el fin de la sesión y duración)
        public async Task<bool> Actualizar(QuizSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            using (var _context = new MensaQuizDbContext())
            {
                var existing = await _context.QuizSessions.FirstOrDefaultAsync(qs => qs.Id == session.Id);
                if (existing is null) return false;

                existing.EndedAt = session.EndedAt;
                existing.ParticipantCount = session.ParticipantCount;
                existing.DurationMinutes = session.DurationMinutes;

                await _context.SaveChangesAsync();
                return true;
            }
        }

        // BORRAR
        public async Task<bool> Borrar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                var entity = await _context.QuizSessions.FirstOrDefaultAsync(qs => qs.Id == id);
                if (entity is null) return false;

                _context.QuizSessions.Remove(entity);
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

        ~ServiceSession()
        {
            Dispose(false);
        }
        #endregion
    }
}