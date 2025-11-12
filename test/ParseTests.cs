using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class ParseTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ParseTest1()
        {
            var message = new Message(HL7Test.HL7_ORM);

            var isParsed = message.ParseMessage();

            Assert.IsTrue(isParsed);
        }

        [TestMethod]
        public void ParseTest2()
        {
            var message = new Message(HL7Test.HL7_ADT);

            var isParsed = message.ParseMessage();
            Assert.IsTrue(isParsed);
        }


        [TestMethod]
        public void ReadSegmentTest()
        {
            var message = new Message(HL7Test.HL7_ORM);
            message.ParseMessage();

            Segment MSH_1 = message.Segments("MSH")[0];
            Assert.IsNotNull(MSH_1);
        }

        [TestMethod]
        public void ReadDefaultSegmentTest()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            Segment MSH = message.DefaultSegment("MSH");
            Assert.IsNotNull(MSH);
        }

        [TestMethod]
        public void ReadFieldTest()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            var MSH_9 = message.GetValue("MSH.9");
            Assert.AreEqual("ADT^O01", MSH_9);
        }

        [TestMethod]
        public void ReadFieldTestWithOccurrence()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            var NK1_2_2 = message.GetValue("NK1(2).2");
            Assert.AreEqual("DOE^JHON^^^^", NK1_2_2);
        }

        [TestMethod]
        public void ReadComponentTest()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            var MSH_9_1 = message.GetValue("MSH.9.1");
            Assert.AreEqual("ADT", MSH_9_1);
        }

        [TestMethod]
        public void EmptyFieldsTest()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            var NK1 = message.DefaultSegment("NK1").GetAllFields();
            Assert.HasCount(34, NK1);
            Assert.AreEqual(string.Empty, NK1[33].Value);
        }

        [TestMethod]
        public void GetMSH1Test()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            var MSH_1 = message.GetValue("MSH.1");
            Assert.AreEqual("|", MSH_1);
        }

        [TestMethod]
        public void MessageWithSegmentNameOnly()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nPID\r\nEVN|A04|20110613083617||\"\"\r\n";
            var message = new Message(sampleMessage);
            message.ParseMessage();
            var serialized = message.SerializeMessage();
            Assert.AreEqual(sampleMessage, serialized);
        }

        [TestMethod]
        public void GetValueTest()
        {
            var sampleMessage =
                @"MSH|^~\&|EPIC||||20191107134803|ALEVIB01|ORM^O01|23|T|2.3|||||||||||
PID|1||MRN_123^^^IDX^MRN||Smith\F\\S\\R\\E\\T\^John||19600101|M";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            string attendingDrId = message.GetValue("PID.5.1");
            Assert.AreEqual(@"Smith|^~\&", attendingDrId);
        }

        [TestMethod]
        public void BypassValidationParseMessage()
        {
            string sampleMessage = @"MSH|^~\&|SCA|SCA|LIS|LIS|202107300000||ORU^R01||P|2.4|||||||
PID|1|1234|1234||JOHN^DOE||19000101||||||||||||||
OBR|1|1234|1234||||20210708|||||||||||||||20210708||||||||||
OBX|1|TX|SCADOCTOR||^||||||F";

            try
            {
                var msg = new Message(sampleMessage);
                msg.ParseMessage(true);

                Assert.IsNull(msg.MessageControlID, "MessageControlID should be null");

                // Just make sure we have actually parsed the invalid MSH
                string messageType = msg.GetValue("MSH.9");
                Assert.AreEqual("ORU^R01", messageType, "Unexpected Message Type");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected exception: " + ex.Message);
            }
        }

        [TestMethod]
        public void MessageIsComponentized()
        {
            string sampleMessage = HL7Test.HL7_ADT;
            var message = new Message(sampleMessage);
            message.ParseMessage();

            Assert.IsTrue(message.IsComponentized("PID.5"));
        }

        [TestMethod]
        public void GetSequenceNo()
        {
            string sampleMessage = @"MSH|^~\&|MEDAT|AESCU|IXZENT||20250714122713|MEDAT_KC|ORU^R01|0027281|P|2.3|||||||
PID|||||Jane^Doe||19901224|F|||Bogus St^^Gotham^^12345^|||||||||||||
PV1||2|Jenkins||||||||||||||||AP0999923||P|||||||
ORC||0490000001|0490000001||CM||||202107141056
OBR|1|0490000001|0490000001|LH^LH^FN|||202107141056|20210714122713||||||202107141056|1||||||||||F|
OBX|1|NM|LH^LH^FN||20.00|mIU/ml|||||F||
NTE|1||      Follikelphase:    2.4 -  12.6 mIU/ml
NTE|2||      Ovulationsphase: 14.0 - 95.6 mIU/ml
NTE|3||      Lutealphase:      1.0 - 11.4 mIU/ml
NTE|4||      Postmenopause:    7.7 - 58.5 mIU/ml
OBR|2|0490000001|0490000001|FSH^FSH^FN|||202107141056|20210714122713||||||202107141056|1||||||||||F|
OBX|1|NM|FSH^FSH^FN||30.00|mIU/ml|||||F||
NTE|1||      Follikelphase:    3.5 - 12.5 mIU/ml
NTE|2||      Ovulationsphase:  4.7 - 21.5 mIU/ml
NTE|3||      Lutealphase:       1.7 - 7.7 mIU/ml
NTE|4||      Postmenopause:  25.8 - 134.8 mIU/ml";
            var message = new Message(sampleMessage);
            message.ParseMessage();

            var nte = message.Segments("NTE")[0];
            Assert.AreEqual(6, nte.GetSequenceNo());

            nte = message.Segments("NTE")[4];
            Assert.AreEqual(12, nte.GetSequenceNo());
        }

        public void BypassValidationParseMessage_ShouldReturnTrue()
        {
            string sampleMessage = @"MSH|^~\&|SCA|SCA|LIS|LIS|202107300000||ORU^R01||P|2.4|||||||
PID|1|1234|1234||JOHN^DOE||19000101||||||||||||||
OBR|1|1234|1234||||20210708|||||||||||||||20210708||||||||||
OBX|1|TX|SCADOCTOR||^||||||F";

            try
            {
                var msg = new Message(sampleMessage);
                Assert.IsTrue(msg.ParseMessage(true));

                Assert.IsNull(msg.MessageControlID, "MessageControlID should be null");

                //just to make sure we have actually parsed the invalid MSH
                string messageType = msg.GetValue("MSH.9");
                Assert.AreEqual("ORU^R01", messageType, "Unexpected Message Type");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected exception: " + ex.Message);
            }
        }

       [TestMethod]
        public async Task SerializeToStream()
        {
            var expected = HL7Test.HL7_ORM.Trim();
            var message = new Message(expected);
            message.ParseMessage();
            using var stream = new MemoryStream();
            await message.SerializeMessageAsync(stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var asyncResult = await reader.ReadToEndAsync();
            Assert.AreEqual(expected, asyncResult.Trim());
        }

        [TestMethod]
        public async Task SerializeToStreamWriter()
        {
            var expected = HL7Test.HL7_ORM.Trim();
            var message = new Message(expected);
            message.ParseMessage();
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            await message.SerializeMessageAsync(writer);
            await writer.FlushAsync();
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var asyncResult = await reader.ReadToEndAsync();
            Assert.AreEqual(expected, asyncResult.Trim());
        }
    }
}