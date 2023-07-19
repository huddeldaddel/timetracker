using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Model;
using TimeTracker.Service;

namespace TimeTracker
{
    public class AddLogEntry
    {
        private readonly ILogger _logger;
        private readonly IEntryService _entryService;

        public AddLogEntry(ILoggerFactory loggerFactory, IEntryService entryService)
        {
            _logger = loggerFactory.CreateLogger<AddLogEntry>();
            _entryService = entryService;
        }

        [Function("AddLogEntry")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "logEntries")] HttpRequestData req)
        {            
            var requestBody = String.Empty;
            using (StreamReader streamReader = new(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var requestEntry = JsonConvert.DeserializeObject<UpsertEntryRequest>(requestBody);
            _logger.LogInformation($"C# HTTP trigger function received a request: {requestBody}");

            if(null != requestEntry && requestEntry.Validate())
            {
                var result = await _entryService.AddEntry(requestEntry.ToEntry()!);
                var response = req.CreateResponse();
                await response.WriteAsJsonAsync(result);
                return response;
            } 
            else
            {                                
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
