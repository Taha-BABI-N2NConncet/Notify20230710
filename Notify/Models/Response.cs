namespace Notify.Models
{
    public class Response<T> where T : class
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public static Response<T> SetSuccess(T response)
        {
            Response<T> Resp = new Response<T>();
            Resp.Status = true;
            Resp.Message = (string)null;
            Resp.Data = response;

            return Resp;
        }

        public static Response<T> SetResponse(T response, bool status, string errormsg) 
        {
            Response<T> Resp = new Response<T>();
            Resp.Status = status;
            Resp.Message = errormsg;
            Resp.Data = response;

            return Resp;
        }
    }
}
