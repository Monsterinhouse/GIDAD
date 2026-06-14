using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPF_Test.Models;
using WPF_Test.Services;
using static WPF_Test.Services.DatabaseService;

namespace WPF_Test.Vista
{
    public partial class EstadisticasWindow : Window
    {
        private readonly DatabaseService _db;
        private readonly IEnumerable<OrdenRow> _filasHoy;

        // Paleta para servicios (barras)
        private static readonly Color[] PaletaServicios =
        {
            Colors.SteelBlue, Colors.Orange, Colors.SeaGreen,
            Colors.IndianRed, Colors.MediumPurple, Colors.GoldenRod, Colors.Teal
        };

        // Colores fijos para cada serie de totales (scatter)
        private static readonly Color ColorEfectivo = Colors.SeaGreen;
        private static readonly Color ColorSeña = Colors.GoldenRod;
        private static readonly Color ColorTarjeta = Colors.SteelBlue;
        private static readonly Color ColorTransferencia = Colors.IndianRed;

        public EstadisticasWindow(DatabaseService db, IEnumerable<OrdenRow> filasOrdenesTrabajoHoy)
        {
            InitializeComponent();
            _db = db;
            _filasHoy = filasOrdenesTrabajoHoy;

            CargarHoy();
        }

        // ── Botones ──
        private void BtnHoy_Click(object sender, RoutedEventArgs e) => CargarHoy();

        private async void Btn7Dias_Click(object sender, RoutedEventArgs e)
            => await CargarPeriodoAsync(DateTime.Today.AddDays(-7), "Últimos 7 días");

        private async void Btn30Dias_Click(object sender, RoutedEventArgs e)
            => await CargarPeriodoAsync(DateTime.Today.AddDays(-30), "Último mes");

        // ── Vista "Hoy": usa las filas actuales en memoria ──
        private void CargarHoy()
        {
            TxtTitulo.Text = "Servicios realizados — Hoy";

            var conteo = _filasHoy
                .Where(f => !string.IsNullOrWhiteSpace(f.NroOrden)
                         && !string.IsNullOrWhiteSpace(f.Servicio))
                .GroupBy(f => f.Servicio)
                .ToDictionary(g => g.Key, g => g.Count());

            GraficarServicios(conteo);

            // Para "hoy" no hay histórico de totales por día; limpiar el scatter
            PlotTotales.Plot.Clear();
            PlotTotales.Plot.Title("Sin datos históricos para 'Hoy' — usá 7 días o 1 mes");
            PlotTotales.Refresh();
        }

        // ── Vista 7 días / 30 días: consulta a la BD ──
        private async Task CargarPeriodoAsync(DateTime desde, string etiqueta)
        {
            TxtTitulo.Text = $"Servicios realizados — {etiqueta}";

            var conteo = await _db.ObtenerConteoServiciosDesdeAsync(desde);
            GraficarServicios(conteo);

            var totales = await _db.ObtenerTotalesDiariosDesdeAsync(desde);
            GraficarTotales(totales, etiqueta);
        }

        // ── Gráfico de barras: cantidad por servicio ──
        private void GraficarServicios(Dictionary<string, int> conteo)
        {
            var plot = PlotServicios.Plot;
            plot.Clear();

            if (conteo.Count == 0)
            {
                plot.Title("No hay datos para mostrar");
                PlotServicios.Refresh();
                return;
            }

            var ordenado = conteo
                .OrderByDescending(kv => kv.Value)
                .ToList();

            double[] posiciones = Enumerable.Range(0, ordenado.Count)
                .Select(i => (double)i).ToArray();
            double[] valores = ordenado.Select(c => (double)c.Value).ToArray();
            string[] etiquetas = ordenado.Select(c => c.Key).ToArray();

            var barras = plot.Add.Bars(posiciones, valores);

            // Un color distinto por servicio
            for (int i = 0; i < barras.Bars.Count; i++)
                barras.Bars[i].FillColor = PaletaServicios[i % PaletaServicios.Length];

            foreach (var bar in barras.Bars)
                bar.Label = bar.Value.ToString();

            barras.ValueLabelStyle.Bold = true;
            barras.ValueLabelStyle.FontSize = 16;

            plot.Axes.Bottom.SetTicks(posiciones, etiquetas);
            plot.Axes.Margins(bottom: 0, top: .2);

            plot.Title("Servicios realizados");
            plot.Axes.Left.Label.Text = "Cantidad";
            plot.Axes.Bottom.Label.Text = "Tipo de servicio";

            plot.Layout.Default();
            PlotServicios.Refresh();
        }

        // ── Scatter plot: evolución diaria de cada total ──
        private void GraficarTotales(List<TotalDiaDto> totales, string etiqueta)
        {
            var plot = PlotTotales.Plot;
            plot.Clear();

            if (totales.Count == 0)
            {
                plot.Title("No hay datos históricos para este período");
                PlotTotales.Refresh();
                return;
            }

            double[] xs = totales.Select(t => t.Fecha.ToOADate()).ToArray();

            AgregarSerie(plot, xs, totales.Select(t => (double)t.Efectivo).ToArray(),
                "Efectivo", ColorEfectivo);
            AgregarSerie(plot, xs, totales.Select(t => (double)t.Seña).ToArray(),
                "Seña", ColorSeña);
            AgregarSerie(plot, xs, totales.Select(t => (double)t.Tarjeta).ToArray(),
                "Tarjeta", ColorTarjeta);
            AgregarSerie(plot, xs, totales.Select(t => (double)t.Transferencia).ToArray(),
                "Transferencia", ColorTransferencia);

            plot.Title($"Totales diarios — {etiqueta}");
            plot.Axes.Left.Label.Text = "Monto ($)";
            plot.Axes.Bottom.Label.Text = "Fecha";

            // Eje X como fechas
            plot.Axes.DateTimeTicksBottom();

            plot.ShowLegend();
            plot.Layout.Default();
            PlotTotales.Refresh();
        }

        private static void AgregarSerie(Plot plot, double[] xs, double[] ys, string nombre, Color color)
        {
            var scatter = plot.Add.Scatter(xs, ys);
            scatter.LegendText = nombre;
            scatter.Color = color;
            scatter.LineWidth = 2;
            scatter.MarkerSize = 6;
        }
    }
}