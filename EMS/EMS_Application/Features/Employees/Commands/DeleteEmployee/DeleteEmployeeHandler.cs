using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using MediatR;

namespace EMS_Application.Features.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeHandler : IRequestHandler<DeleteEmployeeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmployeeHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Employees.FindAsync(e => e.Id == request.Id && e.IsActive);

        if (existing is null)
            throw new NotFoundException("Employee", request.Id);

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Employees.Update(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
