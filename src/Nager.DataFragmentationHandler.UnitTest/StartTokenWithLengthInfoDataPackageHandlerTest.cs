using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.DataFragmentationHandler.UnitTest.Helpers;
using System.Linq;
using System.Threading;

namespace Nager.DataFragmentationHandler.UnitTest
{
    [TestClass]
    public class StartTokenWithLengthInfoDataPackageHandlerTest
    {
        private int _receivedDataPackageCount = 0;
        private DataPackage _dataPackage;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);

        [TestMethod]
        public void MessageHandler_CompleteMessageWithPartOfNext()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartTokenWithLengthInfoDataPackageAnalyzer(0x01);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, logger: loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x06, 0x10, 0x65, 0x6c, 0x6c, 0x01, 0x06 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x01, 0x06, 0x10, 0x65, 0x6c, 0x6c }, this._dataPackage.RawData));
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x10, 0x65, 0x6c, 0x6c }, this._dataPackage.Data.ToArray()));
        }

        [TestMethod]
        public void MessageHandler_FragmentedData()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartTokenWithLengthInfoDataPackageAnalyzer(0x01);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, logger: loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01 });
            dataPackageHandler.AddData(new byte[] { 0x06, 0x10, 0x65, 0x6c, 0x6c });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x01, 0x06, 0x10, 0x65, 0x6c, 0x6c }, this._dataPackage.RawData));
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x10, 0x65, 0x6c, 0x6c }, this._dataPackage.Data.ToArray()));
        }

        private void NewDataPackage(DataPackage dataPackage)
        {
            this._receivedDataPackageCount++;
            this._dataPackage = dataPackage;

            this._semaphoreSlim.Release();
        }
    }
}
