namespace SV22T1020103.Shop.Models
{
    /// <summary>
    /// Model đại diện cho một mặt hàng trong giỏ hàng.
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// Mã định danh của sản phẩm.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Tên sản phẩm.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn hoặc tên file ảnh của sản phẩm.
        /// </summary>
        public string Image { get; set; } = string.Empty;

        /// <summary>
        /// Đơn giá của sản phẩm tại thời điểm bỏ vào giỏ.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng sản phẩm khách hàng chọn mua.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Thành tiền của mặt hàng này (Giá * Số lượng).
        /// Đây là thuộc tính chỉ đọc (Read-only property).
        /// </summary>
        public decimal Total => Price * Quantity;
    }
}