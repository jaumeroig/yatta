namespace Yatta.Core.Models;

/// <summary>
/// Represents the annual quota configuration for a specific year.
/// </summary>
public class AnnualQuota
{
    /// <summary>
    /// Unique identifier for the annual quota.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Year for which this quota applies.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Number of vacation days available for the year.
    /// </summary>
    public int VacationDays { get; set; }

    /// <summary>
    /// Number of free choice days available for the year.
    /// </summary>
    public int FreeChoiceDays { get; set; }

    /// <summary>
    /// Number of intensive workdays available for the year.
    /// </summary>
    public int IntensiveDays { get; set; }
}
