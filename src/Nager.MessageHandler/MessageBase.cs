namespace Nager.MessageHandler
{
    public abstract class MessageBase
    {
        public string MessageType { get; protected set; }

        public MessageBase(string messageType)
        {
            this.MessageType = messageType;
        }
    }
}
