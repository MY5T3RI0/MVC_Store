using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MVC_Store.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using (var db = new Db())
            {
                categoryVMList = db.Categories
                                    .ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x))
                                    .ToList();
            }

            return View(categoryVMList);
        }

        //POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;

            using (var db = new Db())
            {
                if (db.Categories.Any(c => c.Name == catName))
                {
                    return "titletaken";
                }

                CategoryDTO dto = new CategoryDTO();

                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();
            }

            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (var db = new Db())
            {
                int count = 1;

                CategoryDTO dto;

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
        }

        public ActionResult DeleteCategory(int id)
        {

            using (var db = new Db())
            {
                var dto = db.Categories.Find(id);

                if (dto == null)
                {
                    return Content("That page has not exist.");
                }

                db.Categories.Remove(dto);
                db.SaveChanges();

            }

            TempData["SM"] = "You have deleted a category";

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {

            using (var db = new Db())
            {
                if (db.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }

                var dto = db.Categories.Find(id);

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                db.SaveChanges();

            }

            return "ok";
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            var model = new ProductVM();

            using (var db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                using (var db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            int id;

            using (var db = new Db())
            {
                if (db.Products.Any(p => p.Name == model.Name))
                {
                    ModelState.AddModelError("", "This name of product already exists");
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

                var dto = new ProductDTO();

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description.ToString();
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;

                var catDTO = db.Categories.FirstOrDefault(c => c.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.Products.Add(dto);
                db.SaveChanges();

                id = dto.Id;
            }

            TempData["SM"] = "You have added a product!";

            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            var paths = new List<string> { pathString1, pathString2, pathString3, pathString4, pathString5 };

            foreach(var path in paths)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            if (file != null && file.ContentLength > 0)
            {
                var ext = file.ContentType.ToLower();
                if 
                (
                    ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                )
                {
                    using (var db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                        ModelState.AddModelError("", "The image was not upload - wrong image extention");
                        return View(model);
                    }
                }

                string imageName = file.FileName.Replace(" ", "_");

                using (var db = new Db())
                {
                    var product = db.Products.Find(id);
                    product.ImageName = imageName;

                    db.SaveChanges();
                }

                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
            }

            return RedirectToAction("AddProduct");
        }

        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVM> listOfProductVM;

            var pageNumber = page ?? 1;

            using (var db = new Db())
            {
                listOfProductVM = db.Products.ToArray()
                    .Where(p => catId == null || catId == 0 || p.CategoryId == catId)
                    .Select(p => new ProductVM(p))
                    .ToList();
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "id", "Name");

                ViewBag.SelectedCat = catId.ToString();
            }

            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            return View(listOfProductVM);
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            ProductVM model;

            using (var db = new Db())
            {
                var dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content("That product does not exist");
                }

                model = new ProductVM(dto);
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(f => Path.GetFileName(f));
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            var id = model.Id;

            using (var db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                model.GalleryImages = Directory
                   .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                   .Select(f => Path.GetFileName(f));

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Model is not valid");
                    return View(model);
                }

                if (db.Products.Where(p => p.Id != id).Any(p => p.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }

                var dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                var catDTO = db.Categories.FirstOrDefault(c => c.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited a product";

            if (file != null && file.ContentLength > 0)
            {
                var ext = file.ContentType.ToLower();
                if
                (
                    ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                )
                {
                    using (var db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not upload - wrong image extention");
                        return View(model);
                    }
                }

                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                var di1 = new DirectoryInfo(pathString1);
                var di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }
                foreach (var file2 in di2.GetFiles())
                {
                    file2.Delete();
                }

                var imageName = file.FileName.Replace(" ", "_");

                using (var db = new Db())
                {
                    var product = db.Products.Find(id);
                    product.ImageName = imageName;

                    db.SaveChanges();
                }

                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }

            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {

            using (var db = new Db())
            {
                var dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content("That product has not exist.");
                }

                db.Products.Remove(dto);
                db.SaveChanges();

            }

            TempData["SM"] = "You have deleted a product";

            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString2))
            {
                Directory.Delete(pathString2, true);
            }

            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            foreach (string fileName in Request.Files)
            {
                var file = Request.Files[fileName];

                if (file != null && file.ContentLength > 0)
                {
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img.Save(path2);
                }
            }
        }


        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\" + imageName);
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs\\" + imageName);

            if (System.IO.File.Exists(pathString1))
            {
                System.IO.File.Delete(pathString1);
            }
            if (System.IO.File.Exists(pathString2))
            {
                System.IO.File.Delete(pathString2);
            }
        }

    }
}