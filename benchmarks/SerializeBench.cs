using System.IO;
using System.Text;
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
| Method                 | Job          | Runtime            | Arguments                              | Mean     | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------- |------------- |------------------- |--------------------------------------- |---------:|------:|-------:|-------:|----------:|------------:|
| SerializeMessage_Sync  | Net4.8 Local | .NET Framework 4.8 | Default                                | 48.86 us |  2.57 | 3.2959 |      - |   20.4 KB |        0.46 |
| SerializeMessage_Sync  | Net4.8 Nuget | .NET Framework 4.8 | /p:PackageReference=HL7-V2,Version=3.7 | 53.45 us |  2.81 | 8.6670 | 0.0610 |  53.38 KB |        1.20 |
| SerializeMessage_Sync  | Net8 Local   | .NET 8.0           | Default                                | 12.76 us |  0.67 | 2.8229 | 0.0305 |   17.3 KB |        0.39 |
| SerializeMessage_Sync  | Net8 Nuget   | .NET 8.0           | /p:PackageReference=HL7-V2,Version=3.7 | 19.18 us |  1.01 | 7.2632 | 0.0610 |  44.65 KB |        1.00 |
|                        |              |                    |                                        |          |       |        |        |           |             |
| SerializeMessage_Async | Net4.8 Local | .NET Framework 4.8 | Default                                | 61.34 us |  2.51 | 3.5400 | 0.0610 |  21.84 KB |        0.47 |
| SerializeMessage_Async | Net4.8 Nuget | .NET Framework 4.8 | /p:PackageReference=HL7-V2,Version=3.7 | 77.40 us |  3.16 | 8.9111 | 0.1221 |  54.83 KB |        1.19 |
| SerializeMessage_Async | Net8 Local   | .NET 8.0           | Default                                | 15.39 us |  0.63 | 3.0518 | 0.0610 |  18.73 KB |        0.41 |
| SerializeMessage_Async | Net8 Nuget   | .NET 8.0           | /p:PackageReference=HL7-V2,Version=3.7 | 24.56 us |  1.00 | 7.5073 | 0.1526 |  46.08 KB |        1.00 |
*/
        private static readonly string _sampleMessage = File.ReadAllText("Sample-Orm.txt");
        private Message _msg;

        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.ShortRun;
                
                AddJob(baseJob.WithMsBuildArguments("/p:PackageReference=HL7-V2,Version=3.7")
                    .WithRuntime(CoreRuntime.Core80)
                    .WithId("Net8 Nuget").AsBaseline());
                AddJob(baseJob.WithMsBuildArguments("/p:PackageReference=HL7-V2,Version=3.7")
                    .WithRuntime(ClrRuntime.Net48)
                    .WithId("Net4.8 Nuget"));
                
                AddJob(baseJob.WithRuntime(ClrRuntime.Net48).WithCustomBuildConfiguration("LOCAL_CODE")
                    .WithId("Net4.8 Local"));
                AddJob(baseJob.WithRuntime(CoreRuntime.Core80).WithCustomBuildConfiguration("LOCAL_CODE")
                    .WithId("Net8 Local"));
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