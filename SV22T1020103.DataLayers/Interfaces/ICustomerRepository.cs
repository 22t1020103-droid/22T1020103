using SV22T1020103.DataLayers.Interfaces;
using SV22T1020103.Models.Partner;


namespace SV22T1020103.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không?
        /// </summary>
        Task<bool> ValidateEmailAsync(string email, int id = 0);

        /// <summary>
        /// Đổi mật khẩu khách hàng (yêu cầu đúng mật khẩu cũ - đã hash)
        /// </summary>
        Task<bool> ChangePasswordAsync(int customerID, string oldPassword, string newPassword);

        /// <summary>
        /// Thiết lập lại mật khẩu khách hàng (không cần mật khẩu cũ - đã hash)
        /// </summary>
        Task<bool> SetPasswordAsync(int customerID, string newPassword);
    }
}