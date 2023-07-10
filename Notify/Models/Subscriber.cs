using System.Text.Json.Serialization;

namespace Notify.Models
{
    public class Subscriber
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public long MessagesBalance { get; set; }
        public virtual List<DeviceToken> DeviceTokens { get; set;}

        public Subscriber()
        {

        }
        public Subscriber(Classes.Subscriber subscriber)
        {
            Id = subscriber.subscriberId;
            FirstName = subscriber.firstName;
            LastName = subscriber.lastName;
            PhoneNumber = subscriber.phone;
            Email = subscriber.email;
            DeviceTokens = subscriber.Channels.SelectMany(channel => channel.credentials.deviceTokens.ToList()).Select(dt=> new DeviceToken() {TokenString = dt,SbuscriberID = Id }).ToList();
        }
    }
}
