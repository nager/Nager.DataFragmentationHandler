namespace Nager.MessageHandler.UnitTest.Messages
{
    public class HelloMessage : MessageBase
    {
        public string Message { get; private set; }

        public HelloMessage(string message) : base(nameof(HelloMessage))
        {
            this.Message = message;
        }
    }
}
