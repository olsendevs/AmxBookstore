

namespace AmxBookstore.Domain.Entities
{
    public abstract class Entity : ExtendedBaseModel
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt{ get; protected set; }
        public bool Deleted { get; protected set; }

        public Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Deleted = false;
        }

        public void MarkAsDeleted()
        {
            Deleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
