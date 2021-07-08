namespace Nager.MessageHandler
{
    public interface IMessageAnalyzer
    {
        DataAnalyzeResult Analyze(byte[] data);
    }
}
