using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrderFlowBusinessMonitor.Models;
using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;

namespace OrderFlowBusinessMonitor.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly OrderFlowBusinessMonitorContext _context;
        private string storageTableConnection = "DefaultEndpointsProtocol=https;AccountName=svlsb2bin;AccountKey=nTMuqF4UnIterAXS7o+w72/DCEXIH6TVW7DA6qATp3RpX+HfGJH3R6d4I+Qmt9Mm0MbShxEO91ZAIyY9eyoxHQ==;EndpointSuffix=core.windows.net";

        //public OrdersController(OrderFlowBusinessMonitorContext context)
        //{
        //    _context = context;
        //}

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            string userName = User.Identity.Name;
            string domainName = userName.Substring(userName.IndexOf('@') + 1, userName.IndexOf('.') - userName.IndexOf('@') - 1);
            string partner = string.Empty;
            if ("neocorpone".Equals(domainName))
                partner = "neocorp";
            else if("femacorpone".Equals(domainName))
                partner = "femacorp";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageTableConnection);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable cloudTable = tableClient.GetTableReference("orderdata");

            TableQuery rangeQuery = null;
            if (string.IsNullOrEmpty(partner))
                rangeQuery = new TableQuery();
            else
                rangeQuery = new TableQuery().Where(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partner));

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
                    qty = props["qty"].Int32Value.Value,
                    Id = rk
                };
            }
            catch (Exception)
            {
              Console.WriteLine("Order data not in the right format or has missing attributes..");
                return (ActionResult)new NotFoundObjectResult(404);
            }

            do
            {
                allOrders = await cloudTable.ExecuteQuerySegmentedAsync<Orders>(rangeQuery, resolver, token);
                token = allOrders.ContinuationToken;
            } while (token != null);

            //Console.WriteLine("C# HTTP trigger function processed a request.");
            //foreach (Orders refOrder in allOrders.Results)
            //{
            //    _context.Orders.Add(refOrder);
            //}
            //return View(await _context.Orders.ToListAsync());
            return View(allOrders.Results);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("item,partnername,Id,ordernumber,uom,qty,partnercode,currency,amount,status,orderdate,deliverydate")] Orders orders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("item,partnername,Id,ordernumber,uom,qty,partnercode,currency,amount,status,orderdate,deliverydate")] Orders orders)
        {
            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orders.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var orders = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(orders);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdersExists(string id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
