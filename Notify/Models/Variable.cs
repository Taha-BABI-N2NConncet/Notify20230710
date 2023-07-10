using System.Text.Json.Serialization;

namespace Notify.Models
{
    public class Variable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Requiered { get; set; }
        public string TemplateId { get; set; }
        [JsonIgnore]
        public virtual Template Template { get; set; }
        [JsonIgnore]
        public virtual List<VariableValue> VariableValues { get; set; }
        public Variable()
        {
            
        }
        public Variable(Classes.Variable variable, string templateId)
        {
            Id = variable._id;
            Name = variable.Name; 
            Type = variable.Type;
            Requiered = variable.Requiered;
            TemplateId = templateId;
        }

    }
}
