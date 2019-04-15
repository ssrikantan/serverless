using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace ordersflow
{
    public class OrdersEntity:TableEntity
    {
        public OrdersEntity()
        {

        }
        public OrdersEntity(string PartnerName, string OrderNumber)
        {
            PartitionKey = PartnerName;
            RowKey = OrderNumber;
        }
        public string item { get; set; }
        public string uom { get; set; }
        public int qty { get; set; }
        public string partnercode { get; set; }
        public string currency { get; set; }
        public int amount { get; set; }
        public string status { get; set; }
        public string orderdate { get; set; }
        public string deliverydate { get; set; }

    }
}
