using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Notify.Models
{
    public class NotificationStatus
    {
        [Key]
        public string Id { get; set; }
        [Required] 
        public string NotificaitionID { get; set; }
        [Required] 
        public string TemplateID { get; set; }
        [Required] 
        public DateTime StatusDateTime { get; set; }
        [Required] 
        public string StatusID { get; set; }
        public virtual Status Status { get; set; }
        [JsonIgnore]
        public virtual Notification Notification { get; set; }
        [JsonIgnore]
        public virtual Template Template { get; set; }
    }
}
