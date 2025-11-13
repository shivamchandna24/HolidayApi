namespace HolidayApi.Application
{
    public class PreviousHolidayResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<PreviousHolidayDto> Result { get; set; } = new();
    }
}
