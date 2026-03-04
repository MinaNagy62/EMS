using EMS_Application.Interfaces.AppUsers;
using EMS_Domain.Entities;
using EMS_Infrastructure.Data;

namespace EMS_Infrastructure.Repositories;

public class AppUserRepository: GenericRepository<AppUser> , IAppUserRepository
{
    public AppUserRepository(AppDbContext context): base(context)
    {
    }
}
