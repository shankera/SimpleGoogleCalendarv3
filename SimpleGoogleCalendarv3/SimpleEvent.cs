using System;
using Google.Apis.Calendar.v3.Data;

namespace SimpleGoogleCalendarv3
{
    class SimpleEvent : Event
    {
        public SimpleEvent()
        {
            Initialize(Guid.NewGuid().ToString().Replace("-", ""));
        }

        public SimpleEvent(string id)
        {
            Initialize(id);
        }

        private void Initialize(string id)
        {
            Id = id;
        }
    }
}
