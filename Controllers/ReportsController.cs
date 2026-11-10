using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Models;

namespace VetClinic.Controllers
{
    public class ReportsController : Controller
    {
        private readonly VetContext db;

        public ReportsController(VetContext context)
        {
            db = context;
        }

        // Виручка за період: сума цін послуг у завершених прийомах
        public IActionResult Revenue(DateTime? dateFrom, DateTime? dateTo)
        {
            var start = dateFrom ?? DateTime.Today.AddMonths(-1);
            var end = (dateTo ?? DateTime.Today).Date.AddDays(1);

            var query = db.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed
                         && a.DateTime >= start && a.DateTime < end);

            ViewBag.From = start;
            ViewBag.To = end.AddDays(-1);
            ViewBag.Total = query.SelectMany(a => a.Services).Sum(s => s.Price);

            var list = query.Include(a => a.Animal).Include(a => a.Services)
                            .OrderBy(a => a.DateTime).ToList();
            return View(list);
        }
    }
}