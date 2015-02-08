using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SimpleGoogleCalendarv3.Tests
{
    [TestClass]
    public class SimpleGoogleCalendarTest
    {
        private static Mock<ICalendarServiceFacade> _mock;
        private static List<CalendarListEntry> _ownerList;
        private static List<CalendarListEntry> _writerList;
        private static List<CalendarListEntry> _readerList;
        [ClassInitialize]
        public static void Initialize(TestContext s)
        {
            _mock = new Mock<ICalendarServiceFacade>();
            _ownerList = new List<CalendarListEntry>();
            _writerList = new List<CalendarListEntry>();
            _readerList = new List<CalendarListEntry>();
            var list = new List<CalendarListEntry>();
            for (var x = 0; x < 10; x++)
            {
                var cle = new CalendarListEntry
                {
                    AccessRole = "owner",
                    Summary = "CalendarNameOwner" + x,
                    Id = "CalendarId" + x
                };

                list.Add(cle);
                _ownerList.Add(cle);
                cle = new CalendarListEntry
                {
                    AccessRole = "writer",
                    Summary = "CalendarNameWriter" + x,
                    Id = "CalendarId" + x
                };

                list.Add(cle);
                _writerList.Add(cle);
                cle = new CalendarListEntry
                {
                    AccessRole = "reader",
                    Summary = "CalendarNameReader" + x,
                    Id = "CalendarId" + x
                };

                list.Add(cle);
                _readerList.Add(cle);
            }
            _mock.Setup(x => x.GetCalendarListItemsExecuteAsyncItems()).ReturnsAsync(list);
        }

        [TestMethod]
        public async Task GetCalendarIdsOwner()
        {
            var calendar = new SimpleGoogleCalendar(_mock.Object);
            var ids = await calendar.GetCalendarIdsAsync(CalendarAccess.Owner);
            Assert.AreEqual(10, ids.Count);
            foreach (var calendarListEntry in _ownerList)
            {
                Assert.IsTrue(ids.ContainsKey(calendarListEntry.Id));
                Assert.IsTrue(ids[calendarListEntry.Id].Equals(calendarListEntry.Summary));
            }
        }

        [TestMethod]
        public async Task GetCalendarIdsReader()
        {
            var calendar = new SimpleGoogleCalendar(_mock.Object);
            var ids = await calendar.GetCalendarIdsAsync(CalendarAccess.Reader);
            Assert.AreEqual(10, ids.Count);
            foreach (var calendarListEntry in _readerList)
            {
                Assert.IsTrue(ids.ContainsKey(calendarListEntry.Id));
                Assert.IsTrue(ids[calendarListEntry.Id].Equals(calendarListEntry.Summary));
            }
        }

        [TestMethod]
        public async Task GetCalendarIdsWriter()
        {
            var calendar = new SimpleGoogleCalendar(_mock.Object);
            var ids = await calendar.GetCalendarIdsAsync(CalendarAccess.Writer);
            Assert.AreEqual(10, ids.Count);
            foreach (var calendarListEntry in _writerList)
            {
                Assert.IsTrue(ids.ContainsKey(calendarListEntry.Id));
                Assert.IsTrue(ids[calendarListEntry.Id].Equals(calendarListEntry.Summary));
            }
        }
    }
}
