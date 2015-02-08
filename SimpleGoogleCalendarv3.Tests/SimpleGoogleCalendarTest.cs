using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
using Ionic.Zlib;
using Microsoft.Runtime.CompilerServices;
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
        private static List<Calendar> _calendars;
        private static List<string> _ids; 

        private static string _testId;
        private static Calendar _testEntry;

        [ClassInitialize]
        public static void Initialize(TestContext s)
        {
            var fixture = new Fixture();

            _mock = new Mock<ICalendarServiceFacade>();
            _calendars = new List<Calendar>();

            _ownerList = fixture.CreateMany<CalendarListEntry>().ToList();
            _writerList = fixture.CreateMany<CalendarListEntry>().ToList();
            _readerList = fixture.CreateMany<CalendarListEntry>().ToList();
            _ownerList.ForEach(x => x.AccessRole = CalendarAccess.Owner.ToString().ToLower());
            _writerList.ForEach(x => x.AccessRole = CalendarAccess.Writer.ToString().ToLower());
            _readerList.ForEach(x => x.AccessRole = CalendarAccess.Reader.ToString().ToLower());

            var entries = _ownerList.Concat(_writerList).Concat(_readerList).ToList();
            foreach (var entry in entries.Select(calendarListEntry => new Calendar
            {
                Id = calendarListEntry.Id,
                Summary = calendarListEntry.Summary,
                Description = calendarListEntry.Description,
                Location = calendarListEntry.Location,
                ETag = calendarListEntry.ETag,
                Kind = calendarListEntry.Kind,
                TimeZone = calendarListEntry.TimeZone,
            }))
            {
                _calendars.Add(entry);
            }

            _ids = entries.Select(x => x.Id).ToList();
            _testId = _ids[new Random().Next(_ownerList.Count)];
            _mock.Setup(x => x.GetCalendar(It.IsIn<string>(_ids)))
                 .ReturnsAsync(_calendars.FirstOrDefault(y => y.Id.Equals(_testId)));
            _mock.Setup(x => x.GetCalendarListItemsExecuteAsyncItems()).ReturnsAsync(entries);
        }

        [TestMethod]
        public async Task GetCalendarIdsOwner()
        {
            var calendar = new SimpleGoogleCalendar(_mock.Object);
            var ids = await calendar.GetCalendarIdsAsync(CalendarAccess.Owner);
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
            Assert.AreEqual(_writerList.Count, ids.Count);
            foreach (var calendarListEntry in _writerList)
            {
                Assert.IsTrue(ids.ContainsKey(calendarListEntry.Id));
                Assert.IsTrue(ids[calendarListEntry.Id].Equals(calendarListEntry.Summary));
            }
        }
        [TestMethod]
        public async Task GetCalendar()
        {
            var simpleCalendar = new SimpleGoogleCalendar(_mock.Object);
            var calendar = await simpleCalendar.GetCalendarAsync(_testId);
            Assert.IsNotNull(calendar);
            Assert.AreEqual(_testId, calendar.Id);
        }

        
        [TestMethod]
        public async Task GetCalendarEmptyId()
        {
            var simpleCalendar = new SimpleGoogleCalendar(_mock.Object);
            var calendar = await simpleCalendar.GetCalendarAsync("");
            Assert.IsNull(calendar);
        }

        [TestMethod]
        public async Task GetCalendarInvalidId()
        {
            var simpleCalendar = new SimpleGoogleCalendar(_mock.Object);
            var calendar = await simpleCalendar.GetCalendarAsync("definitelynotanId");
            Assert.IsNull(calendar);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetCalendarNull()
        {
            var simpleCalendar = new SimpleGoogleCalendar(_mock.Object);
            await simpleCalendar.GetCalendarAsync(null);
            Assert.Fail("Exception was not thrown for null parameter.");
        }
    }
}
