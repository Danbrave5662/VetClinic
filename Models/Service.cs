using Microsoft.EntityFrameworkCore.Migrations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public class Service
    {
        public int Id { get; set; }                                     
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }                                
        public List<Appointment> Appointments { get; set; } = new();      
    }
}

