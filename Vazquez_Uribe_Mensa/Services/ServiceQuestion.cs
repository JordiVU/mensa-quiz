using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vazquez_Uribe_Mensa.Models;

namespace Vazquez_Uribe_Mensa.Services
{
    public class ServiceQuestion : IDisposable
    {
        #region Variables_Interfaz
        bool disposed;

        public ServiceQuestion()
        {
            disposed = false;
        }
        #endregion

        #region CRUD
        // LISTAR
        public async Task<List<Question>> ListarPorQuiz(int quizId)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.Questions
                    .Include(q => q.QuestionOptions)
                    .Where(q => q.QuizId == quizId)
                    .AsNoTracking()
                    .OrderBy(q => q.OrderIndex)
                    .ToListAsync();
            }
        }

        // LISTAR ID
        public async Task<Question?> Listar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.Questions
                .Include(q => q.QuestionOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);
            }
        }

        // INSERTAR
        public async Task<Question> Insertar(Question question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));

            using (var _context = new MensaQuizDbContext())
            {
                await _context.Questions.AddAsync(question);
                await _context.SaveChangesAsync();
                return question;
            }
        }

        // ACTUALIZAR
        public async Task<bool> Actualizar(Question question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));

            using (var _context = new MensaQuizDbContext())
            {
                var existing = await _context.Questions.FirstOrDefaultAsync(q => q.Id == question.Id);
                if (existing is null) return false;

                existing.Type = question.Type;
                existing.Content = question.Content;
                existing.ImagePath = question.ImagePath;
                existing.OrderIndex = question.OrderIndex;

                await _context.SaveChangesAsync();
                return true;
            }
        }

        // BORRAR
        public async Task<bool> Borrar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                var entity = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id);
                if (entity is null) return false;

                _context.Questions.Remove(entity);
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

        ~ServiceQuestion()
        {
            Dispose(false);
        }
        #endregion
    }
}