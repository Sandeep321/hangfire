using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebApi.Processor
{
    /// <summary>
    /// Processor class implementing IJobProcessor
    /// </summary>
    public class JobProcessor : IJobProcessor
    {
        private readonly string _Url;
        private ILogger Logger { get; }
        public JobProcessor(string url,ILoggerFactory loggerFactory)
        {
            _Url = url;
            Logger = loggerFactory.CreateLogger(nameof(JobProcessor));
        }

        public async Task Invoke()
        {
            var requestId = Guid.NewGuid();
            Logger.LogInformation($"{requestId}: Invoked Job for {_Url}");
            try
            {
                using (WebClient client = new WebClient())
                {
                    Uri uri = new Uri(_Url);
                    await client.DownloadStringTaskAsync(uri);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"{requestId}: Exception Occured: {ex.StackTrace}");
                throw;
            }
            Logger.LogInformation($"{requestId}: Success Job for {_Url}");
        }        
    }
}