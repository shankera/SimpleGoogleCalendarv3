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
        private static Mock<ICalendarServiceFacade> _mock;
        private static List<CalendarListEntry> _ownerList;
        private static List<CalendarListEntry> _writerList;
        private static List<CalendarListEntry> _readerList;
        private static List<Calendar> _calendars;
        private static List<string> _validIdsForGettingCalendar;
        private static List<string> _validCalendarIdsForAddingEvent;
        private static List<string> _validIdsForGettingEvents;
        private static List<Event> _testEventGetter; 
        private static List<Event> _getEventsOutput; 
        private static List<Event> _testAddEvent; 
        private static SimpleGoogleCalendar _simpleCalendar;

        private static string _testId;

        [ClassInitialize]
        public static void Initialize(TestContext s)
        {
            var fixture = new Fixture();

            _mock = new Mock<ICalendarServiceFacade>();

            _ownerList = fixture.CreateMany<CalendarListEntry>().ToList();
            _writerList = fixture.CreateMany<CalendarListEntry>().ToList();
            _readerList = fixture.CreateMany<CalendarListEntry>().ToList();
            _ownerList.ForEach(x => x.AccessRole = CalendarAccess.Owner.ToString().ToLower());
            _writerList.ForEach(x => x.AccessRole = CalendarAccess.Writer.ToString().ToLower());
            _readerList.ForEach(x => x.AccessRole = CalendarAccess.Reader.ToString().ToLower());

            var entries = _ownerList.Concat(_writerList).Concat(_readerList).ToList();
            _mock.Setup(x => x.GetCalendarListItemsExecuteAsyncItems()).ReturnsAsync(entries);

            _calendars = fixture.CreateMany<Calendar>().ToList();
            _validIdsForGettingCalendar = _calendars.Select(x => x.Id).ToList();
            _testId = _validIdsForGettingCalendar[new Random().Next(_ownerList.Count)];

            _mock.Setup(x => x.GetCalendar(It.IsIn<string>(_validIdsForGettingCalendar)))
                .ReturnsAsync(_calendars.FirstOrDefault(y => y.Id.Equals(_testId)));

            _mock.Setup(x => x.GetCalendar(It.IsNotIn<string>(_validIdsForGettingCalendar)))
                .Throws(new Google.GoogleApiException("Google.Apis.Requests.RequestError", "Google.Apis.Requests.RequestError"));

            _validCalendarIdsForAddingEvent = fixture.CreateMany<string>().ToList();
            _testAddEvent = fixture.CreateMany<Event>().ToList();

            _mock.Setup(x => x.AddEvent(It.IsIn<string>(_validCalendarIdsForAddingEvent), It.IsAny<Event>()))
                .Returns(Task.FromResult(true))
                .Callback((string id, Event gEvent) => _testAddEvent.Add(gEvent));

            _mock.Setup(x => x.AddEvent(It.IsNotIn<string>(_validCalendarIdsForAddingEvent), It.IsAny<Event>()))
                .Throws(new Google.GoogleApiException("Google.Apis.Requests.RequestError", "Google.Apis.Requests.RequestError"));


            _testEventGetter = new List<Event>(fixture.CreateMany<Event>(30)
                                .Where(v => v.Start.DateTime < v.End.DateTime).ToList()
                                .OrderBy(e => e.Start.DateTime));
            _validIdsForGettingEvents = _testEventGetter.Select(x => x.Id).ToList();

            _getEventsOutput= new List<Event>();
            _mock.Setup(
                x =>
                    x.GetEventsList(It.IsIn<string>(_validIdsForGettingEvents), It.IsNotNull<DateTime>(),
                        It.IsNotNull<DateTime>(), It.IsAny<bool>()))
                .Callback(
                    (string calendarId, DateTime startTime, DateTime endTime, bool recurringValue) =>
                    _getEventsOutput.AddRange(_testEventGetter.Where(e => e.Start.DateTime >= startTime && e.End.DateTime <= endTime).ToList()))
                .ReturnsAsync(_getEventsOutput);
           
            _mock.Setup(x => x.GetEventsList(It.IsNotIn<string>(_validIdsForGettingEvents), It.IsNotNull<DateTime>(), It.IsNotNull<DateTime>(), It.IsAny<bool>()))
                .Throws(new Google.GoogleApiException("Google.Apis.Requests.RequestError", "Google.Apis.Requests.RequestError"));

            _simpleCalendar = new SimpleGoogleCalendar(_mock.Object);
        }

        [TestMethod]
        public async Task GetEvents()
        {
            var rand = new Random();
            var startIndex = rand.Next(_testEventGetter.Count / 2);
            var endIndex = rand.Next(_testEventGetter.Count / 2);
            var startTime = _testEventGetter[startIndex].Start.DateTime.GetValueOrDefault();
            var endTime = _testEventGetter[startIndex + endIndex].End.DateTime.GetValueOrDefault();
            var returnEnumerable = await _simpleCalendar.GetEventsAsync(_validIdsForGettingEvents[rand.Next(_validIdsForGettingEvents.Count)], startTime, endTime, false);
            var expectedEnumerable = _testEventGetter.Where(x => x.Start.DateTime >= startTime && x.End.DateTime <= endTime);
            var returnList = returnEnumerable.ToList();
            var expectedList = expectedEnumerable.ToList();
            Assert.AreEqual(expectedList.Count, returnList.ToList().Count);
            Assert.IsTrue(expectedList.SequenceEqual(returnList));
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetEventsBad()
        {
            var rand = new Random();
            var fixture = new Fixture();
            var endDate = fixture.Create<DateTime>();
            await _simpleCalendar.GetEventsAsync(_validIdsForGettingEvents[rand.Next(_validIdsForGettingEvents.Count)], endDate.AddDays(1), endDate, false);
            Assert.Fail("startDate was larger than endDate and nothing broke");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetEventsNullId()
        {
            var fixture = new Fixture();
            await _simpleCalendar.GetEventsAsync(null, fixture.Create<DateTime>(), fixture.Create<DateTime>(), fixture.Create<bool>());
            Assert.Fail("Null calendarId should have thrown exception");
        }
        
        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task GetEventsInvalidId()
        {
            var fixture = new Fixture();
            var startDate = fixture.Create<DateTime>();
            await _simpleCalendar.GetEventsAsync("", startDate, startDate.AddDays(1), fixture.Create<bool>());
            Assert.Fail("Invalid calendarId should have thrown exception");
        }

        [TestMethod]
        public async Task AddEvent()
        {
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
            var calendarId = _validCalendarIdsForAddingEvent[new Random().Next(_validCalendarIdsForAddingEvent.Count)];
            await _simpleCalendar.AddEventAsync(calendarId, null);
            Assert.Fail("Null Event should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddEventNullId()
        {
            var newEvent = new Fixture().Create<Event>();
            await _simpleCalendar.AddEventAsync(null, newEvent);
            Assert.Fail("Null Id should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task AddEventInvalidId()
        {
            var calendarId = new Fixture().Create<string>();
            var newEvent = new Fixture().Create<Event>();
            await _simpleCalendar.AddEventAsync(calendarId, newEvent);
            Assert.Fail("Exception should have been thrown");
        }

        [TestMethod]
        public async Task GetCalendarIdsOwner()
        {
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
            var calendar = await _simpleCalendar.GetCalendarAsync(_testId);
            Assert.IsNotNull(calendar);
            Assert.AreEqual(_testId, calendar.Id);
        }
       
        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task GetCalendarEmptyId()
        {
            await _simpleCalendar.GetCalendarAsync("");
            Assert.Fail("Invalid Id - GoogleApiException should have been thrown");
        }
        
        [TestMethod]
        [ExpectedException(typeof(Google.GoogleApiException))]
        public async Task GetCalendarInvalidId()
        {
            await _simpleCalendar.GetCalendarAsync("definitelynotanId");
            Assert.Fail("Invalid Id - GoogleApiException should have been thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetCalendarNull()
        {
            await _simpleCalendar.GetCalendarAsync(null);
            Assert.Fail("Exception was not thrown for null parameter.");
        }
    }
}
