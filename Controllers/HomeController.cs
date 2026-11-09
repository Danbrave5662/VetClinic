using System.Diagnostics;               
using Microsoft.AspNetCore.Mvc;         
using VetClinic.Models;                 

namespace VetClinic.Controllers
{
    public class HomeController : Controller   
    {
        private readonly VetContext db;        

        public HomeController(VetContext context)   
        {
            db = context;                      
        }

        public IActionResult Index()           
        {
            ViewBag.ClientsCount = db.Clients.Count();           
            ViewBag.AnimalsCount = db.Animals.Count();           
            ViewBag.AppointmentsCount = db.Appointments.Count(); 
            ViewBag.ActiveCount = db.Appointments.Count(a =>    
                a.Status != AppointmentStatus.Completed && a.Status != AppointmentStatus.Cancelled);
            return View();                     
        }

        public IActionResult Privacy()         
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]   
        public IActionResult Error()          
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}