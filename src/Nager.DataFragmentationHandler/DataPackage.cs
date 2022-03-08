using System;

namespace Nager.DataFragmentationHandler
{
    /// <summary>
    /// DataPackage
    /// </summary>
    public class DataPackage
    {
        /// <summary>
        /// Content StartIndex
        /// </summary>
        public int ContentStartIndex { get; set; }

        /// <summary>
        /// Content EndIndex
        /// </summary>
        public int ContentEndIndex { get; set; }

        /// <summary>
        /// Raw Data
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// Content Data
        /// </summary>
        public Span<byte> Data { get { return this.RawData.AsSpan().Slice(this.ContentStartIndex, this.ContentEndIndex); } }
    }
}
