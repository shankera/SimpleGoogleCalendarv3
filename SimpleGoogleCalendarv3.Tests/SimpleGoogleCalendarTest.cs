using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleGoogleCalendarv3.Tests
{
    [TestClass]
    public class SimpleGoogleCalendarTest
    {
        private SimpleGoogleCalendar _simpleCalendar;

        [TestInitialize]
        public void Initialize()
        {
            _simpleCalendar = new SimpleGoogleCalendar(PrivateConsts.ClientId, PrivateConsts.ClientSecrets);
        }
        [TestMethod]
        public async Task GetCalendarIdsOwner()
        {
            var idsListOwner = await _simpleCalendar.GetCalendarIdsAsync(CalendarAccess.Owner);
            Assert.IsTrue(idsListOwner.Count > 0);
        }
        [TestMethod]
        public async Task GetCalendarIdsReader()
        {
            var idsListReader = await _simpleCalendar.GetCalendarIdsAsync(CalendarAccess.Reader);
            Assert.IsTrue(idsListReader.Count > 0);
        }
        [TestMethod]
        public async Task GetCalendarIdsWriter()
        {
            var idsListWriter = await _simpleCalendar.GetCalendarIdsAsync(CalendarAccess.Writer);
            Assert.IsTrue(idsListWriter.Count > 0);
        }
    }
}
