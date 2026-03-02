namespace EMS_Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
