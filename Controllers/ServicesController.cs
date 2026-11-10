using Microsoft.AspNetCore.Mvc;
using VetClinic.Models;
using System.Linq;

namespace VetClinic.Controllers
{
    public class ServicesController : Controller
    {
        private readonly VetContext db;

        public ServicesController(VetContext context)
        {
            db = context;
        }

        public IActionResult Index(string searchString, decimal? maxPrice, string sortOrder)
        {
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.PriceSortParam = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";

            var services = db.Services.AsQueryable();

            // Пошук за назвою
            if (!string.IsNullOrEmpty(searchString))
            {
                services = services.Where(s => s.Name.Contains(searchString));
            }

            // Фільтр за максимальною ціною
            if (maxPrice.HasValue)
            {
                services = services.Where(s => s.Price <= maxPrice.Value);
            }

            // Сортування за ціною
            switch (sortOrder)
            {
                case "price_desc":
                    services = services.OrderByDescending(s => s.Price);
                    break;
                default:
                    services = services.OrderBy(s => s.Price);
                    break;
            }

            return View(services.ToList());
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var service = db.Services.FirstOrDefault(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Service service)
        {
            if (ModelState.IsValid)
            {
                db.Services.Add(service);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = db.Services.Find(id);
            if (service == null) return NotFound();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Service service)
        {
            if (id != service.Id) return NotFound();

            if (ModelState.IsValid)
            {
                db.Update(service);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = db.Services.FirstOrDefault(m => m.Id == id);
            if (service == null) return NotFound();

            ViewBag.UsedCount = UsedCount(service.Id);
            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var service = db.Services.Find(id);
            if (service == null) return RedirectToAction(nameof(Index));

            // Послугу з прийомів не видаляємо, щоб не змінювати рахунки й звіт
            if (UsedCount(id) > 0) return RedirectToAction(nameof(Delete), new { id });

            db.Services.Remove(service);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Кількість прийомів, у яких є ця послуга
        private int UsedCount(int serviceId) =>
            db.Appointments.Count(a => a.Services.Any(s => s.Id == serviceId));
    }
}