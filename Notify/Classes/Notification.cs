using System.ComponentModel;

namespace Notify.Classes
{
    public class Notification
    {
        [DefaultValue("")]
        public string id { get; set; }
        public string name { get; set; }
        public string _notificationGroupId { get; set; }
        public string notificationGroupId { get; set; }
        public string description { get; set; }
        public bool active { get; set; } = true;
        public bool draft { get; set; } = false;
        public bool critical { get; set; } = false;
        public virtual PreferenceSettings preferenceSettings { get; set; }
        public virtual List<Step> steps { get; set; }

        public Notification()
        {

        }
        public Notification(Models.Flow flow)
        {
            id = flow.Id;
            name = flow.Name;
            _notificationGroupId = flow.NotificationGroupId;
            notificationGroupId = flow.NotificationGroupId;
            description = flow.Description;
            active = true;
            draft = false;
            critical = false;
            preferenceSettings = new PreferenceSettings() { chat = true,email = true,in_app= true,push= true,sms = true };
            steps = flow.Templates.Select(t => new Step(t)).ToList();
        }

    }
}
