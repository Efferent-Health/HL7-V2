using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class AckNackTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetAckTest()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();
            var ack = message.GetACK();

            var MSH_3 = message.GetValue("MSH.3");
            var MSH_4 = message.GetValue("MSH.4");
            var MSH_5 = message.GetValue("MSH.5");
            var MSH_6 = message.GetValue("MSH.6");
            var MSH_3_A = ack.GetValue("MSH.3");
            var MSH_4_A = ack.GetValue("MSH.4");
            var MSH_5_A = ack.GetValue("MSH.5");
            var MSH_6_A = ack.GetValue("MSH.6");

            Assert.AreEqual(MSH_3, MSH_5_A);
            Assert.AreEqual(MSH_4, MSH_6_A);
            Assert.AreEqual(MSH_5, MSH_3_A);
            Assert.AreEqual(MSH_6, MSH_4_A);

            var MSH_10 = message.GetValue("MSH.10");
            var MSH_10_A = ack.GetValue("MSH.10");
            var MSA_1_1 = ack.GetValue("MSA.1");
            var MSA_1_2 = ack.GetValue("MSA.2");

            Assert.AreEqual("AA", MSA_1_1);
            Assert.AreEqual(MSH_10_A, MSH_10);
            Assert.AreEqual(MSA_1_2, MSH_10);
        }

        [TestMethod]
        public void GetNackTest()
        {
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            var error = "Error message";
            var code = "AR";
            var ack = message.GetNACK(code, error);

            var MSH_3 = message.GetValue("MSH.3");
            var MSH_4 = message.GetValue("MSH.4");
            var MSH_5 = message.GetValue("MSH.5");
            var MSH_6 = message.GetValue("MSH.6");
            var MSH_3_A = ack.GetValue("MSH.3");
            var MSH_4_A = ack.GetValue("MSH.4");
            var MSH_5_A = ack.GetValue("MSH.5");
            var MSH_6_A = ack.GetValue("MSH.6");

            Assert.AreEqual(MSH_3, MSH_5_A);
            Assert.AreEqual(MSH_4, MSH_6_A);
            Assert.AreEqual(MSH_5, MSH_3_A);
            Assert.AreEqual(MSH_6, MSH_4_A);

            var MSH_10 = message.GetValue("MSH.10");
            var MSH_10_A = ack.GetValue("MSH.10");
            var MSA_1_1 = ack.GetValue("MSA.1");
            var MSA_1_2 = ack.GetValue("MSA.2");
            var MSA_1_3 = ack.GetValue("MSA.3");

            Assert.AreEqual(MSH_10, MSH_10_A);
            Assert.AreEqual(MSH_10, MSA_1_2);
            Assert.AreEqual(MSA_1_1, code);
            Assert.AreEqual(MSA_1_3, error);
        }

        [TestMethod]
        public void GenerateAckNoEscapeDelimiterTest()
        {
            var sampleMessage = @"MSH|^~&|EPIC||||20191107134803|ALEVIB01|ORM^O01|23|T|2.3|||||||||||";
            sampleMessage = $"{sampleMessage}\nPID|1||MRN_123^^^IDX^MRN||Smith F S R E T^John||19600101|M";

            var message = new Message(sampleMessage);
            message.ParseMessage();
            var ack = message.GetACK();

            Assert.IsNotNull(ack);

            var ACK_MSH_2 = ack.GetValue("MSH.2");
            var MSG_MSH_2 = message.GetValue("MSH.2");

            Assert.AreEqual("^~&", ACK_MSH_2);
            Assert.AreEqual("^~&", MSG_MSH_2);
        }

        [TestMethod]
        public void BypassValidationGetACK()
        {
            string sampleMessage = @"MSH|^~\&|SCA|SCA|LIS|LIS|202107300000||ORU^R01||P|2.4|||||||
PID|1|1234|1234||JOHN^DOE||19000101||||||||||||||
OBR|1|1234|1234||||20210708|||||||||||||||20210708||||||||||
OBX|1|TX|SCADOCTOR||^||||||F";

            try
            {
                var msg = new Message(sampleMessage);
                msg.ParseMessage(true);

                var ack = msg.GetACK(true);
                string sendingApp = ack.GetValue("MSH.3");
                string sendingFacility = ack.GetValue("MSH.4");
                string receivingApp = ack.GetValue("MSH.5");
                string receivingFacility = ack.GetValue("MSH.6");
                string messageType = ack.GetValue("MSH.9");

                Assert.IsNull(ack.MessageControlID, "MessageControlID should be null");
                Assert.AreEqual("LIS", sendingApp, "Unexpected Sending Application");
                Assert.AreEqual("LIS", sendingApp, "Unexpected Sending Facility");
                Assert.AreEqual("SCA", receivingApp, "Unexpected Receiving Application");
                Assert.AreEqual("SCA", receivingFacility, "Unexpected Receiving Facility");
                Assert.AreEqual("ACK", messageType, "Unexpected Message Type");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected exception: " + ex.Message);
            }
        }

        [TestMethod]
        public void BypassValidationGetNACK()
        {
            string sampleMessage = @"MSH|^~\&|SCA|SCA|LIS|LIS|202107300000||ORU^R01||P|2.4|||||||
PID|1|1234|1234||JOHN^DOE||19000101||||||||||||||
OBR|1|1234|1234||||20210708|||||||||||||||20210708||||||||||
OBX|1|TX|SCADOCTOR||^||||||F";

            try
            {
                var msg = new Message(sampleMessage);
                msg.ParseMessage(true);

                var nack = msg.GetNACK("AE", "Unit test", true);
                string sendingApp = nack.GetValue("MSH.3");
                string sendingFacility = nack.GetValue("MSH.4");
                string receivingApp = nack.GetValue("MSH.5");
                string receivingFacility = nack.GetValue("MSH.6");
                string messageType = nack.GetValue("MSH.9");
                string code = nack.GetValue("MSA.1");
                string errorMessage = nack.GetValue("MSA.3");

                Assert.IsNull(nack.MessageControlID, "MessageControlID should be null");
                Assert.AreEqual("LIS", sendingApp, "Unexpected Sending Application");
                Assert.AreEqual("LIS", sendingApp, "Unexpected Sending Facility");
                Assert.AreEqual("SCA", receivingApp, "Unexpected Receiving Application");
                Assert.AreEqual("SCA", receivingFacility, "Unexpected Receiving Facility");
                Assert.AreEqual("ACK", messageType, "Unexpected Message Type");

                Assert.AreEqual("AE", code, "Unexpected Error Code");
                Assert.AreEqual("Unit test", errorMessage, "Unexpected Error Message");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected exception: " + ex.Message);
            }
        }
    }
}