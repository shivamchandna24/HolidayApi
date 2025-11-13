
namespace HolidayApi.Application
{
    public interface IHolidayService
    {
        Task<List<UpsertHolidayDto>> InsertOrUpdateHolidaysFromApiAsync(int year, string countryCode);

        Task<List<PreviousHolidayDto>> GetPreviousThreeHolidaysAsync(string countryCode);

        Task<List<NonWeekendHolidayDto>> GetNonWeekendHolidayCountAsync(int year, IEnumerable<string> countryCodes);

        Task<List<SharedHolidayDto>> GetSharedCelebrationDatesAsync(int year, string firstCountry, string secondCountry);

    }
}
