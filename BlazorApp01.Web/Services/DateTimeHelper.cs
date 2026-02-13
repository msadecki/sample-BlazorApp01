using TimeZoneConverter;

namespace BlazorApp01.Web.Services;

internal interface IDateTimeHelper
{
    string ConvertToFormattedUserDateTime(DateTime? utcDateTime);
    string ConvertToFormattedUserDate(DateTime? utcDateTime);
    string ConvertToFormattedDate(DateOnly? date);
}

internal sealed class DateTimeHelper(
    IHttpContextAccessor httpContextAccessor) : IDateTimeHelper
{
    public string ConvertToFormattedUserDateTime(DateTime? utcDateTime)
    {
        var userDateTime = ConvertToUserDateTime(utcDateTime);

        return $"{userDateTime:yyyy-MM-dd HH:mm:ss}";
    }

    public string ConvertToFormattedUserDate(DateTime? utcDateTime)
    {
        var userDateTime = ConvertToUserDateTime(utcDateTime);

        return $"{userDateTime:yyyy-MM-dd}";
    }

    public string ConvertToFormattedDate(DateOnly? date)
    {
        return $"{date:yyyy-MM-dd}";
    }

    private DateTime? ConvertToUserDateTime(DateTime? utcDateTime)
    {
        if (!utcDateTime.HasValue)
        {
            return null;
        }

        var userTimeZoneId = httpContextAccessor.HttpContext?.Request.Cookies["UserTimeZone"] ?? "UTC";

        // Get the TimeZoneInfo safely (Handles Windows/Linux differences automatically)
        var destinationTimeZone = TZConvert.GetTimeZoneInfo(userTimeZoneId);

        var userDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Value, destinationTimeZone);

        return userDateTime;
    }
}