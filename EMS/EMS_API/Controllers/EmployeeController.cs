using EMS_Application.Common;
using EMS_Application.DTO.Employee;
using EMS_Application.Interfaces.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(ApiResponse<IEnumerable<EmployeeResponse>>.SuccessResponse(employees));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        return Ok(ApiResponse<EmployeeResponse>.SuccessResponse(employee));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var created = await _employeeService.CreateEmployeeAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<EmployeeResponse>.SuccessResponse(created, "Employee created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var updated = await _employeeService.UpdateEmployeeAsync(id, request);
        return Ok(ApiResponse<EmployeeResponse>.SuccessResponse(updated, "Employee updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeService.DeleteEmployeeAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Employee deleted successfully."));
    }
}
