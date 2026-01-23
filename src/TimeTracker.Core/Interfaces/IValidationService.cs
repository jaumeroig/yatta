namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Servei per a la validació de registres i franges de treball.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Valida que l'hora de fi sigui posterior a l'hora d'inici.
    /// </summary>
    /// <param name="startTime">Hora d'inici.</param>
    /// <param name="endTime">Hora de fi.</param>
    /// <returns>True si és vàlid, false en cas contrari.</returns>
    bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime);

    /// <summary>
    /// Valida que l'hora de fi sigui posterior a l'hora d'inici amb missatge d'error.
    /// </summary>
    /// <param name="startTime">Hora d'inici.</param>
    /// <param name="endTime">Hora de fi.</param>
    /// <param name="errorMessage">Missatge d'error si no és vàlid.</param>
    /// <returns>True si és vàlid, false en cas contrari.</returns>
    bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime, out string errorMessage);

    /// <summary>
    /// Valida que un registre de temps no solapi amb altres registres del mateix dia.
    /// </summary>
    /// <param name="record">Registre a validar.</param>
    /// <param name="existingRecords">Registres existents del mateix dia.</param>
    /// <returns>True si no hi ha solapament, false en cas contrari.</returns>
    bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords);

    /// <summary>
    /// Valida que un registre de temps no solapi amb altres registres del mateix dia amb missatge d'error.
    /// </summary>
    /// <param name="record">Registre a validar.</param>
    /// <param name="existingRecords">Registres existents del mateix dia.</param>
    /// <param name="errorMessage">Missatge d'error si hi ha solapament.</param>
    /// <returns>True si no hi ha solapament, false en cas contrari.</returns>
    bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords, out string errorMessage);

    /// <summary>
    /// Valida que una franja de treball no solapi amb altres franges del mateix dia.
    /// </summary>
    /// <param name="slot">Franja a validar.</param>
    /// <param name="existingSlots">Franges existents del mateix dia.</param>
    /// <returns>True si no hi ha solapament, false en cas contrari.</returns>
    bool ValidateNoOverlap(WorkdaySlot slot, IEnumerable<WorkdaySlot> existingSlots);

    /// <summary>
    /// Valida que una franja de treball no solapi amb altres franges del mateix dia amb missatge d'error.
    /// </summary>
    /// <param name="slot">Franja a validar.</param>
    /// <param name="existingSlots">Franges existents del mateix dia.</param>
    /// <param name="errorMessage">Missatge d'error si hi ha solapament.</param>
    /// <returns>True si no hi ha solapament, false en cas contrari.</returns>
    bool ValidateNoOverlap(WorkdaySlot slot, IEnumerable<WorkdaySlot> existingSlots, out string errorMessage);

    /// <summary>
    /// Valida si un registre de temps és vàlid (hora de fi posterior a hora d'inici).
    /// </summary>
    /// <param name="record">Registre a validar.</param>
    /// <returns>True si és vàlid, false en cas contrari.</returns>
    bool ValidateTimeRecord(TimeRecord record);

    /// <summary>
    /// Valida si un registre de temps és vàlid amb missatge d'error.
    /// </summary>
    /// <param name="record">Registre a validar.</param>
    /// <param name="errorMessage">Missatge d'error si no és vàlid.</param>
    /// <returns>True si és vàlid, false en cas contrari.</returns>
    bool ValidateTimeRecord(TimeRecord record, out string errorMessage);

    /// <summary>
    /// Valida si una franja de treball és vàlida (hora de fi posterior a hora d'inici).
    /// </summary>
    /// <param name="slot">Franja a validar.</param>
    /// <returns>True si és vàlid, false en cas contrari.</returns>
    bool ValidateWorkdaySlot(WorkdaySlot slot);

    /// <summary>
    /// Valida si una franja de treball és vàlida amb missatge d'error.
    /// </summary>
    /// <param name="slot">Franja a validar.</param>
    /// <param name="errorMessage">Missatge d'error si no és vàlid.</param>
    /// <returns>True si és vàlid, false en cas contrari.</returns>
    bool ValidateWorkdaySlot(WorkdaySlot slot, out string errorMessage);
}
