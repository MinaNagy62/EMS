using EMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS_Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(a=> a.FirstName).IsRequired().HasMaxLength(50);

        builder.Property(a=> a.LastName).IsRequired().HasMaxLength(50);

        builder.Property(a=>a.Email).IsRequired();

        builder.HasIndex(a=>a.Email).IsUnique();

        builder.Property(a=> a.PasswordHash).IsRequired();
    }
}
