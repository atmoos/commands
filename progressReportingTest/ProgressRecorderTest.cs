using System;
using System.Linq;
using progressReporting;
using Xunit;
using static progressReportingTest.Convenience;

namespace progressReportingTest
{
    public sealed class ProgressRecorderTest
    {
        private readonly ProgressRecorder<Int32> recorder = new();

        [Fact]
        public void RecorderRecordsInSameOrderAsValuesAreAdded()
        {
            var expected = Report(this.recorder, RandomIntegers()).Take(7).ToList();
            Assert.Equal(expected, this.recorder);
        }
        [Fact]
        public void IndexingOnRecorderIndexesToCorrectElement()
        {
            var reference = Report(this.recorder, RandomIntegers()).Take(11).ToArray();
            for(var index = 0; index < reference.Length; ++index) {
                Assert.Equal(reference[index], this.recorder[index]);
            }
        }
    }
}