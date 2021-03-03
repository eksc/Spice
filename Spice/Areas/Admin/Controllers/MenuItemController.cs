using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Categories = _db.Categories,
                MenuItem = new Models.MenuItem()
            };
        }

        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItems.Include(m => m.Category)
                                               .Include(m => m.SubCategory)
                                               .ToListAsync();

            return View(menuItems);
        }

        //GET Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }


        //POST Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItems.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFormDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                var uploads = Path.Combine(webRootPath, "images");
                var extensions = Path.GetExtension(files[0].FileName);

                using(var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extensions), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFormDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extensions;
            }
            else
            {
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath+@"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFormDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems.Include(m => m.Category)
                                                     .Include(m => m.SubCategory)
                                                     .SingleOrDefaultAsync(m => m.Id == id);

            MenuItemVM.SubCategories = await _db.SubCategories.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId)
                                                              .ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }


        //POST Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategories = await _db.SubCategories.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId)
                                                                  .ToListAsync();
                return View(MenuItemVM);
            }

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFormDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                var imagePath = Path.Combine(webRootPath, menuItemFormDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFormDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_new;
            }

            menuItemFormDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFormDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFormDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFormDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFormDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFormDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems.Include(m => m.Category)
                                                     .Include(m => m.SubCategory)
                                                     .SingleOrDefaultAsync(m => m.Id == id);

            MenuItemVM.SubCategories = await _db.SubCategories.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId)
                                                              .ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems.Include(m => m.Category)
                                                     .Include(m => m.SubCategory)
                                                     .SingleOrDefaultAsync(m => m.Id == id);

            MenuItemVM.SubCategories = await _db.SubCategories.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId)
                                                              .ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems.Include(m => m.Category)
                                                     .Include(m => m.SubCategory)
                                                     .SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, MenuItemVM.MenuItem.Image.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _db.MenuItems.Remove(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}