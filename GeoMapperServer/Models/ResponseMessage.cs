using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GeoMapper.Models
{
    public enum Status
    {
        Success,
        Fail
    }
    public class ResponseMessage
    {
        public Status Status { get; set; }
        public object Data { get; set; }
        public ModelStateDictionary ModelState { get; set; }

        public ResponseMessage(Status status, object data = null, ModelStateDictionary modelState = null)
        {
            Data = data;
            Status = status;
            ModelState = modelState;
        }
    }
}
