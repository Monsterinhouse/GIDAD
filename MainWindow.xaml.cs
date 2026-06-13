using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using WPF_Test.Models;
using WPF_Test.Services;
using WPF_Test.Vista;
using WPF_Test.Vista.UserControls;

namespace WPF_Test
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ── Columnas ──
        public ObservableCollection<string> OrdenesTrabajoColumns { get; }
        public ObservableCollection<string> VentasMostradorColumns { get; }
        public ObservableCollection<string> ProveedoresColumns { get; }
        public ObservableCollection<string> VariosColumns { get; }
        public List<string> ServiciosDisponibles { get; }
        public List<string> MediosPagoDisponibles { get; }

        // ── Totales: Órdenes de Trabajo ──
        private decimal _otEfectivo, _otSeña, _otTarjeta, _otTransferencia;
        public decimal OT_Efectivo { get => _otEfectivo; set { _otEfectivo = value; OnPropertyChanged(nameof(OT_Efectivo)); } }
        public decimal OT_Seña { get => _otSeña; set { _otSeña = value; OnPropertyChanged(nameof(OT_Seña)); } }
        public decimal OT_Tarjeta { get => _otTarjeta; set { _otTarjeta = value; OnPropertyChanged(nameof(OT_Tarjeta)); } }
        public decimal OT_Transferencia { get => _otTransferencia; set { _otTransferencia = value; OnPropertyChanged(nameof(OT_Transferencia)); } }

        // ── Totales: Ventas por Mostrador ──
        private decimal _vmEfectivo, _vmSeña, _vmTarjeta, _vmTransferencia;
        public decimal VM_Efectivo { get => _vmEfectivo; set { _vmEfectivo = value; OnPropertyChanged(nameof(VM_Efectivo)); } }
        public decimal VM_Seña { get => _vmSeña; set { _vmSeña = value; OnPropertyChanged(nameof(VM_Seña)); } }
        public decimal VM_Tarjeta { get => _vmTarjeta; set { _vmTarjeta = value; OnPropertyChanged(nameof(VM_Tarjeta)); } }
        public decimal VM_Transferencia { get => _vmTransferencia; set { _vmTransferencia = value; OnPropertyChanged(nameof(VM_Transferencia)); } }

        // ── Totales: Proveedores ──
        private decimal _prEfectivo, _prTransferencia;
        public decimal PR_Efectivo { get => _prEfectivo; set { _prEfectivo = value; OnPropertyChanged(nameof(PR_Efectivo)); } }
        public decimal PR_Transferencia { get => _prTransferencia; set { _prTransferencia = value; OnPropertyChanged(nameof(PR_Transferencia)); } }

        // ── Totales: Varios ──
        private decimal _vaEfectivo, _vaTransferencia;
        public decimal VA_Efectivo { get => _vaEfectivo; set { _vaEfectivo = value; OnPropertyChanged(nameof(VA_Efectivo)); } }
        public decimal VA_Transferencia { get => _vaTransferencia; set { _vaTransferencia = value; OnPropertyChanged(nameof(VA_Transferencia)); } }

        // ── Totales: Día ──
        private decimal _diaEfectivo, _diaSeña, _diaTarjeta, _diaTransferencia;
        public decimal Dia_Efectivo { get => _diaEfectivo; set { _diaEfectivo = value; OnPropertyChanged(nameof(Dia_Efectivo)); } }
        public decimal Dia_Seña { get => _diaSeña; set { _diaSeña = value; OnPropertyChanged(nameof(Dia_Seña)); } }
        public decimal Dia_Tarjeta { get => _diaTarjeta; set { _diaTarjeta = value; OnPropertyChanged(nameof(Dia_Tarjeta)); } }
        public decimal Dia_Transferencia { get => _diaTransferencia; set { _diaTransferencia = value; OnPropertyChanged(nameof(Dia_Transferencia)); } }

        // ── Caja ──
        private decimal _cajaHoy;

        private decimal _cajaDiaAnterior;
        public decimal Caja_DiaAnterior
        {
            get => _cajaDiaAnterior;
            set { _cajaDiaAnterior = value; OnPropertyChanged(nameof(Caja_DiaAnterior)); RecalcularCaja(); }
        }

        private decimal _cajaRetiro;
        public decimal Caja_Retiro
        {
            get => _cajaRetiro;
            set { _cajaRetiro = value; OnPropertyChanged(nameof(Caja_Retiro)); RecalcularCaja(); }
        }

        private decimal _cajaTotal;
        public decimal Caja_Total
        {
            get => _cajaTotal;
            set { _cajaTotal = value; OnPropertyChanged(nameof(Caja_Total)); }
        }

        private string _rutaTempActual; // RutaTempActual esta aca por algun motivo que desconozco

        public decimal Caja_Hoy { get => _cajaHoy; set { _cajaHoy = value; OnPropertyChanged(nameof(Caja_Hoy)); } }

        // -- DATABASE --
        private readonly DatabaseService _db = new(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GIDAD", "gidad.db")
        );

        private DatabaseService _databaseService;
        private AutoguardadoService _autoguardado;

        private string RutaTempActual => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GIDAD", "temp", $"snapshot_{DateTime.Now:yyyyMMdd}.json");

        private void RecalcularDia()
        {
            Dia_Efectivo = OT_Efectivo + VM_Efectivo + PR_Efectivo + VA_Efectivo;
            Dia_Seña = OT_Seña + VM_Seña;
            Dia_Tarjeta = OT_Tarjeta + VM_Tarjeta;
            Dia_Transferencia = OT_Transferencia + VM_Transferencia + PR_Transferencia + VA_Transferencia;

            // Efectivo del día menos lo que salió por Proveedores y Varios
            Caja_Hoy = (OT_Efectivo + VM_Efectivo) - (PR_Efectivo + VA_Efectivo);
            RecalcularCaja();
        }

        private void RecalcularCaja()
        {
            // Caja Total = lo que había ayer + lo que entró/salió hoy - lo retirado hoy
            Caja_Total = Caja_DiaAnterior + Caja_Hoy - Caja_Retiro;
        }

        public MainWindow()
        {
            InitializeComponent();

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
                "MOTIVO", "MONTO", "MEDIO DE PAGO"
            };
            ServiciosDisponibles = new List<string>
            {
                "1° Service", "2° Service", "3° Service",
                "4° Service", "5° Service", "Reparacion Particular"
            };
            MediosPagoDisponibles = new List<string>
            {
                "Efectivo", "Transferencia"
            };

            DataContext = this;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 1) Crear la BD y las tablas si no existen
            await _db.InicializarAsync();

            await VerificarRecuperacionAsync();

            // 2) Suscribir los 4 grids a los totales
            SuscribirGrid(OrdenesTrabajoGrid, "OT");
            SuscribirGrid(VentasMostradorGrid, "VM");
            SuscribirGrid(ProveedoresGrid, "PR");
            SuscribirGrid(VariosGrid, "VA");

            Caja_DiaAnterior = await _db.ObtenerUltimaCajaHoyAsync();

            // 3) Iniciar autoguardado (temporal cada N min + definitivo a las 23:59)
            _autoguardado = new AutoguardadoService(
                guardarTemporal: GuardarTemporalAsync,
                guardarDefinitivo: GuardarDefinitivoAsync);

            _autoguardado.SetIntervalo(1); // 1 minuto, solo para pruebas
            _autoguardado.Iniciar();
        }

        private void SuscribirGrid(DynamicDataGrid grid, string prefijo)
        {
            // Cuando se agrega o elimina una fila
            grid.Rows.CollectionChanged += (s, e) =>
            {
                // Suscribir filas nuevas
                if (e.NewItems != null)
                    foreach (OrdenRow row in e.NewItems)
                        row.PropertyChanged += (rs, re) => RecalcularTotales(grid, prefijo);

                // Desuscribir filas eliminadas (evita memory leaks)
                if (e.OldItems != null)
                    foreach (OrdenRow row in e.OldItems)
                        row.PropertyChanged -= (rs, re) => RecalcularTotales(grid, prefijo);

                RecalcularTotales(grid, prefijo);
            };

            // Suscribir la fila inicial que AddNewRow() ya agrego
            foreach (OrdenRow row in grid.Rows)
                row.PropertyChanged += (rs, re) => RecalcularTotales(grid, prefijo);

            // Suscribirse a cambios en las colecciones de filas
            OrdenesTrabajoGrid.Rows.CollectionChanged += (s, e) => RecalcularTotales(OrdenesTrabajoGrid, "OT");
            VentasMostradorGrid.Rows.CollectionChanged += (s, e) => RecalcularTotales(VentasMostradorGrid, "VM");

            // Suscribirse a cambios en propiedades de filas ya existentes
            OrdenesTrabajoGrid.Rows.CollectionChanged += SuscribirFilasOT;
            VentasMostradorGrid.Rows.CollectionChanged += SuscribirFilasVM;
        }

        // ── Suscribir PropertyChanged de cada fila nueva ──
        private void SuscribirFilasOT(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (OrdenRow row in e.NewItems)
                    row.PropertyChanged += (s, _) => RecalcularTotales(OrdenesTrabajoGrid, "OT");

            if (e.OldItems != null)
                foreach (OrdenRow row in e.OldItems)
                    row.PropertyChanged -= (s, _) => RecalcularTotales(OrdenesTrabajoGrid, "OT");
        }

        private void SuscribirFilasVM(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (OrdenRow row in e.NewItems)
                    row.PropertyChanged += (s, _) => RecalcularTotales(VentasMostradorGrid, "VM");

            if (e.OldItems != null)
                foreach (OrdenRow row in e.OldItems)
                    row.PropertyChanged -= (s, _) => RecalcularTotales(VentasMostradorGrid, "VM");
        }

        private void RecalcularTotales(DynamicDataGrid grid, string prefijo)
        {
            decimal efectivo = 0, seña = 0, tarjeta = 0, transferencia = 0;

            foreach (var row in grid.Rows)
            {
                decimal.TryParse(row.Efectivo, out var monto);

                if (prefijo == "PR" || prefijo == "VA")
                {
                    // Clasificar según el medio de pago elegido
                    if (row.MedioPago == "Transferencia")
                        transferencia += monto;
                    else
                        efectivo += monto; // Efectivo o vacío → efectivo
                }
                else
                {
                    decimal.TryParse(row.Seña, out var s); seña += s;
                    decimal.TryParse(row.Tarjeta, out var t); tarjeta += t;
                    decimal.TryParse(row.TransferenciaMonto, out var tr); transferencia += tr;
                    efectivo += monto;
                }
            }

            switch (prefijo)
            {
                case "OT":
                    OT_Efectivo = efectivo; OT_Seña = seña;
                    OT_Tarjeta = tarjeta; OT_Transferencia = transferencia;
                    break;
                case "VM":
                    VM_Efectivo = efectivo; VM_Seña = seña;
                    VM_Tarjeta = tarjeta; VM_Transferencia = transferencia;
                    break;
                case "PR":
                    PR_Efectivo = efectivo; PR_Transferencia = transferencia;
                    break;
                case "VA":
                    VA_Efectivo = efectivo; VA_Transferencia = transferencia;
                    break;
            }

            RecalcularDia();
        }

        // ── Guardado temporal (cada N minutos) ──
        private async Task GuardarTemporalAsync()
        {
            var snap = ConstruirSnapshot();
            await SnapshotService.GuardarAsync(snap, _rutaTempActual);
        }

        // ── Guardado definitivo en BD (23:59 o manual) ──
        private async Task GuardarDefinitivoAsync(string rutaOpcional)
        {
            try
            {
                long sesionId = await _db.CrearSesionAsync();

                await _db.GuardarOrdenesTrabajoAsync(sesionId, OrdenesTrabajoGrid.Rows);
                await _db.GuardarVentasMostradorAsync(sesionId, VentasMostradorGrid.Rows);
                await _db.GuardarProveedoresAsync(sesionId, ProveedoresGrid.Rows);
                await _db.GuardarVariosAsync(sesionId, VariosGrid.Rows);

                await _db.GuardarTotalesAsync(sesionId, "OrdenesTrabajo",
                    OT_Efectivo, OT_Seña, OT_Tarjeta, OT_Transferencia);
                await _db.GuardarTotalesAsync(sesionId, "VentasMostrador",
                    VM_Efectivo, VM_Seña, VM_Tarjeta, VM_Transferencia);
                await _db.GuardarTotalesAsync(sesionId, "Proveedores",
                    PR_Efectivo, 0, 0, PR_Transferencia);
                await _db.GuardarTotalesAsync(sesionId, "Varios",
                    VA_Efectivo, 0, 0, VA_Transferencia);

                await _db.GuardarTotalDiaAsync(sesionId,
                    Dia_Efectivo, Dia_Seña, Dia_Tarjeta, Dia_Transferencia);

                await _db.GuardarCajaAsync(sesionId, 0, 0, Caja_Hoy, Caja_Hoy);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error al guardar en BD: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        //private async void Window_Closing(object sender, CancelEventArgs e)
        //{
        //    await GuardarDefinitivoAsync();
        //}

        private AppSnapshot ConstruirSnapshot() => new()
        {
            OrdenesTrabajo = OrdenesTrabajoGrid.Rows.Select(OrdenRowDto.FromModel).ToList(),
            VentasMostrador = VentasMostradorGrid.Rows.Select(OrdenRowDto.FromModel).ToList(),
            Proveedores = ProveedoresGrid.Rows.Select(OrdenRowDto.FromModel).ToList(),
            Varios = VariosGrid.Rows.Select(OrdenRowDto.FromModel).ToList(),
            OT_Efectivo = OT_Efectivo,
            OT_Seña = OT_Seña,
            OT_Tarjeta = OT_Tarjeta,
            OT_Transferencia = OT_Transferencia,
            VM_Efectivo = VM_Efectivo,
            VM_Seña = VM_Seña,
            VM_Tarjeta = VM_Tarjeta,
            VM_Transferencia = VM_Transferencia,
            PR_Efectivo = PR_Efectivo,
            PR_Transferencia = PR_Transferencia,
            VA_Efectivo = VA_Efectivo,
            VA_Transferencia = VA_Transferencia,
            Dia_Efectivo = Dia_Efectivo,
            Dia_Seña = Dia_Seña,
            Dia_Tarjeta = Dia_Tarjeta,
            Dia_Transferencia = Dia_Transferencia,
            Caja_Hoy = Caja_Hoy,
            Caja_Retiro = Caja_Retiro
        };

        private async Task VerificarRecuperacionAsync()
        {
            var carpetaTemp = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GIDAD", "temp");

            if (!Directory.Exists(carpetaTemp)) return;

            var archivos = Directory.GetFiles(carpetaTemp, "snapshot_*.json");
            if (archivos.Length == 0) return;

            var masReciente = archivos.OrderByDescending(File.GetLastWriteTime).First();

            AppSnapshot snap;
            try
            {
                snap = await SnapshotService.CargarAsync(masReciente);
            }
            catch
            {
                return; // archivo corrupto, ignorar
            }

            var respuesta = MessageBox.Show(
                $"Se encontró un guardado automático del {snap.Timestamp:dd/MM/yyyy HH:mm}.\n" +
                "¿Querés recuperar esos datos?",
                "Recuperar sesión",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta == MessageBoxResult.Yes)
                CargarSnapshotEnGrids(snap);

            // Borrar el snapshot ya procesado (se haya cargado o no)
            try { File.Delete(masReciente); } catch { }
        }

        private void CargarSnapshotEnGrids(AppSnapshot snap)
        {
            OrdenesTrabajoGrid.CargarFilas(snap.OrdenesTrabajo.Select(d => d.ToModel()));
            VentasMostradorGrid.CargarFilas(snap.VentasMostrador.Select(d => d.ToModel()));
            ProveedoresGrid.CargarFilas(snap.Proveedores.Select(d => d.ToModel()));
            VariosGrid.CargarFilas(snap.Varios.Select(d => d.ToModel()));

            Caja_Retiro = snap.Caja_Retiro;

            // Los totales se recalculan solos por las suscripciones a PropertyChanged/CollectionChanged
        }

        // ── Abrir ventana de opciones ──
        private void MenuItem_Opciones_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new OpcionesWindow(
                _autoguardado.IntervaloMinutos,
                GuardarDefinitivoAsync);

            ventana.Owner = this;

            if (ventana.ShowDialog() == true)
                _autoguardado.SetIntervalo(ventana.IntervaloMinutos);
        }

        private void MenuItem_Estadisticas_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new EstadisticasWindow(OrdenesTrabajoGrid.Rows)
            {
                Owner = this
            };
            ventana.Show(); // no modal, para poder seguir trabajando en la principal
        }

        // ── Navegación (sin cambios) ──
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

        public Dictionary<string, string> OrdenesTrabajoMap { get; } = new()
        {
            { "N° DE ORDEN",      nameof(OrdenRow.NroOrden) },
            { "SERVICIO",         nameof(OrdenRow.Servicio) },
            { "EFECTIVO",         nameof(OrdenRow.Efectivo) },
            { "SEÑA",             nameof(OrdenRow.Seña) },
            { "TARJETA",          nameof(OrdenRow.Tarjeta) },
            { "TRANSFERENCIA",    "TRANSFERENCIA" }            // clave especial
        };

        public Dictionary<string, string> VentasMostradorMap { get; } = new()
        {
            { "N° DE COMPROBANTE", nameof(OrdenRow.NroOrden) },
            { "EFECTIVO",          nameof(OrdenRow.Efectivo) },
            { "SEÑA",              nameof(OrdenRow.Seña) },
            { "TARJETA",           nameof(OrdenRow.Tarjeta) },
            { "TRANSFERENCIA",     "TRANSFERENCIA" }
        };

        public Dictionary<string, string> ProveedoresMap { get; } = new()
        {
            { "N° DE FACTURA", nameof(OrdenRow.NroOrden) },
            { "MONTO",         nameof(OrdenRow.Efectivo) },   // monto numérico
            { "MEDIO DE PAGO", nameof(OrdenRow.MedioPago) }   // "Efectivo" o "Transferencia"
        };

        public Dictionary<string, string> VariosMap { get; } = new()
        {
            { "MOTIVO",        nameof(OrdenRow.NroOrden)  },
            { "MONTO",         nameof(OrdenRow.Efectivo)  },
            { "MEDIO DE PAGO", nameof(OrdenRow.MedioPago) } // Lo mismo que Proveedores
        };
    }
}