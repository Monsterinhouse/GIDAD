using System.ComponentModel;

namespace WPF_Test.Models
{
    public class OrdenRow : INotifyPropertyChanged
    {
        private string _nroOrden;
        private string _servicio;
        private string _efectivo;
        private string _seña;
        private string _tarjeta;
        private string _transferenciaMonto;
        private string _transferenciaNombre;
        private string _monto;
        private string _motivo;
        private string _medioPago;

        public string NroOrden
        {
            get => _nroOrden;
            set { _nroOrden = value; OnPropertyChanged(nameof(NroOrden)); }
        }

        public string Servicio
        {
            get => _servicio;
            set { _servicio = value; OnPropertyChanged(nameof(Servicio)); }
        }

        public string Efectivo
        {
            get => _efectivo;
            set { _efectivo = value; OnPropertyChanged(nameof(Efectivo)); }
        }

        public string Seña
        {
            get => _seña;
            set { _seña = value; OnPropertyChanged(nameof(Seña)); }
        }

        public string Tarjeta
        {
            get => _tarjeta;
            set { _tarjeta = value; OnPropertyChanged(nameof(Tarjeta)); }
        }

        public string TransferenciaMonto
        {
            get => _transferenciaMonto;
            set { _transferenciaMonto = value; OnPropertyChanged(nameof(TransferenciaMonto)); }
        }

        public string TransferenciaNombre
        {
            get => _transferenciaNombre;
            set { _transferenciaNombre = value; OnPropertyChanged(nameof(TransferenciaNombre)); }
        }

        public string Monto
        {
            get => _monto;
            set { _monto = value; OnPropertyChanged(nameof(Monto)); }
        }

        public string Motivo
        {
            get => _motivo;
            set { _motivo = value; OnPropertyChanged(nameof(Motivo)); }
        }

        public string MedioPago
        {
            get => _medioPago;
            set { _medioPago = value; OnPropertyChanged(nameof(MedioPago)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}