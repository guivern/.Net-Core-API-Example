using System.ComponentModel.DataAnnotations;

namespace RestApiBackend.Contracts.V1.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}