using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progress;

namespace progressTest
{
    public sealed class ReporterTest
    {

        private readonly Reset _reset;
        private readonly Reporter _reporter;
        private readonly ProgressRecorder<Double> _progress;
        public ReporterTest()
        {
            _reset = new Reset();
            _progress = new ProgressRecorder<Double>();
            _reporter = new Reporter(ProgressDriver.Create(3), _progress, _reset);
        }
        [Fact]
        public void ZeroIsReportedUponCreation()
        {
            Assert.Equal(0d, _progress.First());
        }
        [Fact]
        public void OneIsReportedUponDisposal()
        {
            _reporter.Dispose();
            Assert.Equal(1d, _progress.Last());
        }

        [Fact]
        public void ResetIsPerformedUponDisposal()
        {
            _reporter.Dispose();
            Assert.True(_reset.Disposed, "Reset action must be called!");
        }
        private sealed class Reset : IDisposable
        {
            public Boolean Disposed { get; private set; } = false;
            public void Dispose() => Disposed = true;
        }
    }
}