using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SPOReceiver.Models;

namespace SPOReceiver
{
    public static class WebhookReceiver
    {
        [FunctionName("WebhookReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ICollector<string> outputQueueItem,
            ILogger log)
        {
            log.LogInformation("Webhook has been triggered");

            string code = req.Query["code"];

            // If code is supplied in request, that means webhook is being applied to a library, 
            // therefore only the code is needed to be returned.
            if (code != null)
            {
                log.LogInformation($"Code: {code} has been received!");
                return (ActionResult)new OkObjectResult(code);
            }

            log.LogInformation("Sharepoint has triggered this webhookj processing request...");
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Received following payload: {content}");

            var notifications = JsonConvert.DeserializeObject<ResponseModel<NotificationModel>>(content).Value;
            log.LogInformation($"Found {notifications.Count} notifications");

            if (notifications.Count > 0)
            {
                log.LogInformation("Processing notifications...");
                foreach (var notification in notifications)
                {
                    // add message to the queue
                    string message = JsonConvert.SerializeObject(notification);
                    log.LogInformation($"Before adding a message to the queue. Message content: {message}");
                    outputQueueItem.Add(message);
                    log.LogInformation($"Message added :-)");
                }
            }

            // if we get here we assume the request was well received
            return (ActionResult)new OkObjectResult($"Added to queue");
        }
    }
}
