using Microsoft.EntityFrameworkCore;
using SampleApi.Domain;

namespace SampleApi.Infrastruture
{
    public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(CustomerDbContext).Assembly);

            modelBuilder.Entity<Domain.Customer>().HasData(
                    new() { Id = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0"), EmailNotificationsEnabled = true, SmsNotificationsEnabled = false },
                    new() { Id = Guid.Parse("af285565-031a-4df2-9f82-98f101dd08fa"), EmailNotificationsEnabled = false, SmsNotificationsEnabled = true },
                    new() { Id = Guid.Parse("abb0125e-b3ed-4a66-910b-492f431f9486"), EmailNotificationsEnabled = true, SmsNotificationsEnabled = true },
                    new() { Id = Guid.Parse("e2906422-3298-4b81-8a78-39544d94268e"), EmailNotificationsEnabled = false, SmsNotificationsEnabled = false },
                    new() { Id = Guid.Parse("6d4977a7-a9a4-4b72-b75d-00569f699602"), EmailNotificationsEnabled = true, SmsNotificationsEnabled = false },
                    new() { Id = Guid.Parse("de5de5f6-0d9d-472d-98be-b45db46f84ca"), EmailNotificationsEnabled = false, SmsNotificationsEnabled = true });
        }
    }
}
