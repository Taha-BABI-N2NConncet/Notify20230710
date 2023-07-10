using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Notify.Models
{
    public class Flow
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string NotificationGroupId { get; set; }
        [Required]
        public string Description { get; set; }
        public virtual List<Template> Templates { get; set; }
        [JsonIgnore]
        public virtual List<Notification> Notifications { get; set; }


        public Flow()
        {

        }
        public Flow(Classes.Notification item)
        {
            Id = item.id; 
            Name = item.name; 
            NotificationGroupId = item._notificationGroupId; 
            Description = item.description;
            Templates = item.steps.Select(step=> new Template(step)).ToList();
        }
    }
}
