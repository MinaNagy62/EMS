namespace EMS_Application.DTO.Department;

public class CreateDepartmentRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
}
