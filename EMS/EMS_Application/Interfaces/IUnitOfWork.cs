using EMS_Application.Interfaces.Departments;
using EMS_Application.Interfaces.Employees;

namespace EMS_Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDepartmentRepository Departments { get; }
    IEmployeeRepository Employees { get; }
    Task<int> SaveChangesAsync();
}
