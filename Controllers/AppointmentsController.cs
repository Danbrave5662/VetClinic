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

        // Список прийомів з пошуком за твариною, лікарем і статусом
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
            // Поточний час без секунд для поля datetime-local
            var now = DateTime.Now;
            var model = new Appointment { DateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0) };

            FillLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            if (appointment.DateTime.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(Appointment.DateTime), "Не можна записатися на минулу дату!");
            }

            if (IsVetBusy(appointment))
            {
                ModelState.AddModelError("", "Обраний лікар уже має прийом на цю дату!");
            }

            if (ModelState.IsValid)
            {
                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            FillLists(appointment);
            return View(appointment);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = db.Appointments.Find(id);
            if (appointment == null) return NotFound();

            // Завершені й скасовані прийоми не редагуються
            if (IsClosed(appointment.Status)) return RedirectToAction(nameof(Index));

            // Прибираємо секунди, щоб працював datetime-local
            var d = appointment.DateTime;
            appointment.DateTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);

            FillLists(appointment);
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Appointment appointment)
        {
            var current = db.Appointments.AsNoTracking().FirstOrDefault(a => a.Id == appointment.Id);
            if (current == null) return NotFound();

            if (IsClosed(current.Status)) return RedirectToAction(nameof(Index));

            if (appointment.DateTime.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(Appointment.DateTime), "Не можна вказати минулу дату!");
            }

            if (IsVetBusy(appointment))
            {
                ModelState.AddModelError("", "Обраний лікар уже має прийом на цю дату!");
            }

            if (ModelState.IsValid)
            {
                appointment.Status = current.Status;   // статус тут не змінюється
                db.Update(appointment);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            FillLists(appointment);
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

        // Додати кілька послуг: лише до незавершених прийомів і без дублів
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddService(int appointmentId, int[] serviceIds)
        {
            var appointment = db.Appointments.Include(a => a.Services).FirstOrDefault(a => a.Id == appointmentId);

            if (appointment != null && !IsClosed(appointment.Status))
            {
                foreach (var serviceId in serviceIds.Distinct())
                {
                    var service = db.Services.Find(serviceId);
                    if (service != null && !appointment.Services.Any(s => s.Id == serviceId))
                    {
                        appointment.Services.Add(service);
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }

        // Початок огляду: прострочений прийом спочатку переносять через редагування дати
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartExam(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment == null) return NotFound();

            if (appointment.Status == AppointmentStatus.Planned
                && appointment.DateTime.Date >= DateTime.Today)
            {
                appointment.Status = AppointmentStatus.InProgress;
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // Інші зміни статусу: дозволені лише переходи з діаграми станів
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProceedToPayment(int id) =>
            ChangeStatus(id, AppointmentStatus.InProgress, AppointmentStatus.WaitingPayment);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteAppointment(int id) =>
            ChangeStatus(id, AppointmentStatus.WaitingPayment, AppointmentStatus.Completed);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id) =>
            ChangeStatus(id, AppointmentStatus.Planned, AppointmentStatus.Cancelled);

        private IActionResult ChangeStatus(int id, AppointmentStatus from, AppointmentStatus to)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment == null) return NotFound();

            if (appointment.Status == from)
            {
                appointment.Status = to;
                db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private static bool IsClosed(AppointmentStatus status) =>
            status == AppointmentStatus.Completed || status == AppointmentStatus.Cancelled;

        // Чи має лікар інший активний прийом на цю дату
        private bool IsVetBusy(Appointment appointment) =>
            appointment.VetId != null && db.Appointments.Any(a =>
                a.Id != appointment.Id &&
                a.VetId == appointment.VetId &&
                a.DateTime.Date == appointment.DateTime.Date &&
                a.Status != AppointmentStatus.Cancelled);

        // Списки для випадних меню (тварини, лікарі)
        private void FillLists(Appointment? a)
        {
            ViewBag.Animals = new SelectList(db.Animals, "Id", "Name", a?.AnimalId);
            ViewBag.Vets = new SelectList(
                db.Vets.Select(v => new { v.Id, DisplayName = v.FullName + " (" + v.Specialization + ")" }),
                "Id",
                "DisplayName",
                a?.VetId);
        }
    }
}