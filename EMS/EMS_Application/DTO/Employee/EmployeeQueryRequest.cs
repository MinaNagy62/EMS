using EMS_Application.Common;
using EMS_Domain.Enum;

namespace EMS_Application.DTO.Employee;

public class EmployeeQueryRequest : PagedRequest
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public Gender? Gender { get; set; }
}
