using Microsoft.Extensions.Logging;
using System;

namespace Nager.MessageHandler
{
    public class MessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly IMessageAnalyzer _messageAnalyzer;
        private readonly IMessageParser[] _messageParsers;

        private readonly byte[] _buffer;
        private int _bufferStartPosition = 0;
        private int _bufferEndPosition = 0;
        private int _bufferDataLength => this._bufferEndPosition - this._bufferStartPosition;

        public event Action<MessageBase> NewMessage;

        public MessageHandler(
            ILogger<MessageHandler> logger,
            IMessageAnalyzer messageAnalyzer,
            IMessageParser[] messageParsers,
            int bufferSize = 1000)
        {
            this._logger = logger;
            this._messageAnalyzer = messageAnalyzer;
            this._messageParsers = messageParsers;

            this._buffer = new byte[bufferSize];
        }

        public void AddData(byte[] rawData)
        {
            if (rawData.Length > this._buffer.Length)
            {
                throw new BufferSizeTooSmallException();
            }

            //Buffer cleanup required
            while (rawData.Length + this._bufferEndPosition > this._buffer.Length)
            {
                if (this.CleanupViaMove())
                {
                    continue;
                }

                var requiredSize = rawData.Length - (this._buffer.Length - this._bufferEndPosition);
                this.CleanupViaRemove(requiredSize);

                break;
            }

            Array.Copy(rawData, 0, this._buffer, this._bufferEndPosition, rawData.Length);
            this._bufferEndPosition += rawData.Length;

            while (this.ProcessBuffer())
            {
                //additional processing steps required
            }
        }

        private bool CleanupViaMove()
        {
            if (this._bufferStartPosition == 0)
            {
                return false;
            }

            /*
             * Move data to the start of the buffer
             * 
             * BUFFER
             * ┌──────────────────────────┐
             * |                  ┌──────┐|
             * | <--------------- | DATA ||
             * |                  └──────┘|
             * └──────────────────────────┘
            */

            var bufferDataLength = this._bufferDataLength;
            Array.Copy(this._buffer, this._bufferStartPosition, this._buffer, 0, this._bufferDataLength);
            this._bufferStartPosition = 0;
            this._bufferEndPosition = bufferDataLength;

            return true;
        }

        private void CleanupViaRemove(int requiredSize)
        {
            /*
             * Remove data at the beginning of the buffer
             * 
             * BUFFER
             * ┌──────────────────────────┐
             * |┌──────────────────────┐  |
             * ||░░░       DATA        |  |
             * |└──────────────────────┘  |
             * |                       ┌──────┐
             * |                       | DATA |
             * |                       └──────┘
             * └──────────────────────────┘
            */

            var bufferDataLength = this._bufferDataLength - requiredSize;
            Array.Copy(this._buffer, 0, this._buffer, requiredSize, bufferDataLength);
            this._bufferEndPosition = bufferDataLength;
        }

        private bool ProcessBuffer()
        {
            var data = this._buffer.AsSpan().Slice(this._bufferStartPosition, this._bufferDataLength);

            var analyzeResult = this._messageAnalyzer.Analyze(data);
            if (!analyzeResult.MessageAvailable)
            {
                return false;
            }

            if (analyzeResult.MessageAvailable &&
                !analyzeResult.MessageComplete)
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

            return this._bufferStartPosition != this._bufferEndPosition;
        }

        private void RemoveLastMessage(int messageLength)
        {
            this._bufferStartPosition += messageLength;
        }
    }
}
