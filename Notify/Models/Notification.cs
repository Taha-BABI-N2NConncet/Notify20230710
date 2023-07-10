using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Notify.Models
{
    //you need to understand that the notification is the tiggered flow for spicefic subscriber!!
    public class Notification
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string TransactionID { get; set; }
        [Required]
        public string SubscriberID { get; set; }
        [Required]
        public string FlowID { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]
        public virtual Flow Flow { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public virtual List<NotificationStatus> NotificationStatuses { get; set; }
        public virtual List<VariableValue> VariableValues { get; set; }
    }
}
