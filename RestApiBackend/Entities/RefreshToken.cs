using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestApiBackend.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User {get;set;}
    }
}