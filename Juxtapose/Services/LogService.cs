using System;

namespace Juxtapose.Services
{
    public class LogService : ILogService
    {
        public event Action<string>? MessageLogged;

        public void Log(string message)
        {
            string formatted = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            MessageLogged?.Invoke(formatted);
        }
    }
}
