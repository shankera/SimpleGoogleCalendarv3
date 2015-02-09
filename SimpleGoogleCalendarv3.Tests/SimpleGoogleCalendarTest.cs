using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;

namespace SimpleGoogleCalendarv3.Tests
{
    [TestClass]
    public class SimpleGoogleCalendarTest
    {
        private static List<string> _validCalendarIdsForAddingEvent;
        private static SimpleGoogleCalendar _simpleCalendar;
        private static List<CalendarListEntry> _writerList;
        private static List<CalendarListEntry> _readerList;
        private static List<CalendarListEntry> _ownerList;
        private static Mock<ICalendarServiceFacade> _mock;
        private static List<Event> _testEventGetter;
        private static List<Event> _testAddEvent;
        private static Fixture _fixture;
        private static string _testId;

        [ClassInitialize]
        public static void Initialize(TestContext s)
        {
            _fixture = new Fixture();
            _mock = new Mock<ICalendarServiceFacade>();

            _ownerList = _fixture.CreateMany<CalendarListEntry>().ToList();
            _writerList = _fixture.CreateMany<CalendarListEntry>().ToList();
            _readerList = _fixture.CreateMany<CalendarListEntry>().ToList();
            _ownerList.ForEach(x => x.AccessRole = CalendarAccess.Owner.ToString().ToLower());
            _writerList.ForEach(x => x.AccessRole = CalendarAccess.Writer.ToString().ToLower());
            _readerList.ForEach(x => x.AccessRole = CalendarAccess.Reader.ToString().ToLower());


            _validCalendarIdsForAddingEvent = _fixture.CreateMany<string>().ToList();
            _testEventGetter = new List<Event>(_fixture.CreateMany<Event>(30));
            foreach (var e in _testEventGetter)
            {
                if (!(e.Start.DateTime > e.End.DateTime)) continue;
                var temp = e.Start.DateTime;
                e.Start.DateTime = e.End.DateTime;
                e.End.DateTime = temp;
            }

            _simpleCalendar = new SimpleGoogleCalendar(_mock.Object);
        }
        
        private static string GetEventHelper()
        {
            var validIds = _testEventGetter.Select(x => x.Id).ToList();

            var output = new List<Event>();
            _mock.Setup(
                x =>
                    x.GetEventsList(It.IsIn<string>(validIds), It.IsNotNull<DateTime>(),
                        It.IsNotNull<DateTime>(), It.IsAny<bool>()))
                .Callback(
                    (string calendarId, DateTime startTime, DateTime endTime, bool recurringValue) =>
                    output.AddRange(_testEventGetter.Where(e => e.Start.DateTime >= startTime && e.End.DateTime <= endTime).ToList()))
                .ReturnsAsync(output);

            _mock.Setup(x => x.GetEventsList(It.IsNotIn<string>(validIds), It.IsNotNull<DateTime>(), It.IsNotNull<DateTime>(), It.IsAny<bool>()))
                .Throws(new Google.GoogleApiException("Google.Apis.Requests.RequestError", "Google.Apis.Requests.RequestError"));

            return validIds[0];
        }

        private static void AddEventHelper()
        {
            _testAddEvent = _fixture.CreateMany<Event>().ToList();

            _mock.Setup(x => x.AddEvent(It.IsIn<string>(_validCalendarIdsForAddingEvent), It.IsAny<Event>()))
                .Returns(Task.FromResult(true))
                .Callback((string id, Event gEvent) => _testAddEvent.Add(gEvent));

            _mock.Setup(x => x.AddEvent(It.IsNotIn<string>(_validCalendarIdsForAddingEvent), It.IsAny<Event>()))
                .Throws(new Google.GoogleApiException("Google.Apis.Requests.RequestError", "Google.Apis.Requests.RequestError"));

        }

        private static void GetCalendarIdsHelper()
        {
            var entries = _ownerList.Concat(_writerList).Concat(_readerList).ToList();
            _mock.Setup(x => x.GetCalendarListItemsExecuteAsyncItems()).ReturnsAsync(entries);
        }

        private static void GetCalendarHelper()
        {
            var calendars = _fixture.CreateMany<Calendar>().ToList();
            var validIds = calendars.Select(x => x.Id).ToList();
            _testId = validIds[new Random().Next(_ownerList.Count)];

            _mock.Setup(x => x.GetCalendar(It.IsIn<string>(validIds)))
                .ReturnsAsync(calendars.FirstOrDefault(y => y.Id.Equals(_testId)));

            _mock.Setup(x => x.GetCalendar(It.IsNotIn<string>(validIds)))
                .Throws(new Google.GoogleApiException("Google.Apis.Requests.RequestError", "Google.Apis.Requests.RequestError"));
        }
        
        [TestMethod]
        public async Task GetEvents()
        {
            var validId = GetEventHelper();

            var start = _testEventGetter[5].Start.DateTime.GetValueOrDefault();
            var end = _testEventGetter[22].End.DateTime.GetValueOrDefault();

            var returnEnumerable = await _simpleCalendar.GetEventsAsync(validId, start, end, false);
            var expectedEnumerable = _testEventGetter.Where(x => x.Start.DateTime >= start && x.End.DateTime <= end);
            var returnList = returnEnumerable.ToList();
            var expectedList = expectedEnumerable.ToList();

            Assert.AreEqual(expectedList.Count, returnList.ToList().Count);
            Assert.IsTrue(expectedList.SequenceEqual(returnList));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetEventsBad()
        {
            var validId = GetEventHelper();

            var fixture = new Fixture();
            var endDate = fixture.Create<DateTime>();
            await _simpleCalendar.GetEventsAsync(validId, endDate.AddDays(1), endDate, false);
            Assert.Fail("startDate was larger than endDate and nothing broke");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEventsNullId()
        {
            GetEventHelper();

            var fixture = new Fixture();
            await _simpleCalendar.GetEventsAsync(null, fixture.Create<DateTime>(), fixture.Create<DateTime>(), fixture.Create<bool>());
            Assert.Fail("Null calendarId should have thrown exception");
        }
        
        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task GetEventsInvalidId()
        {
            GetEventHelper();

            var fixture = new Fixture();
            var startDate = fixture.Create<DateTime>();
            await _simpleCalendar.GetEventsAsync("", startDate, startDate.AddDays(1), fixture.Create<bool>());
            Assert.Fail("Invalid calendarId should have thrown exception");
        }
        
        [TestMethod]
        public async Task AddEvent()
        {
            AddEventHelper();

            var countBefore = _testAddEvent.Count;
            var newEvent = new Fixture().Create<Event>();
            var calendarId = _validCalendarIdsForAddingEvent[new Random().Next(_validCalendarIdsForAddingEvent.Count)];
            await _simpleCalendar.AddEventAsync(calendarId, newEvent);
            var countAfter = _testAddEvent.Count;
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddEventNullEvent()
        {
            AddEventHelper();

            var calendarId = _validCalendarIdsForAddingEvent[new Random().Next(_validCalendarIdsForAddingEvent.Count)];
            await _simpleCalendar.AddEventAsync(calendarId, null);
            Assert.Fail("Null Event should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddEventNullId()
        {
            AddEventHelper();

            var newEvent = new Fixture().Create<Event>();
            await _simpleCalendar.AddEventAsync(null, newEvent);
            Assert.Fail("Null Id should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task AddEventInvalidId()
        {
            AddEventHelper();

            var calendarId = new Fixture().Create<string>();
            var newEvent = new Fixture().Create<Event>();
            await _simpleCalendar.AddEventAsync(calendarId, newEvent);
            Assert.Fail("Exception should have been thrown");
        }
        
        [TestMethod]
        public async Task GetCalendarIdsOwner()
        {
            GetCalendarIdsHelper();

            var ids = await _simpleCalendar.GetCalendarIdsAsync(CalendarAccess.Owner);
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
            GetCalendarIdsHelper();
   
            var ids = await _simpleCalendar.GetCalendarIdsAsync(CalendarAccess.Reader);
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
            GetCalendarIdsHelper();

            var ids = await _simpleCalendar.GetCalendarIdsAsync(CalendarAccess.Writer);
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
            GetCalendarHelper();

            var calendar = await _simpleCalendar.GetCalendarAsync(_testId);
            Assert.IsNotNull(calendar);
            Assert.AreEqual(_testId, calendar.Id);
        }
       
        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task GetCalendarEmptyId()
        {
            GetCalendarHelper();

            await _simpleCalendar.GetCalendarAsync("");
            Assert.Fail("Invalid Id - GoogleApiException should have been thrown");
        }
        
        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task GetCalendarInvalidId()
        {
            GetCalendarHelper();

            await _simpleCalendar.GetCalendarAsync("definitelynotanId");
            Assert.Fail("Invalid Id - GoogleApiException should have been thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetCalendarNull()
        {
            GetCalendarHelper();

            await _simpleCalendar.GetCalendarAsync(null);
            Assert.Fail("Exception was not thrown for null parameter.");
        }
    }
}
