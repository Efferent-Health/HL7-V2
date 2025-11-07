using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class RepetitionTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataRow("PV1.7.1", "1447312459")]
        [DataRow("PV1.7(1).1", "1447312459")]
        [DataRow("PV1.7[1].1", "1447312459")]
        [DataRow("PV1.7(2).1", "DOEM06")]
        [DataRow("PV1.7[2].1", "DOEM06")]
        [DataRow("PV1.7[2].3", "MICHAEL")]
        public void RepetitionTest(string index, string expected)
        {
            var sampleMessage =
                @"MSH|^~\&|EPIC||||20191107134803|ALEVIB01|ORM^O01|23|T|2.3|||||||||||
PID|1||1005555^^^NYU MRN^MRN||OSTRICH^DODUO||19820605|M||U|000 PARK AVE SOUTH^^NEW YORK^NY^10010^US^^^60|60|(555)555-5555^HOME^PH|||S|||999-99-9999|||U||N||||||||
PV1||O|NWSLED^^^NYULHLI^^^^^LI NW SLEEP DISORDER^^DEPID||||1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|1447312459^DOE^MICHAEL^^^^^^EPIC^^^^PNPI~DOEM06^DOE^MICHAEL^^^^^^KID^^^^KID|||||||||||496779945|||||||||||||||||||||||||20191107|||||||V";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            string attendingDrId = message.GetValue(index);
            Assert.AreEqual(expected, attendingDrId);
        }

        [TestMethod]
        public void RepetitionTest1()
        {
            var sampleMessage =
                @"MSH|^~\&|IA PHIMS Stage^2.16.840.1.114222.4.3.3.5.1.2^ISO|IA Public Health Lab^2.16.840.1.114222.4.1.10411^ISO|IA.DOH.IDSS^2.16.840.1.114222.4.3.3.19^ISO|IADOH^2.16.840.1.114222.4.1.3650^ISO|201203312359||ORU^R01^ORU_R01|2.16.840.1.114222.4.3.3.5.1.2-20120314235954.325|T|2.5.1|||AL|NE|USA||||PHLabReport-Ack^^2.16.840.1.113883.9.10^ISO
PID|1||14^^^IA PHIMS Stage&2.16.840.1.114222.4.3.3.5.1.2&ISO^PI^IA Public Health Lab&2.16.840.1.114222.4.1.10411&ISO~145^^^IA PHIMS Stage&2.16.840.1.114222.4.3.3.5.1.2&ISO^PI^IA Public Health Lab&2.16.840.1.114222.4.1.10411&ISO||Finn^Huckleberry^^^^^L||19630815|M||2106-3^White^CDCREC^^^^04/24/2007~1002-5^American Indian or Alaska Native^CDCREC^^^^04/24/2007|721SPRING STREET^^GRINNELL^IA^50112^USA^H|||||M^Married^HL70002^^^^2.5.1||||||H^Hispanic orLatino^HL70189^^^^2.5.1";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            Assert.IsTrue(message.HasRepetitions("PID.3"));
            Assert.IsTrue(message.Segments("PID")[0].Fields(3).HasRepetitions);
        }

        [TestMethod]
        public void InvalidRepetitionTest()
        {
            var sampleMessage =
                @"MSH|^~\&|SYSTEM1|ABC|SYSTEM2||201803262027||DFT^P03|20180326202737608457|P|2.3||||||8859/1
EVN|P03|20180326202540
PID|1|0002381795|0002381795||Supermann^Peter^^^Herr||19990101|M|||Hamburgerstrasse 123^^Mimamu^BL^12345^CH||123456~123456^^CP||D|2|02|321|8.2.24.||| 
PV1||A|00004620^00001318^1318||||000123456^Superfrau^Maria W.^|^Superarzt^Anton^L|00097012345^Superarzt^Herbert^~~0009723456^Superarzt^Markus^||||||||000998765^Assistent A^ONKO^D||0087123456||||||||||||||||||||2140||O|||201905220600|201908201100|||||";

            var message = new Message(sampleMessage);
            message.ParseMessage();

            // Check for invalid repetition number
            try
            {
                var value = message.GetValue("PV1.8(2).1");
                Assert.IsNull(value);
                value = message.GetValue("PV1.8(3).1");
                Assert.IsNull(value);

                Assert.Fail("Unexpected non-exception");
            }
            catch
            {
                // Pass
            }
        }

        [TestMethod]
        public void FieldHasRepetitions()
        {
            string sampleMessage = HL7Test.HL7_ADT;
            var message = new Message(sampleMessage);
            message.ParseMessage();

            Assert.IsFalse(message.HasRepetitions("PID.3"));
            Assert.IsTrue(message.HasRepetitions("PID.18"));
        }

        [TestMethod]
        public void FieldHasAnyPopulatedRepetitions()
        {
            //string sampleMessage = HL7Test.HL7_ADT;
            string sampleMessage = "MSH|^~\\&|AcmeHIS|StJohn|CATH|StJohn|20061019172719||ADT^O01|MSGID12349876|P|2.3\r\nPID||0493575^^^2^ID 1|454721||DOE^JOHN^^^^|DOE^JOHN^^^^|19480203|M||B|254 MYSTREET AVE^^MYTOWN^OH^44123^USA||(216)123-4567|||M|NON|400003403~1129086|TEST~|\r\n ";
            var message = new Message(sampleMessage);
            message.ParseMessage();

            Assert.IsFalse(message.Segments("PID")[0].Fields(3).HasAnyPopulatedRepetitions);
            Assert.IsTrue(message.Segments("PID")[0].Fields(18).HasAnyPopulatedRepetitions);
            Assert.IsTrue(message.Segments("PID")[0].Fields(19).HasAnyPopulatedRepetitions);
        }

        [TestMethod]
        public void FieldHasOnlyPopulatedRepetitions()
        {
            //string sampleMessage = HL7Test.HL7_ADT;
            string sampleMessage = "MSH|^~\\&|AcmeHIS|StJohn|CATH|StJohn|20061019172719||ADT^O01|MSGID12349876|P|2.3\r\nPID||0493575^^^2^ID 1|454721||DOE^JOHN^^^^|DOE^JOHN^^^^|19480203|M||B|254 MYSTREET AVE^^MYTOWN^OH^44123^USA||(216)123-4567|||M|NON|400003403~1129086|TEST~|\r\n ";

            var message = new Message(sampleMessage);
            message.ParseMessage();
         
            Assert.IsFalse(message.Segments("PID")[0].Fields(3).HasOnlyPopulatedRepetitions);
            Assert.IsTrue(message.Segments("PID")[0].Fields(18).HasOnlyPopulatedRepetitions);
            Assert.IsFalse(message.Segments("PID")[0].Fields(19).HasOnlyPopulatedRepetitions);
        }

        [TestMethod]
        public void AddRepeatingField()
        {
            var enc = new HL7Encoding();
            Segment PID = new Segment("PID", enc);
            Field f = new Field(enc);
            Field f1 = new Field("A", enc);
            Field f2 = new Field("B", enc);

            f.HasRepetitions = true;
            f.AddRepeatingField(f1);
            f.AddRepeatingField(f2);

            // Creates a new Field
            PID.AddNewField(f, 1);

            Message message = new Message();
            message.AddNewSegment(PID);
            var str = message.SerializeMessage();

            Assert.AreEqual("PID|A~B\r", str);
        }

        [TestMethod]
        public void SetValueRepeatingSegments()
        {
            var strValueFormat = "NK1.2.1";
            var unchangedValuePath = "NK1.2.2";
            var newFamilyName = "SCHMOE";
            var message = new Message(HL7Test.HL7_ADT);
            message.ParseMessage();

            message.SetValue(strValueFormat, newFamilyName);

            var firstNK1ChangedValue = message.GetValue(strValueFormat);
            var firstNK1UnchangedValue = message.GetValue(unchangedValuePath);
            Assert.IsTrue(message.RemoveSegment("NK1", 0));
            var secondNk1ChangedValue = message.GetValue(strValueFormat);
            var secondNk1UnchangedValue = message.GetValue(unchangedValuePath);

            Assert.AreEqual(newFamilyName, firstNK1ChangedValue);
            Assert.AreEqual(newFamilyName, secondNk1ChangedValue);
            Assert.AreEqual("MARIE", firstNK1UnchangedValue);
            Assert.AreEqual("JHON", secondNk1UnchangedValue);
        }
    }
}