using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Areas.Admin.Controllers
{
    public class PageController : Controller
    {
        // GET: Admin/Page
        public ActionResult Index()
        {
            List<PageVM> pageList;

            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            return View(pageList);
        }

        // GET: Admin/Page/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Page/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new Db())
            {
                string slug;

                var dto = new PagesDTO();

                dto.Title = model.Title.ToUpper();

                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower(); 
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if(db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "You have added a new page";

            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;
            using(var db = new Db())
            {
                var dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("That page has not exist.");
                }
                model = new PageVM(dto);  
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var id = model.Id;

            using (var db = new Db())
            {
                var dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("That page has not exist.");
                }

                dto.Title = model.Title;

                string slug = "home";

                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                if (db.Pages.Where(p => p.Id != id).Any(p => p.Title == model.Title))
                {
                    ModelState.AddModelError("","This title has already exist");
                    return View(model);
                }
                else if (db.Pages.Where(p => p.Id != id).Any(p => p.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "This slug has already exist");
                    TempData["edited"] = true;
                    return View(model);
                }

                dto.Slug = model.Slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited the page";

            return RedirectToAction("EditPage");
        }

        public ActionResult PageDetails(int id)
        {
            PageVM model;
            using (var db = new Db())
            {
                var dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("That page has not exist.");
                }
                model = new PageVM(dto);
            }

            return View(model);
        }

        public ActionResult DeletePage(int id)
        {

            using (var db = new Db())
            {
                var dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("That page has not exist.");
                }

                db.Pages.Remove(dto);
                db.SaveChanges();

            }

            TempData["SM"] = "You have deleted the page";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (var db = new Db())
            {
                int count = 1;

                PagesDTO dto;

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
        }
    }
}