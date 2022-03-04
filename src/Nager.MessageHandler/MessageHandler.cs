using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Nager.MessageHandler
{
    public class MessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly IMessageAnalyzer _messageAnalyzer;
        private readonly IMessageParser[] _messageParsers;
        private readonly int _maxMessageSize;

        private readonly ConcurrentQueue<byte> _buffer = new ConcurrentQueue<byte>();

        public event Action<MessageBase> NewMessage;

        public MessageHandler(
            ILogger<MessageHandler> logger,
            IMessageAnalyzer messageAnalyzer,
            IMessageParser[] messageParsers,
            int maxMessageSize = 100)
        {
            this._logger = logger;
            this._messageAnalyzer = messageAnalyzer;
            this._messageParsers = messageParsers;
            this._maxMessageSize = maxMessageSize;
        }

        public void AddData(byte[] rawData)
        {
            foreach (var b in rawData)
            {
                this._buffer.Enqueue(b);
            }

            while (this.ProcessBuffer())
            {
                //additional processing steps required
            }
        }

        private bool ProcessBuffer()
        {
            var data = this._buffer.Take(this._maxMessageSize).ToArray().AsSpan();

            var analyzeResult = this._messageAnalyzer.Analyze(data);
            if (!analyzeResult.MessageAvailable)
            {
                return false;
            }

            if (analyzeResult.MessageAvailable &&
                analyzeResult.MessageLength == 0)
            {
                this._logger?.LogInformation($"{nameof(ProcessBuffer)} - Corrupt message remove");
                this.RemoveLastMessage(analyzeResult.MessageEndIndex);
                return true;
            }

            var messageData = data.Slice(analyzeResult.MessageStartIndex, analyzeResult.MessageLength);

            MessageBase message = default;
            foreach (var messageParser in this._messageParsers)
            {
                message = messageParser.Parse(messageData);
                if (message != null)
                {
                    break;
                }
            }

            if (message == default)
            {
                var hex = BitConverter.ToString(messageData.ToArray());
                this._logger?.LogDebug($"{nameof(ProcessBuffer)} - Cannot parse message {hex}");
            }
            else
            {
                this.NewMessage?.Invoke(message);
            }

            this.RemoveLastMessage(analyzeResult.MessageEndIndex);

            return true;
        }

        private void RemoveLastMessage(int messageLength)
        {
            for (var i = 0; i < messageLength; i++)
            {
                this._buffer.TryDequeue(out var _);
            }
        }
    }
}
