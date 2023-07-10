using Notify.Interfaces;

namespace Notify.Repositories
{
    public class LoggingEnable : ILoggingEnable
    {
        public bool Enabled { get; set; }
    }
}
