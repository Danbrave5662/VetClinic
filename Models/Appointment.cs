namespace VetClinic.Models
{
    public class Appointment
    {
        public int Id { get; set; }                                       
        public DateTime DateTime { get; set; }                            
        public string Complaints { get; set; } = string.Empty;            
        public AppointmentStatus Status { get; set; }                     
        public int AnimalId { get; set; }                                 
        public Animal? Animal { get; set; }
        public int? VetId { get; set; }                                   
        public Vet? Vet { get; set; }                                     
        public List<Service> Services { get; set; } = new();             
    }
}