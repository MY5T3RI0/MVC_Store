using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Account;
using MVC_Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.WebPages.Html;

namespace MVC_Store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("","Password do not match!");
                return View("CreateAccount", model);
            }

            using (var db = new Db())
            {
                if (db.Users.Any(u => u.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", $"Username {model.UserName} is taken.");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                var userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    UserName = model.UserName,
                    Password = model.Password
                };
                
                db.Users.Add(userDTO);
                db.SaveChanges();

                var id = userDTO.Id;

                var userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            TempData["SM"] = "You are now registered and can log in";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("user-profile");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isValid = false;

            using (var db = new Db())
            {
                if (db.Users.Any(u => u.UserName.Equals(model.Username) && u.Password.Equals(model.Password)))
                {
                    isValid = true;
                }

                if (!isValid)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            var userName = User.Identity.Name;

            var model = new UserNavPartialVM();

            using (var db = new Db())
            {
                var dto = db.Users.FirstOrDefault(u => u.UserName == userName);
                model.FirstName = dto.FirstName;
                model.LastName = dto.LastName;
            }

            return PartialView(model);
        }

        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            var userName = User.Identity.Name;

            UserProfileVM model;

            using (var db = new Db())
            {
                var dto = db.Users.FirstOrDefault(u => u.UserName == userName);
                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);
        }

        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Password do not match!");
                    return View("UserProfile", model);
                }
            }

            var userName = User.Identity.Name;

            using (var db = new Db())
            {
                if (db.Users.Where(u => u.Id != model.Id).Any(u => u.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", $"Username {model.UserName} is taken.");
                    model.UserName = userName;
                    return View("UserProfile", model);
                }
                var dto = db.Users.FirstOrDefault(u => u.UserName == userName);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited you profile";

            if (userName == model.UserName)
                return View("UserProfile", model);
            else
            {
                return RedirectToAction("Logout");
            }
        }

        [Authorize(Roles = "User")]
        public ActionResult Order()
        {
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();
            using (var db = new Db())
            {
                UserDTO user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                int id = user.Id;
                List<OrderVM> orders = db.Orders.Where(o => o.UserId == id).ToArray().Select(o => new OrderVM(o)).ToList();

                foreach (var order in orders)
                {
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(o => o.OrderId == order.OrederId).ToList();

                    foreach (var orderDetails in orderDetailsList)
                    {
                        ProductDTO product = db.Products.FirstOrDefault(p => p.Id == orderDetails.ProductId);

                        decimal price = product.Price;
                        string productName = product.Name;

                        productAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrederId,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            return View(ordersForUser);
        }
    }
}