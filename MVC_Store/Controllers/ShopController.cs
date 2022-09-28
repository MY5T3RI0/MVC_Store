using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            List<CategoryVM> categoryVMList;

            using (var db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(c => c.Sorting)
                    .Select(c => new CategoryVM(c)).ToList();
            }

            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        public ActionResult Category(string name)
        {
            List<ProductVM> productVMList;

            using (var db = new Db())
            {
                var categoryDTO = db.Categories.Where(c => c.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                productVMList = db.Products.ToArray().Where(p => p.CategoryId == catId)
                    .Select(p => new ProductVM(p)).ToList();
                var productCat = db.Products.Where(p => p.CategoryId == catId).FirstOrDefault();

                if (productCat == null)
                {
                    var catName = db.Categories.Where(c => c.Slug == name).Select(c => c.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }

            return View(productVMList);
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            ProductVM model;
            ProductDTO dto;

            int id;

            using (var db = new Db())
            {
                if (!db.Products.Any(p => p.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                dto = db.Products.Where(p => p.Slug == name).FirstOrDefault();

                id = dto.Id;
            }
            model = new ProductVM(dto);
            model.GalleryImages = Directory
                .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(f => Path.GetFileName(f));

            return View("ProductDetails", model);
        }
    }
}