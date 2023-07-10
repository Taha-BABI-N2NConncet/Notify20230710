using Serilog.Events;
using Serilog;

namespace Notify.Repositories
{
    //this class is used as a middlware so that any modification can be done easly 
    //even if it does not contain any logic, this is the purpuse of it.
    public class NotifyLogger : Serilog.ILogger
    {
        private Serilog.ILogger _logger = Log.Logger;

        public void Write(LogEvent logEvent)
        {
            _logger.Write(logEvent);
        }
        public void Information(string messageTemplate)
        {
            _logger.Information(messageTemplate);
        }
        public void Error(string messageTemplate)
        {
            _logger.Error(messageTemplate);
        }
        public void Fatal(string messageTemplate)
        {
            _logger.Fatal(messageTemplate);
        }
        public void Warning(string messageTemplate)
        {
            _logger.Warning(messageTemplate);
        }
        public void Verbose(string messageTemplate)
        {
            _logger.Verbose(messageTemplate);
        }
        public void Debug(string messageTemplate)
        {
            _logger.Debug(messageTemplate);
        }
    }
}
