using System;

namespace RestApiBackend.Entities
{
    public class EntityBase
    {
        public long Id { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}