using System;

namespace Nager.DataPackageHandler
{
    public interface IDataPackageAnalyzer
    {
        DataPackageAnalyzeResult Analyze(Span<byte> data);
    }
}
