using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    // implementace rozhraní IEmployeeRepository
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SQLEmployeeRepository> logger;

        public SQLEmployeeRepository(AppDbContext context,
                                     ILogger<SQLEmployeeRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public Employee Add(Employee employee)
        {
            context.Employees.Add(employee);
            context.SaveChanges();
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee employee = context.Employees.Find(id);
            if (employee != null)
            {
                context.Employees.Remove(employee);
                context.SaveChanges();
            }
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return context.Employees.Include(ad => ad.InstuctorAdditionalGames)
                                    .Include(ag => ag.BarmaidAdditionalGames);
        }

        public Employee GetEmployee(int id)
        {
            return context.Employees.Include(ag => ag.InstuctorAdditionalGames)
                                    .Include(ag => ag.BarmaidAdditionalGames)
                                    .AsEnumerable().FirstOrDefault(v => v.Id == id);
        }

        //public IEnumerable<Employee> GetEmployeesForAdditionalGameSummary(DateTime month)
        //{
        //    return context.Employees.Include(ig => ig.InstuctorAdditionalGames)
        //                                .Where(ig => ig.InstuctorAdditionalGames.Any(i => i.Date.Month == month.Month))
        //                            .Include(bg => bg.BarmaidAdditionalGames)
        //                                .Where(bg => bg.BarmaidAdditionalGames.Any(b => b.Date.Month == month.Month));

        //}

        public Employee Update(Employee employeeChanges)
        {
            var employee = context.Employees.Attach(employeeChanges);
            employee.State = EntityState.Modified;
            context.SaveChanges();
            return employeeChanges;
        }
    }
}
