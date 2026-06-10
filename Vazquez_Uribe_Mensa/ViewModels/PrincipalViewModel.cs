using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Vazquez_Uribe_Mensa.Models;
using Vazquez_Uribe_Mensa.Services;
using Vazquez_Uribe_Mensa.ViewModels.Base;

namespace Vazquez_Uribe_Mensa.ViewModels
{
    public class PrincipalViewModel : BaseViewModel
    {
        private readonly Action<object?> _navegarBuilder;
        private ObservableCollection<Quiz> _quizzes = new();
        private readonly Action<object?> _navegarPlayer;

        public User UsuarioActivo { get; }

        public ObservableCollection<Quiz> Quizzes
        {
            get => _quizzes;
            set => SetProperty(ref _quizzes, value);
        }

        public ICommand CrearQuizCommand { get; }
        public ICommand EditarQuizCommand { get; }
        public ICommand BorrarQuizCommand { get; }
        public ICommand IniciarQuizCommand { get; }
        public ICommand DuplicarQuizCommand { get; }


        public PrincipalViewModel(User usuario, Action<object?> navegarBuilder, Action<object?> navegarPlayer)
        {
            UsuarioActivo = usuario;
            _navegarPlayer = navegarPlayer;
            _navegarBuilder = navegarBuilder;
            CrearQuizCommand = new RelayCommand(_ => _navegarBuilder(null));
            EditarQuizCommand = new RelayCommand(EditarQuiz);
            BorrarQuizCommand = new RelayCommand(async o => await BorrarQuiz(o));
            IniciarQuizCommand = new RelayCommand(IniciarQuiz);
            DuplicarQuizCommand = new RelayCommand(async o => await DuplicarQuiz(o));

            _ = CargarQuizzes();
        }

        private async Task CargarQuizzes()
        {
            using var service = new ServiceQuiz();
            var lista = await service.ListarPorUsuario(UsuarioActivo.Id);
            Quizzes = new ObservableCollection<Quiz>(lista);
        }

        private void EditarQuiz(object? parameter)
        {
            if (parameter is Quiz quiz)
                _navegarBuilder(quiz);
        }

        private void IniciarQuiz(object? parameter)
        {
            if (parameter is not Quiz quiz) return;

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "¿Cuántos participantes hay?",
                "Iniciar Quiz",
                "0");

            if (!int.TryParse(input, out int participantes) || participantes < 0) return;

            _navegarPlayer((quiz, participantes));
        }

        private async Task BorrarQuiz(object? parameter)
        {
            if (parameter is not Quiz quiz) return;

            var resultado = MessageBox.Show(
                $"¿Seguro que quieres eliminar \"{quiz.Title}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            using var service = new ServiceQuiz();
            await service.Borrar(quiz.Id);
            Quizzes.Remove(quiz);
        }

        private async Task DuplicarQuiz(object? parameter)
        {
            if (parameter is not Quiz quiz) return;

            // Cargar el quiz completo con preguntas y opciones
            using var service = new ServiceQuiz();
            var quizCompleto = await service.Listar(quiz.Id);
            if (quizCompleto == null) return;

            var quizNuevo = new Quiz
            {
                Title = $"{quizCompleto.Title} (copia)",
                Description = quizCompleto.Description,
                UserId = UsuarioActivo.Id
            };

            foreach (var pregunta in quizCompleto.Questions.OrderBy(q => q.OrderIndex))
            {
                var preguntaNueva = new Question
                {
                    Type = pregunta.Type,
                    Content = pregunta.Content,
                    ImagePath = pregunta.ImagePath,
                    OrderIndex = pregunta.OrderIndex
                };

                foreach (var opcion in pregunta.QuestionOptions)
                {
                    preguntaNueva.QuestionOptions.Add(new QuestionOption
                    {
                        Text = opcion.Text,
                        IsCorrect = opcion.IsCorrect,
                        OrderIndex = opcion.OrderIndex,
                        MatchPair = opcion.MatchPair
                    });
                }

                quizNuevo.Questions.Add(preguntaNueva);
            }

            await service.Insertar(quizNuevo);
            await CargarQuizzes();
        }
    }
}