using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class EncodingTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void NotEncodingTest()
        {
            var enc = new HL7Encoding().Encode("<1");
            Assert.AreEqual(enc, "<1");
        }

        [TestMethod]
        public void EncodingForOutputTest()
        {
            const string oruUrl = "domain.com/resource.html?Action=1&ID=2";  // Text with special character (&)

            var obx = new Segment("OBX", new HL7Encoding());
            obx.AddNewField("1");
            obx.AddNewField("RP");
            obx.AddNewField("70030^Radiologic Exam, Eye, Detection, FB^CDIRadCodes");
            obx.AddNewField("1");
            obx.AddNewField(obx.Encoding.Encode(oruUrl));  // Encoded field
            obx.AddNewField("F", 11);
            obx.AddNewField(MessageHelper.LongDateWithFractionOfSecond(DateTime.Now), 14);

            var oru = new Message();
            oru.AddNewSegment(obx);

            var str = oru.SerializeMessage();

            Assert.IsFalse(str.Contains("&"));  // Should have \T\ instead
        }

        [TestMethod]
        public void MessageWithDoubleNewlineTest()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\n\nEVN|A04|20110613083617||\r\n";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();
            Assert.IsTrue(isParsed);
            Assert.IsTrue(message.SegmentCount > 0);
        }

        [TestMethod]
        public void MessageWithDoubleCarriageReturnTest()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\n\nEVN|A04|20110613083617||\r\n";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();
            Assert.IsTrue(isParsed);
            Assert.IsTrue(message.SegmentCount > 0);
        }

        [TestMethod]
        public void MessageWithTabsIsReversable()
        {
            const string sampleMessage = "MSH|^~\\&|Sending\tApplication|Sending\tFacility|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nEVN|A04|20110613083617\r\n";
            var message = new Message(sampleMessage);
            message.ParseMessage();
            var serialized = message.SerializeMessage();
            Assert.AreEqual(sampleMessage, serialized);
        }

        [TestMethod]
        public void SkipInvalidEscapeSequenceTest()
        {
            var sampleMessage =
                @"MSH|^~\&|TEST^TEST|TEST|||11111||ADT^A08|11111|P|2.4|||AL||D||||||
ZZ1|1|139378|20201230100000|ghg^ghgh-HA||s1^OP-Saal 1|gh^gjhg 1|ghg^ghjg-HA|BSV 4\5 re.||||||";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            string serializedMessage = message.SerializeMessage();
        }

        [TestMethod]
        public void CustomDelimiterTest()
        {
            var encoding = new HL7Encoding
            {
                FieldDelimiter = '1',
                ComponentDelimiter = '2',
                SubComponentDelimiter = '3',
                RepeatDelimiter = '4',
                EscapeCharacter = '5'
            };

            var message = new Message();
            message.Encoding = encoding;
            message.AddSegmentMSH("FIRST", "SECOND", "THIRD", "FOURTH",
                "FIFTH", "ORU2R05F5", "SIXTH", "SEVENTH", "2.8");
            var result = message.SerializeMessage();

            Assert.AreEqual("MSH124531FIRST1SECOND1", result.Substring(0, 22));
        }

        [TestMethod]
        public void DecodedValue()
        {
            var msg = "MSH|^~\\&|SAP|aaa|JCAPS||20210330150502||ADT^A28|0000000111300053|P|2.5||||||UNICODE UTF-8\r\nPID|||704251200^^^SAP^PI^0001~XXXXXXX^^^SS^SS^066^20210330~\"\"^^^^PRC~\"\"^^^^DL~\"\"^^^^PPN~XXXXXXXX^^^Ministero finanze^NN~\"\"^^^^PNT^^^\"\"~\"\"^^^^NPI^^\"\"^^\"\"&&\"\"^\"\"&\"\"||TEST\\F\\TEST^TEST2^^^SIG.^\"\"||19610926|M|||^^SANTEUSANIO FORCONESE^^^IT^BDL^^066090~&VIA DELLA PIEGA 12 TRALLaae^\"\"^SANTEUSANIO FORCONESE^AQ^67020^IT^L^^066090^^^^20210330||^ORN^^^^^^^^^^349 6927621~^NET^^\"\"|||2||||||||||IT^^100^Italiana|||\"\"||||20160408\r\n";

            var message = new Message(msg);
            message.ParseMessage();

            var field = message.Segments("PID")[0].Fields(5).Value;
            var component = message.Segments("PID")[0].Fields(5).Components(1).Value;
            var subcomponent = message.Segments("PID")[0].Fields(5).Components(1).SubComponents(1).Value;

            Assert.AreEqual(component, subcomponent);
            Assert.IsTrue(field.StartsWith(component));
        }

        [TestMethod]
        public void DecodedValue1()
        {
            var msg = "MSH|^~\\&|SAP|aaa|JCAPS||20210330150502||ADT^A28|0000000111300053|P|2.5||||||UNICODE UTF-8\r\nPID|||704251200^^^SAP^PI^0001~XXXXXXX^^^SS^SS^066^20210330~\"\"^^^^PRC~\"\"^^^^DL~\"\"^^^^PPN~XXXXXXXX^^^Ministero finanze^NN~\"\"^^^^PNT^^^\"\"~\"\"^^^^NPI^^\"\"^^\"\"&&\"\"^\"\"&\"\"||TEST\\F\\TEST^TEST2^^^SIG.^\"\"||19610926|M|||^^SANTEUSANIO FORCONESE^^^IT^BDL^^066090~&VIA DELLA PIEGA 12 TRALLaae^\"\"^SANTEUSANIO FORCONESE^AQ^67020^IT^L^^066090^^^^20210330||^ORN^^^^^^^^^^349 6927621~^NET^^\"\"|||2||||||||||IT^^100^Italiana|||\"\"||||20160408\r\n";

            var message = new Message(msg);
            message.ParseMessage();

            var field = message.GetValue("PID.5");
            var component = message.GetValue("PID.5.1");
            var subcomponent = message.GetValue("PID.5.1.1");

            Assert.AreEqual(component, subcomponent);
            Assert.IsTrue(field.StartsWith(component));
        }

        [TestMethod]
        public void DecodeNonLatinChars()
        {
            var enconding = new HL7Encoding();

            Assert.AreEqual(enconding.Decode(@"T\XC3A4\glich 1 Tablette oral einnehmen"), "Täglich 1 Tablette oral einnehmen");
            Assert.AreEqual(enconding.Decode(@"\XE6AF8F\\XE5A4A9\\XE69C8D\\XE794A8\"), "每天服用");
        }

        [TestMethod]
        public void DoubleEncoding()
        {
            var sampleMessage = @"MSH|^~\&|Main_HIS|XYZ_HOSPITAL|iFW|ABC_Lab|20160915003015||ACK|9B38584D|P|2.6.1|
MSA|AA|9B38584D|Double encoded value: \E\T\E\|";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();

            Assert.IsTrue(isParsed);
            Assert.IsTrue(message.GetValue("MSA.3").EndsWith("&"));
        }

        [TestMethod]
        public void DoubleEncoding2()
        {
            var sampleMessage = @"MSH|^~\&|ADT1|MCM|FINGER|MCM|198808181126|SECURITY|ADT^A01|MSG00001|P|2.3.1
EVN|A01|198808181123
PID|1||PATID1234^5^M11^ADT1^MR^MCM~123456789^\E\^^USSSA^SS||SMITH^WILLIAM^A^III||19610615|M||C|1200 N ELM STREET^^JERUSALEM^TN^99999?1020|GL|(999)999?1212|(999)999?3333||S||PATID12345001^2^M10^ADT1^AN^A|123456789|987654^NC
NK1|1|SMITH^OREGANO^K|WI^WIFE||||NK^NEXT OF KIN
PV1|1|I|2000^2012^01||||004777^CASTRO^FRANK^J.|||SUR||||ADM|A0";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();

            Assert.IsTrue(isParsed);
        }

        [TestMethod]
        public void DecodeBrokenCharMessage()
        {
            var sampleMessage = @"MSH|^~\&|ADT1|MCM|FINGER|MCM|198808181126|SECURITY|ADT^A01|MSG00001|P|2.3.1
EVN|A01|198808181123
PID|1||PATID1234^5^M11^ADT1^MR^MCM~123456789^\E\^^USSSA^SS||fake^fakey^A^III||19610615|M||C|9999 N FAKE STREET^^FAKESTAR^TN^99999?1020|GL|(999)999?1212|(999)999?3333||S||PATID12345001^2^M10^ADT1^AN^A|123456789|987654^NC
NK1|1|FAKE^KAKIER^K|WI^WIFE||||NK^NEXT OF KIN
PV1|1|I|1999^2012^01||||004777^FAKES^FAKESSSS^J.|||SUR||||ADM|A0";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            var field = message.GetValue("PID.5");
            var component = message.GetValue("PID.5.1");
            var subcomponent = message.GetValue("PID.5.1.1");

            Assert.AreEqual(component, subcomponent);
            Assert.IsTrue(field.StartsWith(component));
        }

        [TestMethod]
        public void ParseBrokenCharMessage()
        {
            var sampleMessage = @"MSH|^~\&|ADT1|MCM|FINGER|MCM|198808181126|SECURITY|ADT^A01|MSG00001|P|2.3.1
EVN|A01|198808181123
PID|1||PATID1234^5^M11^ADT1^MR^MCM~123456789^\E\^^USSSA^SS||fake^fakey^A^III||19610615|M||C|9999 N FAKE STREET^^FAKESTAR^TN^99999?1020|GL|(999)999?1212|(999)999?3333||S||PATID12345001^2^M10^ADT1^AN^A|123456789|987654^NC
NK1|1|FAKE^KAKIER^K|WI^WIFE||||NK^NEXT OF KIN
PV1|1|I|1999^2012^01||||004777^FAKES^FAKESSSS^J.|||SUR||||ADM|A0";

            var message = new Message(sampleMessage);

            var isParsed = message.ParseMessage();
            Assert.IsTrue(isParsed);
        }

        [TestMethod]
        public void SpecialCharacter()
        {
            string sampleMessage = @"MSH|^~\&|HELIOS|DEDALUS|||20240609213244||ADT^A01|HL7Gtw018FFE7D23AC00|P|2.5|||AL|AL|D|8859/1
EVN|A01|||RO||20240609213200
PID|1||5928948^^^X1V1_MPI^PI~1053251221^^^HELIOS^ANT~757HA514^^^SS^SS~WLMHLP81R56Z209U^^^NNITA^NNITA||ANONYMIZED ANONYMIZED^ANONYMIZED ANONYMIZED^^^^^^^^^^^^3||19811016|F|||V. ANTONIO BAZZINI 9&V. ANTONIO BAZZINI&9^^ANONYMIZED^^20125^^H^^015146~V. ANTONIO ANONYMIZED 9&V. ANTONIO BAZZINI&9^^ANONYMIZED^^12345^^L^^015146^030~^^SRI LANKA^^^^BDL^^999311||^PRN^PH^^^^^^^^^3279945913|||2||ANONYMIZED^MEF^NNITA|757HA514|||||||^^311^SRI LANKA (gi\X00E0\ CEYLON)||||N
PV1|1|I|XOSTPIO^^^ICHPIO^^^^^ANONYMIZED|4|P2024126713||ANONYMIZED^ANONYMIZED^TOMMASO||||1HB^602^D02|||3|||ANONYMIZED^ANONYMIZED^ANONYMIZED||G2024005887^^^PRZ^VN|1||||||||||||||||||||||||20240609213200";

            var message = new Message(sampleMessage);

            var isParsed = message.ParseMessage();

            Assert.IsTrue(isParsed);
        }

        [TestMethod]
        public void TrailingBackSlash()
        {
            string sampleMessage = @"MSH|^~\&|SendingApp|SendingFac|ReceivingApp|ReceivingFac|20241206123456||ADT^A01|123456|P|2.5.1
PID|1|1234|1234||JOHN^DOE||19000101||||||||||||||
OBR|1|1234|1234||||20210708|||||||||||||||20210708||||||||||
OBX|36|TX|^^^^^|| OTHER: Right medial orbital wall fracture noted\";

            var message = new Message(sampleMessage);
            message.ParseMessage(true);

            var strMessage = message.SerializeMessage();

            Assert.IsTrue(strMessage.EndsWith("\\E\\\n"));
            TestContext.WriteLine(strMessage);
        }

        [TestMethod]
        public void CloneMessageWithNewline()
        {
            string sampleMessage = @"MSH|^~\&|ATHENANET|18802555^Orthopaedic Sample Org|Aspyra - 18802555|13274090^ORTHOPAEDIC INSTITUTE|20241119113500||ORM^O01|57492555M18802|P|2.5.1||||||||
PID||500036547^^^Enterprise ID||500036547^^^Patient ID|JOHN^DOE^||20050904|M||2106-3|1380 SAMPLE STREET^\X0A\^NEW YORK^NY^55755-5055||(555)261-2203|||S||^^^||||2186-5||||||||
PV1||O|80D18802^^^SAMPLE ORTHO URGENT CARE||||1905555652^JANE^D^DOE||||||||||1037^ABCDE8|||||||||||||||||||||||||||20241119||||||||";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            var message2 = new Message();

            foreach (var segment in message.Segments())
            {
                message2.AddNewSegment(segment.DeepCopy());
            }

            Assert.IsTrue(message2.GetValue("PID.11").IndexOf('\n') > 1);
        }

        [TestMethod]
        public void PresentButNull()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nEVN|A04|20110613083617||\"\"\r\n";

            var message = new Message(sampleMessage);
            message.Encoding.PresentButNull = null;
            message.ParseMessage();

            var evn = message.Segments("EVN")[0];
            var expectDoubleQuotes = evn.Fields(4).Value;
            Assert.AreEqual("\"\"", expectDoubleQuotes);
        }

        [TestMethod]
        public void MessageWithNullsIsReversable()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nEVN|A04|20110613083617||\"\"\r\n";
            var message = new Message(sampleMessage);
            message.ParseMessage();
            var serialized = message.SerializeMessage();
            Assert.AreEqual(sampleMessage, serialized);
        }

        [TestMethod]
        public void EmptyAndNullFieldsTest()
        {
            const string sampleMessage = "MSH|^~\\&|SA|SF|RA|RF|20110613083617||ADT^A04|123|P|2.7||||\r\nEVN|A04|20110613083617||\"\"\r\n";

            var message = new Message(sampleMessage);
            var isParsed = message.ParseMessage();
            Assert.IsTrue(isParsed);
            Assert.IsTrue(message.SegmentCount > 0);
            var evn = message.Segments("EVN")[0];
            var expectEmpty = evn.Fields(3).Value;
            Assert.AreEqual(string.Empty, expectEmpty);
            var expectNull = evn.Fields(4).Value;
            Assert.AreEqual(null, expectNull);
        }
    }    
}