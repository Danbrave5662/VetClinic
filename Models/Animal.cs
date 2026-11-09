namespace VetClinic.Models
{
    public class Animal
    {
        public int Id { get; set; }                                       
        public string Name { get; set; } = string.Empty;                  
        public string Species { get; set; } = string.Empty;               
        public string Breed { get; set; } = string.Empty;                
        public int Age { get; set; }                                      
        public int ClientId { get; set; }                                 
        public Client? Client { get; set; }                              
        public List<Appointment> Appointments { get; set; } = new();    
    }
}