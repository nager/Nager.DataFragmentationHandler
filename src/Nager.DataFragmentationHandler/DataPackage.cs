using System;

namespace Nager.DataFragmentationHandler
{
    public class DataPackage
    {
        public int ContentStartIndex { get; set; }
        public int ContentEndIndex { get; set; }

        public byte[] RawData { get; set; }

        public Span<byte> Data { get { return this.RawData.AsSpan().Slice(this.ContentStartIndex, this.ContentEndIndex); } }
    }
}
