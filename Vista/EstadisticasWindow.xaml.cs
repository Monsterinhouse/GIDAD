using ScottPlot;
using ScottPlot.Plottables;
using System.Windows;
using WPF_Test.Models;

namespace WPF_Test.Vista
{
    public partial class EstadisticasWindow : Window
    {
        public EstadisticasWindow(IEnumerable<OrdenRow> filasOrdenesTrabajo)
        {
            InitializeComponent();
            CargarGrafico(filasOrdenesTrabajo);
        }

        private void CargarGrafico(IEnumerable<OrdenRow> filas)
        {
            // Agrupar por servicio, contando solo filas con NroOrden no vacío
            var conteo = filas
                .Where(f => !string.IsNullOrWhiteSpace(f.NroOrden)
                         && !string.IsNullOrWhiteSpace(f.Servicio))
                .GroupBy(f => f.Servicio)
                .Select(g => new { Servicio = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            var plot = PlotServicios.Plot;
            plot.Clear();

            if (conteo.Count == 0)
            {
                plot.Title("No hay datos para mostrar");
                PlotServicios.Refresh();
                return;
            }

            // Posiciones X: 0, 1, 2, ...
            double[] posiciones = Enumerable.Range(0, conteo.Count)
                .Select(i => (double)i).ToArray();
            double[] valores = conteo.Select(c => (double)c.Cantidad).ToArray();
            string[] etiquetas = conteo.Select(c => c.Servicio).ToArray();

            var barras = plot.Add.Bars(posiciones, valores);

            // Etiquetas en el eje X con el nombre del servicio
            plot.Axes.Bottom.SetTicks(posiciones, etiquetas);
            plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;

            foreach (var bar in barras.Bars)
            {
                bar.Label = bar.Value.ToString();
            }

            barras.ValueLabelStyle.Bold = true;
            barras.ValueLabelStyle.FontSize = 16;
            plot.Axes.Margins(bottom: 0, top: .2);

            plot.Title("Servicios realizados");
            plot.Axes.Left.Label.Text = "Cantidad";
            plot.Axes.Bottom.Label.Text = "Tipo de servicio";

            // Margen inferior extra para que entren las etiquetas rotadas
            //plot.Layout.Frameless();
            plot.Layout.Default();

            PlotServicios.Refresh();
        }
    }
}