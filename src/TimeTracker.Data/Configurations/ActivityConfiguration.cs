namespace TimeTracker.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Core.Models;

/// <summary>
/// Configuració de l'entitat Activity per Entity Framework.
/// </summary>
public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Color)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Active)
            .IsRequired();

        builder.HasIndex(a => a.Name)
            .IsUnique();
    }
}
