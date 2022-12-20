namespace Dolite.Domain.Entities;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
    public Guid? DeleterId { get; set; }
    public DateTime? DeletionTime { get; set; }

    void Delete();

    void DeleteBy(Guid id);
}