using EMS_Application.Common;
using EMS_Application.DTO.Department;
using EMS_Application.Features.Departments.Commands.CreateDepartment;
using EMS_Application.Features.Departments.Commands.DeleteDepartment;
using EMS_Application.Features.Departments.Commands.UpdateDepartment;
using EMS_Application.Features.Departments.Queries.GetAllDepartments;
using EMS_Application.Features.Departments.Queries.GetDepartmentById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllDepartmentsQuery query)
    {
        var departments = await _mediator.Send(query);
        return Ok(ApiResponse<PagedResponse<DepartmentResponse>>.SuccessResponse(departments));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _mediator.Send(new GetDepartmentByIdQuery { Id = id });
        return Ok(ApiResponse<DepartmentResponse>.SuccessResponse(department));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentCommand command)
    {
        var created = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<DepartmentResponse>.SuccessResponse(created, "Department created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentCommand command)
    {
        command.Id = id;
        var updated = await _mediator.Send(command);
        return Ok(ApiResponse<DepartmentResponse>.SuccessResponse(updated, "Department updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteDepartmentCommand { Id = id });
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Department deleted successfully."));
    }
}
