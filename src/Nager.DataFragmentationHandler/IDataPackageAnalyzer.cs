using System;

namespace Nager.DataFragmentationHandler
{
    public interface IDataPackageAnalyzer
    {
        DataPackageAnalyzeResult Analyze(Span<byte> data);
    }
}
