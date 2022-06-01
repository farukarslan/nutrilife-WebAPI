using Microsoft.EntityFrameworkCore;
using nutrilifeAPI.Models;

namespace nutrilifeAPI.Context
{
    public class nutrilifeDbContext : DbContext
    {
        public nutrilifeDbContext(DbContextOptions<nutrilifeDbContext> options) : base(options)
        {

        }

        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<NotificationMessages> NotificationMessages { get; set; }
        public DbSet<Nutritionists> Nutritionists { get; set; }
        public DbSet<PatientInformations> PatientInformations { get; set; }
        public DbSet<Patients> Patients { get; set; }
        public DbSet<Ratings> Ratings { get; set; }
        public DbSet<Recipes> Recipes { get; set; }
        public DbSet<NutritionistHavingPatient> NutritionistHavingPatient { get; set; }
    }
}
