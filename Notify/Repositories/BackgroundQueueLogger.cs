using System.Collections;
using System.Collections.Concurrent;
using static Notify.Repositories.BackgroundQueueLogger;

namespace Notify.Repositories
{
    public class BackgroundQueueLogger
    {
        public delegate void logMethod();
        private static ConcurrentQueue<LoggingItem> loggingQueue = new ConcurrentQueue<LoggingItem>();
        private static CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();
        private static object _lock = new object();
        private static ManualResetEvent manualReset = new ManualResetEvent(false);
        private static long _latencyTime = 0;
        private static long _invokedMethodsCount = 0;
        private static Serilog.ILogger _logger;
        public static double AverageLatencyTimeInNanoSecond
        { 
            get 
            {
                return InvokedMethodsCount != 0? LatencyTime / (double)InvokedMethodsCount : 0.0;
            }
        }

        public static long LatencyTime { get => _latencyTime; }
        public static long InvokedMethodsCount { get => _invokedMethodsCount;}

        public static void StartProcessLogging(Serilog.ILogger logger)
        {
            var ltoken = m_cancellationTokenSource.Token;
            var lProcessLoggingThrd = new Thread(ProcessLogging);
            lProcessLoggingThrd.IsBackground = true;
            lProcessLoggingThrd.Start(ltoken);
            _logger = logger;
        }
        public static void AddLoggingTaskToQueue(logMethod logMethod)
        {
            loggingQueue.Enqueue(new LoggingItem() { 
            logMethod = logMethod,
            EnqueuingTime = DateTime.Now,
            DequeuingTime = DateTime.Now
            });
            manualReset.Set();
        }
        private static void ProcessLogging(object obj)
        {
            while (!((CancellationToken)obj).IsCancellationRequested)
            {
                try
                {
                    LoggingItem loggingItem;
                    int lCount = loggingQueue.Count;
                    if (lCount > 0)
                    {
                        lock (_lock)
                        {
                            for (int i = 0; i < lCount; i++)
                            {
                                var lSuccess = loggingQueue.TryDequeue(out loggingItem);
                                if (lSuccess)
                                {
                                    loggingItem.DequeuingTime = DateTime.Now;
                                    loggingItem.logMethod.Invoke();
                                    long oldLatencyVal = LatencyTime;
                                    long oldInvokedMethodsCount = LatencyTime;
                                    try
                                    {
                                        _latencyTime += (int)(loggingItem.DequeuingTime - loggingItem.EnqueuingTime).TotalNanoseconds;
                                        _invokedMethodsCount++;
                                    }
                                    catch (OverflowException)
                                    {
                                        _logger.Information($"the latency Time has been reset on latencyTime={LatencyTime} and invokedMethodsCount={InvokedMethodsCount}\n");
                                        _latencyTime = 0;
                                        _invokedMethodsCount = 0;
                                    }
                                    
                                }
                            }
                        }
                    }
                    else
                        manualReset.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProcessNewAckMsgRecv, {ex.Message}");
                }
            }
        }
        public static string GetObjectJsonString(object obj, int indentationLevel = 0, bool isArrayItem = false)
        {
            if (obj == null)
            {
                return "null";
            }

            var objectType = obj.GetType();

            if (objectType.IsPrimitive || objectType == typeof(string) || objectType == typeof(System.DateTime))
            {
                return obj.ToString();
            }

            if (obj is IEnumerable enumerable)
            {
                string result = $"{GetIndentation(indentationLevel)}{(isArrayItem ? "" : $"\n{GetIndentation(indentationLevel)}[")}";

                if (enumerable.GetEnumerator().MoveNext())
                {
                    foreach (var item in enumerable)
                    {
                        result += $"{GetIndentation(indentationLevel + 1)}{GetObjectJsonString(item, indentationLevel + 1)}\n";
                    }
                }

                result += $"{GetIndentation(indentationLevel)}{(isArrayItem ? "" : "]")}";
                return result;
            }

            string output = $"\n{GetIndentation(indentationLevel)}" + "{\n";
            foreach (var property in objectType.GetProperties())
            {
                output += $"{GetIndentation(indentationLevel + 1)}{property.Name}: ";
                var propertyValue = property.GetValue(obj);
                output += $"{GetObjectJsonString(propertyValue, indentationLevel + 1)}\n";
            }
            output += $"{GetIndentation(indentationLevel)}" + "}";
            return output;
        }


        static string GetIndentation(int indentationLevel)
        {
            return new string('\t', indentationLevel);
        }
    }
    public class LoggingItem 
    {
        public logMethod logMethod { get; set; }
        public DateTime EnqueuingTime { get; set; }
        public DateTime DequeuingTime { get; set; }
    }
}
