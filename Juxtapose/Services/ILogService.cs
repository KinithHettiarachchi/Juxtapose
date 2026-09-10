using System;

namespace Juxtapose.Services
{
    public interface ILogService
    {
        event Action<string>? MessageLogged;
        void Log(string message);
    }
}
