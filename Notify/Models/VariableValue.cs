using System.Text.Json.Serialization;

namespace Notify.Models
{
    public class VariableValue
    {
        public string ID { get; set; }
        public string Value { get; set; }
        public string NotificationID { get; set; }
        public string VariableID { get; set; }
        [JsonIgnore]
        public virtual Notification Notification { get; set; }
        [JsonIgnore]
        public virtual Variable Variable { get; set; }
    }
}
