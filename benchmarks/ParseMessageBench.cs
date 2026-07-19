using System.IO;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

using Efferent.HL7.V2;

namespace Benchmarks
{
    /// <summary>
    /// Basic benchmark comparing the nuget version to the local code, for both .NET Framework 4.8 and .NET 8.0.  
    /// Calls a copy of one of the tests that best reflects the usage scenarios (Parse and GetValue)
    /// </summary>
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    [HideColumns("BuildConfiguration", "Error", "StdDev", "RatioSD")]
    public class ParseMessageBench
    {
        internal static readonly string _sampleMessage = File.ReadAllText("Sample-Orm.txt");

        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun;

                AddJob(baseJob.WithMsBuildArguments("/p:HL7V2Version=3.7.2").WithId("NuGet 3.7.2").AsBaseline());
                AddJob(baseJob.WithId("Local"));
            }
        }

        [Benchmark]
        public void ParseMessageAndGetValues()
        {
            var msg = new Message(_sampleMessage);
            msg.ParseMessage(true);

            // 2-component paths: SegmentRegex + FieldSegmentRegex (×2 each)
            _ = msg.GetValue("MSH.3");
            _ = msg.GetValue("MSH.4");
            _ = msg.GetValue("MSH.5");
            _ = msg.GetValue("MSH.9");
            _ = msg.GetValue("PID.7");
            _ = msg.GetValue("PID.8");
            _ = msg.GetValue("ORC.1");
            _ = msg.GetValue("ORC.2");
            _ = msg.GetValue("OBR.2");

            // 3-component paths: SegmentRegex + FieldSegmentRegex + OtherRegex (×3 each)
            _ = msg.GetValue("PID.5.1");
            _ = msg.GetValue("PID.5.2");
            _ = msg.GetValue("PID.5.3");
            _ = msg.GetValue("PID.11.3");
            _ = msg.GetValue("PID.11.4");
            _ = msg.GetValue("PID.11.5");
            _ = msg.GetValue("ORC.12.1");
            _ = msg.GetValue("ORC.12.2");
            _ = msg.GetValue("OBR.4.1");
            _ = msg.GetValue("OBR.4.2");
            _ = msg.GetValue("OBR.4.3");

            // Segment occurrence index: exercises the [n] capture group in SegmentRegex
            _ = msg.GetValue("NTE[1].3");
            _ = msg.GetValue("NTE[2].3");
        }
    }
}