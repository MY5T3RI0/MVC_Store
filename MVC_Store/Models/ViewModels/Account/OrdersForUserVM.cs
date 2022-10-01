using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MVC_Store.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        [DisplayName("Order Details")]
        public Dictionary<string, int> ProductsAndQty { get; set; }
        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }
    }
}