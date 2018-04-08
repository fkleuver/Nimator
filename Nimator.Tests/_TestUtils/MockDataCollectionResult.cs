using System;

namespace Nimator.Tests
{
    public sealed class MockDataCollectionResult : IDataCollectionResult
    {
        public IDataCollectionResult Mock { get; }

        public MockDataCollectionResult(IDataCollectionResult mock)
        {
            Mock = mock;
        }

        public IDataCollector Origin => Mock.Origin;

        public long Start => Mock.Start;

        public long End => Mock.End;

        public object Data => Mock.Data;

        public Exception Error => Mock.Error;

        public bool NeedsProcessing => Mock.NeedsProcessing;

        public void StopProcessing()
        {
            Mock.StopProcessing();
        }
    }
}
