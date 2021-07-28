using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RestApiBackend.Entities
{
    public class Role: IdentityRole<long>
    {
        public enum RolesEnum
        {
            Administrador = 1,
            Vendedor = 2,
            Cobrador = 3
        }
        
        public ICollection<UserRole> UserRoles { get; set; }
    }
}