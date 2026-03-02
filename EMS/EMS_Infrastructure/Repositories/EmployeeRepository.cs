using EMS_Application.Interfaces.Employees;
using EMS_Domain.Entities;
using EMS_Infrastructure.Data;

namespace EMS_Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context)
    { }
}
