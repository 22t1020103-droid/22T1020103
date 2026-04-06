using System;
using System.Collections.Generic;

namespace SV22T1020103.Shop.Models
{
    /// <summary>
    /// Đại diện cho một đơn hàng trong hệ thống Shop.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Mã định danh duy nhất của đơn hàng.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Thời điểm khách hàng đặt hàng.
        /// </summary>
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// Tên của khách hàng thực hiện đặt hàng.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên lạc của khách hàng.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ giao hàng.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Tổng giá trị của toàn bộ đơn hàng (bao gồm tất cả các mặt hàng).
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Trạng thái hiện tại của đơn hàng (Ví dụ: Chờ xác nhận, Đang giao, Đã hoàn thành).
        /// Mặc định là "Chờ xác nhận".
        /// </summary>
        public string Status { get; set; } = "Chờ xác nhận";

        /// <summary>
        /// Danh sách các mặt hàng chi tiết có trong đơn hàng này.
        /// </summary>
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }

    /// <summary>
    /// Chi tiết về một mặt hàng cụ thể trong đơn hàng.
    /// </summary>
    public class OrderDetail
    {
        /// <summary>
        /// Tên của sản phẩm tại thời điểm đặt hàng.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng sản phẩm khách đặt mua.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Giá bán của sản phẩm tại thời điểm chốt đơn.
        /// </summary>
        public decimal SalePrice { get; set; }

        /// <summary>
        /// Thành tiền của mặt hàng (Số lượng x Giá bán).
        /// </summary>
        public decimal TotalPrice => Quantity * SalePrice;
    }
}