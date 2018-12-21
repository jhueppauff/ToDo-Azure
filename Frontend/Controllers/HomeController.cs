using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            // Remove Example, replace with load of items
            List<Item> items = new List<Item>()
            {
                new Item()
                {
                    Category = "Stuff",
                    Completed = false,
                    Description = "Lorem ipsum",
                    Id = Guid.NewGuid(),
                    Name = "Stuff to do"
                }
            };

            return View(items);
        }

        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Id,Name,Description,Completed,Category")] Item item)
        {
            if (ModelState.IsValid)
            {
                // ToDo Save

                // After Save redirect back to index
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("Id,Name,Description,Completed,Category")] Item item)
        {
            if (ModelState.IsValid)
            {
                // ToDo Save

                // After Save redirect back to index
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id, string category)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }

            // Replace with Load of Item
            Item item = new Item()
            {
                Category = "Stuff",
                Completed = false,
                Description = "Lorem ipsum",
                Id = Guid.NewGuid(),
                Name = "Stuff to do"
            };

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id, string category)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }

            // Replace with Load of Item
            Item item = new Item()
            {
                Category = "Stuff",
                Completed = false,
                Description = "Lorem ipsum",
                Id = Guid.NewGuid(),
                Name = "Stuff to do"
            };

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id, Category")] string id, string category)
        {
            // ToDo Delete

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id, string category)
        {
            // Replace with Load of Item
            Item item = new Item()
            {
                Category = "Stuff",
                Completed = false,
                Description = "Lorem ipsum",
                Id = Guid.NewGuid(),
                Name = "Stuff to do"
            };

            return View(item);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
