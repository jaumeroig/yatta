namespace TimeTracker.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Core.Models;

/// <summary>
/// Configuration of the AppSettings entity for Entity Framework.
/// </summary>
public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.ToTable("AppSettings");

        // AppSettings will be a table with a single row (singleton)
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .IsRequired()
            .ValueGeneratedNever(); // No auto-increment to guarantee Id = 1

        builder.Property(s => s.Theme)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Notifications)
            .IsRequired();

        builder.Property(s => s.WorkdayTotalTime)
            .IsRequired();

        builder.Property(s => s.RetentionPolicy)
            .IsRequired()
            .HasDefaultValue(RetentionPolicy.Forever);

        builder.Property(s => s.CustomRetentionDays)
            .IsRequired()
            .HasDefaultValue(365);

        builder.Property(s => s.GlobalHotkey)
            .IsRequired(false);

        builder.Property(s => s.HistoricSortAscending)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.KeepNotificationsVisible)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
