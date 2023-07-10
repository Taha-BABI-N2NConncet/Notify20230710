using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Notify.Models
{
    public class Status
    {
        [Key]
        public string Id { get; set; }
        public string StatusName { get; set; }
        [JsonIgnore]
        public virtual List<NotificationStatus> NotificationStatuses { get; set; }
    }
}
