using System.Linq.Expressions;
using EMS_Application.Common;
using EMS_Application.DTO.Employee;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.Employees;
using EMS_Application.Mapping;
using EMS_Domain.Entities;
using FluentValidation;

namespace EMS_Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateEmployeeRequest> _createValidator;
    private readonly IValidator<UpdateEmployeeRequest> _updateValidator;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        IValidator<CreateEmployeeRequest> createValidator,
        IValidator<UpdateEmployeeRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResponse<EmployeeResponse>> GetAllEmployeesAsync(EmployeeQueryRequest request)
    {
        var filters = new List<Expression<Func<Employee, bool>>>
        {
            e => e.IsActive
        };

        // Search filter — searches FirstName, LastName, and Email
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            filters.Add(e => e.FirstName.ToLower().Contains(search)
                           || e.LastName.ToLower().Contains(search)
                           || e.Email.ToLower().Contains(search));
        }

        // Department filter
        if (request.DepartmentId.HasValue)
            filters.Add(e => e.DepartmentId == request.DepartmentId.Value);

        // Gender filter
        if (request.Gender.HasValue)
            filters.Add(e => e.Gender == request.Gender.Value);

        var pagedEmployees = await _unitOfWork.Employees.GetPagedAsync(
            request, filters, e => e.Department);

        return new PagedResponse<EmployeeResponse>
        {
            Items = pagedEmployees.Items.Select(e => e.ToResponse()).ToList(),
            PageNumber = pagedEmployees.PageNumber,
            PageSize = pagedEmployees.PageSize,
            TotalCount = pagedEmployees.TotalCount
        };
    }

    public async Task<EmployeeResponse> GetEmployeeByIdAsync(int id)
    {
        var employee = await _unitOfWork.Employees.FindAsync(
            e => e.Id == id && e.IsActive,
            e => e.Department);

        if (employee is null)
            throw new NotFoundException("Employee", id);

        return employee.ToResponse();
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.ToErrorDictionary());

        var employee = request.ToEntity();
        employee.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        return employee.ToResponse();
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.ToErrorDictionary());

        var existing = await _unitOfWork.Employees.FindAsync(e => e.Id == id && e.IsActive);

        if (existing is null)
            throw new NotFoundException("Employee", id);

        existing.ApplyUpdate(request);

        _unitOfWork.Employees.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing.ToResponse();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var existing = await _unitOfWork.Employees.FindAsync(e => e.Id == id && e.IsActive);

        if (existing is null)
            throw new NotFoundException("Employee", id);

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Employees.Update(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
