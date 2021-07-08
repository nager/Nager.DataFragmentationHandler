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

        public DataAnalyzeResult Analyze(byte[] data)
        {
            var messageEndIndex = Array.IndexOf(data, this._endToken);
            if (messageEndIndex == -1)
            {
                return new DataAnalyzeResult { MessageAvailable = false };
            }

            var messageStartIndex = Array.LastIndexOf(data, this._startToken, messageEndIndex);
            if (messageStartIndex == -1)
            {
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
