using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestApiBackend.Contracts.V1.Requests
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [MinLength(1)]
        public List<long> RolesIds { get; set; }
    }
}