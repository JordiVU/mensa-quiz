using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vazquez_Uribe_Mensa.ViewModels;
using Colors = QuestPDF.Helpers.Colors;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace Vazquez_Uribe_Mensa.Documents
{
    public class HistorialDocument : IDocument
    {
        private readonly List<SesionResumen> _sesiones;
        private readonly int _totalQuizzes;
        private readonly int _totalParticipantes;
        private readonly int _totalSesiones;
        private readonly double _tiempoMedio;
        private readonly string _titulo;

        // Colores corporativos
        private static readonly string NaranjaHex = "#FFA000";
        private static readonly string OscuroHex = "#0D111F";
        private static readonly string GrisHex = "#6B7280";
        private static readonly string GrisClaro = "#F3F4F6";

        public HistorialDocument(List<SesionResumen> sesiones, int totalQuizzes,
            int totalParticipantes, int totalSesiones, double tiempoMedio, string titulo)
        {
            _sesiones = sesiones;
            _totalQuizzes = totalQuizzes;
            _totalParticipantes = totalParticipantes;
            _totalSesiones = totalSesiones;
            _tiempoMedio = tiempoMedio;
            _titulo = titulo;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(OscuroHex));

                // ENCABEZADO con fondo oscuro
                page.Header().Column(col =>
                {
                    col.Item().Background(OscuroHex).Padding(30).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("MENSA QUIZ")
                                .FontSize(22).Bold().FontColor(NaranjaHex);
                            c.Item().PaddingTop(4).Text(_titulo)
                                .FontSize(13).FontColor(Colors.White);
                        });
                        row.ConstantItem(160).AlignRight().AlignMiddle().Column(c =>
                        {
                            c.Item().AlignRight().Text("Informe de Actividad")
                                .FontSize(11).FontColor(Colors.White).Bold();
                            c.Item().AlignRight().PaddingTop(4)
                                .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9).FontColor(GrisHex);
                        });
                    });

                    // Línea naranja decorativa
                    col.Item().Height(4).Background(NaranjaHex);
                });

                page.Content().Padding(30).Column(col =>
                {
                    // --- TARJETAS ESTADÍSTICAS ---
                    col.Item().PaddingBottom(15).Text("Resumen General")
                        .FontSize(13).Bold().FontColor(OscuroHex);

                    col.Item().PaddingBottom(25).Row(row =>
                    {
                        TarjetaStat(row.RelativeItem(), "&#x1F4DA;", "Quizzes", _totalQuizzes.ToString());
                        row.ConstantItem(12);
                        TarjetaStat(row.RelativeItem(), "&#x1F3AF;", "Sesiones", _totalSesiones.ToString());
                        row.ConstantItem(12);
                        TarjetaStat(row.RelativeItem(), "&#x1F465;", "Participantes", _totalParticipantes.ToString());
                        row.ConstantItem(12);
                        TarjetaStat(row.RelativeItem(), "&#x23F1;", "Tiempo Medio", $"{_tiempoMedio} min");
                    });

                    // --- SEPARADOR ---
                    col.Item().PaddingBottom(15).Row(row =>
                    {
                        row.RelativeItem().Height(1).Background(NaranjaHex);
                    });

                    // --- TABLA SESIONES ---
                    col.Item().PaddingBottom(12).Text("Detalle de Sesiones")
                        .FontSize(13).Bold().FontColor(OscuroHex);

                    if (!_sesiones.Any())
                    {
                        col.Item().Background(GrisClaro).Padding(20).AlignCenter()
                            .Text("No hay sesiones registradas.")
                            .FontColor(GrisHex).Italic();
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            // Cabecera tabla
                            table.Header(header =>
                            {
                                header.Cell().Element(CeldaCabecera).Text("Quiz");
                                header.Cell().Element(CeldaCabecera).Text("Fecha");
                                header.Cell().Element(CeldaCabecera).Text("Participantes");
                                header.Cell().Element(CeldaCabecera).Text("Duración");
                            });

                            // Filas alternadas
                            bool par = false;
                            foreach (var s in _sesiones)
                            {
                                var fondo = par ? GrisClaro : "#FFFFFF";
                                par = !par;

                                table.Cell().Element(c => CeldaFila(c, fondo))
                                    .Text(s.TituloQuiz).Bold();
                                table.Cell().Element(c => CeldaFila(c, fondo))
                                    .Text(s.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Element(c => CeldaFila(c, fondo))
                                    .Text($"{s.Participantes} personas");
                                table.Cell().Element(c => CeldaFila(c, fondo))
                                    .Text($"{s.Duracion} min");
                            }
                        });
                    }
                });

                // PIE DE PÁGINA
                page.Footer().BorderTop(1).BorderColor(GrisClaro)
                    .Padding(15).Row(row =>
                    {
                        row.RelativeItem().AlignLeft()
                            .Text("Mensa Quiz — Informe confidencial")
                            .FontSize(8).FontColor(GrisHex);
                        row.RelativeItem().AlignRight()
                            .Text($"Página 1 de 1")
                            .FontSize(8).FontColor(GrisHex);
                    });
            });
        }

        // Tarjeta de estadística
        private void TarjetaStat(IContainer container, string emoji, string etiqueta, string valor)
        {
            container
                .Border(1).BorderColor(GrisClaro)
                .Background(Colors.White)
                .Padding(15)
                .Column(c =>
                {
                    c.Item().PaddingBottom(6).Text(etiqueta)
                        .FontSize(9).FontColor(GrisHex).Bold();
                    c.Item().Text(valor)
                        .FontSize(20).Bold().FontColor(NaranjaHex);
                });
        }

        // Celda cabecera tabla
        private IContainer CeldaCabecera(IContainer container)
        {
            return container
                .Background(OscuroHex)
                .Padding(8)
                .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));
        }

        // Celda fila tabla
        private IContainer CeldaFila(IContainer container, string fondo)
        {
            return container
                .Background(fondo)
                .BorderBottom(0.5f)
                .BorderColor(GrisClaro)
                .Padding(8);
        }
    }
}