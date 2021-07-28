using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RestApiBackend.Entities
{
    public class User: IdentityUser<long>
    {
        public string PhotoUrl { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
        
        public ICollection<UserRole> UserRoles { get; set; }
    }
}