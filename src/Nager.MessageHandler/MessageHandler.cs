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

        private readonly ConcurrentQueue<byte> _buffer = new ConcurrentQueue<byte>();

        public event Action<MessageBase> NewMessage;

        public MessageHandler(
            ILogger<MessageHandler> logger,
            IMessageAnalyzer messageAnalyzer,
            IMessageParser[] messageParsers)
        {
            this._logger = logger;
            this._messageAnalyzer = messageAnalyzer;
            this._messageParsers = messageParsers;
        }

        public void AddData(byte[] rawData)
        {
            foreach (var b in rawData)
            {
                this._buffer.Enqueue(b);
            }

            while (this.ProcessBuffer())
            {
                //process
            }
        }

        private bool ProcessBuffer()
        {
            var data = this._buffer.Take(100).ToArray();

            var analyzeResult = this._messageAnalyzer.Analyze(data);
            if (!analyzeResult.MessageAvailable)
            {
                return false;
            }

            if (analyzeResult.MessageAvailable && analyzeResult.MessageLength == 0)
            {
                this._logger?.LogInformation($"{nameof(ProcessBuffer)} - Corrupt message remove");
                this.RemoveLastMessage(analyzeResult.MessageEndIndex);
                return true;
            }

            var messageData = data.Skip(analyzeResult.MessageStartIndex).Take(analyzeResult.MessageLength).ToArray();

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
                var hex = BitConverter.ToString(messageData);
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
