using Microsoft.Extensions.Configuration;
using Notify.Models;
using RestSharp;
using System.ComponentModel;

namespace Notify.Classes
{
    public class Macrokiosk
    {
        private string user;
        private string password;
        private string uRL;
        private RestClient _client;
        private RestRequest _restRequest;
        private IConfiguration _configuration;
        public Macrokiosk(IConfiguration configuration)
        { 
            _configuration = configuration;
            //user = Environment.GetEnvironmentVariable("MacrokioskUserID");
            //password = Environment.GetEnvironmentVariable("MacrokioskPassword");
            //uRL = Environment.GetEnvironmentVariable("MacrokioskURL");
            user = _configuration.GetValue<string>("Macrokiosk:UserID");
            password = _configuration.GetValue<string>("Macrokiosk:Password");
            uRL = _configuration.GetValue<string>("Macrokiosk:URL");
            _client = new RestClient(uRL);
            _restRequest = new RestRequest();
        }
        /// <summary>
        ///  use the Macrokiosk API to send the requiered message 
        /// </summary>
        /// <param name="Message">the message that you want to send</param>
        /// <param name="PhoneNumber">the receiver phone number</param>
        /// <returns>tuple<bool (true if the request successed false if not), string(the error message if the request failed and "Success" if not)></bool></returns>
        public async Task<Response<string>> SendMessage(string Message, string PhoneNumber)
        {
            _restRequest.RequestFormat = DataFormat.Json;
            var bodyObject = new
            {
                user = user,
                pass = password,
                to = PhoneNumber,
                text = Message,
                tranid = DateTime.Now.ToString("yyyyMMddHHmmssff") + (new Random()).Next(10, 100)
            };
            _restRequest.AddJsonBody(
                bodyObject
            );
            RestResponse response = await _client.ExecutePostAsync(_restRequest);

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return Response<string>.SetResponse(bodyObject.tranid, false,  message);
            }
            if (Convert.ToInt32(response.Content) == 200)
                return Response<string>.SetResponse(bodyObject.tranid , true, $"Transction ID {bodyObject.tranid} Success");
            return Response<string>.SetResponse(bodyObject.tranid , false, $"Transction ID {bodyObject.tranid} Status Code {response.Content}");
        }

        public enum MacrokioskErrors
        {
            [Description("Red color")]
            E200 = 200,
            [Description("Missing parameter or invalid field type")]
            E400 = 400,
            [Description("Invalid user, password")]
            E401 = 401,
            [Description("Insufficient SMS credit")]
            E402 = 402,
            [Description("Invalid Client IP address")]
            E403 = 403,
            [Description("Invalid SenderID length *MK Internal Use")]
            E404 = 404,
            [Description("Invalid msg type")]
            E405 = 405,
            [Description("Invalid MSISDN length")]
            E406 = 406,
            [Description("Invalid MSISDN")]
            E456 = 456,
            [Description("Message over length")]
            E462 = 462,
            [Description("Internal server error")]
            E500 = 500,
        }
    }
}
