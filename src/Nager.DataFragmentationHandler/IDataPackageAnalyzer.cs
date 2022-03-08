using System;

namespace Nager.DataFragmentationHandler
{
    /// <summary>
    /// Interface DataPackageAnalyzer
    /// </summary>
    public interface IDataPackageAnalyzer
    {
        /// <summary>
        /// Analyze the data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        DataPackageAnalyzeResult Analyze(Span<byte> data);
    }
}
