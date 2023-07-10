namespace Notify.Classes
{
    public class Variable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Requiered { get; set; }
        public string _id { get; set; }

        public Variable()
        {

        }
        public Variable(Models.Variable variable)
        {
            Name = variable.Name;
            Type = variable.Type;
            Requiered= variable.Requiered;
            _id = variable.Id;
        }
    }
}
