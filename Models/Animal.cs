using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace VetClinic.Models
{
    public class Animal
    {
        public int Id { get; set; }                                       
        public string Name { get; set; } = string.Empty;                  
        public string Species { get; set; } = string.Empty;               
        public string Breed { get; set; } = string.Empty;

        [Range(0, 40, ErrorMessage = "Роки: 0–40")]
        public int Age { get; set; }

        [Range(0, 11, ErrorMessage = "Місяці: 0–11")]
        public int AgeMonths { get; set; }

        // Текст для відображення
        [NotMapped]
        public string AgeText => Age > 0 && AgeMonths > 0 ? $"{Age} р. {AgeMonths} міс."
                               : Age > 0 ? $"{Age} р."
                               : $"{AgeMonths} міс.";
        public int ClientId { get; set; }                                 
        public Client? Client { get; set; }                              
        public List<Appointment> Appointments { get; set; } = new();    
    }
}