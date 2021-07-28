using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RestApiBackend.Contracts.V1.Responses;

namespace RestApiBackend.Contracts.V1.Requests
{
    public class UpdateUserRequest
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }
        
        [Required]
        [MinLength(1)]
        public List<long> RolesIds { get; set; }
    }
}