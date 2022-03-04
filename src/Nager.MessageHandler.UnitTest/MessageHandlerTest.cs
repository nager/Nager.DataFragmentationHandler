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
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);

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
            messageHandler.NewMessage -= NewMessage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
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
            messageHandler.NewMessage -= NewMessage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
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
            messageHandler.NewMessage -= NewMessage;

            var isTimeout = !this._semaphoreSlim.Wait(1000);

            Assert.IsFalse(isTimeout, "Run into timeout");
            Assert.AreEqual(1, this._receivedMessageCount);
        }

        [TestMethod]
        [ExpectedException(typeof(BufferSizeTooSmallException))]
        public void MessageHandler_BufferSizeTooSmall()
        {
            var loggerMock = LoggerHelper.GetLogger<MessageHandler>();
            var messageAnalyzer = new StartEndTokenMessageAnalyzer(0x01, 0x02);
            var messageParsers = new IMessageParser[0];

            var messageHandler = new MessageHandler(loggerMock.Object, messageAnalyzer, messageParsers, bufferSize: 5);
            messageHandler.AddData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
        }

        [TestMethod]
        public void MessageHandler_BufferCleanupViaMove()
        {
            var loggerMock = LoggerHelper.GetLogger<MessageHandler>();
            var messageAnalyzer = new StartEndTokenMessageAnalyzer(0x01, 0xFF);
            var messageParsers = new IMessageParser[]
            {
                new HelloMessageParser(),
                new StatusInfoMessageParser()
            };

            var messageHandler = new MessageHandler(loggerMock.Object, messageAnalyzer, messageParsers, bufferSize: 9);
            messageHandler.NewMessage += NewMessage;
            messageHandler.AddData(new byte[] { 0x01, 0x11, 0xFF, 0x01, 0x11 });
            messageHandler.AddData(new byte[] { 0xFF, 0x02, 0x03, 0x04, 0x05 });
            messageHandler.NewMessage -= NewMessage;

            //TODO: Add Assert logic
        }

        [TestMethod]
        public void MessageHandler_BufferCleanupViaRemove()
        {
            var loggerMock = LoggerHelper.GetLogger<MessageHandler>();
            var messageAnalyzer = new StartEndTokenMessageAnalyzer(0xF1, 0xFF);
            var messageParsers = new IMessageParser[]
            {
                new HelloMessageParser(),
                new StatusInfoMessageParser()
            };

            var messageHandler = new MessageHandler(loggerMock.Object, messageAnalyzer, messageParsers, bufferSize: 5);
            messageHandler.NewMessage += NewMessage;
            messageHandler.AddData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 });
            messageHandler.AddData(new byte[] { 0xF1, 0x11, 0xFF });
            messageHandler.NewMessage -= NewMessage;

            //TODO: Add Assert logic
        }

        private void NewMessage(MessageBase message)
        {
            try
            {
                if (message is HelloMessage helloMessage)
                {
                    Trace.WriteLine(helloMessage.Message);
                    return;
                }

                if (message is StatusInfoMessage statusInfoMessage)
                {
                    Trace.WriteLine(statusInfoMessage.IsMotorActive);
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
