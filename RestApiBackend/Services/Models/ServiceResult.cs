using System.Collections.Generic;

namespace RestApiBackend.Services.Models
{
    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }

        public ServiceResult()
        {
            Succeeded = false;
            Errors = new List<string>();
        }
    }
}