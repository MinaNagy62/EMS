using EMS_Application.DTO.Employee;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using MediatR;

namespace EMS_Application.Features.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmployeeByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeResponse> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.FindAsync(
            e => e.Id == request.Id && e.IsActive,
            e => e.Department);

        if (employee is null)
            throw new NotFoundException("Employee", request.Id);

        return employee.ToResponse();
    }
}
