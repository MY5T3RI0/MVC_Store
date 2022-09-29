using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if (cart.Count == 0 ||Session["cart"] == null)
            {
                ViewBag.Message = "You cart is empty.";
                return View();
            }

            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            var model = new CartVM();

            int cnt = 0;

            decimal price = 0m;

            if (Session["cart"] != null)
            {
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    cnt += item.Count;
                    price += item.Count * item.Price;
                }

                model.Count = cnt;
                model.Price = price;
            }
            else
            {
                model.Count = 0;
                model.Price = 0m;
            }

            return PartialView("_CartPartial", model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            var model = new CartVM();

            using (var db = new Db())
            {
                var product = db.Products.Find(id);

                var productInCart = cart.FirstOrDefault(p => p.ProductId == id);

                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Count = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Count++;
                }
            }

            int cnt = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                cnt += item.Count;
                price += item.Count * item.Price;
            }

            model.Count = cnt;
            model.Price = price;

            Session["cart"] = cart;

            return PartialView("_AddToCartPartial", model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            var cart = Session["cart"] as List<CartVM>;

            using (var db = new Db())
            {
                var model = cart.FirstOrDefault(c => c.ProductId == productId);

                model.Count++;

                var result = new { cnt = model.Count, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DecrementProduct(int productId)
        {
            var cart = Session["cart"] as List<CartVM>;

            using (var db = new Db())
            {
                var model = cart.FirstOrDefault(c => c.ProductId == productId);

                if (model.Count > 1)
                {
                    model.Count--;
                }
                else
                {
                    model.Count = 0;
                    cart.Remove(model);
                }

                var result = new { cnt = model.Count, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct(int productId)
        {
            var cart = Session["cart"] as List<CartVM>;

            using (var db = new Db())
            {
                var model = cart.FirstOrDefault(c => c.ProductId == productId);

                cart.Remove(model);
            }
        }
    }
}