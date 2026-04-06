using SV22T1020103.BusinessLayers;
using SV22T1020103.DataLayers.SQLServer;
using SV22T1020103.DataLayers.Interfaces;
using SV22T1020103.Models.Common;
using SV22T1020103.Models.Partner;

namespace SV22T1020103.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến các đối tác của hệ thống
    /// </summary>
    public static class PartnerDataService
    {
        // Bỏ 'readonly' để có thể khởi tạo trong hàm Initialize
        private static IGenericRepository<Supplier>? supplierDB;
        private static ICustomerRepository? customerDB;
        private static IGenericRepository<Shipper>? shipperDB;

        /// <summary>
        /// Khởi tạo Service (Phải được gọi từ Program.cs)
        /// </summary>
        public static void Initialize(string connectionString)
        {
            supplierDB = new SupplierRepository(connectionString);
            customerDB = new CustomerRepository(connectionString);
            shipperDB = new ShipperRepository(connectionString);
        }

        #region Supplier
        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await supplierDB!.ListAsync(input);
        }

        public static async Task<Supplier?> GetSupplierAsync(int supplierID) => await supplierDB!.GetAsync(supplierID);
        public static async Task<int> AddSupplierAsync(Supplier data) => await supplierDB!.AddAsync(data);
        public static async Task<bool> UpdateSupplierAsync(Supplier data) => await supplierDB!.UpdateAsync(data);
        public static async Task<bool> DeleteSupplierAsync(int supplierID)
        {
            if (await supplierDB!.IsUsedAsync(supplierID)) return false;
            return await supplierDB!.DeleteAsync(supplierID);
        }
        public static async Task<bool> IsUsedSupplierAsync(int supplierID) => await supplierDB!.IsUsedAsync(supplierID);
        #endregion

        #region Customer
        public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input) => await customerDB!.ListAsync(input);
        public static async Task<Customer?> GetCustomerAsync(int customerID) => await customerDB!.GetAsync(customerID);
        public static async Task<int> AddCustomerAsync(Customer data) => await customerDB!.AddAsync(data);
        public static async Task<bool> UpdateCustomerAsync(Customer data) => await customerDB!.UpdateAsync(data);
        public static async Task<bool> DeleteCustomerAsync(int customerID)
        {
            if (await customerDB!.IsUsedAsync(customerID)) return false;
            return await customerDB!.DeleteAsync(customerID);
        }
        public static async Task<bool> IsUsedCustomerAsync(int customerID) => await customerDB!.IsUsedAsync(customerID);
        public static async Task<bool> ValidatelCustomerEmailAsync(string email, int customerID = 0) => await customerDB!.ValidateEmailAsync(email, customerID);
        public static async Task<bool> ChangeCustomerPasswordAsync(int customerID, string oldPassword, string newPassword) => await customerDB!.ChangePasswordAsync(customerID, oldPassword, newPassword);
        public static async Task<bool> SetCustomerPasswordAsync(int customerID, string newPassword) => await customerDB!.SetPasswordAsync(customerID, newPassword);
        #endregion

        #region Shipper
        public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input) => await shipperDB!.ListAsync(input);
        public static async Task<Shipper?> GetShipperAsync(int shipperID) => await shipperDB!.GetAsync(shipperID);
        public static async Task<int> AddShipperAsync(Shipper data) => await shipperDB!.AddAsync(data);
        public static async Task<bool> UpdateShipperAsync(Shipper data) => await shipperDB!.UpdateAsync(data);
        public static async Task<bool> DeleteShipperAsync(int shipperID)
        {
            if (await shipperDB!.IsUsedAsync(shipperID)) return false;
            return await shipperDB!.DeleteAsync(shipperID);
        }
        public static async Task<bool> IsUsedShipperAsync(int shipperID) => await shipperDB!.IsUsedAsync(shipperID);
        #endregion
    }
}