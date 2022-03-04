using System;

namespace Nager.MessageHandler
{
    public interface IMessageAnalyzer
    {
        DataAnalyzeResult Analyze(Span<byte> data);
    }
}
