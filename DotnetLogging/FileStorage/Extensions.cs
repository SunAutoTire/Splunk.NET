namespace SunAuto.Logging.FileStorage;

/// <summary>
/// Extension methods
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Surround value with double quotes.
    /// </summary>
    /// <param name="value">Value to surround</param>
    /// <returns>Value surrounded w/ double-quotes.</returns>
    internal static string? Quote(this object? value) => value == null ? null : String.Concat('"', value.ToString(), '"');
}
