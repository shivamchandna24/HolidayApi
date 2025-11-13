namespace HolidayApi.Application
{
    public class UpsertHolidayResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<UpsertHolidayDto> Result { get; set; } = new();
    }
}
