using EMS_Application.Interfaces.Departments;
using EMS_Application.Interfaces.Employees;
using EMS_Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EMS_Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        return services;
    }
}
