using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Efferent.HL7.V2.Test
{
    [TestClass]
    public class HL7Test
    {
        public static readonly string HL7_ORM;
        public static readonly string HL7_ADT;
        public TestContext TestContext { get; set; }

        /*
        public static void Main(string[] args)
        {
            var test = new HL7Test();
        }
        */

        static HL7Test()
        {
            var path = Path.GetDirectoryName(typeof(HL7Test).GetTypeInfo().Assembly.Location) + "/";
            HL7_ORM = File.ReadAllText(path + "Sample-ORM.txt");
            HL7_ADT = File.ReadAllText(path + "Sample-ADT.txt");
        }

        [TestMethod]
        public void SmokeTest()
        {
            Message message = new Message(HL7_ORM);
            Assert.IsNotNull(message);

            // message.ParseMessage();
            // File.WriteAllText("SmokeTestResult.txt", message.SerializeMessage());
        }
    }
}
