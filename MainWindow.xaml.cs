using System.Collections.ObjectModel;
using System.Windows;
using WPF_Test.Vista.UserControls;

namespace WPF_Test
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> OrdenesTrabajoColumns { get; }
        public ObservableCollection<string> VentasMostradorColumns { get; }
        public ObservableCollection<string> ProveedoresColumns { get; }
        public ObservableCollection<string> VariosColumns { get; }

        public MainWindow()
        {
            InitializeComponent();

            // Inicializar columnas para cada tipo de vista
            OrdenesTrabajoColumns = new ObservableCollection<string>
            {
                "N° DE ORDEN", "SERVICIO", "EFECTIVO", "SEÑA", "TARJETA", "TRANSFERENCIA"
            };

            VentasMostradorColumns = new ObservableCollection<string>
            {
                "N° DE COMPROBANTE", "EFECTIVO", "SEÑA", "TARJETA", "TRANSFERENCIA" 
            };

            ProveedoresColumns = new ObservableCollection<string>
            {
                "N° DE FACTURA", "MONTO", "MEDIO DE PAGO"
            };

            VariosColumns = new ObservableCollection<string>
            {
                "N° DE COMPROBANTE", "MONTO", "MEDIO DE PAGO"
            };

            DataContext = this;
        }

        private void BtnOrdenesTrabajoClick(object sender, RoutedEventArgs e)
        {
            OrdenesTrabajoGrid.Visibility = Visibility.Visible;
            VentasMostradorGrid.Visibility = Visibility.Collapsed;
            ProveedoresGrid.Visibility = Visibility.Collapsed;
            VariosGrid.Visibility = Visibility.Collapsed;

            GB_Default.Visibility = Visibility.Collapsed;
            GB_OrdenTrabajo.Visibility = Visibility.Visible;
            GB_VentasMostrador.Visibility = Visibility.Collapsed;
            GB_Proveedores.Visibility = Visibility.Collapsed;
            GB_Varios.Visibility = Visibility.Collapsed;
        }

        private void BtnVentasMostradorClick(object sender, RoutedEventArgs e)
        {
            OrdenesTrabajoGrid.Visibility = Visibility.Collapsed;
            VentasMostradorGrid.Visibility = Visibility.Visible;
            ProveedoresGrid.Visibility = Visibility.Collapsed;
            VariosGrid.Visibility = Visibility.Collapsed;

            GB_Default.Visibility = Visibility.Collapsed;
            GB_OrdenTrabajo.Visibility = Visibility.Collapsed;
            GB_VentasMostrador.Visibility = Visibility.Visible;
            GB_Proveedores.Visibility = Visibility.Collapsed;
            GB_Varios.Visibility = Visibility.Collapsed;
        }

        private void BtnProveedoresClick(object sender, RoutedEventArgs e)
        {
            OrdenesTrabajoGrid.Visibility = Visibility.Collapsed;
            VentasMostradorGrid.Visibility = Visibility.Collapsed;
            ProveedoresGrid.Visibility = Visibility.Visible;
            VariosGrid.Visibility = Visibility.Collapsed;

            GB_Default.Visibility = Visibility.Collapsed;
            GB_OrdenTrabajo.Visibility = Visibility.Collapsed;
            GB_VentasMostrador.Visibility = Visibility.Collapsed;
            GB_Proveedores.Visibility = Visibility.Visible;
            GB_Varios.Visibility = Visibility.Collapsed;
        }

        private void BtnVariosClick(object sender, RoutedEventArgs e)
        {
            OrdenesTrabajoGrid.Visibility = Visibility.Collapsed;
            VentasMostradorGrid.Visibility = Visibility.Collapsed;
            ProveedoresGrid.Visibility = Visibility.Collapsed;
            VariosGrid.Visibility = Visibility.Visible;

            GB_Default.Visibility = Visibility.Collapsed;
            GB_OrdenTrabajo.Visibility = Visibility.Collapsed;
            GB_VentasMostrador.Visibility = Visibility.Collapsed;
            GB_Proveedores.Visibility = Visibility.Collapsed;
            GB_Varios.Visibility = Visibility.Visible;
        }

        public List<string> ServiciosDisponibles { get; } = new()
        {
            "1° Service",
            "2° Service",
            "3° Service",
            "4° Service",
            "Reparacion Particular"
        };


    }
}