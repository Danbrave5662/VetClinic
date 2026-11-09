namespace VetClinic.Models
{
    public class Vet
    {
        public int Id { get; set; }                                       
        public string FullName { get; set; } = string.Empty;             
        public string Phone { get; set; } = string.Empty;                 
        public string Specialization { get; set; } = string.Empty;        
        public List<Appointment> Appointments { get; set; } = new();      
    }
}