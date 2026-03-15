using EMS_Application.Common;
using EMS_Application.DTO.Employee;
using EMS_Application.Features.Employees.Commands.CreateEmployee;
using EMS_Application.Features.Employees.Commands.DeleteEmployee;
using EMS_Application.Features.Employees.Commands.UpdateEmployee;
using EMS_Application.Features.Employees.Queries.GetAllEmployees;
using EMS_Application.Features.Employees.Queries.GetEmployeeById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllEmployeesQuery query)
    {
        var employees = await _mediator.Send(query);
        return Ok(ApiResponse<PagedResponse<EmployeeResponse>>.SuccessResponse(employees));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _mediator.Send(new GetEmployeeByIdQuery { Id = id });
        return Ok(ApiResponse<EmployeeResponse>.SuccessResponse(employee));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command)
    {
        var created = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<EmployeeResponse>.SuccessResponse(created, "Employee created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeCommand command)
    {
        command.Id = id;
        var updated = await _mediator.Send(command);
        return Ok(ApiResponse<EmployeeResponse>.SuccessResponse(updated, "Employee updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteEmployeeCommand { Id = id });
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Employee deleted successfully."));
    }
}
