using System;

namespace Nager.MessageHandler
{
    public class StartEndTokenMessageAnalyzer : IMessageAnalyzer
    {
        private readonly byte _startToken;
        private readonly byte _endToken;

        public StartEndTokenMessageAnalyzer(
            byte startToken,
            byte endToken)
        {
            this._startToken = startToken;
            this._endToken = endToken;
        }

        public DataAnalyzeResult Analyze(Span<byte> data)
        {
            var messageEndIndex = data.IndexOf(this._endToken);
            if (messageEndIndex == -1)
            {
                return new DataAnalyzeResult { MessageAvailable = false };
            }

            var messageStartIndex = data.Slice(0, messageEndIndex).IndexOf(this._startToken);
            if (messageStartIndex == -1)
            {
                // Truncated message without startToken available
                return new DataAnalyzeResult { MessageAvailable = true, MessageEndIndex = messageEndIndex + 1 };
            }

            return new DataAnalyzeResult
            {
                MessageAvailable = true,
                MessageStartIndex = messageStartIndex,
                MessageEndIndex = messageEndIndex + 1
            };
        }
    }
}
