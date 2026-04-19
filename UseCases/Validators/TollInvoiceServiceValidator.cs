namespace UseCases.Validators;

internal static class TollInvoiceServiceValidator
{
    internal static string ValidateAndNormalizeRegistrationNumber(string registrationNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationNumber);
        return registrationNumber.Trim();
    }

    internal static void ValidateYear(int year)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 2013, nameof(year));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, DateTime.UtcNow.Year, nameof(year));
    }

    internal static void ValidateMonth(int month)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1, nameof(month));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12, nameof(month));
    }
}
