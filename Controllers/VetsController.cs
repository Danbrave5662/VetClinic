using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    public class VetsController : Controller
    {
        private readonly VetContext db;

        public VetsController(VetContext context)
        {
            db = context;
        }

        public IActionResult Index(string searchName, string searchSpec)
        {
            ViewBag.CurrentName = searchName;
            ViewBag.CurrentSpec = searchSpec;

            var vets = db.Vets.AsQueryable();

            // Фільтр за ПІБ
            if (!string.IsNullOrEmpty(searchName))
            {
                vets = vets.Where(v => v.FullName.Contains(searchName));
            }

            // Фільтр за спеціалізацією
            if (!string.IsNullOrEmpty(searchSpec))
            {
                vets = vets.Where(v => v.Specialization == searchSpec);
            }

            return View(vets.ToList());
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var vet = db.Vets
                .Include(v => v.Appointments)
                .FirstOrDefault(m => m.Id == id);

            if (vet == null) return NotFound();

            return View(vet);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Vet vet)
        {
            if (ModelState.IsValid)
            {
                db.Vets.Add(vet);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(vet);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var vet = db.Vets.Find(id);
            if (vet == null) return NotFound();

            return View(vet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Vet vet)
        {
            if (id != vet.Id) return NotFound();

            if (ModelState.IsValid)
            {
                db.Update(vet);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(vet);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var vet = db.Vets.FirstOrDefault(m => m.Id == id);
            if (vet == null) return NotFound();

            return View(vet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var vet = db.Vets
                .Include(v => v.Appointments)
                .FirstOrDefault(v => v.Id == id);

            if (vet != null)
            {
                // Історія прийомів зберігається, лікар в них стає не призначеним
                foreach (var appointment in vet.Appointments)
                {
                    appointment.VetId = null;
                }

                db.Vets.Remove(vet);
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}