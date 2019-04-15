using System;
using System.Collections.Generic;
using System.Text;

namespace ordersflow
{
    public class Orders
    {
        public string item { get; set; }
        public string partnername { get; set; }
        public string ordernumber { get; set; }
        public string uom { get; set; }
        public int qty { get; set; }
        public string partnercode { get; set; }
        public string currency { get; set; }
        public long amount { get; set; }
        public string status { get; set; }
        public string orderdate { get; set; }
        public string deliverydate { get; set; }
    }
}
