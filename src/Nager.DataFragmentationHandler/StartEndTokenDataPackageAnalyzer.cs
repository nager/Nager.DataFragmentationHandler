using System;

namespace Nager.DataFragmentationHandler
{
    public class StartEndTokenDataPackageAnalyzer : IDataPackageAnalyzer
    {
        private readonly byte _startToken;
        private readonly byte _endToken;

        public StartEndTokenDataPackageAnalyzer(
            byte startToken,
            byte endToken)
        {
            this._startToken = startToken;
            this._endToken = endToken;
        }

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

            var messageEndIndex = data.IndexOf(this._endToken);
            if (messageEndIndex == -1)
            {
                return new DataPackageAnalyzeResult
                {
                    Status = DataPackageStatus.Uncompleted
                };
            }

            return new DataPackageAnalyzeResult
            {
                Status = DataPackageStatus.Available,
                StartIndex = messageStartIndex,
                EndIndex = messageEndIndex + 1,
                ContentStartIndex = messageStartIndex + 1,
                ContentEndIndex = messageEndIndex
            };
        }
    }
}
