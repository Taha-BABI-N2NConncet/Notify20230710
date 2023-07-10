using System.ComponentModel;

namespace Notify.Classes
{
    public class Step
    {
        [DefaultValue("")]
        public string id { get; set; }
        public bool active { get; set; }
        public bool shouldStopOnFail { get; set; }
        public string _templateId { get; set; }
        public string _parentId { get; set; }
        public Template template { get; set; }

        public Step()
        {

        }
        public Step(Models.Template template)
        {
            active = true;
            shouldStopOnFail = false;
            _templateId = template.Id;
            _parentId = template.ParentID;
            this.template = new Template(template);
        }
    }
}
