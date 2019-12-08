using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    // rozhraní pro práci s entitou "Zaměstnaci"
    public interface IEmployeeRepository
    {
        Employee GetEmployee(int id);
        IEnumerable<Employee> GetAllEmployees();
        Employee Add(Employee employee);
        Employee Update(Employee employeeChanges);
        Employee Delete(int id);
        //IEnumerable<Employee> GetEmployeesForAdditionalGameSummary(DateTime month);
    }
}
