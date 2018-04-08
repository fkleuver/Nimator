using System;
using Couchbase;

namespace Nimator.Couchbase.Tests
{
    public sealed class MockResult<T> : IResult<T>
    {
        public IResult<T> Mock { get; }


        public MockResult(IResult<T> mock)
        {
            Mock = mock;
        }

        public bool ShouldRetry()
        {
            return Mock.ShouldRetry();
        }

        public bool Success => Mock.Success;

        public string Message => Mock.Message;

        public Exception Exception => Mock.Exception;

        public T Value => Mock.Value;
    }
}
