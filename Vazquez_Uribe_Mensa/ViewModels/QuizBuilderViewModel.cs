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
    public class QuizBuilderViewModel : BaseViewModel
    {
        private readonly Action<object?> _volverAPrincipal;
        private int? _quizId;
        private Question? _preguntaEditando;

        // --- Datos del quiz ---
        private string _titulo = string.Empty;
        private string _descripcion = string.Empty;
        private string _tituloVista = "Crear Nuevo Quiz";

        public string Titulo
        {
            get => _titulo;
            set => SetProperty(ref _titulo, value);
        }
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }
        public string TituloVista
        {
            get => _tituloVista;
            set => SetProperty(ref _tituloVista, value);
        }

        // --- Lista de preguntas ---
        public ObservableCollection<Question> Preguntas { get; } = new();

        // --- Control de panel ---
        private bool _panelVisible;
        private string _tipoPanelActivo = string.Empty;

        public bool PanelVisible
        {
            get => _panelVisible;
            set => SetProperty(ref _panelVisible, value);
        }

        public bool EstaEditando => _preguntaEditando != null;

        // Visibilidad de cada panel
        public bool PanelMultipleVisible => _tipoPanelActivo == "MultipleChoice";
        public bool PanelLibreVisible => _tipoPanelActivo == "OpenQuestion";
        public bool PanelOrdenarVisible => _tipoPanelActivo == "Ordering";
        public bool PanelEmparejarVisible => _tipoPanelActivo == "Matching";
        public bool PanelPausaVisible => _tipoPanelActivo == "Break";

        // --- Campos compartidos ---
        private string _textoPregunta = string.Empty;
        public string TextoPregunta
        {
            get => _textoPregunta;
            set => SetProperty(ref _textoPregunta, value);
        }

        // --- Campos Multiple ---
        private string _opcionA = string.Empty;
        private string _opcionB = string.Empty;
        private string _opcionC = string.Empty;
        private string _opcionD = string.Empty;
        private bool _correctaA = true;
        private bool _correctaB;
        private bool _correctaC;
        private bool _correctaD;
        private string _opcionCorrecta = "A";

        public string OpcionA { get => _opcionA; set => SetProperty(ref _opcionA, value); }
        public string OpcionB { get => _opcionB; set => SetProperty(ref _opcionB, value); }
        public string OpcionC { get => _opcionC; set => SetProperty(ref _opcionC, value); }
        public string OpcionD { get => _opcionD; set => SetProperty(ref _opcionD, value); }

        public bool CorrectaA
        {
            get => _correctaA;
            set { SetProperty(ref _correctaA, value); if (value) _opcionCorrecta = "A"; }
        }
        public bool CorrectaB
        {
            get => _correctaB;
            set { SetProperty(ref _correctaB, value); if (value) _opcionCorrecta = "B"; }
        }
        public bool CorrectaC
        {
            get => _correctaC;
            set { SetProperty(ref _correctaC, value); if (value) _opcionCorrecta = "C"; }
        }
        public bool CorrectaD
        {
            get => _correctaD;
            set { SetProperty(ref _correctaD, value); if (value) _opcionCorrecta = "D"; }
        }

        // --- Campos Ordenar ---
        // Lista de elementos a ordenar
        public ObservableCollection<string> ElementosOrdenar { get; } = new();
        private string _nuevoElementoOrdenar = string.Empty;
        public string NuevoElementoOrdenar
        {
            get => _nuevoElementoOrdenar;
            set => SetProperty(ref _nuevoElementoOrdenar, value);
        }
        public ICommand AnadirElementoOrdenarCommand { get; }
        public ICommand EliminarElementoOrdenarCommand { get; }

        // --- Campos Emparejar ---
        // Lista de pares (Término, Definición)
        public ObservableCollection<ParEmparejar> ParesEmparejar { get; } = new();
        private string _nuevoTermino = string.Empty;
        private string _nuevaDefinicion = string.Empty;
        public string NuevoTermino
        {
            get => _nuevoTermino;
            set => SetProperty(ref _nuevoTermino, value);
        }
        public string NuevaDefinicion
        {
            get => _nuevaDefinicion;
            set => SetProperty(ref _nuevaDefinicion, value);
        }
        public ICommand AnadirParCommand { get; }
        public ICommand EliminarParCommand { get; }

        // --- Campos Pausa ---
        private string _tituloPausa = string.Empty;
        private string _subtituloPausa = string.Empty;
        public string TituloPausa
        {
            get => _tituloPausa;
            set => SetProperty(ref _tituloPausa, value);
        }
        public string SubtituloPausa
        {
            get => _subtituloPausa;
            set => SetProperty(ref _subtituloPausa, value);
        }

        // --- Comandos principales ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand CerrarPanelCommand { get; }
        public ICommand AbrirPanelMultipleCommand { get; }
        public ICommand AbrirPanelLibreCommand { get; }
        public ICommand AbrirPanelOrdenarCommand { get; }
        public ICommand AbrirPanelEmparejarCommand { get; }
        public ICommand AbrirPanelPausaCommand { get; }
        public ICommand AnadirPreguntaCommand { get; }
        public ICommand EliminarPreguntaCommand { get; }
        public ICommand EditarPreguntaCommand { get; }
        public ICommand SubirPreguntaCommand { get; }
        public ICommand BajarPreguntaCommand { get; }
        public ICommand SubirElementoOrdenarCommand { get; }
        public ICommand BajarElementoOrdenarCommand { get; }

        public QuizBuilderViewModel(Action<object?> volverAPrincipal, Quiz? quizExistente = null)
        {
            _volverAPrincipal = volverAPrincipal;

            GuardarCommand = new RelayCommand(async _ => await Guardar());
            CancelarCommand = new RelayCommand(_ => _volverAPrincipal(null));
            CerrarPanelCommand = new RelayCommand(_ => CerrarPanel());
            AbrirPanelMultipleCommand = new RelayCommand(_ => AbrirPanel("MultipleChoice"));
            AbrirPanelLibreCommand = new RelayCommand(_ => AbrirPanel("OpenQuestion"));
            AbrirPanelOrdenarCommand = new RelayCommand(_ => AbrirPanel("Ordering"));
            AbrirPanelEmparejarCommand = new RelayCommand(_ => AbrirPanel("Matching"));
            AbrirPanelPausaCommand = new RelayCommand(_ => AbrirPanel("Break"));
            AnadirPreguntaCommand = new RelayCommand(_ => AnadirPregunta());
            EliminarPreguntaCommand = new RelayCommand(EliminarPregunta);
            EditarPreguntaCommand = new RelayCommand(EditarPregunta);
            AnadirElementoOrdenarCommand = new RelayCommand(_ => AnadirElementoOrdenar());
            EliminarElementoOrdenarCommand = new RelayCommand(EliminarElementoOrdenar);
            AnadirParCommand = new RelayCommand(_ => AnadirPar());
            EliminarParCommand = new RelayCommand(EliminarPar);
            SubirPreguntaCommand = new RelayCommand(SubirPregunta);
            BajarPreguntaCommand = new RelayCommand(BajarPregunta);
            SubirElementoOrdenarCommand = new RelayCommand(SubirElementoOrdenar);
            BajarElementoOrdenarCommand = new RelayCommand(BajarElementoOrdenar);

            if (quizExistente != null)
                _ = CargarQuizExistente(quizExistente.Id);
        }

        // --- Abrir/cerrar panel ---
        private void AbrirPanel(string tipo)
        {
            _preguntaEditando = null;
            _tipoPanelActivo = tipo;
            TextoPregunta = string.Empty;

            // Reset Multiple
            OpcionA = string.Empty; OpcionB = string.Empty;
            OpcionC = string.Empty; OpcionD = string.Empty;
            CorrectaA = true; CorrectaB = false;
            CorrectaC = false; CorrectaD = false;

            // Reset Ordenar
            ElementosOrdenar.Clear();
            NuevoElementoOrdenar = string.Empty;

            // Reset Emparejar
            ParesEmparejar.Clear();
            NuevoTermino = string.Empty;
            NuevaDefinicion = string.Empty;

            // Reset Pausa
            TituloPausa = string.Empty;
            SubtituloPausa = string.Empty;

            NotificarPaneles();
            OnPropertyChanged(nameof(EstaEditando));
            PanelVisible = true;
        }

        private void CerrarPanel()
        {
            _preguntaEditando = null;
            _tipoPanelActivo = string.Empty;
            NotificarPaneles();
            OnPropertyChanged(nameof(EstaEditando));
            PanelVisible = false;
        }

        private void NotificarPaneles()
        {
            OnPropertyChanged(nameof(PanelMultipleVisible));
            OnPropertyChanged(nameof(PanelLibreVisible));
            OnPropertyChanged(nameof(PanelOrdenarVisible));
            OnPropertyChanged(nameof(PanelEmparejarVisible));
            OnPropertyChanged(nameof(PanelPausaVisible));
        }

        // --- Ordenar: añadir/eliminar elementos ---
        private void AnadirElementoOrdenar()
        {
            if (string.IsNullOrWhiteSpace(NuevoElementoOrdenar)) return;
            ElementosOrdenar.Add(NuevoElementoOrdenar);
            NuevoElementoOrdenar = string.Empty;
        }

        private void EliminarElementoOrdenar(object? parameter)
        {
            if (parameter is string elemento)
                ElementosOrdenar.Remove(elemento);
        }

        // --- Emparejar: añadir/eliminar pares ---
        private void AnadirPar()
        {
            if (string.IsNullOrWhiteSpace(NuevoTermino) || string.IsNullOrWhiteSpace(NuevaDefinicion)) return;
            ParesEmparejar.Add(new ParEmparejar { Termino = NuevoTermino, Definicion = NuevaDefinicion });
            NuevoTermino = string.Empty;
            NuevaDefinicion = string.Empty;
        }

        private void EliminarPar(object? parameter)
        {
            if (parameter is ParEmparejar par)
                ParesEmparejar.Remove(par);
        }

        // --- Añadir/editar pregunta ---
        private void AnadirPregunta()
        {
            Question? pregunta = null;

            switch (_tipoPanelActivo)
            {
                case "MultipleChoice":
                    if (string.IsNullOrWhiteSpace(TextoPregunta)) return;
                    if (string.IsNullOrWhiteSpace(OpcionA) || string.IsNullOrWhiteSpace(OpcionB)) return;
                    pregunta = CrearPreguntaMultiple();
                    break;

                case "OpenQuestion":
                    if (string.IsNullOrWhiteSpace(TextoPregunta)) return;
                    pregunta = new Question
                    {
                        Type = "OpenQuestion",
                        Content = TextoPregunta,
                        OrderIndex = Preguntas.Count,
                        ImagePath = string.Empty
                    };
                    break;

                case "Ordering":
                    if (string.IsNullOrWhiteSpace(TextoPregunta)) return;
                    if (ElementosOrdenar.Count < 2) return;
                    pregunta = CrearPreguntaOrdenar();
                    break;

                case "Matching":
                    if (string.IsNullOrWhiteSpace(TextoPregunta)) return;
                    if (ParesEmparejar.Count < 2) return;
                    pregunta = CrearPreguntaEmparejar();
                    break;

                case "Break":
                    pregunta = new Question
                    {
                        Type = "Break",
                        Content = TituloPausa,
                        ImagePath = string.Empty,
                        OrderIndex = Preguntas.Count
                    };
                    // Usamos MatchPair del QuestionOption para guardar el subtítulo
                    pregunta.QuestionOptions.Add(new QuestionOption
                    {
                        Text = SubtituloPausa,
                        IsCorrect = false,
                        OrderIndex = 0,
                        MatchPair = string.Empty
                    });
                    break;

                default:
                    return;
            }

            if (_preguntaEditando != null)
            {
                // Reemplazar en la lista
                var index = Preguntas.IndexOf(_preguntaEditando);
                pregunta!.Id = _preguntaEditando.Id;
                pregunta.QuizId = _preguntaEditando.QuizId;
                Preguntas.RemoveAt(index);
                Preguntas.Insert(index, pregunta!);
                _preguntaEditando = null;
            }
            else
            {
                Preguntas.Add(pregunta!);
            }

            OnPropertyChanged(nameof(EstaEditando));
            CerrarPanel();
        }

        private Question CrearPreguntaMultiple()
        {
            return new Question
            {
                Type = "MultipleChoice",
                Content = TextoPregunta,
                OrderIndex = Preguntas.Count,
                ImagePath = string.Empty,
                QuestionOptions = new ObservableCollection<QuestionOption>
                {
                    new QuestionOption { Text = OpcionA, IsCorrect = _opcionCorrecta == "A", OrderIndex = 0, MatchPair = string.Empty },
                    new QuestionOption { Text = OpcionB, IsCorrect = _opcionCorrecta == "B", OrderIndex = 1, MatchPair = string.Empty },
                    new QuestionOption { Text = OpcionC, IsCorrect = _opcionCorrecta == "C", OrderIndex = 2, MatchPair = string.Empty },
                    new QuestionOption { Text = OpcionD, IsCorrect = _opcionCorrecta == "D", OrderIndex = 3, MatchPair = string.Empty },
                }
            };
        }

        private Question CrearPreguntaOrdenar()
        {
            var pregunta = new Question
            {
                Type = "Ordering",
                Content = TextoPregunta,
                OrderIndex = Preguntas.Count,
                ImagePath = string.Empty
            };
            for (int i = 0; i < ElementosOrdenar.Count; i++)
            {
                pregunta.QuestionOptions.Add(new QuestionOption
                {
                    Text = ElementosOrdenar[i],
                    IsCorrect = false,
                    OrderIndex = i, // el índice correcto es el orden actual
                    MatchPair = string.Empty
                });
            }
            return pregunta;
        }

        private Question CrearPreguntaEmparejar()
        {
            var pregunta = new Question
            {
                Type = "Matching",
                Content = TextoPregunta,
                OrderIndex = Preguntas.Count,
                ImagePath = string.Empty
            };
            for (int i = 0; i < ParesEmparejar.Count; i++)
            {
                pregunta.QuestionOptions.Add(new QuestionOption
                {
                    Text = ParesEmparejar[i].Termino,
                    MatchPair = ParesEmparejar[i].Definicion,
                    IsCorrect = false,
                    OrderIndex = i
                });
            }
            return pregunta;
        }

        private void EliminarPregunta(object? parameter)
        {
            if (parameter is Question q)
                Preguntas.Remove(q);
        }

        private void EditarPregunta(object? parameter)
        {
            if (parameter is not Question pregunta) return;

            _preguntaEditando = pregunta;
            _tipoPanelActivo = pregunta.Type;
            TextoPregunta = pregunta.Content ?? string.Empty;

            var opciones = pregunta.QuestionOptions.ToList();

            switch (pregunta.Type)
            {
                case "MultipleChoice":
                    OpcionA = opciones.ElementAtOrDefault(0)?.Text ?? string.Empty;
                    OpcionB = opciones.ElementAtOrDefault(1)?.Text ?? string.Empty;
                    OpcionC = opciones.ElementAtOrDefault(2)?.Text ?? string.Empty;
                    OpcionD = opciones.ElementAtOrDefault(3)?.Text ?? string.Empty;
                    CorrectaA = opciones.ElementAtOrDefault(0)?.IsCorrect ?? true;
                    CorrectaB = opciones.ElementAtOrDefault(1)?.IsCorrect ?? false;
                    CorrectaC = opciones.ElementAtOrDefault(2)?.IsCorrect ?? false;
                    CorrectaD = opciones.ElementAtOrDefault(3)?.IsCorrect ?? false;
                    break;

                case "Ordering":
                    ElementosOrdenar.Clear();
                    foreach (var op in opciones.OrderBy(o => o.OrderIndex))
                        ElementosOrdenar.Add(op.Text);
                    break;

                case "Matching":
                    ParesEmparejar.Clear();
                    foreach (var op in opciones)
                        ParesEmparejar.Add(new ParEmparejar { Termino = op.Text, Definicion = op.MatchPair ?? string.Empty });
                    break;

                case "Break":
                    TituloPausa = pregunta.Content ?? string.Empty;
                    SubtituloPausa = opciones.FirstOrDefault()?.Text ?? string.Empty;
                    break;
            }

            NotificarPaneles();
            OnPropertyChanged(nameof(EstaEditando));
            PanelVisible = true;
        }

        // --- Cargar quiz existente ---
        private async Task CargarQuizExistente(int id)
        {
            using var service = new ServiceQuiz();
            var quiz = await service.Listar(id);
            if (quiz == null) return;

            _quizId = quiz.Id;
            Titulo = quiz.Title;
            Descripcion = quiz.Description ?? string.Empty;
            TituloVista = "Editar Quiz";

            foreach (var pregunta in quiz.Questions.OrderBy(q => q.OrderIndex))
                Preguntas.Add(pregunta);
        }

        // --- Guardar ---
        private async Task Guardar()
        {
            if (string.IsNullOrWhiteSpace(Titulo)) return;

            // Actualizar OrderIndex según posición actual
            for (int i = 0; i < Preguntas.Count; i++)
                Preguntas[i].OrderIndex = i;

            using var service = new ServiceQuiz();

            if (_quizId.HasValue)
            {
                var quiz = new Quiz
                {
                    Id = _quizId.Value,
                    Title = Titulo,
                    Description = Descripcion,
                    UserId = ServiceAuth.UsuarioActivo!.Id
                };
                await service.Actualizar(quiz, Preguntas.ToList());
            }
            else
            {
                var quiz = new Quiz
                {
                    Title = Titulo,
                    Description = Descripcion,
                    UserId = ServiceAuth.UsuarioActivo!.Id
                };
                foreach (var p in Preguntas)
                    quiz.Questions.Add(p);

                await service.Insertar(quiz);
            }

            _volverAPrincipal(null);
        }

        private void SubirPregunta(object? parameter)
        {
            if (parameter is not Question pregunta) return;
            var index = Preguntas.IndexOf(pregunta);
            if (index <= 0) return;
            Preguntas.Move(index, index - 1);
        }

        private void BajarPregunta(object? parameter)
        {
            if (parameter is not Question pregunta) return;
            var index = Preguntas.IndexOf(pregunta);
            if (index >= Preguntas.Count - 1) return;
            Preguntas.Move(index, index + 1);
        }

        private void SubirElementoOrdenar(object? parameter)
        {
            if (parameter is not string elemento) return;
            var index = ElementosOrdenar.IndexOf(elemento);
            if (index <= 0) return;
            ElementosOrdenar.Move(index, index - 1);
        }

        private void BajarElementoOrdenar(object? parameter)
        {
            if (parameter is not string elemento) return;
            var index = ElementosOrdenar.IndexOf(elemento);
            if (index >= ElementosOrdenar.Count - 1) return;
            ElementosOrdenar.Move(index, index + 1);
        }
    }


    // Clase auxiliar para los pares de emparejar
    public class ParEmparejar
    {
        public string Termino { get; set; } = string.Empty;
        public string Definicion { get; set; } = string.Empty;
    }
}