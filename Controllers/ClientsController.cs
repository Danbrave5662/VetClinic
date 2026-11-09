using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Models;
using System.Linq;

namespace VetClinic.Controllers
{
    public class ClientsController : Controller
    {
        private readonly VetContext db;

        public ClientsController(VetContext context)
        {
            db = context;
        }

        public IActionResult Index(string searchName, string searchPhone)
        {
            ViewBag.CurrentName = searchName;
            ViewBag.CurrentPhone = searchPhone;

            var clients = db.Clients.Include(c => c.Animals).AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                clients = clients.Where(c => c.FullName.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchPhone))
            {
                clients = clients.Where(c => c.Phone.Contains(searchPhone));
            }

            return View(clients.ToList());
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var client = db.Clients
                .Include(c => c.Animals)
                .FirstOrDefault(m => m.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Client client)
        {
            if (ModelState.IsValid)
            {
                db.Clients.Add(client);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = db.Clients.Find(id);
            if (client == null) return NotFound();

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Client client)
        {
            if (id != client.Id) return NotFound();

            if (ModelState.IsValid)
            {
                db.Update(client);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = db.Clients.FirstOrDefault(m => m.Id == id);
            if (client == null) return NotFound();

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var client = db.Clients.Find(id);
            if (client != null)
            {
                db.Clients.Remove(client);
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}