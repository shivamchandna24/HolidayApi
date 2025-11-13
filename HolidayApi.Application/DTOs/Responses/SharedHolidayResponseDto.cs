namespace HolidayApi.Application
{
    public class SharedHolidayResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<SharedHolidayDto> Result { get; set; } = new();
    }
}
