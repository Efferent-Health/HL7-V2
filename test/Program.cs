using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class HL7Test
    {
        private readonly string HL7_ORM;
        private readonly string HL7_ADT;
        public TestContext TestContext { get; set; }

        /*
        public static void Main(string[] args)
        {
            var test = new HL7Test();
        }
        */

        public HL7Test()
        {
            var path = Path.GetDirectoryName(typeof(HL7Test).GetTypeInfo().Assembly.Location) + "/";
            this.HL7_ORM = File.ReadAllText(path + "Sample-ORM.txt");
            this.HL7_ADT = File.ReadAllText(path + "Sample-ADT.txt");
        }

        [TestMethod]
        public void SmokeTest()
        {
            Message message = new Message(this.HL7_ORM);
            Assert.IsNotNull(message);

            // message.ParseMessage();
            // File.WriteAllText("SmokeTestResult.txt", message.SerializeMessage());
        }

        [TestMethod]
        public void ParseTest1()
        {
            var message = new Message(this.HL7_ORM);

            var isParsed = message.ParseMessage();

            Assert.IsTrue(isParsed);
        }

        [TestMethod]
        public void ParseTest2()
        {
            var message = new Message(this.HL7_ADT);

            var isParsed = message.ParseMessage();
            Assert.IsTrue(isParsed);
        }


        [TestMethod]
        public void ReadSegmentTest()
        {
            var message = new Message(this.HL7_ORM);
            message.ParseMessage();

            Segment MSH_1 = message.Segments("MSH")[0];
            Assert.IsNotNull(MSH_1);
        }

        [TestMethod]
        public void ReadDefaultSegmentTest()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            Segment MSH = message.DefaultSegment("MSH");
            Assert.IsNotNull(MSH);
        }

        [TestMethod]
        public void ReadFieldTest()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var MSH_9 = message.GetValue("MSH.9");
            Assert.AreEqual("ADT^O01", MSH_9);
        }

        [TestMethod]
        public void ReadFieldTestWithOccurrence()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var NK1_2_2 = message.GetValue("NK1(2).2");
            Assert.AreEqual("DOE^JHON^^^^", NK1_2_2);
        }

        [TestMethod]
        public void ReadComponentTest()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var MSH_9_1 = message.GetValue("MSH.9.1");
            Assert.AreEqual("ADT", MSH_9_1);
        }

        [TestMethod]
        public void AddComponentsTest()
        {
            var encoding = new HL7Encoding();

            // Create a Segment with name ZIB
            Segment newSeg = new Segment("ZIB", encoding);

            // Create Field ZIB_1
            Field ZIB_1 = new Field("ZIB1", encoding);
            // Create Field ZIB_5
            Field ZIB_5 = new Field("ZIB5", encoding);

            // Create Component ZIB.5.3
            Component com1 = new Component("ZIB.5.3_", encoding);

            // Add Component ZIB.5.3 to Field ZIB_5
            ZIB_5.AddNewComponent(com1, 3);

            // Overwrite the same field again
            ZIB_5.AddNewComponent(new Component("ZIB.5.3", encoding), 3);

            // Add Field ZIB_1 to segment ZIB, this will add a new filed to next field location, in this case first field
            newSeg.AddNewField(ZIB_1);

            // Add Field ZIB_5 to segment ZIB, this will add a new filed as 5th field of segment
            newSeg.AddNewField(ZIB_5, 5);

            // Add segment ZIB to message
            var message = new Message(this.HL7_ADT);
            message.AddNewSegment(newSeg);

            string serializedMessage = message.SerializeMessage();
            Assert.AreEqual("ZIB|ZIB1||||ZIB5^^ZIB.5.3\r", serializedMessage);
        }

        [TestMethod]
        public void EmptyFieldsTest()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var NK1 = message.DefaultSegment("NK1").GetAllFields();
            Assert.HasCount(34, NK1);
            Assert.AreEqual(string.Empty, NK1[33].Value);
        }

        [TestMethod]
        public void AddFieldTest()
        {
            var enc = new HL7Encoding();
            Segment PID = new Segment("PID", enc);

            // Creates a new Field
            PID.AddNewField("1", 1);

            // Overwrites the old Field
            PID.AddNewField("2", 1);

            Message message = new Message();
            message.AddNewSegment(PID);
            var str = message.SerializeMessage();

            Assert.AreEqual("PID|2\r", str);
        }

        [TestMethod]
        public void GetMSH1Test()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();

            var MSH_1 = message.GetValue("MSH.1");
            Assert.AreEqual("|", MSH_1);
        }

        [TestMethod]
        public void AddSegmentMSHTest()
        {
            var message = new Message();
            message.AddSegmentMSH("test", "sendingFacility", "test", "test", "test", "ADR^A19", "test", "D", "2.5");
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
        public void RemoveSegment()
        {
            var message = new Message(this.HL7_ADT);
            message.ParseMessage();
            Assert.HasCount(2, message.Segments("NK1"));
            Assert.AreEqual(5, message.SegmentCount);

            message.RemoveSegment("NK1", 1);
            Assert.HasCount(1, message.Segments("NK1"));
            Assert.AreEqual(4, message.SegmentCount);

            message.RemoveSegment("NK1");
            Assert.HasCount(0, message.Segments("NK1"));
            Assert.AreEqual(3, message.SegmentCount);
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
        public void RemoveTrailingComponentsTest_OnlyTrailingComponentsRemoved()
        {
            var message = new Message();

            var orcSegment = new Segment("ORC", message.Encoding);
            for (int eachField = 1; eachField <= 12; eachField++)
            {
                orcSegment.AddEmptyField();
            }

            for (int eachComponent = 1; eachComponent < 8; eachComponent++)
            {
                orcSegment.Fields(12).AddNewComponent(new Component(message.Encoding));
            }

            orcSegment.Fields(12).Components(1).Value = "should not be removed";
            orcSegment.Fields(12).Components(2).Value = "should not be removed";
            orcSegment.Fields(12).Components(3).Value = "should not be removed";
            orcSegment.Fields(12).Components(4).Value = string.Empty; // should not be removed because in between valid values
            orcSegment.Fields(12).Components(5).Value = "should not be removed";
            orcSegment.Fields(12).Components(6).Value = string.Empty; // should be removed because trailing
            orcSegment.Fields(12).Components(7).Value = string.Empty; // should be removed because trailing
            orcSegment.Fields(12).Components(8).Value = string.Empty; // should be removed because trailing

            orcSegment.Fields(12).RemoveEmptyTrailingComponents();
            message.AddNewSegment(orcSegment);

            string serializedMessage = message.SerializeMessage();
            Assert.HasCount(5, orcSegment.Fields(12).Components());
            Assert.AreEqual("ORC||||||||||||should not be removed^should not be removed^should not be removed^^should not be removed\r", serializedMessage);
        }

        [TestMethod]
        public void RemoveTrailingComponentsTest_RemoveAllFieldComponentsIfEmpty()
        {
            var message = new Message();

            var orcSegment = new Segment("ORC", message.Encoding);
            for (int eachField = 1; eachField <= 12; eachField++)
            {
                orcSegment.AddEmptyField();
            }

            for (int eachComponent = 1; eachComponent < 8; eachComponent++)
            {
                orcSegment.Fields(12).AddNewComponent(new Component(message.Encoding));
                orcSegment.Fields(12).Components(eachComponent).Value = string.Empty;
            }

            orcSegment.Fields(12).RemoveEmptyTrailingComponents();
            message.AddNewSegment(orcSegment);

            string serializedMessage = message.SerializeMessage();
            Assert.HasCount(0, orcSegment.Fields(12).Components());
            Assert.AreEqual("ORC||||||||||||\r", serializedMessage);
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
        public void SetValueSingleSegment()
        {
            var strValueFormat = "PID.2.1";
            var unchangedValuePath = "PID.3.1";
            var newPatientId = "1234567";
            var message = new Message(HL7_ADT);
            message.ParseMessage();

            message.SetValue(strValueFormat, newPatientId);
            
            Assert.AreEqual(newPatientId, message.GetValue(strValueFormat));
            Assert.AreEqual("454721", message.GetValue(unchangedValuePath));
        }

        [TestMethod]
        public void SetValueUnavailableComponents()
        {
            var sampleMessage = @"MSH|^~\&|SYSTEM1|ABC|SYSTEM2||201803262027||DFT^P03|20180326202737608457|P|2.3|||"
                                    +"\nPID|1|0002381795|0002381795||Supermann^Peter^^^Herr||19990101|M|||";
            var testValue = "test";
            var message = new Message(sampleMessage);
            message.ParseMessage();

            var invalidRequest = Assert.Throws<HL7Exception>(() => message.SetValue(string.Empty, testValue));
            Assert.Contains("Request format", invalidRequest.Message, "Should have thrown exception because of invalid request format");
            
            var unavailableSegment = Assert.Throws<HL7Exception>(() => message.SetValue("OBX.1", testValue));
            Assert.Contains("Segment name", unavailableSegment.Message, "Should have thrown exception because of unavailable Segment");
            
            var segmentLevel = Assert.Throws<HL7Exception>(() => message.SetValue("PID.30", testValue));
            Assert.Contains("Field not available", segmentLevel.Message, "Should have thrown exception because of unavailable Field");
            
            var componentLevel = Assert.Throws<HL7Exception>(() => message.SetValue("PID.3.7", testValue));
            Assert.Contains("Component not available", componentLevel.Message, "Should have thrown exception because of unavailable Component");
            
            var subComponentLevel = Assert.Throws<HL7Exception>(() => message.SetValue("PID.3.1.2", testValue));
            Assert.Contains("SubComponent not available", subComponentLevel.Message, "Should have thrown exception because of unavailable SubComponent");
        }

        [TestMethod]
        public void SetValueAvailableComponents()
        {
            var testValue = "test";
            var message = new Message(HL7_ADT);
            message.ParseMessage();

            Assert.IsTrue(message.SetValue("PID.1", testValue), "Should have successfully set value of Field");
            Assert.AreEqual(testValue, message.GetValue("PID.1"));
            
            Assert.IsTrue(message.SetValue("PID.2.2", testValue), "Should have successfully set value of Component");
            Assert.AreEqual(testValue, message.GetValue("PID.2.2"));
            
            Assert.IsTrue(message.SetValue("PID.2.4.1", testValue), "Should have successfully set value of SubComponent");
            Assert.AreEqual(testValue, message.GetValue("PID.2.4.1"));
        }


        [TestMethod]
        public void MessageIsComponentized()
        {
            string sampleMessage = this.HL7_ADT;
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
            var expected = this.HL7_ORM.Trim();
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
            var expected = this.HL7_ORM.Trim();
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
