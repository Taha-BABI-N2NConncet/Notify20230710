using Notify.Models;

namespace Notify.Classes
{
    public class Credentials
    {
        public List<string> deviceTokens { get; set; }

        public Credentials()
        {

        }
        public Credentials(List<DeviceToken> deviceTokens)
        {
            this.deviceTokens = deviceTokens.Select(dt => dt.TokenString).ToList();
        }
    }
}
