document.addEventListener("DOMContentLoaded", function () {
    // Get the IANA Time Zone ID (e.g., "America/New_York")
    const timeZoneId = Intl.DateTimeFormat().resolvedOptions().timeZone;

    // Check if the current timezone cookie exists and matches
    const currentCookie = document.cookie
        .split(';')
        .find(c => c.trim().startsWith('UserTimeZone='));

    const currentTimeZone = currentCookie?.split('=')[1];

    // Set or update cookie if timezone changed or doesn't exist
    if (currentTimeZone !== timeZoneId) {
        const date = new Date();
        date.setTime(date.getTime() + (365 * 24 * 60 * 60 * 1000)); // 1 year
        const expires = "expires=" + date.toUTCString();

        document.cookie = `UserTimeZone=${timeZoneId};${expires};path=/;SameSite=Lax`;
    }
});