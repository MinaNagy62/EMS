using EMS_Application.Common;

namespace EMS_Application.DTO.Department;

public class DepartmentQueryRequest : PagedRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
