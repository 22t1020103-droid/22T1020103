using SV22T1020103.Models.Sales;

namespace SV22T1020103.Admin
{
    /// <summary>
    /// Lớp cung cấp các chức năng xử lý trên giỏ hàng
    /// (Giỏ hàng được lưu trong Session)
    /// </summary>
    public static class ShoppingCartHelper
    {
        private const string CART = "ShoppingCart";

        /// <summary>
        /// Lấy giỏ hàng từ session 
        /// </summary>
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }
            return cart;
        }

        /// <summary>
        /// Lấy thông tin 1 mặt hàng từ giỏ hàng
        /// </summary>
        public static OrderDetailViewInfo? GetCartItem(int productID)
        {
            var cart = GetShoppingCart();
            var item = cart.Find(m => m.ProductID == productID);
            return item;
        }

        /// <summary>
        /// Thêm hàng vào giỏ
        /// </summary>
        public static void AddItemToCart(OrderDetailViewInfo item)
        {
            var cart = GetShoppingCart();
            var existItem = cart.Find(m => m.ProductID == item.ProductID);
            if (existItem == null)
            {
                cart.Add(item);
            }
            else
            {
                existItem.Quantity += item.Quantity;
                existItem.SalePrice = item.SalePrice;
            }
            ApplicationContext.SetSessionData(CART, cart);
        }

        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong giỏ
        /// </summary>
        public static void UpdateCartItem(int productID, int quantity, decimal salePrice)
        {
            var cart = GetShoppingCart();
            var item = cart.Find(m => m.ProductID == productID);

            // SỬA LỖI: Phải là khác null mới cập nhật được
            if (item != null)
            {
                item.Quantity = quantity;
                item.SalePrice = salePrice;
                ApplicationContext.SetSessionData(CART, cart);
            }
        }

        /// <summary>
        /// Xóa mặt hàng khỏi giỏ hàng
        /// </summary>
        public static void RemoveItemFromCart(int productID)
        {
            var cart = GetShoppingCart();
            int index = cart.FindIndex(m => m.ProductID == productID);

            // SỬA LỖI: index >= 0 nghĩa là tìm thấy món hàng ở bất kỳ vị trí nào
            if (index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData(CART, cart);
            }
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public static void ClearCart()
        {
            var newCart = new List<OrderDetailViewInfo>();
            ApplicationContext.SetSessionData(CART, newCart);
        }
    }
}