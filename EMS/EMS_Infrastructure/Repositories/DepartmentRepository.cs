using EMS_Application.Interfaces.Departments;
using EMS_Domain.Entities;
using EMS_Infrastructure.Data;

namespace EMS_Infrastructure.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context)
    {
    }
}
