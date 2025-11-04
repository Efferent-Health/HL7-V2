using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class ComposeTests
    {
        public TestContext TestContext { get; set; }

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
            var message = new Message(HL7Test.HL7_ADT);
            message.AddNewSegment(newSeg);

            string serializedMessage = message.SerializeMessage();
            Assert.AreEqual("ZIB|ZIB1||||ZIB5^^ZIB.5.3\r", serializedMessage);
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
        public void AddSegmentMSHTest()
        {
            var message = new Message();
            message.AddSegmentMSH("test", "sendingFacility", "test", "test", "test", "ADR^A19", "test", "D", "2.5");
        }

        [TestMethod]
        public void RemoveSegment()
        {
            var message = new Message(HL7Test.HL7_ADT);
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
        public void SetValueSingleSegment()
        {
            var strValueFormat = "PID.2.1";
            var unchangedValuePath = "PID.3.1";
            var newPatientId = "1234567";
            var message = new Message(HL7Test.HL7_ADT);
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
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            Assert.IsTrue(message.SetValue("PID.1", testValue), "Should have successfully set value of Field");
            Assert.AreEqual(testValue, message.GetValue("PID.1"));
            
            Assert.IsTrue(message.SetValue("PID.2.2", testValue), "Should have successfully set value of Component");
            Assert.AreEqual(testValue, message.GetValue("PID.2.2"));
            
            Assert.IsTrue(message.SetValue("PID.2.4.1", testValue), "Should have successfully set value of SubComponent");
            Assert.AreEqual(testValue, message.GetValue("PID.2.4.1"));
        }
    }
}