using Encryption;

namespace Notify.Repositories.NOVUSettingsClasses
{
    public class Sponsor
    {
        public string SponserID { get; set; }
        public string APIKeyEncryption { get; set; }
        public List<SponserWorkflow> Workflows { get; set; }

    }
}
