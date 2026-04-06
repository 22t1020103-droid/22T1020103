using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020103.Models.Security;
using System.Data;

namespace SV22T1020103.BusinessLayers
{
    public static class SecurityDataService
    {
        private static IDbConnection OpenConnection()
        {
            Console.WriteLine(">>> OpenConnection: " + Configuration.ConnectionString);

            var connection = new SqlConnection(Configuration.ConnectionString);
            connection.Open();
            return connection;
        }

        public static async Task<UserAccount?> EmployeeAuthorizeAsync(string userName, string password)
        {
            using var connection = OpenConnection();

            var sql = @"
                SELECT TOP (1)
                    CAST(EmployeeID AS varchar(20)) AS UserId,
                    Email AS UserName,
                    FullName AS DisplayName,
                    Email,
                    Photo,
                    RoleNames
                FROM Employees
                WHERE Email = @userName
                  AND [Password] = @password
                  AND IsWorking = 1;
            ";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql, new { userName, password });
        }
    }
}