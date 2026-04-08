namespace Yatta.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Repository implementation for managing annual quota configurations.
/// </summary>
public class AnnualQuotaRepository : IAnnualQuotaRepository
{
    private readonly YattaDbContext _context;

    public AnnualQuotaRepository(YattaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the annual quota configuration for a specific year.
    /// </summary>
    public async Task<AnnualQuota?> GetByYearAsync(int year)
    {
        return await _context.AnnualQuotas
            .FirstOrDefaultAsync(q => q.Year == year);
    }

    /// <summary>
    /// Saves an annual quota configuration (insert or update).
    /// </summary>
    public async Task<AnnualQuota> SaveAsync(AnnualQuota quota)
    {
        if (quota == null)
            throw new ArgumentNullException(nameof(quota));

        var existing = await _context.AnnualQuotas
            .FirstOrDefaultAsync(q => q.Year == quota.Year);

        if (existing != null)
        {
            // Update existing
            existing.VacationDays = quota.VacationDays;
            existing.FreeChoiceDays = quota.FreeChoiceDays;
            existing.IntensiveDays = quota.IntensiveDays;
            _context.AnnualQuotas.Update(existing);
        }
        else
        {
            // Insert new
            quota.Id = Guid.NewGuid();
            await _context.AnnualQuotas.AddAsync(quota);
        }

        await _context.SaveChangesAsync();
        return existing ?? quota;
    }

    /// <summary>
    /// Deletes the annual quota configuration for a specific year.
    /// </summary>
    public async Task DeleteAsync(int year)
    {
        var quota = await _context.AnnualQuotas
            .FirstOrDefaultAsync(q => q.Year == year);

        if (quota != null)
        {
            _context.AnnualQuotas.Remove(quota);
            await _context.SaveChangesAsync();
        }
    }
}
