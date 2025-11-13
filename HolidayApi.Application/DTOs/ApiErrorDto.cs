namespace HolidayApi.Application
{
    public class ApiErrorDto
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}
