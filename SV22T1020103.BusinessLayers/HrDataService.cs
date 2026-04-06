using SV22T1020103.BusinessLayers;
using SV22T1020103.DataLayers.Interfaces;
using SV22T1020103.DataLayers.SQLServer;
using SV22T1020103.Models.Common;
using SV22T1020103.Models.HR;

namespace SV22T1020103.BusinessLayers
{
    public static class HRDataService
    {
        private static IEmployeeRepository? employeeDB;

        /// <summary>
        /// Khởi tạo service (Gọi từ Program.cs)
        /// </summary>
        public static void Initialize(string connectionString)
        {
            employeeDB = new EmployeeRepository(connectionString);
        }

        #region Employee

        public static async Task<PagedResult<Employee>> ListEmployeesAsync(PaginationSearchInput input)
        {
            if (employeeDB == null) throw new InvalidOperationException("HRDataService chưa được khởi tạo.");
            return await employeeDB.ListAsync(input);
        }

        public static async Task<Employee?> GetEmployeeAsync(int employeeID)
        {
            return await employeeDB!.GetAsync(employeeID);
        }

        public static async Task<int> AddEmployeeAsync(Employee data)
        {
            return await employeeDB!.AddAsync(data);
        }

        public static async Task<bool> UpdateEmployeeAsync(Employee data)
        {
            return await employeeDB!.UpdateAsync(data);
        }

        public static async Task<bool> DeleteEmployeeAsync(int employeeID)
        {
            if (await employeeDB!.IsUsedAsync(employeeID))
                return false;

            return await employeeDB!.DeleteAsync(employeeID);
        }

        public static async Task<bool> IsUsedEmployeeAsync(int employeeID)
        {
            return await employeeDB!.IsUsedAsync(employeeID);
        }

        public static async Task<bool> ValidateEmployeeEmailAsync(string email, int employeeID = 0)
        {
            return await employeeDB!.ValidateEmailAsync(email, employeeID);
        }

        public static async Task<string?> GetEmployeeRoleNamesAsync(int employeeID)
        {
            return await employeeDB!.GetRoleNamesAsync(employeeID);
        }

        public static async Task<bool> UpdateEmployeeRoleNamesAsync(int employeeID, string roleNames)
        {
            return await employeeDB!.UpdateRoleNamesAsync(employeeID, roleNames);
        }

        public static async Task<bool> ChangeEmployeePasswordAsync(int employeeID, string oldPassword, string newPassword)
        {
            return await employeeDB!.ChangePasswordAsync(employeeID, oldPassword, newPassword);
        }

        public static async Task<bool> SetEmployeePasswordAsync(int employeeID, string newPassword)
        {
            return await employeeDB!.SetPasswordAsync(employeeID, newPassword);
        }

        #endregion
    }
}