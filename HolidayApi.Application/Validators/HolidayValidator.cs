using HolidayApi.Domain;

namespace HolidayApi.Application
{
    public static class HolidayValidator
    {
        public static bool IsValidCountryCode(string code)
        {
            return !string.IsNullOrWhiteSpace(code) && code.Trim().Length == Numerics.Two;
        }

        public static bool AreValidCountryCodes(IEnumerable<string> codes)
        {
            return codes != null && codes.All(IsValidCountryCode);
        }

        public static bool IsValidYear(int? year)
        {
            return year.HasValue && year.Value >= 1975;
        }
    }
}
