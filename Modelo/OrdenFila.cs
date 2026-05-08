// Modelo/OrdenFila.cs
using System.ComponentModel;

namespace WPF_Test.Models
{
    public class OrdenRow : INotifyPropertyChanged
    {
        private string _nroOrden;
        private string _efectivo;
        private string _seña;
        private string _tarjeta;
        private string _transferencia;

        public string NroOrden
        {
            get => _nroOrden;
            set { _nroOrden = value; OnPropertyChanged(nameof(NroOrden)); }
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

        public string Transferencia
        {
            get => _transferencia;
            set { _transferencia = value; OnPropertyChanged(nameof(Transferencia)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}