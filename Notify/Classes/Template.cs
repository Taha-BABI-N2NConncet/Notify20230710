using Newtonsoft.Json;
using System.ComponentModel;

namespace Notify.Classes
{
    public class Template
    {
        [DefaultValue("")]
        public string id { get; set; }
        public string type { get; set; }
        [DefaultValue("")]
        public string subject { get; set; }
        [DefaultValue("")]
        public string senderName { get; set; }
        [DefaultValue("")]
        public string preheader { get; set; }
        [DefaultValue("")]
        public string title { get; set; }
        public bool active { get; set; }
        [JsonConverter(typeof(TemplateContentJsonConverter))]
        public string content { get; set; }
        public string contentType { get; set; }
        [DefaultValue("")]
        public string _environmentId { get; set; }
        [DefaultValue("")]
        public string _organizationId { get; set; }
        [DefaultValue("")]
        public string _creatorId { get; set; }
        [DefaultValue("")]
        public string _feedId { get; set; }
        [DefaultValue("")]
        public string _layoutId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string __v { get { return "0"; } }
        public List<Variable> variables { get; set; }

        public Template()
        {

        }
        public Template(Models.Template template)
        {
            id = template.Id;
            type = template.Type;
            subject = template.Subject;
            senderName = template.SenderName;
            preheader = template.Preheader;
            active = true;
            content = template.Content;
            contentType = type.ToLower() == "sms" || type.ToLower() == "push" ? "editor" : "customHtml";
            title = template.Title;
            _environmentId =_organizationId = _creatorId = _feedId = _layoutId = "";
            createdAt = updatedAt = DateTime.Now;
            if (template.Variables != null)
              variables = template.Variables.Select(v=> new Variable(v)).ToList();
        }
    }
}
