namespace Nager.DataPackageHandler
{
    /// <summary>
    /// Data Package Status
    /// </summary>
    public enum DataPackageStatus
    {
        /// <summary>
        /// A complete data package is available
        /// </summary>
        Available,

        /// <summary>
        /// An uncompleted data package is present
        /// </summary>
        Uncompleted,

        /// <summary>
        /// A truncated data package is present that can no longer be completed.
        /// </summary>
        Truncated,

        /// <summary>
        /// No data package is available
        /// </summary>
        NotAvailable
    }
}
