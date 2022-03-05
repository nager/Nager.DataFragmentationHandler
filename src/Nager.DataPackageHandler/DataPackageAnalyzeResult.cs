namespace Nager.DataPackageHandler
{
    public class DataPackageAnalyzeResult
    {
        public DataPackageStatus DataPackageStatus { get; set; }

        public int DataPackageStartIndex { get; set; }
        public int DataPackageEndIndex { get; set; }

        public int ContentStartIndex { get; set; }
        public int ContentEndIndex { get; set; }

        public int DataPackageLength { get { return this.DataPackageEndIndex - this.DataPackageStartIndex; } }
    }
}
