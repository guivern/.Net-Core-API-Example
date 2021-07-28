using System.ComponentModel.DataAnnotations;

namespace RestApiBackend.Contracts.V1.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}