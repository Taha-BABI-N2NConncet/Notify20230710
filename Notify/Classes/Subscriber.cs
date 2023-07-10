namespace Notify.Classes
{
    public class Subscriber
    {
        public string subscriberId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public DateTime lastOnlineAt { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public bool isOnline { get; set; }
        public List<Channel> Channels { get; set; }

        public Subscriber()
        {

        }
        public Subscriber(Models.Subscriber subscriber)
        {
            subscriberId = subscriber.Id;
            firstName = subscriber.FirstName;
            lastName = subscriber.LastName;
            phone = subscriber.PhoneNumber;
            email = subscriber.Email;
            // please note that i assumes that the subscriber have only one channel credential
            // the other channeles credentials are saved in the subscriber object
            Channels = new List<Channel>();
            if (subscriber.DeviceTokens.Count != 0)
            {
                Channels.Add(new Channel(subscriber.DeviceTokens));
            }
        }
    }
}
