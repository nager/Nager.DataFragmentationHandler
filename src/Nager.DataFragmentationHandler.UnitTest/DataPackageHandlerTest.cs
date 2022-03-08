using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nager.DataFragmentationHandler.UnitTest.Helpers;
using System.Linq;
using System.Threading;

namespace Nager.DataFragmentationHandler.UnitTest
{
    [TestClass]
    public class DataPackageHandlerTest
    {
        private int _receivedDataPackageCount = 0;
        private DataPackage _dataPackage;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);

        [TestMethod]
        [ExpectedException(typeof(BufferSizeTooSmallException))]
        public void DataPackageHandler_BufferSizeTooSmall()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzerMock = new Mock<IDataPackageAnalyzer>();

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzerMock.Object, bufferSize: 5, loggerMock.Object);
            dataPackageHandler.AddData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
        }

        [TestMethod]
        public void DataPackageHandler_BufferCleanupViaMove()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0xff);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, bufferSize: 9, loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x11, 0xff, 0x01, 0x11 });
            dataPackageHandler.AddData(new byte[] { 0xff, 0x01, 0x02, 0x03, 0x04 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(2, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0x01, 0x11, 0xff }, this._dataPackage.RawData));
        }

        [TestMethod]
        public void DataPackageHandler_BufferCleanupViaRemove()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0xf1, 0xff);

            var dataPackageHandler = new DataPackageHandler(dataPackageAnalyzer, bufferSize: 5, loggerMock.Object);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 });
            dataPackageHandler.AddData(new byte[] { 0xf1, 0x11, 0xff });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 0xf1, 0x11, 0xff }, this._dataPackage.RawData));
        }

        private void NewDataPackage(DataPackage dataPackage)
        {
            this._receivedDataPackageCount++;
            this._dataPackage = dataPackage;

            this._semaphoreSlim.Release();
        }
    }
}
