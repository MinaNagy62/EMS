using EMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS_Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.DateOfBirth)
            .IsRequired();

        builder.Property(e => e.HireDate)
            .IsRequired();

        builder.Property(e => e.Salary)
            .HasPrecision(18, 2);

        builder.Property(e => e.Gender)
            .IsRequired();

        builder.Property(e => e.JobTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
}
