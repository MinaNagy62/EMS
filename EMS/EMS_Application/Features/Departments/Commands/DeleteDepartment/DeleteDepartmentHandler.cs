using EMS_Application.Exceptions;
using EMS_Application.Features.Departments.Queries.GetAllDepartments;
using EMS_Application.Interfaces;
using MediatR;

namespace EMS_Application.Features.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDepartmentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Departments.FindAsync(d => d.Id == request.Id && d.IsActive);

        if (existing is null)
            throw new NotFoundException("Department", request.Id);

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Departments.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        GetAllDepartmentsHandler.InvalidateCache();
    }
}
