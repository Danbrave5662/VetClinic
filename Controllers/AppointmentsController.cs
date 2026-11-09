using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Models;
using System.Linq;
using System.Collections.Generic;

namespace VetClinic.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly VetContext db;

        public AppointmentsController(VetContext context)
        {
            db = context;
        }

        public IActionResult Index(string searchAnimal, string searchVet, string searchStatus)
        {
            ViewBag.CurrentAnimal = searchAnimal;
            ViewBag.CurrentVet = searchVet;
            ViewBag.CurrentStatus = searchStatus;

            var appointments = db.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Vet)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchAnimal))
            {
                appointments = appointments.Where(a => a.Animal.Name.Contains(searchAnimal));
            }

            if (!string.IsNullOrEmpty(searchVet))
            {
                appointments = appointments.Where(a => a.Vet != null && a.Vet.FullName.Contains(searchVet));
            }

            if (!string.IsNullOrEmpty(searchStatus))
            {
                appointments = appointments.Where(a => a.Status.ToString() == searchStatus);
            }

            return View(appointments.OrderByDescending(a => a.DateTime).ToList());
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = db.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Vet)
                .Include(a => a.Services)
                .FirstOrDefault(m => m.Id == id);

            if (appointment == null) return NotFound();

            ViewBag.ServicesList = new SelectList(db.Services, "Id", "Name");
            return View(appointment);
        }

        public IActionResult Create()
        {
            ViewBag.Animals = new SelectList(db.Animals, "Id", "Name");
            ViewBag.Vets = new SelectList(
                db.Vets.Select(v => new { Id = v.Id, DisplayName = v.FullName + " (" + v.Specialization + ")" }),
                "Id",
                "DisplayName"
            );
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            bool isVetBusy = db.Appointments.Any(a =>
                a.VetId == appointment.VetId &&
                a.DateTime.Date == appointment.DateTime.Date &&
                a.Status != AppointmentStatus.Cancelled);

            if (isVetBusy)
            {
                ModelState.AddModelError("", "Обраний лікар уже має прийом на цю дату!");
            }

            if (ModelState.IsValid)
            {
                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Animals = new SelectList(db.Animals, "Id", "Name", appointment.AnimalId);
            ViewBag.Vets = new SelectList(
                db.Vets.Select(v => new { Id = v.Id, DisplayName = v.FullName + " (" + v.Specialization + ")" }),
                "Id",
                "DisplayName",
                appointment.VetId
            );
            return View(appointment);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = db.Appointments
                .Include(a => a.Animal)
                .Include(a => a.Vet)
                .FirstOrDefault(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment != null)
            {
                db.Appointments.Remove(appointment);
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddService(int appointmentId, int serviceId)
        {
            var appointment = db.Appointments.Include(a => a.Services).FirstOrDefault(a => a.Id == appointmentId);
            var service = db.Services.Find(serviceId);

            if (appointment != null && service != null)
            {
                if (appointment.Services == null)
                {
                    appointment.Services = new List<Service>();
                }
                appointment.Services.Add(service);
                db.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProceedToPayment(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.WaitingPayment;
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteAppointment(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Completed;
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartExam(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.InProgress;
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}