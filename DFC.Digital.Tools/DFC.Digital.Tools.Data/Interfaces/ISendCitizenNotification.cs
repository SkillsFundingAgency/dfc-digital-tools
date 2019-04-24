namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ISendCitizenNotification<in T>
        where T : CitizenNotification
    {
        bool SendCitizenNotification(T notification);
    }
}
