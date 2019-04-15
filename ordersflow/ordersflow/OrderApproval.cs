using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Host;


namespace ordersflow
{
    public static class OrderApproval
    {
        [FunctionName("OrderApproval")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Table("orderdata", Connection = "orderdataconnection")] CloudTable cloudTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string partner = req.Query["partner"];
            string ordernumber = req.Query["ordernumber"];
            string status = req.Query["status"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            partner = partner ?? data?.partner;
            ordernumber = ordernumber ?? data?.ordernumber;
            status = status ?? data?.status;

            TableOperation retrieveOperation = TableOperation.Retrieve<OrdersEntity>(partner, ordernumber);
            TableResult result = await cloudTable.ExecuteAsync(retrieveOperation);
            OrdersEntity orderobject = result.Result as OrdersEntity;
            if (orderobject == null)
            {
                log.LogInformation("Error updating the existing order");
                return (ActionResult)new BadRequestObjectResult("Order number not found to approve");
            }
            orderobject.status = status;

            TableOperation updateOperation = TableOperation.InsertOrMerge(orderobject);
            TableResult updateResult = await cloudTable.ExecuteAsync(updateOperation);
            log.LogInformation("Order update operation result"+ updateResult.Result.ToString()); 
            return (ActionResult)new OkObjectResult($"order data updated, {ordernumber}");
                //: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
