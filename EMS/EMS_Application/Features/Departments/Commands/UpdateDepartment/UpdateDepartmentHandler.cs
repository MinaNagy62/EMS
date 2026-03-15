using EMS_Application.DTO.Department;
using EMS_Application.Exceptions;
using EMS_Application.Features.Departments.Queries.GetAllDepartments;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using MediatR;

namespace EMS_Application.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentResponse> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Departments.FindAsync(d => d.Id == request.Id && d.IsActive);

        if (existing is null)
            throw new NotFoundException("Department", request.Id);

        existing.Name = request.Name;
        existing.Code = request.Code;
        existing.Description = request.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Departments.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        GetAllDepartmentsHandler.InvalidateCache();

        return existing.ToResponse();
    }
}
