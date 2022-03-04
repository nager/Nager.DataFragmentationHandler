using System;

namespace Nager.MessageHandler
{
    public interface IMessageParser
    {
        MessageBase Parse(Span<byte> data);
    }
}
