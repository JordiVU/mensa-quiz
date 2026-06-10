using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Vazquez_Uribe_Mensa.Models;
using Vazquez_Uribe_Mensa.Services;
using Vazquez_Uribe_Mensa.ViewModels.Base;

namespace Vazquez_Uribe_Mensa.ViewModels
{
    public class QuizPlayerViewModel : BaseViewModel
    {
        private readonly Action<object?> _volverAPrincipal;
        private readonly Quiz _quiz;
        private readonly int _participantes;
        private readonly DateTime _inicio;
        private int _indiceActual = 0;
        private List<QuestionOption> _opcionesEmparejarIzquierda = new();
        private List<QuestionOption> _opcionesEmparejarDerecha = new();

        public List<QuestionOption> OpcionesEmparejarIzquierda
        {
            get => _opcionesEmparejarIzquierda;
            set => SetProperty(ref _opcionesEmparejarIzquierda, value);
        }

        public List<QuestionOption> OpcionesEmparejarDerecha
        {
            get => _opcionesEmparejarDerecha;
            set => SetProperty(ref _opcionesEmparejarDerecha, value);
        }

        // --- Pregunta actual ---
        private Question _preguntaActual;
        public Question PreguntaActual
        {
            get => _preguntaActual;
            set => SetProperty(ref _preguntaActual, value);
        }

        // --- Progreso ---
        private int _numeroPreguntaActual = 1;
        public int NumeroPreguntaActual
        {
            get => _numeroPreguntaActual;
            set => SetProperty(ref _numeroPreguntaActual, value);
        }

        public int TotalPreguntas => _quiz.Questions.Count;

        // --- Controles de navegación ---
        private bool _mostrandoRespuesta;
        public bool MostrandoRespuesta
        {
            get => _mostrandoRespuesta;
            set
            {
                SetProperty(ref _mostrandoRespuesta, value);
                OnPropertyChanged(nameof(TextoBotonSiguiente));
            }
        }

        public string TextoBotonSiguiente
        {
            get
            {
                if (_indiceActual >= TotalPreguntas - 1 && MostrandoRespuesta)
                    return "Finalizar";
                if (PreguntaActual?.Type == "Break" || PreguntaActual?.Type == "OpenQuestion")
                    return _indiceActual >= TotalPreguntas - 1 ? "Finalizar" : "Siguiente";
                return MostrandoRespuesta ? (_indiceActual >= TotalPreguntas - 1 ? "Finalizar" : "Siguiente") : "Ver Respuesta";
            }
        }

        // Visibilidad por tipo
        public bool EsMultiple => PreguntaActual?.Type == "MultipleChoice";
        public bool EsLibre => PreguntaActual?.Type == "OpenQuestion";
        public bool EsOrdenar => PreguntaActual?.Type == "Ordering";
        public bool EsEmparejar => PreguntaActual?.Type == "Matching";
        public bool EsPausa => PreguntaActual?.Type == "Break";

        public ICommand SiguienteCommand { get; }

        public QuizPlayerViewModel(Quiz quiz, int participantes, Action<object?> volverAPrincipal)
        {
            _quiz = quiz;
            _participantes = participantes;
            _volverAPrincipal = volverAPrincipal;
            _inicio = DateTime.Now;

            SiguienteCommand = new RelayCommand(async _ => await Siguiente());

            CargarPregunta();
        }

        private void CargarPregunta()
        {
            var preguntas = _quiz.Questions.OrderBy(q => q.OrderIndex).ToList();
            if (_indiceActual >= preguntas.Count) return;

            var pregunta = preguntas[_indiceActual];

            // Desordenar opciones para Ordenar y Emparejar
            if (pregunta.Type == "Ordering" || pregunta.Type == "Matching")
            {
                var opcionesDesordenadas = pregunta.QuestionOptions
                    .OrderBy(_ => Guid.NewGuid())
                    .ToList();

                // Crear nueva instancia de la pregunta para forzar el binding
                PreguntaActual = new Question
                {
                    Id = pregunta.Id,
                    QuizId = pregunta.QuizId,
                    Type = pregunta.Type,
                    Content = pregunta.Content,
                    ImagePath = pregunta.ImagePath,
                    OrderIndex = pregunta.OrderIndex,
                    QuestionOptions = new ObservableCollection<QuestionOption>(opcionesDesordenadas)
                };
            }
            else
            {
                PreguntaActual = pregunta;
            }

            if (pregunta.Type == "Matching")
            {
                var izquierda = pregunta.QuestionOptions.OrderBy(_ => Guid.NewGuid()).ToList();
                var derecha = pregunta.QuestionOptions.OrderBy(_ => Guid.NewGuid()).ToList();

                // Asegurarse de que ninguna fila coincide con su par correcto
                for (int i = 0; i < derecha.Count; i++)
                {
                    if (derecha[i].MatchPair == izquierda[i].Text)
                    {
                        int swap = (i + 1) % derecha.Count;
                        (derecha[i], derecha[swap]) = (derecha[swap], derecha[i]);
                    }
                }

                OpcionesEmparejarIzquierda = izquierda;
                OpcionesEmparejarDerecha = derecha;
            }

            NumeroPreguntaActual = _indiceActual + 1;
            MostrandoRespuesta = false;

            NotificarTipo();
            OnPropertyChanged(nameof(TextoBotonSiguiente));
        }

        private async Task Siguiente()
        {
            bool sinRespuesta = PreguntaActual?.Type == "Break" ||
                                PreguntaActual?.Type == "OpenQuestion";

            if (!sinRespuesta && !MostrandoRespuesta)
            {
                if (PreguntaActual.Type == "Ordering")
                {
                    var opcionesOrdenadas = PreguntaActual.QuestionOptions
                        .OrderBy(o => o.OrderIndex)
                        .ToList();
                    PreguntaActual.QuestionOptions = new ObservableCollection<QuestionOption>(opcionesOrdenadas);
                    OnPropertyChanged(nameof(PreguntaActual));
                }

                if (PreguntaActual.Type == "Matching")
                {
                    // Reordenar derecha para que cada fila muestre el par correcto
                    OpcionesEmparejarDerecha = OpcionesEmparejarIzquierda
                        .Select(izq => PreguntaActual.QuestionOptions
                            .First(o => o.Text == izq.Text))
                        .ToList();
                    OnPropertyChanged(nameof(OpcionesEmparejarDerecha));
                }

                MostrandoRespuesta = true;
                return;
            }

            if (_indiceActual >= TotalPreguntas - 1)
            {
                await Finalizar();
                return;
            }

            _indiceActual++;
            CargarPregunta();
        }

        private async Task Finalizar()
        {
            var fin = DateTime.Now;
            var duracion = (int)(fin - _inicio).TotalMinutes;

            // Pedir duración real al presentador
            string input = Interaction.InputBox(
                "¿Cuántos minutos duró la sesión?",
                "Finalizar Quiz",
                duracion.ToString());

            if (!int.TryParse(input, out int duracionFinal))
                duracionFinal = duracion;

            var session = new QuizSession
            {
                QuizId = _quiz.Id,
                StartedAt = _inicio,
                EndedAt = fin,
                ParticipantCount = _participantes,
                DurationMinutes = duracionFinal
            };

            using var service = new ServiceSession();
            await service.Insertar(session);

            _volverAPrincipal(null);
        }

        private void NotificarTipo()
        {
            OnPropertyChanged(nameof(EsMultiple));
            OnPropertyChanged(nameof(EsLibre));
            OnPropertyChanged(nameof(EsOrdenar));
            OnPropertyChanged(nameof(EsEmparejar));
            OnPropertyChanged(nameof(EsPausa));
        }
    }
}