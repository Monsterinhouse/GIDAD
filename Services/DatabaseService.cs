using Microsoft.Data.Sqlite;
using System.IO;
using WPF_Test.Models;

namespace WPF_Test.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _dbPath;

        public DatabaseService(string dbPath)
        {
            _dbPath = dbPath;
            _connectionString = $"Data Source={dbPath}";
        }

        private SqliteConnection GetConnection() => new(_connectionString);

        private static object Dec(string v) =>
            decimal.TryParse(v, out var r) ? (object)(double)r : DBNull.Value;

        private static object Str(string v) =>
            string.IsNullOrWhiteSpace(v) ? (object)DBNull.Value : v;

        // ── Crea el archivo .db y las tablas si no existen. Se llama al arrancar. ──
        public async Task InicializarAsync()
        {
            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"

                CREATE TABLE IF NOT EXISTS sesiones (
                    id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha     TEXT NOT NULL DEFAULT (date('now','localtime')),
                    creado_en TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS ordenes_trabajo (
                    id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    sesion_id    INTEGER NOT NULL REFERENCES sesiones(id) ON DELETE CASCADE,
                    nro_orden    TEXT,
                    servicio     TEXT,
                    efectivo     REAL,
                    seña         REAL,
                    tarjeta      REAL,
                    trans_monto  REAL,
                    trans_nombre TEXT,
                    creado_en    TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS ventas_mostrador (
                    id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    sesion_id       INTEGER NOT NULL REFERENCES sesiones(id) ON DELETE CASCADE,
                    nro_comprobante TEXT,
                    efectivo        REAL,
                    seña            REAL,
                    tarjeta         REAL,
                    trans_monto     REAL,
                    trans_nombre    TEXT,
                    creado_en       TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS proveedores (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    sesion_id   INTEGER NOT NULL REFERENCES sesiones(id) ON DELETE CASCADE,
                    nro_factura TEXT,
                    monto       REAL,
                    medio_pago  TEXT,
                    creado_en   TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS varios (
                    id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    sesion_id  INTEGER NOT NULL REFERENCES sesiones(id) ON DELETE CASCADE,
                    motivo     TEXT,
                    monto      REAL,
                    medio_pago TEXT,
                    creado_en  TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS totales_grilla (
                    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    sesion_id           INTEGER NOT NULL REFERENCES sesiones(id) ON DELETE CASCADE,
                    grilla              TEXT NOT NULL,
                    total_efectivo      REAL,
                    total_seña          REAL,
                    total_tarjeta       REAL,
                    total_transferencia REAL,
                    creado_en           TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS total_dia (
                    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    sesion_id           INTEGER NOT NULL REFERENCES sesiones(id) ON DELETE CASCADE,
                    total_efectivo      REAL,
                    total_seña          REAL,
                    total_tarjeta       REAL,
                    total_transferencia REAL,
                    creado_en           TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS caja (
                    id                INTEGER PRIMARY KEY AUTOINCREMENT,
                    sesion_id         INTEGER NOT NULL REFERENCES sesiones(id) ON DELETE CASCADE,
                    caja_dia_anterior REAL,
                    caja_retiro       REAL,
                    caja_hoy          REAL,
                    caja_total        REAL,
                    creado_en         TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );
            ";
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<long> CrearSesionAsync()
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO sesiones (fecha) VALUES (date('now','localtime')); SELECT last_insert_rowid();";
            var result = await cmd.ExecuteScalarAsync();
            return (long)result;
        }

        public async Task GuardarOrdenesTrabajoAsync(long sesionId, IEnumerable<OrdenRow> filas)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tx = conn.BeginTransaction();

            foreach (var f in filas)
            {
                if (string.IsNullOrWhiteSpace(f.NroOrden)) continue;

                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO ordenes_trabajo
                        (sesion_id, nro_orden, servicio, efectivo, seña, tarjeta, trans_monto, trans_nombre)
                    VALUES (@s,@nro,@srv,@ef,@seña,@tar,@tm,@tn)";
                cmd.Parameters.AddWithValue("@s", sesionId);
                cmd.Parameters.AddWithValue("@nro", Str(f.NroOrden));
                cmd.Parameters.AddWithValue("@srv", Str(f.Servicio));
                cmd.Parameters.AddWithValue("@ef", Dec(f.Efectivo));
                cmd.Parameters.AddWithValue("@seña", Dec(f.Seña));
                cmd.Parameters.AddWithValue("@tar", Dec(f.Tarjeta));
                cmd.Parameters.AddWithValue("@tm", Dec(f.TransferenciaMonto));
                cmd.Parameters.AddWithValue("@tn", Str(f.TransferenciaNombre));
                await cmd.ExecuteNonQueryAsync();
            }
            await tx.CommitAsync();
        }

        public async Task GuardarVentasMostradorAsync(long sesionId, IEnumerable<OrdenRow> filas)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tx = conn.BeginTransaction();

            foreach (var f in filas)
            {
                if (string.IsNullOrWhiteSpace(f.NroOrden)) continue;

                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO ventas_mostrador
                        (sesion_id, nro_comprobante, efectivo, seña, tarjeta, trans_monto, trans_nombre)
                    VALUES (@s,@nro,@ef,@seña,@tar,@tm,@tn)";
                cmd.Parameters.AddWithValue("@s", sesionId);
                cmd.Parameters.AddWithValue("@nro", Str(f.NroOrden));
                cmd.Parameters.AddWithValue("@ef", Dec(f.Efectivo));
                cmd.Parameters.AddWithValue("@seña", Dec(f.Seña));
                cmd.Parameters.AddWithValue("@tar", Dec(f.Tarjeta));
                cmd.Parameters.AddWithValue("@tm", Dec(f.TransferenciaMonto));
                cmd.Parameters.AddWithValue("@tn", Str(f.TransferenciaNombre));
                await cmd.ExecuteNonQueryAsync();
            }
            await tx.CommitAsync();
        }

        public async Task GuardarProveedoresAsync(long sesionId, IEnumerable<OrdenRow> filas)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tx = conn.BeginTransaction();

            foreach (var f in filas)
            {
                if (string.IsNullOrWhiteSpace(f.NroOrden)) continue;

                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO proveedores (sesion_id, nro_factura, monto, medio_pago)
                    VALUES (@s,@nro,@monto,@mp)";
                cmd.Parameters.AddWithValue("@s", sesionId);
                cmd.Parameters.AddWithValue("@nro", Str(f.NroOrden));
                cmd.Parameters.AddWithValue("@monto", Dec(f.Efectivo));
                cmd.Parameters.AddWithValue("@mp", Str(f.MedioPago));
                await cmd.ExecuteNonQueryAsync();
            }
            await tx.CommitAsync();
        }

        public async Task GuardarVariosAsync(long sesionId, IEnumerable<OrdenRow> filas)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tx = conn.BeginTransaction();

            foreach (var f in filas)
            {
                if (string.IsNullOrWhiteSpace(f.NroOrden)) continue;

                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO varios (sesion_id, motivo, monto, medio_pago)
                    VALUES (@s,@mot,@monto,@mp)";
                cmd.Parameters.AddWithValue("@s", sesionId);
                cmd.Parameters.AddWithValue("@mot", Str(f.NroOrden));
                cmd.Parameters.AddWithValue("@monto", Dec(f.Efectivo));
                cmd.Parameters.AddWithValue("@mp", Str(f.MedioPago));
                await cmd.ExecuteNonQueryAsync();
            }
            await tx.CommitAsync();
        }

        public async Task GuardarTotalesAsync(
            long sesionId, string grilla,
            decimal ef, decimal seña, decimal tar, decimal trans)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO totales_grilla
                    (sesion_id, grilla, total_efectivo, total_seña, total_tarjeta, total_transferencia)
                VALUES (@s,@g,@ef,@seña,@tar,@tr)";
            cmd.Parameters.AddWithValue("@s", sesionId);
            cmd.Parameters.AddWithValue("@g", grilla);
            cmd.Parameters.AddWithValue("@ef", (double)ef);
            cmd.Parameters.AddWithValue("@seña", (double)seña);
            cmd.Parameters.AddWithValue("@tar", (double)tar);
            cmd.Parameters.AddWithValue("@tr", (double)trans);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task GuardarTotalDiaAsync(
            long sesionId, decimal ef, decimal seña, decimal tar, decimal trans)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO total_dia
                    (sesion_id, total_efectivo, total_seña, total_tarjeta, total_transferencia)
                VALUES (@s,@ef,@seña,@tar,@tr)";
            cmd.Parameters.AddWithValue("@s", sesionId);
            cmd.Parameters.AddWithValue("@ef", (double)ef);
            cmd.Parameters.AddWithValue("@seña", (double)seña);
            cmd.Parameters.AddWithValue("@tar", (double)tar);
            cmd.Parameters.AddWithValue("@tr", (double)trans);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task GuardarCajaAsync(
            long sesionId, decimal anterior, decimal retiro, decimal hoy, decimal total)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO caja
                    (sesion_id, caja_dia_anterior, caja_retiro, caja_hoy, caja_total)
                VALUES (@s,@ant,@ret,@hoy,@tot)";
            cmd.Parameters.AddWithValue("@s", sesionId);
            cmd.Parameters.AddWithValue("@ant", (double)anterior);
            cmd.Parameters.AddWithValue("@ret", (double)retiro);
            cmd.Parameters.AddWithValue("@hoy", (double)hoy);
            cmd.Parameters.AddWithValue("@tot", (double)total);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<decimal> ObtenerUltimaCajaHoyAsync()
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT caja_hoy FROM caja ORDER BY id DESC LIMIT 1";

            var result = await cmd.ExecuteScalarAsync();
            return (result == null || result == DBNull.Value) ? 0 : Convert.ToDecimal(result);
        }
    }
}