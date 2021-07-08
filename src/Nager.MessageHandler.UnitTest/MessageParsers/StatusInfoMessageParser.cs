using Nager.MessageHandler.UnitTest.Messages;

namespace Nager.MessageHandler.UnitTest.MessageParsers
{
    public class StatusInfoMessageParser : IMessageParser
    {
        private readonly byte _messageTypeToken = 0x11;

        public MessageBase Parse(byte[] data)
        {
            if (data[1] != this._messageTypeToken)
            {
                return null;
            }

            return new StatusInfoMessage(false);
        }
    }
}
