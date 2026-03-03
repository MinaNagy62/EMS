using EMS_Application.Common;
using EMS_Application.DTO.Department;
using EMS_Application.Interfaces.Departments;
using Microsoft.AspNetCore.Mvc;

namespace EMS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentResponse>>.SuccessResponse(departments));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        return Ok(ApiResponse<DepartmentResponse>.SuccessResponse(department));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
    {
        var created = await _departmentService.CreateDepartmentAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<DepartmentResponse>.SuccessResponse(created, "Department created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentRequest request)
    {
        var updated = await _departmentService.UpdateDepartmentAsync(id, request);
        return Ok(ApiResponse<DepartmentResponse>.SuccessResponse(updated, "Department updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteDepartmentAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Department deleted successfully."));
    }
}
