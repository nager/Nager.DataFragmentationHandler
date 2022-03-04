using Nager.MessageHandler.UnitTest.Messages;
using System;
using System.Linq;
using System.Text;

namespace Nager.MessageHandler.UnitTest.MessageParsers
{
    public class HelloMessageParser : IMessageParser
    {
        private readonly byte _messageTypeToken = 0x10;

        public MessageBase Parse(Span<byte> data)
        {
            if (data[1] != this._messageTypeToken)
            {
                return null;
            }

            return new HelloMessage(Encoding.ASCII.GetString(data.Slice(1)));
        }
    }
}
