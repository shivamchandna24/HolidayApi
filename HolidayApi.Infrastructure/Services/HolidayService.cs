using HolidayApi.Application;
using HolidayApi.Domain;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace HolidayApi.Infrastructure
{
    public class HolidayService : IHolidayService
    {
        private readonly HolidayContext db;
        private readonly IHttpClientFactory _httpClientFactory;
        public HolidayService(HolidayContext db, IHttpClientFactory httpClientFactory)
        {
            this.db = db;
            _httpClientFactory = httpClientFactory;
        }

        #region Methods

        /// <summary>
        /// Service mehtod to create records in DB. This insert or updates the records..
        /// Extensive exception handling done in line and using middleware to handle all possible exceptions at one place.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        /// <exception cref="ExternalServiceException"></exception>
        public async Task<List<UpsertHolidayDto>> InsertOrUpdateHolidaysFromApiAsync(int year, string countryCode)
        {
            var client = _httpClientFactory.CreateClient();
            var nagerHolidayUrl = $"{ApiEndpoints.NagerHolidayURL}/{year}/{countryCode}";

            HttpResponseMessage response = await client.GetAsync(nagerHolidayUrl);
            // Stop here if no success code
            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadFromJsonAsync<ApiErrorDto>();

                string apiErrorMessage = apiError != null ? $"{ExceptionMessages.ApiError} {apiError.Status}: {apiError.Title}. " +
                      $"{string.Join("; ", apiError.Errors.SelectMany(kv => kv.Value))}"
                    : $"{ExceptionMessages.ApiStatusCode} {response.StatusCode}.";

                throw new ExternalServiceException(apiErrorMessage);
            }

            var holidays = await client.GetFromJsonAsync<List<Holiday>>(nagerHolidayUrl);
            if (holidays == null || !holidays.Any())
            {
                return [];
            }

            var upsertedHolidays = new List<Holiday>();
            var existingHolidays = (await db.Holidays.Where(h => h.CountryCode == countryCode && h.Date.Year == year).Select(h => h.Date).ToListAsync()).ToHashSet();
            var newHolidays = holidays.Where(h => !existingHolidays.Contains(h.Date)).ToList(); db.Holidays.AddRange(newHolidays);

            foreach (var newHoliday in newHolidays)
            {
                upsertedHolidays.Add(newHoliday);
            }

            if (upsertedHolidays.Count != 0)
            {
                await db.SaveChangesAsync();
            }

            return upsertedHolidays.Select(upsertHoliday => new UpsertHolidayDto
            {
                Counties = upsertHoliday.Counties,
                CountryCode = upsertHoliday.CountryCode,
                Date = upsertHoliday.Date,
                Fixed = upsertHoliday.Fixed,
                Global = upsertHoliday.Global,
                LaunchYear = upsertHoliday.LaunchYear,
                LocalName = upsertHoliday.LocalName,
                Name = upsertHoliday.Name,
                Types = upsertHoliday.Types
            }).ToList();

        }

        /// <summary>
        /// Service method to fetch last three holidays for a given country. 
        /// It could be many years back, and it would depend on the most recent in chornological order.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        public async Task<List<PreviousHolidayDto>> GetPreviousThreeHolidaysAsync(string countryCode)
        {
            var today = DateTime.UtcNow.Date;
            return await db.Holidays
                           .AsNoTracking().Where(h => h.CountryCode == countryCode && h.Date <= today)
                           .OrderByDescending(h => h.Date)
                           .Select(h => new PreviousHolidayDto
                           {
                               Date = h.Date,
                               Name = h.Name
                           })
                           .Take(3)
                           .ToListAsync();
        }

        /// <summary>
        /// Service method to find holidays for a country/countries for given year falling on weekdays.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="countryCodes"></param>
        /// <returns></returns>
        public async Task<List<NonWeekendHolidayDto>> GetNonWeekendHolidayCountAsync(int year, IEnumerable<string> countryCodes)
        {
            var startDateOfYear = new DateTime(year, 1, 1);
            var endDateOfYear = new DateTime(year + 1, 1, 1);

            var currentYearHolidays = await db.Holidays.Where(h => countryCodes.Contains(h.CountryCode)
                                        && h.Date >= startDateOfYear
                                        && h.Date < endDateOfYear).ToListAsync();


            var nonWeekendHolidayCount = currentYearHolidays
                .Where(h => h.Date.DayOfWeek != DayOfWeek.Saturday && h.Date.DayOfWeek != DayOfWeek.Sunday)
                .GroupBy(h => h.CountryCode)
                .Select(g => new NonWeekendHolidayDto
                {
                    CountryCode = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var nonWeekendHoldiayPerCountry = countryCodes
                .Select(cc => nonWeekendHolidayCount.FirstOrDefault(h => h.CountryCode == cc)
                              ?? new NonWeekendHolidayDto { CountryCode = cc, Count = 0 })
                .OrderByDescending(x => x.Count)
                .ToList();

            return nonWeekendHoldiayPerCountry;
        }

        /// <summary>
        /// Service method to see if there are any common holidays between two contries in a given year
        /// and display it with their local names.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="firstCountry"></param>
        /// <param name="secondCountry"></param>
        /// <returns></returns>
        public async Task<List<SharedHolidayDto>> GetSharedCelebrationDatesAsync(int year, string firstCountry, string secondCountry)
        {
            return await (
                         from fch in db.Holidays
                         join sch in db.Holidays
                         on fch.Date equals sch.Date
                         where fch.CountryCode == firstCountry && sch.CountryCode == secondCountry && fch.Date.Year == year
                         select new SharedHolidayDto
                         {
                             Date = fch.Date,
                             HolidayLocalNameFirstCountry = fch.LocalName,
                             HolidayLocalNameSecondCountry = sch.LocalName
                         }).ToListAsync();

        }

        #endregion
    }
}
