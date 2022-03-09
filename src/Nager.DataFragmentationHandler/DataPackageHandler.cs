using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Nager.DataFragmentationHandler
{
    /// <summary>
    /// DataPackageHandler
    /// </summary>
    public class DataPackageHandler
    {
        private readonly ILogger _logger;
        private readonly IDataPackageAnalyzer _dataPackageAnalyzer;
        private readonly object _syncLock = new object();

        private readonly byte[] _buffer;
        private int _bufferStartPosition = 0;
        private int _bufferEndPosition = 0;
        private int BufferDataLength => this._bufferEndPosition - this._bufferStartPosition;

        /// <summary>
        /// New DataPackage available
        /// </summary>
        public event Action<DataPackage> NewDataPackage;

        /// <summary>
        /// Truncated data was removed
        /// </summary>
        public event Action TruncatedDataRemoved;

        /// <summary>
        /// DataPackage Handler
        /// </summary>
        /// <param name="dataPackageAnalyzer"></param>
        /// <param name="bufferSize"></param>
        /// <param name="logger"></param>
        public DataPackageHandler(
            IDataPackageAnalyzer dataPackageAnalyzer,
            int bufferSize = 1024,
            ILogger logger = default)
        {
            if (logger == default)
            {
                logger = NullLogger.Instance;
            }
            this._logger = logger;

            this._dataPackageAnalyzer = dataPackageAnalyzer;

            this._buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Add new data from device
        /// </summary>
        /// <param name="rawData"></param>
        /// <exception cref="BufferSizeTooSmallException"></exception>
        public void AddData(byte[] rawData)
        {
            lock (this._syncLock)
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
            if (this._bufferStartPosition == this._bufferEndPosition)
            {
                return false;
            }

            var data = this._buffer.AsSpan().Slice(this._bufferStartPosition, this.BufferDataLength);

            var analyzeResult = this._dataPackageAnalyzer.Analyze(data);
            switch (analyzeResult.Status)
            {
                case DataPackageStatus.NotAvailable:
                    return false;
                case DataPackageStatus.Uncompleted:
                    return false;
                case DataPackageStatus.Truncated:
                    this.TruncatedDataRemoved?.Invoke();
                    this._logger?.LogInformation($"{nameof(ProcessBuffer)} - Truncated data removed");
                    this.RemoveLastMessage(analyzeResult.EndIndex);
                    return true;
                case DataPackageStatus.Available:
                    break;
                default:
                    throw new NotImplementedException("DataAnalyzeStatus is unknown");
            }

            var rawData = data.Slice(analyzeResult.StartIndex, analyzeResult.EndIndex - analyzeResult.StartIndex);
            var dataPackage = new DataPackage
            {
                RawData = rawData.ToArray(),
                ContentStartIndex = analyzeResult.ContentStartIndex,
                ContentEndIndex = analyzeResult.ContentEndIndex - analyzeResult.ContentStartIndex
            };

            this.NewDataPackage?.Invoke(dataPackage);

            this.RemoveLastMessage(analyzeResult.EndIndex);

            return this._bufferStartPosition != this._bufferEndPosition;
        }

        private void RemoveLastMessage(int messageLength)
        {
            this._bufferStartPosition += messageLength;
        }
    }
}
