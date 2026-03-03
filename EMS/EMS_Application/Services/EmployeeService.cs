using EMS_Application.DTO.Employee;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.Employees;
using EMS_Application.Mapping;
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

    public async Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(
            e => e.IsActive,
            e => e.Department);

        return employees.ToResponse();
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
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new Exceptions.ValidationException(errors);
        }

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
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new Exceptions.ValidationException(errors);
        }

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
