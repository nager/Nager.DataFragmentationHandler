using System;

namespace Nager.DataFragmentationHandler
{
    /// <summary>
    /// StartToken with message length info on second position DataPackageAnalyzer
    /// </summary>
    public class StartTokenWithLengthInfoDataPackageAnalyzer : IDataPackageAnalyzer
    {
        private readonly byte _startToken;

        /// <summary>
        /// StartToken with message length info on second position DataPackageAnalyzer
        /// </summary>
        /// <param name="startToken"></param>
        public StartTokenWithLengthInfoDataPackageAnalyzer(
            byte startToken)
        {
            this._startToken = startToken;
        }

        /// <inheritdoc/>
        public DataPackageAnalyzeResult Analyze(Span<byte> data)
        {
            var messageStartIndex = data.IndexOf(this._startToken);
            if (messageStartIndex == -1)
            {
                // Truncated message without startToken available
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Truncated,
                    EndIndex = data.Length
                };
            }

            if (data.Length == 1)
            {
                // Only startToken is available, wait for more data
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Uncompleted,
                    StartIndex = messageStartIndex
                };
            }

            var messageLength = data.Slice(messageStartIndex)[1];
            if (messageLength == 0)
            {
                // Corrupt message length is not possible
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Truncated,
                    EndIndex = 1
                };
            }

            if (messageLength > data.Length)
            {
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Uncompleted,
                    StartIndex = messageStartIndex
                };
            }

            return new DataPackageAnalyzeResult
            {
                Status = DataPackageStatus.Available,
                StartIndex = messageStartIndex,
                EndIndex = messageStartIndex + messageLength,
                ContentStartIndex = messageStartIndex + 2, //StartToken + LengthInfo
                ContentEndIndex = messageStartIndex + messageLength
            };
        }
    }
}
