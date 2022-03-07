using Microsoft.Extensions.Logging;
using System;

namespace Nager.DataFragmentationHandler
{
    public class DataPackageHandler
    {
        private readonly ILogger<DataPackageHandler> _logger;
        private readonly IDataPackageAnalyzer _dataPackageAnalyzer;

        private readonly byte[] _buffer;
        private int _bufferStartPosition = 0;
        private int _bufferEndPosition = 0;
        private int BufferDataLength => this._bufferEndPosition - this._bufferStartPosition;

        public event Action<byte[]> NewDataPackage;

        public DataPackageHandler(
            ILogger<DataPackageHandler> logger,
            IDataPackageAnalyzer dataPackageAnalyzer,
            int bufferSize = 1000)
        {
            this._logger = logger;
            this._dataPackageAnalyzer = dataPackageAnalyzer;

            this._buffer = new byte[bufferSize];
        }

        public void AddData(byte[] rawData)
        {
            if (rawData.Length > this._buffer.Length)
            {
                throw new BufferSizeTooSmallException();
            }

            //Check buffer cleanup required
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

            var bufferDataLength = this.BufferDataLength;
            Array.Copy(this._buffer, this._bufferStartPosition, this._buffer, 0, this.BufferDataLength);
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

            var bufferDataLength = this.BufferDataLength - requiredSize;
            Array.Copy(this._buffer, 0, this._buffer, requiredSize, bufferDataLength);
            this._bufferEndPosition = bufferDataLength;
        }

        private bool ProcessBuffer()
        {
            var data = this._buffer.AsSpan().Slice(this._bufferStartPosition, this.BufferDataLength);

            var analyzeResult = this._dataPackageAnalyzer.Analyze(data);
            switch (analyzeResult.DataPackageStatus)
            {
                case DataPackageStatus.NotAvailable:
                    return false;
                case DataPackageStatus.Uncompleted:
                    return false;
                case DataPackageStatus.Truncated:
                    this._logger?.LogInformation($"{nameof(ProcessBuffer)} - Truncated message remove");
                    this.RemoveLastMessage(analyzeResult.DataPackageEndIndex);
                    return true;
                case DataPackageStatus.Available:
                    break;
                default:
                    throw new NotImplementedException("DataAnalyzeStatus is unknown");
            }

            var dataPackage = data.Slice(analyzeResult.ContentStartIndex, analyzeResult.ContentEndIndex - analyzeResult.ContentStartIndex);

            this.NewDataPackage?.Invoke(dataPackage.ToArray());

            this.RemoveLastMessage(analyzeResult.DataPackageEndIndex);

            return this._bufferStartPosition != this._bufferEndPosition;
        }

        private void RemoveLastMessage(int messageLength)
        {
            this._bufferStartPosition += messageLength;
        }
    }
}
