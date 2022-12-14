using MVC_Store.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models.ViewModels.Shop
{
    public class OrderVM
    {
        public OrderVM()
        {

        }
        public OrderVM(OrderDTO row)
        {
            OrederId = row.OrderId;
            UserId = row.UserId;
            CreatedAt = row.CreatedAt;
        }
        public int OrederId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}