using EMS_Domain.Enum;

namespace EMS_Application.DTO.Employee;

public class UpdateEmployeeRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public Gender Gender { get; set; }
    public string JobTitle { get; set; }
    public int DepartmentId { get; set; }
}
