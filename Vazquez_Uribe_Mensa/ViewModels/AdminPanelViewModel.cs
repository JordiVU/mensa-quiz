using BCrypt.Net;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vazquez_Uribe_Mensa.Models;
using Vazquez_Uribe_Mensa.Services;
using Vazquez_Uribe_Mensa.ViewModels.Base;

namespace Vazquez_Uribe_Mensa.ViewModels
{
    public class AdminPanelViewModel : BaseViewModel
    {
        private ObservableCollection<User> _usuarios = new();
        private User? _usuarioEditando;

        // --- Formulario ---
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _role = "User";
        private bool _isActive = true;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        // Para saber si estamos editando o creando
        private string _tituloFormulario = "Nuevo Usuario";
        public string TituloFormulario
        {
            get => _tituloFormulario;
            set => SetProperty(ref _tituloFormulario, value);
        }

        public bool EstaEditando => _usuarioEditando != null;

        // Roles disponibles
        public string[] Roles { get; } = { "User", "Admin" };

        // --- Lista ---
        public ObservableCollection<User> Usuarios
        {
            get => _usuarios;
            set => SetProperty(ref _usuarios, value);
        }

        // --- Comandos ---
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EditarUsuarioCommand { get; }
        public ICommand EliminarUsuarioCommand { get; }

        public AdminPanelViewModel()
        {
            GuardarCommand = new RelayCommand(async o => await Guardar(o));
            CancelarCommand = new RelayCommand(_ => LimpiarFormulario());
            EditarUsuarioCommand = new RelayCommand(EditarUsuario);
            EliminarUsuarioCommand = new RelayCommand(async o => await EliminarUsuario(o));
            _ = CargarUsuarios();
        }

        private async Task CargarUsuarios()
        {
            using var service = new ServiceUser();
            var lista = await service.Listar();
            Usuarios = new ObservableCollection<User>(lista);
        }

        private void EditarUsuario(object? parameter)
        {
            if (parameter is not User usuario) return;

            _usuarioEditando = usuario;
            Username = usuario.Username;
            Password = string.Empty; // no mostramos el hash
            Role = usuario.Role;
            IsActive = usuario.IsActive;
            TituloFormulario = $"Editando: {usuario.Username}";
            OnPropertyChanged(nameof(EstaEditando));
        }

        private async Task Guardar(object? parameter = null)
        {
            if (parameter is PasswordBox pb)
                Password = pb.Password;

            if (string.IsNullOrWhiteSpace(Username)) return;

            using var service = new ServiceUser();

            if (_usuarioEditando != null)
            {
                // EDITAR
                _usuarioEditando.Username = Username;
                _usuarioEditando.Role = Role;
                _usuarioEditando.IsActive = IsActive;

                // Solo actualizar contraseña si se escribió una nueva
                if (!string.IsNullOrWhiteSpace(Password))
                    _usuarioEditando.Password = BCrypt.Net.BCrypt.HashPassword(Password);

                await service.Actualizar(_usuarioEditando);
            }
            else
            {
                // CREAR — contraseña obligatoria
                if (string.IsNullOrWhiteSpace(Password)) return;

                var usuario = new User
                {
                    Username = Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(Password),
                    Role = Role,
                    IsActive = IsActive,
                    CreatedAt = DateTime.Now
                };

                await service.Insertar(usuario);
            }

            LimpiarFormulario();
            await CargarUsuarios();
        }

        private async Task EliminarUsuario(object? parameter)
        {
            if (parameter is not User usuario) return;

            // No permitir eliminar el propio usuario activo
            if (usuario.Id == ServiceAuth.UsuarioActivo?.Id)
            {
                MessageBox.Show("No puedes eliminar tu propio usuario.",
                    "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Seguro que quieres eliminar a \"{usuario.Username}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            using var service = new ServiceUser();
            await service.Borrar(usuario.Id);
            Usuarios.Remove(usuario);
        }

        private void LimpiarFormulario()
        {
            _usuarioEditando = null;
            Username = string.Empty;
            Password = string.Empty;
            Role = "User";
            IsActive = true;
            TituloFormulario = "Nuevo Usuario";
            OnPropertyChanged(nameof(EstaEditando));
        }
    }
}