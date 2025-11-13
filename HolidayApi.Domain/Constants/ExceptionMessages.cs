namespace HolidayApi.Domain
{
    public static class ExceptionMessages
    {
        public const string InvalidJson = "Invalid JSON/data received from API.";
        public const string InvalidHTTPRequest = "Invalid http request.";

        public const string CountryAndYear = "Country & Year : ";
        public const string CountriesAndYear = "Countries & Year : ";
        public const string CountryCode = "Country : ";

        public const string ApiError = "API returned error.";
        public const string ApiStatusCode = "API returned status code.";

        public const string SaveFailed = "An error while saving your data.";
        public const string RetrieveFailed = "An error while fetching your data.";
        public const string DatabaseOperationFailed = "An error occurred while performing operation with the database.";

        public const string InternalServiceError = "An internal service error has occured while processing your request.";
        public const string ExternalServiceError = "An external service error has occured while processing your request.";

        public const string InvalidOperation = "An error occurred while performing an invalid operation.";
        public const string NullArgument = "An error occured due to null argument. ";

        public const string UnknownError = "An unexpected error occurred while processing your request.";
        public const string UnhandledError = "An unhandled exception occurred.";

    }
}
