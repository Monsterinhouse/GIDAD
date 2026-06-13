using System.Windows;
using Microsoft.Win32;

namespace WPF_Test.Vista
{
    public partial class OpcionesWindow : Window
    {
        public int IntervaloMinutos { get; private set; }
        public string RutaElegida { get; private set; }

        // Acción inyectada desde MainWindow para forzar guardado en BD
        private readonly Func<string, Task> _guardarDefinitivo;

        public OpcionesWindow(int intervaloActual, Func<string, Task> guardarDefinitivo)
        {
            InitializeComponent();
            _guardarDefinitivo = guardarDefinitivo;
            TxtIntervalo.Text = intervaloActual.ToString();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtIntervalo.Text, out var min) || min < 1)
            {
                MessageBox.Show("Ingresá un número entero mayor a 0.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IntervaloMinutos = min;
            DialogResult = true;
        }

        private async void BtnGuardarAhora_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(RutaElegida))
            {
                MessageBox.Show(
                    "Seleccione una carpeta.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            await _guardarDefinitivo(RutaElegida);
            MessageBox.Show("Guardado en base de datos correctamente.", "GIDAD",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnElegirCarpeta_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog
            {
                Title = "Elegir carpeta de destino para guardado definitivo"
            };
            if (dlg.ShowDialog() == true)
                RutaElegida = dlg.FolderName;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) =>
            DialogResult = false;
    }
}