using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.DataFragmentationHandler.UnitTest.Helpers;
using System.Linq;
using System.Threading;

namespace Nager.DataFragmentationHandler.UnitTest
{
    [TestClass]
    public class StartEndTokenDataPackageHandlerTest
    {
        private int _receivedDataPackageCount = 0;
        private DataPackage _dataPackage;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);

        [TestMethod]
        public void DataPackageHandler_CompleteDataPackage()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, logger: loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x02 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x01, 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x02 }, this._dataPackage.RawData));
        }

        [TestMethod]
        public void DataPackageHandler_FragmentedData()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, logger: loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01 });
            dataPackageHandler.AddData(new byte[] { 0x10, 0x68, 0x65 });
            dataPackageHandler.AddData(new byte[] { 0x6c, 0x6c, 0x6f, 0x02 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x01, 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x02 }, this._dataPackage.RawData));
        }

        [TestMethod]
        public void DataPackageHandler_MultipleDataPackagePartsWithCorruptDataPackageOnStart()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, logger: loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x50, 0x02, 0x01 });
            dataPackageHandler.AddData(new byte[] { 0x10, 0x68, 0x65 });
            dataPackageHandler.AddData(new byte[] { 0x6c, 0x6c, 0x6f, 0x02 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x01, 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x02 }, this._dataPackage.RawData));
        }

        [TestMethod]
        public void DataPackageHandler_MultipleDataPackagePartsWithCorruptDataPackageOnStart1()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, logger: loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            dataPackageHandler.AddData(new byte[] { 0x00, 0x00, 0x00 });
            dataPackageHandler.AddData(new byte[] { 0x01, 0x00, 0x02 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x01, 0x00, 0x02 }, this._dataPackage.RawData));
        }

        private void NewDataPackage(DataPackage dataPackage)
        {
            this._receivedDataPackageCount++;
            this._dataPackage = dataPackage;

            this._semaphoreSlim.Release();
        }
    }
}
