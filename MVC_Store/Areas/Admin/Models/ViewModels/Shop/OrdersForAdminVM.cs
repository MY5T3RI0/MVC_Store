using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MVC_Store.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
        public string Username { get; set; }
        public decimal Total { get; set; }
        [DisplayName("Order Details")]
        public Dictionary<string, int> ProductsAndQty { get; set; }
        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }
    }
}