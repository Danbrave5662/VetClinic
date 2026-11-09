using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Models;
using System.Linq;

namespace VetClinic.Controllers
{
    public class AnimalsController : Controller
    {
        private readonly VetContext db;

        public AnimalsController(VetContext context)
        {
            db = context;
        }

        public IActionResult Index(string searchName, string searchSpecies, string searchOwner)
        {
            ViewBag.CurrentName = searchName;
            ViewBag.CurrentSpecies = searchSpecies;
            ViewBag.CurrentOwner = searchOwner;

            var animals = db.Animals.Include(a => a.Client).AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                animals = animals.Where(a => a.Name.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchSpecies))
            {
                animals = animals.Where(a => a.Species.Contains(searchSpecies));
            }

            if (!string.IsNullOrEmpty(searchOwner))
            {
                animals = animals.Where(a => a.Client != null && a.Client.FullName.Contains(searchOwner));
            }

            return View(animals.ToList());
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var animal = db.Animals
                .Include(a => a.Client)
                .Include(a => a.Appointments)
                .FirstOrDefault(m => m.Id == id);

            if (animal == null) return NotFound();

            return View(animal);
        }

        public IActionResult Create()
        {
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Animal animal)
        {
            if (ModelState.IsValid)
            {
                db.Animals.Add(animal);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "FullName", animal.ClientId);
            return View(animal);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var animal = db.Animals.Find(id);
            if (animal == null) return NotFound();

            ViewBag.ClientId = new SelectList(db.Clients, "Id", "FullName", animal.ClientId);
            return View(animal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Animal animal)
        {
            if (id != animal.Id) return NotFound();

            if (ModelState.IsValid)
            {
                db.Update(animal);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ClientId = new SelectList(db.Clients, "Id", "FullName", animal.ClientId);
            return View(animal);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var animal = db.Animals
                .Include(a => a.Client)
                .FirstOrDefault(m => m.Id == id);

            if (animal == null) return NotFound();

            return View(animal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var animal = db.Animals.Find(id);
            if (animal != null)
            {
                db.Animals.Remove(animal);
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}