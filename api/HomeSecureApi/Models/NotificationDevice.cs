namespace HomeSecureApi.Models
{
    public class NotificationDevice
    {
        public string Id{get;set;}

        public NotificationType Type{get;set;}

        public bool AreSame(NotificationDevice device){
            if(device==null){
                return false;
            }
            return Id==device.Id && Type==device.Type;
        }
    }
}