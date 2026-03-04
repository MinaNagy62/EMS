using EMS_Application.Interfaces;
using EMS_Application.Interfaces.AppUsers;
using EMS_Application.Interfaces.Departments;
using EMS_Application.Interfaces.Employees;
using EMS_Infrastructure.Data;
using EMS_Infrastructure.Repositories;

namespace EMS_Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDepartmentRepository? _departments;
    private IEmployeeRepository? _employees;
    private IAppUserRepository? _appUsers;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IDepartmentRepository Departments =>
        _departments ??= new DepartmentRepository(_context);

    public IEmployeeRepository Employees =>
        _employees ??= new EmployeeRepository(_context);

    public IAppUserRepository AppUsers => 
        _appUsers ??= new AppUserRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
