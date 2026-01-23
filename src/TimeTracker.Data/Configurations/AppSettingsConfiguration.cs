namespace TimeTracker.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Core.Models;

/// <summary>
/// Configuració de l'entitat AppSettings per Entity Framework.
/// </summary>
public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.ToTable("AppSettings");

        // AppSettings serà una taula amb una sola fila
        // Utilitzem un valor de clau fix
        builder.HasKey(s => s.Theme);

        builder.Property(s => s.Theme)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Notifications)
            .IsRequired();

        builder.Property(s => s.WorkdayTotalTime)
            .IsRequired();
    }
}
