using Notify.Classes;
using System.Text.Json.Serialization;

namespace Notify.Models
{
    
    public class Template
    {
        public string Id { get; set; }
        public string ParentID { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string SenderName { get; set; }
        public string Preheader { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual List<Variable> Variables { get; set; }
        [JsonIgnore]
        public virtual Flow Flow { get; set; }
        [JsonIgnore]
        public virtual List<NotificationStatus> NotificationStatuses { get; set; }

        public Template()
        {

        }
        public Template(Step step)
        {
            Id = step.id;
            ParentID = step._parentId;
            Type = step.template == null ? null : step.template.type;
            Subject = step.template == null ? null : step.template.subject;
            SenderName = step.template == null ? null : step.template.senderName;
            Preheader = step.template == null ? null : step.template.preheader;
            Title = step.template == null ? null : step.template.title;
            Content = step.template == null ? null : step.template.content;
            CreatedAt = step.template == null ? DateTime.Now : step.template.createdAt;
            UpdatedAt = step.template == null ? DateTime.Now : step.template.updatedAt;
            Variables = step.template == null ? null : step.template.variables.Select(variable => new Variable(variable, step.template.id)).ToList();
        }
    }
}
