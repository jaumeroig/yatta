namespace TimeTracker.Core.Services;

using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementació del servei per a la validació de registres i franges de treball.
/// Nota: Els missatges d'error s'hauran de traduir a través del servei de localització.
/// </summary>
public class ValidationService : IValidationService
{
    /// <inheritdoc/>
    public bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        return endTime > startTime;
    }

    /// <inheritdoc/>
    public bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime, out string errorMessage)
    {
        if (endTime > startTime)
        {
            errorMessage = string.Empty;
            return true;
        }

        // Clau de recursos: Validation_EndTimeAfterStartTime
        errorMessage = "Validation_EndTimeAfterStartTime";
        return false;
    }

    /// <inheritdoc/>
    public bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords)
    {
        return ValidateNoOverlap(record, existingRecords, out _);
    }

    /// <inheritdoc/>
    public bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords, out string errorMessage)
    {
        errorMessage = string.Empty;

        // Si el registre no té hora de fi, no es pot validar solapament
        if (!record.EndTime.HasValue)
        {
            return true;
        }

        foreach (var existing in existingRecords)
        {
            // No validar contra el mateix registre
            if (existing.Id == record.Id)
            {
                continue;
            }

            // Si el registre existent no té hora de fi, no es pot validar solapament
            if (!existing.EndTime.HasValue)
            {
                continue;
            }

            // Comprovar si hi ha solapament
            // Solapament si:
            // - L'inici del nou registre està entre l'inici i fi d'un existent
            // - La fi del nou registre està entre l'inici i fi d'un existent
            // - El nou registre envolta completament un existent
            if ((record.StartTime >= existing.StartTime && record.StartTime < existing.EndTime.Value) ||
                (record.EndTime.Value > existing.StartTime && record.EndTime.Value <= existing.EndTime.Value) ||
                (record.StartTime <= existing.StartTime && record.EndTime.Value >= existing.EndTime.Value))
            {
                // Clau de recursos: Validation_OverlappingRecord (amb format de 2 arguments: startTime, endTime)
                errorMessage = $"Validation_OverlappingRecord|{existing.StartTime:HH\\:mm}|{existing.EndTime.Value:HH\\:mm}";
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public bool ValidateNoOverlap(WorkdaySlot slot, IEnumerable<WorkdaySlot> existingSlots)
    {
        return ValidateNoOverlap(slot, existingSlots, out _);
    }

    /// <inheritdoc/>
    public bool ValidateNoOverlap(WorkdaySlot slot, IEnumerable<WorkdaySlot> existingSlots, out string errorMessage)
    {
        errorMessage = string.Empty;

        foreach (var existing in existingSlots)
        {
            // No validar contra la mateixa franja
            if (existing.Id == slot.Id)
            {
                continue;
            }

            // Comprovar si hi ha solapament
            // Solapament si:
            // - L'inici de la nova franja està entre l'inici i fi d'una existent
            // - La fi de la nova franja està entre l'inici i fi d'una existent
            // - La nova franja envolta completament una existent
            if ((slot.StartTime >= existing.StartTime && slot.StartTime < existing.EndTime) ||
                (slot.EndTime > existing.StartTime && slot.EndTime <= existing.EndTime) ||
                (slot.StartTime <= existing.StartTime && slot.EndTime >= existing.EndTime))
            {
                // Clau de recursos: Validation_OverlappingSlot (amb format de 2 arguments: startTime, endTime)
                errorMessage = $"Validation_OverlappingSlot|{existing.StartTime:HH\\:mm}|{existing.EndTime:HH\\:mm}";
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public bool ValidateTimeRecord(TimeRecord record)
    {
        return ValidateTimeRecord(record, out _);
    }

    /// <inheritdoc/>
    public bool ValidateTimeRecord(TimeRecord record, out string errorMessage)
    {
        errorMessage = string.Empty;

        // Si no hi ha hora de fi, el registre és vàlid (pot estar en curs)
        if (!record.EndTime.HasValue)
        {
            return true;
        }

        // Validar que l'hora de fi sigui posterior a l'hora d'inici
        return ValidateTimeRange(record.StartTime, record.EndTime.Value, out errorMessage);
    }

    /// <inheritdoc/>
    public bool ValidateWorkdaySlot(WorkdaySlot slot)
    {
        return ValidateWorkdaySlot(slot, out _);
    }

    /// <inheritdoc/>
    public bool ValidateWorkdaySlot(WorkdaySlot slot, out string errorMessage)
    {
        // Validar que l'hora de fi sigui posterior a l'hora d'inici
        return ValidateTimeRange(slot.StartTime, slot.EndTime, out errorMessage);
    }
}
