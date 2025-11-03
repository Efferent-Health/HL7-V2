using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Efferent.HL7.V2;

namespace Benchmarks
{
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    [HideColumns("BuildConfiguration", "Error", "StdDev", "RatioSD")]
    public class SerializeBench
    {
/*
| Method                 | Job    | Runtime            | Mean     | Gen0   | Gen1   | Allocated |
|----------------------- |------- |------------------- |---------:|-------:|-------:|----------:|
| SerializeMessage_Sync  | Net4.8 | .NET Framework 4.8 | 43.95 us | 8.4839 | 0.0610 |   52.5 KB |
| SerializeMessage_Async | Net4.8 | .NET Framework 4.8 | 58.96 us | 3.5400 | 0.0610 |  21.84 KB |
| SerializeMessage_Sync  | Net8   | .NET 8.0           | 17.37 us | 7.1411 | 0.0610 |  43.84 KB |
| SerializeMessage_Async | Net8   | .NET 8.0           | 15.34 us | 3.0518 | 0.0610 |  18.73 KB |
*/
        private static readonly string _sampleMessage = File.ReadAllText("Sample-Orm.txt");
        private Message _msg;

        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.ShortRun;
                AddJob(baseJob.WithRuntime(ClrRuntime.Net48).WithCustomBuildConfiguration("LOCAL_CODE")
                    .WithId("Net4.8"));
                AddJob(baseJob.WithRuntime(CoreRuntime.Core80).WithCustomBuildConfiguration("LOCAL_CODE")
                    .WithId("Net8"));
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
            _ = _msg.SerializeMessage();
        }

#if LOCAL_CODE
        [Benchmark]
        public async Task SerializeMessage_Async()
        {
            using var ms = new MemoryStream();
            await _msg.SerializeMessageAsync(ms);
        }
#endif
    }
}