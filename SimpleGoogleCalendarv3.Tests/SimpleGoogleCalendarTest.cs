using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;

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

            var fixture = new Fixture();
            var entries = fixture.CreateMany<CalendarListEntry>(60);
            var index = 0;
            var calendarListEntries = entries as IList<CalendarListEntry> ?? entries.ToList();
            foreach (var calendarListEntry in calendarListEntries)
            {
                calendarListEntry.Id = fixture.Create<string>();
                calendarListEntry.Summary = fixture.Create<string>();
                if (index%4 == 0)
                {
                    calendarListEntry.AccessRole = CalendarAccess.Owner;
                    _ownerList.Add(calendarListEntry);
                }
                else if (index%3 == 0)
                {
                    calendarListEntry.AccessRole = CalendarAccess.Reader;
                    _readerList.Add(calendarListEntry);
                }
                else
                {
                    calendarListEntry.AccessRole = CalendarAccess.Writer;
                    _writerList.Add(calendarListEntry);
                }
                index++;
            }
            _mock.Setup(x => x.GetCalendarListItemsExecuteAsyncItems()).ReturnsAsync(calendarListEntries);
        }

        [TestMethod]
        public async Task GetCalendarIdsOwner()
        {
            var calendar = new SimpleGoogleCalendar(_mock.Object);
            var ids = await calendar.GetCalendarIdsAsync(CalendarAccess.Owner);
            Console.Out.WriteLine(ids.Count);
            Assert.AreEqual(_ownerList.Count, ids.Count);
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
            Console.Out.WriteLine(ids.Count);
            Assert.AreEqual(_readerList.Count, ids.Count);
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
            Console.Out.WriteLine(ids.Count);
            Assert.AreEqual(_writerList.Count, ids.Count);
            foreach (var calendarListEntry in _writerList)
            {
                Assert.IsTrue(ids.ContainsKey(calendarListEntry.Id));
                Assert.IsTrue(ids[calendarListEntry.Id].Equals(calendarListEntry.Summary));
            }
        }
    }
}
