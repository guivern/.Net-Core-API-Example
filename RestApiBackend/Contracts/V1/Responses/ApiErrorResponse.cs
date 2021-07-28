using System.Collections.Generic;

namespace RestApiBackend.Contracts.V1.Responses
{
    public class ApiErrorResponse
    {
        public List<string> Errors { get; set; }
        
        public ApiErrorResponse()
        {
            Errors = new List<string>();
        }

        public ApiErrorResponse(List<string> errors)
        {
            Errors = errors;
        }
    }
}