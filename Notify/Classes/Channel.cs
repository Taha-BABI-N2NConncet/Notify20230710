using Notify.Models;

namespace Notify.Classes
{
    public class Channel
    {
        public string _integrationId { get; set; }
        public string providerId { get; set; }
        public Credentials credentials { get; set; }
        public Channel()
        {

        }
        public Channel(List<DeviceToken> deviceTokens)
        {
            credentials = new Credentials(deviceTokens);
        }
    }
}
