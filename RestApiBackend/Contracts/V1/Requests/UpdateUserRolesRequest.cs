using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestApiBackend.Contracts.V1.Requests
{
    public class UpdateUserRolesRequest
    {
        [Required]
        [MinLength(1)]
        public List<long> RolesIds { get; set; }
    }
}