namespace Nager.MessageHandler
{
    public class DataAnalyzeResult
    {
        public bool MessageAvailable { get; set; }
        public int MessageStartIndex { get; set; }
        public int MessageEndIndex { get; set; }

        public int MessageLength { get { return this.MessageEndIndex - this.MessageStartIndex; } }
    }
}
