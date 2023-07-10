using Notify.Models;

namespace Notify.Interfaces
{
    public interface INOVUContext
    {
        #region Flow Region
        Task<Tuple<Classes.Notification, string>> CreateFlow(Flow Flow, string SponsorID);
        Task<Tuple<Classes.Notification, string>> UpdateFlow(Flow Flow, string SponsorID);
        Task<Tuple<List<Flow>, string>> GetFlowsByPage(int Page, string SponsorID);
        Task<Tuple<Flow, string>> GetFlowByID(string ID, string SponsorID);
        #endregion

        #region Subscriber Region
        Task<Tuple<List<Subscriber>, string>> GetSubscribersByPage(int Page, string SponsorID);
        Task<Tuple<Models.Subscriber, string>> GetSubscriberByID(string ID, string SponsorID);
        Task<Tuple<Classes.Subscriber, string>> CreateSubscriber(Models.Subscriber Subscriber, string SponsorID);
        Task<Tuple<Classes.Subscriber, string>> UpdateSubscriber(Models.Subscriber Subscriber, string SponsorID);
        Task<Tuple<bool,string>> DeleteSubscriber(string SubscriberID, string SponsorID);
        Task<Tuple<Classes.Subscriber, string>> SetSubscriberDeviceToken(string SubscriberID,Classes.Channel channel, string SponsorID);
        #endregion

        #region Template Region
        Task<Tuple<Models.Template, string>> GetTemplateByID(string flowID, string templateID, string SponsorID);
        //Task<Tuple<Classes.Notification, string>> CreateTemplate(string flowID, Models.Template template);
        Task<Tuple<Classes.Notification, string>> UpdateTemplate(string flowID, Models.Template template, string SponsorID);
        #endregion

        #region Notification
        Task<string> Trigger(string FlowTriggerID, string SubscriberId, string SponsorID, Dictionary<string, string> Payload);
        Task<string> Trigger(string FlowTriggerID, string SubscriberId, string FirstName, string LastName, string PhoneNumber, string Email, string SponsorID, Dictionary<string, string> Payload);
        #endregion
    }
}
