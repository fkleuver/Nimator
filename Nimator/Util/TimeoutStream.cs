using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Nimator.Util
{
    public sealed class TimeoutStream : Stream
    {

        private readonly Action<string> _log;
        private readonly Stream _innerStream;
        private readonly TimeSpan _timeout;
        private readonly Timer _timer;

        public bool TimedOut;

        public TimeoutStream([NotNull]Stream innerStream, TimeSpan timeout, Action<string> log = null)
        {
            Guard.AgainstNull(nameof(innerStream), innerStream);
            _log = log ?? (msg => { });
            _innerStream = innerStream;
            _timeout = timeout;
            _timer = new Timer(_timeout.TotalMilliseconds)
            {
                AutoReset = false
            };
            _timer.Elapsed += (sender, args) =>
            {
                _log($"Timeout of {_timeout} reached.");
                Close();
                TimedOut = true;
            };
            _timer.Start();
        }

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => _innerStream.CanSeek;

        public override bool CanWrite => _innerStream.CanWrite;

        public override long Length => _innerStream.Length;

        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            _timer.Dispose();
            _innerStream.Close();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            Reset();
            return read;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            Reset();
        }

        private void Reset()
        {
            _timer.Stop();
            _log("Timeout timer reseted.");
            _timer.Start();
        }
    }
}
