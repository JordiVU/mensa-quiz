using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vazquez_Uribe_Mensa.Models;
using Vazquez_Uribe_Mensa.ViewModels.Base;

namespace Vazquez_Uribe_Mensa.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        // Objeto privado para almacenar la vista actual
        private object _currentView;
        
        private bool _isLoggedIn;

        private User _usuario;

        public User Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        // Propiedad publica para controlar que vista se muestra en la ventana
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Definimos los comandos para navegar entre las vistas
        public ICommand ShowPrincipalCommand { get; }
        public ICommand ShowHistorialCommand { get; }
        public ICommand ShowAdminPanelCommand { get; }
        public ICommand ShowLoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowQuizPlayerCommand { get; }
        public ICommand ShowQuizBuilderCommand { get; }

        public MainWindowViewModel()
        {
            // Inicializamos los comandos de navegacion
            ShowPrincipalCommand = new RelayCommand(CargarVistaPrincipal);
            ShowHistorialCommand = new RelayCommand(CargarVistaHistorial);
            ShowAdminPanelCommand = new RelayCommand(CargarVistaAdminPanel);
            ShowLoginCommand = new RelayCommand(CargarVistaLogin);
            LogoutCommand = new RelayCommand(CargarLogout);
            ShowQuizBuilderCommand = new RelayCommand(CargarVistaQuizBuilder);
            ShowQuizPlayerCommand = new RelayCommand(CargarVistaQuizPlayer);

            // Cargamos el Login al arrancar
            CargarVistaLogin();
        }

        // Metodo para cargar la Vista Principal
        private void CargarVistaPrincipal(object? obj = null)
        {
            CurrentView = new PrincipalViewModel(Usuario, CargarVistaQuizBuilder, CargarVistaQuizPlayer);
        }


        // Metodo para cargar la Vista de Historial
        private void CargarVistaHistorial(object? obj = null)
        {
            CurrentView = new HistorialViewModel();
        }

        // Metodo para cargar la Vista del Panel Admin
        private void CargarVistaAdminPanel(object? parameter = null)
        {
            CurrentView = new AdminPanelViewModel();
        }

        // Metodo para cargar la Vista para Login
        private void CargarVistaLogin(object? parameter = null)
        {
            CurrentView = new LoginViewModel(LoginExitoso);
        }
        private void CargarVistaQuizPlayer(object? obj = null)
        {
            if (obj is not (Quiz quiz, int participantes)) return;
            CurrentView = new QuizPlayerViewModel(quiz, participantes, CargarVistaPrincipal);
        }

        private void CargarVistaQuizBuilder(object? obj = null)
        {
            var quiz = obj as Quiz;
            CurrentView = new QuizBuilderViewModel(CargarVistaPrincipal, quiz);
        }

        private void LoginExitoso(User usuario)
        {
            IsLoggedIn = true;
            Usuario = usuario;
            CurrentView = new PrincipalViewModel(usuario, CargarVistaQuizBuilder, CargarVistaQuizPlayer);
        }

        private void CargarLogout(object? parameter = null)
        {
            IsLoggedIn = false;
            Usuario = null;
            CargarVistaLogin();
        }
    }
}
