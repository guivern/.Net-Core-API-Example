using System.ComponentModel.DataAnnotations;

namespace RestApiBackend.Contracts.V1.Requests
{
    public class UpdateAccountInfoRequest
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }
    }
}