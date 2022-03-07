using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.DataFragmentationHandler.UnitTest.Helpers;
using System.Threading;

namespace Nager.DataFragmentationHandler.UnitTest
{
    [TestClass]
    public class StartTokenWithLengthInfoDataPackageHandlerTest
    {
        private int _receivedDataPackageCount = 0;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);

        [TestMethod]
        public void MessageHandler_CompleteMessage()
        {
            var loggerMock = LoggerHelper.GetLogger<DataPackageHandler>();
            var dataPackageAnalyzer = new StartTokenWithLengthInfoDataPackageAnalyzer(0x01);

            var dataPackageHandler = new DataPackageHandler(loggerMock.Object, dataPackageAnalyzer);
            dataPackageHandler.NewDataPackage += this.NewDataPackage;
            dataPackageHandler.AddData(new byte[] { 0x01, 0x06, 0x10, 0x65, 0x6c, 0x6c, 0x01, 0x06 });
            dataPackageHandler.NewDataPackage -= this.NewDataPackage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedDataPackageCount);
        }

        private void NewDataPackage(byte[] message)
        {
            this._receivedDataPackageCount++;
            this._semaphoreSlim.Release();
        }
    }
}
