# SimpleGoogleCalendarv3
This is a wrapper for the Google Calendar Api v3 for C#/.NET. The goal of this library is to alleviate the frustration of using Google's API directly. The user is only required to provide simple information.

If a calendarId is unknown, DefaultCalendarId can be used to get information for the primary calendar, or GetCalendarIdsAsync can be used to retrieve a Dictionary<string calendarIds, string calendarTitle>.
