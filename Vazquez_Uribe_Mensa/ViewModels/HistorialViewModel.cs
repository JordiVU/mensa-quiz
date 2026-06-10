using Microsoft.Win32;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Vazquez_Uribe_Mensa.Documents;
using Vazquez_Uribe_Mensa.Models;
using Vazquez_Uribe_Mensa.Services;
using Vazquez_Uribe_Mensa.ViewModels.Base;

namespace Vazquez_Uribe_Mensa.ViewModels
{
    public class HistorialViewModel : BaseViewModel
    {
        private bool _esAdmin;

        private int _totalQuizzes;
        private int _totalParticipantes;
        private int _totalSesiones;
        private double _tiempoMedio;
        public bool EsAdmin => _esAdmin;

        public int TotalQuizzes
        {
            get => _totalQuizzes;
            set => SetProperty(ref _totalQuizzes, value);
        }
        public int TotalParticipantes
        {
            get => _totalParticipantes;
            set => SetProperty(ref _totalParticipantes, value);
        }
        public int TotalSesiones
        {
            get => _totalSesiones;
            set => SetProperty(ref _totalSesiones, value);
        }
        public double TiempoMedio
        {
            get => _tiempoMedio;
            set => SetProperty(ref _tiempoMedio, value);
        }

        private string _titulo = "Historial";
        public string Titulo
        {
            get => _titulo;
            set => SetProperty(ref _titulo, value);
        }

        public ObservableCollection<SesionResumen> Sesiones { get; } = new();
        public ObservableCollection<DatoMes> DatosPorMes { get; } = new();

        public ICommand GenerarPdfCommand { get; }

        public HistorialViewModel()
        {
            _esAdmin = ServiceAuth.UsuarioActivo?.Role == "Admin";
            Titulo = _esAdmin ? "Historial Global" : "Mi Historial";
            GenerarPdfCommand = new RelayCommand(_ => GenerarPdf());
            _ = CargarDatos();
        }

        private async Task CargarDatos()
        {
            using var serviceSesion = new ServiceSession();
            using var serviceQuiz = new ServiceQuiz();

            List<QuizSession> sesiones;
            List<Quiz> quizzes;

            if (_esAdmin)
            {
                sesiones = await serviceSesion.Listar();
                quizzes = await serviceQuiz.Listar();
            }
            else
            {
                var userId = ServiceAuth.UsuarioActivo!.Id;
                sesiones = await serviceSesion.ListarPorUsuario(userId);
                quizzes = await serviceQuiz.ListarPorUsuario(userId);
            }

            TotalQuizzes = quizzes.Count;
            TotalSesiones = sesiones.Count;
            TotalParticipantes = sesiones.Sum(s => s.ParticipantCount);
            TiempoMedio = sesiones.Any()
                ? Math.Round(sesiones.Average(s => s.DurationMinutes), 1)
                : 0;

            Sesiones.Clear();
            foreach (var s in sesiones.OrderByDescending(s => s.StartedAt))
            {
                Sesiones.Add(new SesionResumen
                {
                    TituloQuiz = s.Quiz?.Title ?? "Quiz eliminado",
                    Fecha = s.StartedAt,
                    Participantes = s.ParticipantCount,
                    Duracion = s.DurationMinutes
                });
            }

            // Gráfica
            var anyo = DateTime.Now.Year;
            var datosBrutos = new List<DatoMes>();
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun",
                                 "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            for (int i = 1; i <= 12; i++)
            {
                var count = sesiones.Count(s => s.StartedAt.Year == anyo && s.StartedAt.Month == i);
                datosBrutos.Add(new DatoMes { Mes = meses[i - 1], Valor = count });
            }

            int max = datosBrutos.Max(d => d.Valor);
            foreach (var dato in datosBrutos)
                dato.Altura = max == 0 ? 4.0 : Math.Max(4.0, (dato.Valor / (double)max) * 120);

            DatosPorMes.Clear();
            foreach (var dato in datosBrutos)
                DatosPorMes.Add(dato);
        }

        private void GenerarPdf()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var dialog = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Historial_MensaQuiz_{DateTime.Now:dd-MM-yyyy_HH-mm}.pdf"
            };

            if (dialog.ShowDialog() != true) return;

            var doc = new HistorialDocument(
                Sesiones.ToList(),
                TotalQuizzes,
                TotalParticipantes,
                TotalSesiones,
                TiempoMedio,
                Titulo);

            doc.GeneratePdf(dialog.FileName);
        }
    }

    public class SesionResumen
    {
        public string TituloQuiz { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int Participantes { get; set; }
        public int Duracion { get; set; }
    }

    public class DatoMes
    {
        public string Mes { get; set; } = string.Empty;
        public int Valor { get; set; }
        public double Altura { get; set; }
    }
}