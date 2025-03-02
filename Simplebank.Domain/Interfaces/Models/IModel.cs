namespace Simplebank.Domain.Interfaces.Models;

public interface IModel
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}