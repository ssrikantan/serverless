using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;


namespace ordersflow
{
    public static class PendingApprovals
    {

        [FunctionName("PendingApprovals")]
        public static async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
          [Table("orderdata", Connection = "orderdataconnection")] CloudTable cloudTable,
          ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string partner = req.Query["partner"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            partner = partner ?? data?.partner;

            TableQuery rangeQuery = null;
            if (string.IsNullOrEmpty(partner))
                rangeQuery = new TableQuery().Where(
                TableQuery.GenerateFilterCondition("status", QueryComparisons.Equal, "PendingApproval")); 
            else
                rangeQuery = new TableQuery().Where(
                     TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partner),
             TableOperators.And,
                TableQuery.GenerateFilterCondition("status", QueryComparisons.Equal, "PendingApproval")));

            TableContinuationToken token = null;
            TableQuerySegment<Orders> allOrders = null;
            EntityResolver<Orders> resolver = null;
            try
            {
                resolver = (pk, rk, ts, props, etag) => new Orders
                {
                    partnername = pk,
                    ordernumber = rk,
                    item = props["item"].StringValue,
                    uom = props["uom"].StringValue,
                    currency = props["currency"].StringValue,
                    deliverydate = props["deliverydate"].StringValue,
                    orderdate = props["orderdate"].StringValue,
                    partnercode = props["partnercode"].StringValue,
                    status = props["status"].StringValue,
                    amount = props["amount"].Int32Value.Value,
                    qty = props["qty"].Int32Value.Value
                };
            }
            catch(Exception)
            {
                log.LogInformation("Order data not in the right format or has missing attributes..");
                return (ActionResult)new NotFoundObjectResult(404);
            }

            do
            {
                allOrders = await cloudTable.ExecuteQuerySegmentedAsync<Orders>(rangeQuery, resolver,token);
                token = allOrders.ContinuationToken;
            } while (token != null);

            log.LogInformation("C# HTTP trigger function processed a request.");
            return (ActionResult)new OkObjectResult(allOrders);
            //return name != null
            //    ? (ActionResult)new OkObjectResult($"Here are the orders, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

    }
}
