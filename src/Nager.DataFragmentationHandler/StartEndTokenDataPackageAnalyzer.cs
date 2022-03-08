using System;

namespace Nager.DataFragmentationHandler
{
    /// <summary>
    /// StartToken and EndToken DataPackageAnalyzer
    /// </summary>
    public class StartEndTokenDataPackageAnalyzer : IDataPackageAnalyzer
    {
        private readonly byte _startToken;
        private readonly byte _endToken;

        /// <summary>
        /// StartToken and EndToken DataPackageAnalyzer
        /// </summary>
        /// <param name="startToken"></param>
        /// <param name="endToken"></param>
        public StartEndTokenDataPackageAnalyzer(
            byte startToken,
            byte endToken)
        {
            this._startToken = startToken;
            this._endToken = endToken;
        }
        
        /// <inheritdoc/>
        public DataPackageAnalyzeResult Analyze(Span<byte> data)
        {
            var messageStartIndex = data.IndexOf(this._startToken);
            if (messageStartIndex == -1)
            {
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.NotAvailable
                };
            }

            if (messageStartIndex > 0)
            {
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Truncated,
                    EndIndex = messageStartIndex
                };
            }

            var messageEndIndex = data.Slice(1).IndexOf(this._endToken);
            if (messageEndIndex == -1)
            {
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Uncompleted
                };
            }

            var messageSecondStartIndex = data.Slice(1).IndexOf(this._startToken);
            if (messageSecondStartIndex != -1 && messageEndIndex > messageSecondStartIndex)
            {
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Truncated,
                    EndIndex = messageSecondStartIndex
                };
            }

            return new DataPackageAnalyzeResult
            {
                Status = DataPackageStatus.Available,
                StartIndex = messageStartIndex,
                EndIndex = messageEndIndex + 2,
                ContentStartIndex = messageStartIndex + 1,
                ContentEndIndex = messageEndIndex + 1
            };
        }
    }
}
