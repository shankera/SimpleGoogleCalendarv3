using System.Collections.Generic;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SimpleGoogleCalendarv3.Tests
{
    [TestClass]
    public class SimpleGoogleCalendarTest
    {
        private SimpleGoogleCalendar _simpleCalendar;
        /* IGNORE ALL OF THIS FOR NOW
        [TestInitialize]
        public void Initialize()
        {
            var mock = new Mock<CalendarService>();
            var req = new Mock<CalendarList>();
            var itemsList = new List<Mock<CalendarListEntry>>();
            for (var x = 0; x < 10; x++)
            {
                var item = new Mock<CalendarListEntry>();
                item.Setup(y => y.AccessRole).Returns("owner");
                item.Setup(y => y.Id).Returns("ownerCalendarId" + x);
                item.Setup(y => y.Summary).Returns("ownerCalendarName" + x);
                itemsList.Add(item);
            }
            //mock.Setup(x => x.CalendarList.List().ExecuteAsync()).Returns(() =>);
        }*/
    }
}
