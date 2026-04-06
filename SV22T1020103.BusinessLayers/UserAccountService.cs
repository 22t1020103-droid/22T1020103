using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020103.Models.Partner;

namespace SV22T1020103.BusinessLayers
{
    public static class UserAccountService
    {
        private static string? _connectionString;

        // Hàm để nhận chuỗi kết nối từ Program.cs
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        private static SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public static Customer? Authorize(string username, string password)
        {
            using (var db = GetConnection())
            {
                // Đăng nhập bằng Email hoặc Tên hiển thị (Khớp với logic cũ của bạn)
                string sql = @"SELECT * FROM Customers 
                               WHERE (CustomerName = @u OR Email = @u) 
                               AND Password = @p";
                return db.QueryFirstOrDefault<Customer>(sql, new { u = username, p = password });
            }
        }

        public static void RegisterCustomer(Customer customer)
        {
            if (customer == null) return;

            using (var db = GetConnection())
            {
                string sql = @"INSERT INTO Customers (CustomerName, Email, Password, Phone, Address, Province)
                               VALUES (@CustomerName, @Email, @Password, @Phone, @Address, @Province)";
                db.Execute(sql, customer);
            }
        }

        public static Customer? GetUser(int id)
        {
            using (var db = GetConnection())
            {
                string sql = "SELECT * FROM Customers WHERE CustomerID = @id";
                return db.QueryFirstOrDefault<Customer>(sql, new { id });
            }
        }

        public static void Update(Customer customer)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("Connection string chưa được khởi tạo!");

            using (var db = GetConnection())
            {
                // Câu lệnh SQL xử lý thông minh: Nếu mật khẩu mới trống thì giữ mật khẩu cũ
                string sql = @"UPDATE Customers 
                       SET CustomerName = @CustomerName, 
                           Email = @Email, 
                           Phone = @Phone,
                           Address = @Address,
                           Province = @Province,
                           Password = CASE 
                                        WHEN @Password = '' OR @Password IS NULL THEN Password 
                                        ELSE @Password 
                                      END
                       WHERE CustomerID = @CustomerID";
                db.Execute(sql, customer);
            }
        }
    }
    }
