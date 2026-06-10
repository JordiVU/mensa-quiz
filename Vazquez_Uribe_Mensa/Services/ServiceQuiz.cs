using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vazquez_Uribe_Mensa.Models;

namespace Vazquez_Uribe_Mensa.Services
{
    /// <summary>
    /// Proporciona servicios de acceso a datos para la entidad <see cref="Quiz"/>,
    /// implementando operaciones CRUD sobre la base de datos.
    /// </summary>
    public class ServiceQuiz : IDisposable
    {
        #region Variables_Interfaz
        bool disposed;

        public ServiceQuiz()
        {
            disposed = false;
        }
        #endregion

        #region CRUD
        // METODOS CRUD

        /// <summary>
        /// Obtiene la lista completa de quizzes registrados en el sistema, incluyendo sus creadores (User).
        /// </summary>
        // LISTAR
        public async Task<List<Quiz>> Listar()
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.Quizzes
                    .Include(q => q.User)
                    .AsNoTracking()
                    .OrderBy(q => q.Id)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Obtiene los quizzes de un usuario concreto, para la pantalla "Mis Quizzes".
        /// </summary>
        public async Task<List<Quiz>> ListarPorUsuario(int userId)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.Quizzes
                    .Where(q => q.UserId == userId)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                    .AsNoTracking()
                    .OrderByDescending(q => q.UpdatedAt)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Obtiene un quiz específico a partir de su identificador, incluyendo preguntas.
        /// </summary>
        // LISTAR ID
        public async Task<Quiz?> Listar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                return await _context.Quizzes
                    .Include(q => q.User)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
        }

        /// <summary>
        /// Inserta un nuevo quiz en la base de datos.
        /// </summary>
        // INSERTAR
        public async Task<Quiz> Insertar(Quiz quiz)
        {
            if (quiz == null) throw new ArgumentNullException(nameof(quiz));

            using (var _context = new MensaQuizDbContext())
            {
                // Guardamos primero el quiz solo
                var preguntas = quiz.Questions.ToList();
                quiz.Questions.Clear();
                quiz.CreatedAt = DateTime.Now;
                quiz.UpdatedAt = DateTime.Now;

                await _context.Quizzes.AddAsync(quiz);
                await _context.SaveChangesAsync();

                // Ahora guardamos las preguntas con el QuizId ya generado
                foreach (var pregunta in preguntas)
                {
                    pregunta.QuizId = quiz.Id;
                    var opciones = pregunta.QuestionOptions.ToList();
                    pregunta.QuestionOptions.Clear();

                    await _context.Questions.AddAsync(pregunta);
                    await _context.SaveChangesAsync();

                    // Y las opciones con el QuestionId ya generado
                    foreach (var opcion in opciones)
                    {
                        opcion.QuestionId = pregunta.Id;
                        await _context.QuestionOptions.AddAsync(opcion);
                    }

                    await _context.SaveChangesAsync();
                }

                return quiz;
            }
        }

        /// <summary>
        /// Actualiza los datos de un quiz existente.
        /// </summary>
        // ACTUALIZAR
        public async Task<bool> Actualizar(Quiz quiz, List<Question> preguntas)
        {
            if (quiz == null) throw new ArgumentNullException(nameof(quiz));

            using (var _context = new MensaQuizDbContext())
            {
                var existing = await _context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                    .FirstOrDefaultAsync(q => q.Id == quiz.Id);

                if (existing is null) return false;

                // Actualiza campos del quiz
                existing.Title = quiz.Title;
                existing.Description = quiz.Description;
                existing.UpdatedAt = DateTime.Now;

                // Actualiza cada pregunta existente
                foreach (var pregunta in preguntas)
                {
                    var existingPregunta = existing.Questions
                        .FirstOrDefault(q => q.Id == pregunta.Id);

                    if (existingPregunta != null)
                    {
                        // Editar pregunta existente
                        existingPregunta.Content = pregunta.Content;
                        existingPregunta.OrderIndex = pregunta.OrderIndex;

                        foreach (var opcion in pregunta.QuestionOptions)
                        {
                            var existingOpcion = existingPregunta.QuestionOptions
                                .FirstOrDefault(o => o.Id == opcion.Id);

                            if (existingOpcion != null)
                            {
                                existingOpcion.Text = opcion.Text;
                                existingOpcion.IsCorrect = opcion.IsCorrect;
                            }
                        }
                    }
                    else
                    {
                        // Pregunta nueva añadida durante la edición
                        pregunta.QuizId = existing.Id;
                        existing.Questions.Add(pregunta);
                    }
                }

                // Borrar preguntas que se eliminaron
                var preguntasABorrar = existing.Questions
                    .Where(q => !preguntas.Any(p => p.Id == q.Id) && q.Id != 0)
                    .ToList();

                foreach (var p in preguntasABorrar)
                    _context.Questions.Remove(p);

                await _context.SaveChangesAsync();
                return true;
            }
        }

        /// <summary>
        /// Elimina un quiz de la base de datos a partir de su identificador.
        /// </summary>
        // BORRAR
        public async Task<bool> Borrar(int id)
        {
            using (var _context = new MensaQuizDbContext())
            {
                var entity = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
                if (entity is null) return false;

                _context.Quizzes.Remove(entity);
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

        ~ServiceQuiz()
        {
            Dispose(false);
        }
        #endregion
    }
}