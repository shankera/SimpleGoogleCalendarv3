using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
﻿using CS498.Lib;
﻿using Google.Apis.Auth.OAuth2;
﻿using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
namespace SimpleGoogleCalendarv3
{
    public class SimpleGoogleCalendar
    {

        private static SimpleGoogleCalendar _instance;
        private static CalendarService _service;

        private SimpleGoogleCalendar()
        {
            Authorize().Wait();
        }

        public static SimpleGoogleCalendar Instance
        {
            get { return _instance ?? (_instance = new SimpleGoogleCalendar()); }
        }

        private static async Task Authorize()
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = PrivateConsts.ClientId,
                    ClientSecret = PrivateConsts.ClientSecrets
                },
                new[] {CalendarService.Scope.Calendar},
                "user",
                CancellationToken.None,
                new FileDataStore("Calendar.Auth.Store"));

            _service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample"
            });

        }

        public const string DefaultCalendarId = "primary";

        public enum CalendarAccess
        {
            [Description("Reader")]
            Reader,
            [Description("Owner")]
            Owner,
            [Description("Writer")]
            Writer
        }

        public async Task<IDictionary<string, string>> GetCalendarIds(CalendarAccess accessLevel)
        {
            var calendarIds = new Dictionary<string, string>();
            var req = await _service.CalendarList.List().ExecuteAsync();
            foreach (var calendarListEntry in req.Items.Where(x => x.AccessRole.Equals(accessLevel.ToString())))
            {
                var calendarId = calendarListEntry.Id;
                var calendarSummary = calendarListEntry.Summary;
                if (calendarIds.ContainsKey(calendarListEntry.Id))
                {
                    calendarId += calendarSummary;
                }
                calendarIds.Add(calendarId, calendarSummary);
            }
            return calendarIds;
        }

        public async Task<IEnumerable<Event>> GetEvents(string calendarId, DateTime startDate, DateTime endDate, bool singleEvents)
        {
            var listRequest = _service.Events.List(calendarId);
            listRequest.TimeMin = startDate;
            listRequest.TimeMax = endDate;
            listRequest.SingleEvents = singleEvents;
            var request = await listRequest.ExecuteAsync();

            return request.Items.ToList();
        }

        public async Task AddEvent(string calendarId, Event gEvent)
        {
            await _service.Events.Insert(gEvent, calendarId).ExecuteAsync();
        }

        public async Task DeleteEvent(string calendarId, string eventId)
        {
            await _service.Events.Delete(calendarId, eventId).ExecuteAsync();
        }
    }

}
