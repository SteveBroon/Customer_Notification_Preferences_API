using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleApi.Domain;

namespace SampleApi.Infrastruture.EntityConfiguration
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.EmailNotificationsEnabled).IsRequired();
            builder.Property(c => c.SmsNotificationsEnabled).IsRequired();
        }
    }
}
