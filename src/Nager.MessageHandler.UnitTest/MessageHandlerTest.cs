using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.MessageHandler.UnitTest.Helpers;
using Nager.MessageHandler.UnitTest.MessageParsers;
using Nager.MessageHandler.UnitTest.Messages;
using System.Diagnostics;
using System.Threading;

namespace Nager.MessageHandler.UnitTest
{
    [TestClass]
    public class MessageHandlerTest
    {
        private int _receivedMessageCount = 0;
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(10);

        [TestMethod]
        public void MessageHandler_CompleteMessage()
        {
            var loggerMock = LoggerHelper.GetLogger<MessageHandler>();
            var messageAnalyzer = new StartEndTokenMessageAnalyzer(0x01, 0x02);
            var messageParsers = new IMessageParser[]
            {
                new HelloMessageParser(),
                new StatusInfoMessageParser()
            };

            var messageHandler = new MessageHandler(loggerMock.Object, messageAnalyzer, messageParsers);
            messageHandler.NewMessage += NewMessage;
            messageHandler.AddData(new byte[] { 0x01, 0x10, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x02 });
            this._semaphoreSlim.Wait();

            Assert.AreEqual(1, this._receivedMessageCount);
        }

        [TestMethod]
        public void MessageHandler_MultipleMessagParts()
        {
            var loggerMock = LoggerHelper.GetLogger<MessageHandler>();
            var messageAnalyzer = new StartEndTokenMessageAnalyzer(0x01, 0x02);
            var messageParsers = new IMessageParser[]
            {
                new HelloMessageParser(),
                new StatusInfoMessageParser()
            };

            var messageHandler = new MessageHandler(loggerMock.Object, messageAnalyzer, messageParsers);
            messageHandler.NewMessage += NewMessage;
            messageHandler.AddData(new byte[] { 0x01 });
            messageHandler.AddData(new byte[] { 0x10, 0x68, 0x65 });
            messageHandler.AddData(new byte[] { 0x6c, 0x6c, 0x6f, 0x02 });
            this._semaphoreSlim.Wait(1000);

            Assert.AreEqual(1, this._receivedMessageCount);
        }

        [TestMethod]
        public void MessageHandler_MultipleMessagPartsWithCorruptOldMessageOnStart()
        {
            var loggerMock = LoggerHelper.GetLogger<MessageHandler>();
            var messageAnalyzer = new StartEndTokenMessageAnalyzer(0x01, 0x02);
            var messageParsers = new IMessageParser[]
            {
                new HelloMessageParser(),
                new StatusInfoMessageParser()
            };

            var messageHandler = new MessageHandler(loggerMock.Object, messageAnalyzer, messageParsers);
            messageHandler.NewMessage += NewMessage;
            messageHandler.AddData(new byte[] { 0x50, 0x02, 0x01 });
            messageHandler.AddData(new byte[] { 0x10, 0x68, 0x65 });
            messageHandler.AddData(new byte[] { 0x6c, 0x6c, 0x6f, 0x02 });
            this._semaphoreSlim.Wait(1000);

            Assert.AreEqual(1, this._receivedMessageCount);
        }

        private void NewMessage(MessageBase message)
        {
            try
            {
                if (message is HelloMessage helloMessage)
                {
                    Debug.WriteLine(helloMessage.Message);
                    return;
                }

                if (message is StatusInfoMessage statusInfoMessage)
                {
                    Debug.WriteLine(statusInfoMessage.IsMotorActive);
                    return;
                }
            }
            finally
            {
                this._receivedMessageCount++;
                this._semaphoreSlim.Release();
            }
        }
    }
}
