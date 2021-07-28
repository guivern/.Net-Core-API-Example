using System.Collections.Generic;

namespace RestApiBackend.Services.Models
{
    public class AuthServiceResult : ServiceResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public AuthServiceResult()
        {
            Errors = new List<string>();
        }

        public AuthServiceResult(bool succeeded, List<string> errors)
        {
            this.Succeeded = succeeded;
            this.Errors = errors;
        }
    }
}