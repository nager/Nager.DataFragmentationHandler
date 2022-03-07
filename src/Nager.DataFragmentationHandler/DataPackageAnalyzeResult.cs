namespace Nager.DataFragmentationHandler
{
    public class DataPackageAnalyzeResult
    {
        public DataPackageStatus Status { get; set; }

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public int ContentStartIndex { get; set; }
        public int ContentEndIndex { get; set; }

        public int Length { get { return this.EndIndex - this.StartIndex; } }
    }
}
