using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notify.Repositories;

namespace Notify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoggingInformationController : ControllerBase
    {
        [HttpGet("GetLoggingOperationAverageLetancyInNanoSeconds")]
        public double GetLoggingOperationAverageLetancyInNanoSeconds()
        {
            return BackgroundQueueLogger.AverageLatencyTimeInNanoSecond;
        }

        [HttpGet("GetLoggingOperationsCount")]
        public long GetLoggingOperationsCount()
        {
            return BackgroundQueueLogger.InvokedMethodsCount;
        }
        
        [HttpGet("GetLoggingOperationsLetancyInNanoSeconds")]
        public long GetLoggingOperationsLetancyInNanoSeconds()
        {
            return BackgroundQueueLogger.InvokedMethodsCount;
        }
    }
}
