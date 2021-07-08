namespace Nager.MessageHandler
{
    public interface IMessageParser
    {
        MessageBase Parse(byte[] data);
    }
}
