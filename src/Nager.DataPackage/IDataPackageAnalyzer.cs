using System;

namespace Nager.MessageHandler
{
    public interface IDataPackageAnalyzer
    {
        DataPackageAnalyzeResult Analyze(Span<byte> data);
    }
}
