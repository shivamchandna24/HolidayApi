
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using HolidayApi.Application;
using HolidayApi.Domain;

namespace HolidayApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        #region Properties & Variable

        private readonly IHolidayService holidayService;

        #endregion

        #region Constructor
        public HolidaysController(IHolidayService holidayService)
        {
            this.holidayService = holidayService;
        }
        #endregion

        #region Controller Methods

        /// <summary>
        /// Controller method to insert or update holidays in your Db for given country code and year.
        /// For better and short naming  used rounting with Refresh
        /// </summary>
        /// <param name="year"></param>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        [HttpPost("Refresh/{year}/{countryCode}")]
        public async Task<ActionResult<UpsertHolidayResponseDto>> InsertOrUpdateHolidays(int year, string countryCode)
        {
            // Gets the records to be inserted or updated in DB. 
            var upsertedRecords = await holidayService.InsertOrUpdateHolidaysFromApiAsync(year, countryCode);

            var response = new UpsertHolidayResponseDto
            {
                Message = upsertedRecords.Count != 0 ? InformationMessages.HolidaysUpdated : InformationMessages.NoHolidaysUpdated,
                Result = upsertedRecords
            };
            return Ok(response);
        }


        /// <summary>
        /// Controller method to invoke service to fetch last three holidays for a country in DB
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        [HttpGet("PreviousThree/{countryCode}")]
        public async Task<ActionResult<PreviousHolidayResponseDto>> GetPreviousThreeHolidays(string countryCode)
        {
            var previousThreeHolidays = await holidayService.GetPreviousThreeHolidaysAsync(countryCode);

            var response = new PreviousHolidayResponseDto
            {
                Message = previousThreeHolidays.Count != 0 ? InformationMessages.RecordsFound : InformationMessages.NoRecordsFound,
                Result = previousThreeHolidays
            };
            return Ok(response);
        }

        /// <summary>
        /// Controller method to get holidays on weekdays for given country(ies) in a year.
        /// This is the controller method which uses client side validation.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="countryCodes"></param>
        /// <returns></returns>
        [HttpGet("NonWeekendCount/{year}")]
        public async Task<ActionResult<NonWeekendHolidayResponseDto>> GetNonWeekendHolidayCount(int year, [FromQuery][Required] string[] countryCodes)
        {
            if (!HolidayValidator.IsValidYear(year))
            {
                return BadRequest(new NonWeekendHolidayResponseDto
                {
                    Message = InformationMessages.InvalidYear,
                    Result = []
                });
            }

            if (!HolidayValidator.AreValidCountryCodes(countryCodes))
            {
                return BadRequest(new NonWeekendHolidayResponseDto
                {
                    Message = InformationMessages.InvalidCountryCode,
                    Result = []
                });
            }

            var weekdayHolidayCount = await holidayService.GetNonWeekendHolidayCountAsync(year, countryCodes);

            var response = new NonWeekendHolidayResponseDto
            {
                Message = weekdayHolidayCount.Count != 0 ? InformationMessages.RecordsFound : InformationMessages.NoRecordsFound,
                Result = weekdayHolidayCount
            };
            return Ok(response);
        }

        /// <summary>
        /// Controller method to get shared holidays in a year between two countires.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="firstCountry"></param>
        /// <param name="secondCountry"></param>
        /// <returns></returns>
        [HttpGet("Shared/{year}/{firstCountry}/{secondCountry}")]
        public async Task<ActionResult<SharedHolidayResponseDto>> GetSharedHolidays(int year, string firstCountry, string secondCountry)
        {
            var sharedHolidays = await holidayService.GetSharedCelebrationDatesAsync(year, firstCountry, secondCountry);

            var response = new SharedHolidayResponseDto
            {
                Message = sharedHolidays.Count != 0 ? InformationMessages.RecordsFound : InformationMessages.NoRecordsFound,
                Result = sharedHolidays
            };
            return Ok(response);
        }

        #endregion

    }
}