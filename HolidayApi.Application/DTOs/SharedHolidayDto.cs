namespace HolidayApi.Application
{
    public class SharedHolidayDto
    {
        public DateTime Date { get; set; }

        public string HolidayLocalNameFirstCountry { get; set; } = string.Empty;

        public string HolidayLocalNameSecondCountry { get; set; } = string.Empty;
    }
}
