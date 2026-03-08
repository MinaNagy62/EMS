using EMS_Application.DTO.Department;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using MediatR;

namespace EMS_Application.Features.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDepartmentByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentResponse> Handle(
        GetDepartmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.Departments.FindAsync(
            d => d.Id == request.Id && d.IsActive);

        if (department is null)
            throw new NotFoundException("Department", request.Id);

        return department.ToResponse();
    }
}
