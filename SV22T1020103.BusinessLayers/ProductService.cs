using System.Collections.Generic;

using System.Linq;

using Dapper;

using Microsoft.Data.SqlClient;

using SV22T1020103.Models.Catalog;



namespace SV22T1020103.BusinessLayers

{

    public static class ProductService

    {

        private static string? _connectionString;

        public static void Initialize(string connectionString) => _connectionString = connectionString;

        private static SqlConnection GetConnection() => new SqlConnection(_connectionString);



        /// <summary>

        /// Tìm kiếm sản phẩm (Lọc theo ID danh mục thay vì tên)

        /// </summary>

        public static List<Product> Search(string name = "", int categoryId = 0, decimal minPrice = 0, decimal maxPrice = 0)

        {

            using (var db = GetConnection())

            {

                // Sử dụng LEFT JOIN và lọc theo p.CategoryID (kiểu int) để khớp với Controller

                string sql = @"SELECT p.ProductID, p.ProductName, p.Price, p.Photo, 

                                      p.ProductDescription, p.CategoryID, c.CategoryName

                               FROM Products p 

                               LEFT JOIN Categories c ON p.CategoryID = c.CategoryID

                               WHERE (@n = '' OR p.ProductName LIKE @n)

                                 AND (@c = 0 OR p.CategoryID = @c)

                                 AND (@min = 0 OR p.Price >= @min)

                                 AND (@max = 0 OR p.Price <= @max)";



                return db.Query<Product>(sql, new

                {

                    n = string.IsNullOrEmpty(name) ? "" : "%" + name + "%",

                    c = categoryId, // Truyền ID danh mục vào đây

                    min = minPrice,

                    max = maxPrice

                }).ToList();

            }

        }



        public static Product? GetProduct(int id)

        {

            using (var db = GetConnection())

            {

                string sql = @"SELECT p.ProductID, p.ProductName, p.Price, p.Photo, 

                                      p.ProductDescription, p.CategoryID, c.CategoryName

                               FROM Products p 

                               LEFT JOIN Categories c ON p.CategoryID = c.CategoryID

                               WHERE p.ProductID = @id";

                return db.QueryFirstOrDefault<Product>(sql, new { id });

            }

        }

    }

}