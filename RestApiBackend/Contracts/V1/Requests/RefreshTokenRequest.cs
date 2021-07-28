using System.ComponentModel.DataAnnotations;

namespace RestApiBackend.Contracts.V1.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }
        
        [Required]
        public string RefreshToken { get; set; }
    }
}