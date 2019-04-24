namespace DFC.Digital.Tools.Data.Models
{
    public class CitizenEmailNotification : CitizenNotification
    {
        public string EmailAddress { get; set; }

        public GovUkNotifyPersonalisation EmailPersonalisation { get; set; }

        public bool NotProcessed { get; set; }
    }
}
