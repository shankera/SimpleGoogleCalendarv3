using System;
using Google.Apis.Calendar.v3.Data;

namespace SimpleGoogleCalendarv3
{
    class SimpleEventBuilder
    {
        private static Event BuildEvent(DateTime startTime, DateTime endTime, string title, string description, string location)
        {
            return new Event
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                Start = new EventDateTime
                {
                    DateTime = startTime
                },
                End = new EventDateTime()
                {
                    DateTime = endTime
                },
                Summary = title,
                Description = description,
                Location = location,
            };
        }

        public static Event CreateEvent(DateTime startTime, DateTime endTime)
        {
            return BuildEvent(startTime, endTime, null, null, null);
        }

        public static Event CreateEvent(DateTime startTime, DateTime endTime, string title)
        {
            return BuildEvent(startTime, endTime, title, null, null);
        }

        public static Event CreateEvent(DateTime startTime, DateTime endTime, string title, string description)
        {
            return BuildEvent(startTime, endTime, title, description, null);
        }

        public static Event CreateEvent(DateTime startTime, DateTime endTime, string title, string description, string location)
        {
            return BuildEvent(startTime, endTime, title, description, location);
        }
    }
}
