using System;

namespace Nager.DataPackageHandler
{
    public class StartTokenWithLengthInfoDataPackageAnalyzer : IDataPackageAnalyzer
    {
        private readonly byte _startToken;

        public StartTokenWithLengthInfoDataPackageAnalyzer(
            byte startToken)
        {
            this._startToken = startToken;
        }

        public DataPackageAnalyzeResult Analyze(Span<byte> data)
        {
            var messageStartIndex = data.IndexOf(this._startToken);
            if (messageStartIndex == -1)
            {
                // Truncated message without startToken available
                return new DataPackageAnalyzeResult
                {
                    DataPackageStatus = DataPackageStatus.Truncated,
                    DataPackageEndIndex = data.Length + 1
                };
            }

            var messageLength = data.Slice(messageStartIndex)[1];

            if (messageLength > data.Length)
            {
                return new DataPackageAnalyzeResult
                {
                    DataPackageStatus = DataPackageStatus.Uncompleted,
                    DataPackageStartIndex = messageStartIndex
                };
            }

            return new DataPackageAnalyzeResult
            {
                DataPackageStatus = DataPackageStatus.Available,
                DataPackageStartIndex = messageStartIndex,
                DataPackageEndIndex = messageStartIndex + messageLength,
                ContentStartIndex = messageStartIndex + 2, //StartToken + LengthInfo
                ContentEndIndex = messageStartIndex + messageLength
            };
        }
    }
}
