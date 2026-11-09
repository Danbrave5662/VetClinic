using Microsoft.EntityFrameworkCore;   

namespace VetClinic.Models
{
    public class VetContext : DbContext   
    {
        public VetContext(DbContextOptions<VetContext> options) : base(options) { }   

        public DbSet<Client> Clients { get; set; }               
        public DbSet<Animal> Animals { get; set; }               
        public DbSet<Appointment> Appointments { get; set; }     
        public DbSet<Service> Services { get; set; }
        public DbSet<Vet> Vets { get; set; }
    }
}