using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WPF_Test.Models;

namespace WPF_Test.Services
{
    public class AppSnapshot
    {
        public List<OrdenRowDto> OrdenesTrabajo { get; set; } = new();
        public List<OrdenRowDto> VentasMostrador { get; set; } = new();
        public List<OrdenRowDto> Proveedores { get; set; } = new();
        public List<OrdenRowDto> Varios { get; set; } = new();

        public decimal OT_Efectivo { get; set; }
        public decimal OT_Seña { get; set; }
        public decimal OT_Tarjeta { get; set; }
        public decimal OT_Transferencia { get; set; }

        public decimal VM_Efectivo { get; set; }
        public decimal VM_Seña { get; set; }
        public decimal VM_Tarjeta { get; set; }
        public decimal VM_Transferencia { get; set; }

        public decimal PR_Efectivo { get; set; }
        public decimal PR_Transferencia { get; set; }

        public decimal VA_Efectivo { get; set; }
        public decimal VA_Transferencia { get; set; }

        public decimal Dia_Efectivo { get; set; }
        public decimal Dia_Seña { get; set; }
        public decimal Dia_Tarjeta { get; set; }
        public decimal Dia_Transferencia { get; set; }

        public decimal Caja_Hoy { get; set; }
        public decimal Caja_Retiro { get; set; } // NUEVO

        public DateTime Timestamp { get; set; }
    }

    public class OrdenRowDto
    {
        public string NroOrden { get; set; }
        public string Servicio { get; set; }
        public string Efectivo { get; set; }
        public string Seña { get; set; }
        public string Tarjeta { get; set; }
        public string MedioPago { get; set; }
        public string TransferenciaMonto { get; set; }
        public string TransferenciaNombre { get; set; }

        public static OrdenRowDto FromModel(OrdenRow r) => new()
        {
            NroOrden = r.NroOrden,
            Servicio = r.Servicio,
            Efectivo = r.Efectivo,
            Seña = r.Seña,
            Tarjeta = r.Tarjeta,
            MedioPago = r.MedioPago,
            TransferenciaMonto = r.TransferenciaMonto,
            TransferenciaNombre = r.TransferenciaNombre
        };

        public OrdenRow ToModel() => new()
        {
            NroOrden = NroOrden,
            Servicio = Servicio,
            Efectivo = Efectivo,
            Seña = Seña,
            Tarjeta = Tarjeta,
            MedioPago = MedioPago,
            TransferenciaMonto = TransferenciaMonto,
            TransferenciaNombre = TransferenciaNombre
        };
    }

    public static class SnapshotService
    {
        private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

        public static async Task GuardarAsync(AppSnapshot snap, string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new ArgumentException("La ruta de destino del snapshot está vacía o nula.");

            snap.Timestamp = DateTime.Now;

            var dir = Path.GetDirectoryName(ruta);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(snap, _opts);
            await File.WriteAllTextAsync(ruta, json);
        }

        public static async Task<AppSnapshot> CargarAsync(string ruta)
        {
            var json = await File.ReadAllTextAsync(ruta);
            return JsonSerializer.Deserialize<AppSnapshot>(json);
        }
    }
}