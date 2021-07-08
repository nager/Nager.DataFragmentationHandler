namespace Nager.MessageHandler.UnitTest.Messages
{
    public class StatusInfoMessage : MessageBase
    {
        public bool IsMotorActive { get; private set; }

        public StatusInfoMessage(bool isMotorActive) : base(nameof(StatusInfoMessage))
        {
            this.IsMotorActive = isMotorActive;
        }
    }
}
