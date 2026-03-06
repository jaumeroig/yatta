namespace Yatta.App.Extensions;


internal static class TimeSpanExtensions
{
    extension(TimeSpan timeSpan)
    {
        /// <summary>
        /// Returns a string representation of the TimeSpan in the format "Xh Ym", where X is the total hours and Y is the remaining minutes.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format.</param>
        /// <param name="showSign">Whether to show the sign for positive and negative durations.</param>
        /// <returns>A formatted string representing the duration.</returns>
        public string FormatDuration(bool showSign = false)
        {
            var totalMinutes = (int)timeSpan.TotalMinutes;
            var absMinutes = Math.Abs(totalMinutes);
            var h = absMinutes / 60;
            var m = absMinutes % 60;
            var format = Resources.Resources.Format_Duration;
            var duration = string.Format(format, h, m);

            if (!showSign || totalMinutes == 0)
                return duration;

            var sign = totalMinutes < 0 ? "-" : "+";
            return sign + duration;
        }
    }
}
