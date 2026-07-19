using System.IO;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Efferent.HL7.V2;

namespace Benchmarks
{
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    [HideColumns("BuildConfiguration", "Error", "StdDev", "RatioSD")]
    public class SerializeBench
    {

        private static readonly string _sampleMessage = File.ReadAllText("Sample-Orm.txt");
        private Message _msg;

        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun;

                AddJob(baseJob.WithMsBuildArguments("/p:HL7V2Version=3.7.2").WithId("NuGet 3.7.2").AsBaseline());
                AddJob(baseJob.WithId("Local"));
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            _msg = new Message(_sampleMessage);
            _msg.ParseMessage(true);
        }

        [Benchmark]
        public void SerializeMessage_Sync()
        {
            using var ms = new MemoryStream();
            var message = _msg.SerializeMessage();
            ms.Write(Encoding.UTF8.GetBytes(message), 0, 0);
        }

        [Benchmark]
        public async Task SerializeMessage_Async()
        {
            using var ms = new MemoryStream();
            await _msg.SerializeMessageAsync(ms);
        }
    }
}