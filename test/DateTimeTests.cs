using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class DateTimeTests
    {
        public TestContext TestContext { get; set; }

        [DataTestMethod]
        [DataRow("   20151231234500.1234+2358   ")]
        [DataRow("20151231234500.1234+2358")]
        [DataRow("20151231234500.1234-2358")]
        [DataRow("20151231234500.1234")]
        [DataRow("20151231234500.12")]
        [DataRow("20151231234500")]
        [DataRow("201512312345")]
        [DataRow("2015123123")]
        [DataRow("20151231")]
        [DataRow("201512")]
        [DataRow("2015")]
        public void ParseDateTime_Smoke_Positive(string dateTimeString)
        {
            var date = MessageHelper.ParseDateTime(dateTimeString);
            Assert.IsNotNull(date);
            Assert.AreEqual(DateTimeKind.Utc, date.Value.Kind);
        }

        [DataTestMethod]
        [DataRow("   20151231234500.1234+23581")]
        [DataRow("20151231234500.1234+23")]
        [DataRow("20151231234500.12345")]
        [DataRow("20151231234500.")]
        [DataRow("2015123123450")]
        [DataRow("20151231234")]
        [DataRow("201512312")]
        [DataRow("2015123")]
        [DataRow("20151")]
        [DataRow("201")]
        public void ParseDateTime_Smoke_Negative(string dateTimeString)
        {
            var date = MessageHelper.ParseDateTime(dateTimeString);
            Assert.IsNull(date);
        }

        [TestMethod]
        public void ParseDateTimeMilliseconds_Correctness()
        {
            var date = MessageHelper.ParseDateTime("20151231234500.123-2358", applyOffset: false).Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 00, 123), date);
            Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
        }

        [TestMethod]
        public void ParseDateTimeMilliseconds_Correctness_WithOffset()
        {
            var date = MessageHelper.ParseDateTime("20151231234500.123-2358").Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 00, 123).Subtract(new TimeSpan(-23, 58, 0)), date);
            Assert.AreEqual(DateTimeKind.Utc, date.Kind);
        }

        [TestMethod]
        public void ParseDateTimeMilliseconds_TimeSpanOut_Correctness()
        {
            TimeSpan offset;
            var date = MessageHelper.ParseDateTime("20151231234502.123-2358", out offset).Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 02, 123), date);
            Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
            Assert.AreEqual(new TimeSpan(-23, 58, 0), offset);
        }

        [TestMethod]
        public void ParseDateTimeMilliseconds_TimeSpanOut_Correctness_WithOffset()
        {
            TimeSpan offset;
            var date = MessageHelper.ParseDateTime("20151231234500.123-2358", out offset, applyOffset: true).Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 00, 123).Subtract(new TimeSpan(-23, 58, 0)), date);
            Assert.AreEqual(DateTimeKind.Utc, date.Kind);
            Assert.AreEqual(new TimeSpan(-23, 58, 0), offset);
        }

#if NET8_0_OR_GREATER
        [TestMethod]
        public void ParseDateTimeMicroseconds_Correctness()
        {
            var date = MessageHelper.ParseDateTime("20151231234500.1234-2358", applyOffset: false).Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 00, 123).AddMicroseconds(400), date);
            Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
        }

        [TestMethod]
        public void ParseDateTimeMicroseconds_Correctness_WithOffset()
        {
            var date = MessageHelper.ParseDateTime("20151231234500.1234-2358").Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 00, 123).AddMicroseconds(400).Subtract(new TimeSpan(-23, 58, 0)), date);
            Assert.AreEqual(DateTimeKind.Utc, date.Kind);
        }

        [TestMethod]
        public void ParseDateTimeMicroseconds_TimeSpanOut_Correctness()
        {
            TimeSpan offset;
            var date = MessageHelper.ParseDateTime("20151231234502.1234-2358", out offset).Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 02, 123).AddMicroseconds(400), date);
            Assert.AreEqual(DateTimeKind.Unspecified, date.Kind);
            Assert.AreEqual(new TimeSpan(-23, 58, 0), offset);
        }

        [TestMethod]
        public void ParseDateTimeMicroseconds_TimeSpanOut_Correctness_WithOffset()
        {
            TimeSpan offset;
            var date = MessageHelper.ParseDateTime("20151231234500.1234-2358", out offset, applyOffset: true).Value;
            Assert.AreEqual(new DateTime(2015, 12, 31, 23, 45, 00, 123).AddMicroseconds(400).Subtract(new TimeSpan(-23, 58, 0)), date);
            Assert.AreEqual(DateTimeKind.Utc, date.Kind);
            Assert.AreEqual(new TimeSpan(-23, 58, 0), offset);
        }
#endif

        [TestMethod]
        public void ParseDateTime_WithException()
        {
            try
            {
                var date = MessageHelper.ParseDateTime("201", true);
                Assert.Fail();
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch
            {
            }
        }

        [TestMethod]
        public void ParseDateTimeOffset_WithException()
        {
            try
            {
                var date = MessageHelper.ParseDateTime("201", out TimeSpan offset, true);
                Assert.Fail();
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch
            {
            }
        }

        [DataTestMethod]
        [DataRow("18151231")]
        [DataRow("19151231")]
        [DataRow("20151231")]
        public void ParseDateTime_Year(string dateTimeString)
        {
            var date = MessageHelper.ParseDateTime(dateTimeString);
            Assert.IsNotNull(date);
            Assert.AreEqual(DateTimeKind.Utc, date.Value.Kind);
        }

        [DataTestMethod]
        [DataRow("1701231")]
        [DataRow("16151231")]
        [DataRow("00001231")]
        public void ParseDateTime_Year_Negative(string dateTimeString)
        {
            var date = MessageHelper.ParseDateTime(dateTimeString);
            Assert.IsNull(date);
        }
    }
}