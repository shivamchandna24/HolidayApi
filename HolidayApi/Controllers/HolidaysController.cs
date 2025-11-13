
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
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


        //[HttpGet("Shared/{year}/{countryA}/{countryB}")]
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