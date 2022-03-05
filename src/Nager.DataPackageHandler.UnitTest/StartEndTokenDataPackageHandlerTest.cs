using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.DataPackageHandler.UnitTest.Helpers;
using System.Threading;

namespace Nager.DataPackageHandler.UnitTest
{
    [TestClass]
    public class StartEndTokenDataPackageHandlerTest
    {
        private int _receivedDataPackageCount = 0;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);

        [TestMethod]
        public void DataPackageHandler_CompleteDataPackage()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(loggerMock.Object, dataPackageAnalyzer);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x02 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
        }

        [TestMethod]
        public void DataPackageHandler_FragmentedData()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(loggerMock.Object, dataPackageAnalyzer);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01 });
            dataPackageHandler.AddData(new byte[] { 0x10, 0x68, 0x65 });
            dataPackageHandler.AddData(new byte[] { 0x6c, 0x6c, 0x6f, 0x02 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
        }

        [TestMethod]
        public void DataPackageHandler_MultipleDataPackagePartsWithCorruptDataPackageOnStart()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(loggerMock.Object, dataPackageAnalyzer);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x50, 0x02, 0x01 });
            dataPackageHandler.AddData(new byte[] { 0x10, 0x68, 0x65 });
            dataPackageHandler.AddData(new byte[] { 0x6c, 0x6c, 0x6f, 0x02 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
        }

        [TestMethod]
        [ExpectedException(typeof(BufferSizeTooSmallException))]
        public void DataPackageHandler_BufferSizeTooSmall()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0x02);

            var dataPackageHandler = new DataPackageHandler(loggerMock.Object, dataPackageAnalyzer, bufferSize: 5);
            dataPackageHandler.AddData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
        }

        [TestMethod]
        public void DataPackageHandler_BufferCleanupViaMove()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0x01, 0xFF);

            var dataPackageHandler = new DataPackageHandler(loggerMock.Object, dataPackageAnalyzer, bufferSize: 9);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x11, 0xFF, 0x01, 0x11 });
            dataPackageHandler.AddData(new byte[] { 0xFF, 0x02, 0x03, 0x04, 0x05 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            //TODO: Add Assert logic
        }

        [TestMethod]
        public void DataPackageHandler_BufferCleanupViaRemove()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartEndTokenDataPackageAnalyzer(0xF1, 0xFF);

            var dataPackageHandler = new DataPackageHandler(loggerMock.Object, dataPackageAnalyzer, bufferSize: 5);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 });
            dataPackageHandler.AddData(new byte[] { 0xF1, 0x11, 0xFF });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            //TODO: Add Assert logic
        }

        private void NewDataPackage(byte[] message)
        {
            this._receivedDataPackageCount++;
            this._semaphoreSlim.Release();
        }
    }
}
