using EMS_Application.DTO.Department;
using EMS_Application.Features.Departments.Queries.GetAllDepartments;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using EMS_Domain.Entities;
using MediatR;

namespace EMS_Application.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, DepartmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentResponse> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = new Department
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        GetAllDepartmentsHandler.InvalidateCache();

        return department.ToResponse();
    }
}
