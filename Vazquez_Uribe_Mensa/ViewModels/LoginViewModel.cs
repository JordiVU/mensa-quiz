using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Vazquez_Uribe_Mensa.Models;
using Vazquez_Uribe_Mensa.Services;
using Vazquez_Uribe_Mensa.ViewModels.Base;

namespace Vazquez_Uribe_Mensa.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private readonly Action<User> _loginExitoso;

        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }
        public ICommand LoginCommand { get; }

        public LoginViewModel(Action<User> loginExitoso)
        {
            _loginExitoso = loginExitoso;
            LoginCommand = new RelayCommand(PerformLogin);
        }

        private async void PerformLogin(object? parameter = null)
        {
            if (parameter is PasswordBox pb)
                Password = pb.Password;

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                var service = new ServiceAuth();
                var usuario = await service.Login(Username, Password);

                if (usuario != null)
                {
                    _loginExitoso(usuario);
                }
            }
        }
    }
}
