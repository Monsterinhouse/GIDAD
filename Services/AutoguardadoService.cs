using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WPF_Test.Services
{
    public class AutoguardadoService : IDisposable
    {
        private Timer _timerPeriodico;
        private Timer _timerFin;
        private readonly Func<Task> _guardarTemporal;
        private readonly Func<string, Task> _guardarDefinitivo;

        public int IntervaloMinutos { get; private set; } = 5;

        public AutoguardadoService(
            Func<Task> guardarTemporal,
            Func<string, Task> guardarDefinitivo)
        {
            _guardarTemporal = guardarTemporal;
            _guardarDefinitivo = guardarDefinitivo;
        }

        public void SetIntervalo(int minutos)
        {
            IntervaloMinutos = minutos;
            IniciarTimerPeriodico();
        }

        public void Iniciar()
        {
            IniciarTimerPeriodico();
            IniciarTimerFin();
        }

        private void IniciarTimerPeriodico()
        {
            _timerPeriodico?.Dispose();
            var intervalo = TimeSpan.FromMinutes(IntervaloMinutos);

            _timerPeriodico = new Timer(async _ =>
            {
                try
                {
                    await _guardarTemporal();
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }, null, intervalo, intervalo);
        }

        private static void LogError(Exception ex)
        {
            try
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GIDAD", "logs");
                Directory.CreateDirectory(dir);

                File.AppendAllText(
                    Path.Combine(dir, "autoguardado_errores.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {ex}\n\n");
            }
            catch { /* si ni esto funciona, no hay mucho más que hacer */ }
        }

        private void IniciarTimerFin()
        {
            _timerFin?.Dispose();

            var ahora = DateTime.Now;
            var fin = ahora.Date.AddHours(23).AddMinutes(59);
            var demora = fin > ahora ? fin - ahora : TimeSpan.FromDays(1) + (fin - ahora);

            _timerFin = new Timer(async _ =>
            {
                try { await _guardarDefinitivo(null); }
                catch (Exception ex) { LogError(ex); }

                IniciarTimerFin();
            }, null, demora, Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            _timerPeriodico?.Dispose();
            _timerFin?.Dispose();
        }
    }
}