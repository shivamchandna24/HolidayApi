namespace HolidayApi.Application
{
    public class NonWeekendHolidayResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<NonWeekendHolidayDto> Result { get; set; } = new();
    }
}
