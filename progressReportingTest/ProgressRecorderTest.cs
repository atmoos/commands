using System;
using System.Linq;
using Xunit;
using progressReporting;

using static progressReportingTest.Convenience;

namespace progressReportingTest
{
    public sealed class ProgressRecorderTest
    {
        private readonly ProgressRecorder<Int32> _recorder = new ProgressRecorder<Int32>();
        [Fact]
        public void RecorderRecordsInSameOrderAsValuesAreAdded()
        {
            var expected = Report(_recorder, RandomIntegers()).Take(7).ToList();
            Assert.Equal(expected, _recorder);
        }
        [Fact]
        public void IndexingOnRecorderIndexesToCorrectElement()
        {
            var reference = Report(_recorder, RandomIntegers()).Take(11).ToArray();
            for(var index = 0; index < reference.Length; ++index) {
                Assert.Equal(reference[index], _recorder[index]);
            }
        }
    }
}