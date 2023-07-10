using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Notify.Models
{
    public class DeviceToken
    {
        public string SbuscriberID { get; set; }
        [Key]
        public string TokenString { get; set; }
        [JsonIgnore]
        public virtual Subscriber Subscriber { get; set; }

       
    }
}
