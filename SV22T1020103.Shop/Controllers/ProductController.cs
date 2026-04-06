using Microsoft.AspNetCore.Mvc;
using SV22T1020103.BusinessLayers;
using SV22T1020103.Models.Common;
using SV22T1020103.Shop.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

namespace SV22T1020103.Shop.Controllers
{
    public class ProductController : Controller
    {
        /// <summary>
        /// Trang danh sách sản phẩm với bộ lọc tìm kiếm
        /// </summary>
        public async Task<IActionResult> Index(string searchValue = "", int categoryId = 0, string minPrice = "0", string maxPrice = "0")
        {
            // 1. Xử lý ép kiểu từ string (có thể chứa dấu chấm từ View) sang decimal
            decimal min = 0;
            decimal max = 0;

            // Xóa dấu chấm (nếu có) trước khi parse để tránh lỗi định dạng
            if (!string.IsNullOrEmpty(minPrice))
                decimal.TryParse(minPrice.Replace(".", ""), out min);

            if (!string.IsNullOrEmpty(maxPrice))
                decimal.TryParse(maxPrice.Replace(".", ""), out max);

            // 2. Thực hiện tìm kiếm sản phẩm qua Business Layer
            var productList = ProductService.Search(searchValue ?? "", categoryId, min, max);

            // 3. Lấy danh sách danh mục để hiển thị lên Sidebar
            var categories = new List<SV22T1020103.Models.Catalog.Category>();
            try
            {
                var categoryInput = new PaginationSearchInput() { Page = 1, PageSize = 100, SearchValue = "" };
                var categoryResult = await CatalogDataService.ListCategoriesAsync(categoryInput);
                if (categoryResult != null) categories = categoryResult.DataItems;
            }
            catch { /* Ghi log nếu cần thiết */ }

            // 4. Đổ dữ liệu ra ViewBag để View tái sử dụng (giữ lại giá trị sau khi submit)
            ViewBag.Categories = categories;
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(productList);
        }

        /// <summary>
        /// Xem chi tiết một sản phẩm
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var product = ProductService.GetProduct(id);
            if (product == null) return RedirectToAction("Index");
            return View(product);
        }

        #region Giỏ hàng & Thanh toán

        public IActionResult AddToCart(int id)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var product = ProductService.GetProduct(id);
            if (product != null)
            {
                var item = cart.FirstOrDefault(p => p.ProductId == id);
                if (item == null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductID,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Image = product.Photo ?? "no-image.png",
                        Quantity = 1
                    });
                }
                else { item.Quantity++; }
                HttpContext.Session.Set("Cart", cart);

                // Cập nhật số lượng Badge trên Header
                int totalQuantity = cart.Sum(c => c.Quantity);
                HttpContext.Session.SetInt32("CartCount", totalQuantity);
            }
            return RedirectToAction("ViewCart");
        }

        public IActionResult ViewCart() => View(HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>());

        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(p => p.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set("Cart", cart);

                int totalQuantity = cart.Sum(c => c.Quantity);
                HttpContext.Session.SetInt32("CartCount", totalQuantity);
            }
            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(p => p.ProductId == id);
            if (item != null)
            {
                item.Quantity = quantity <= 1 ? 1 : quantity;
                HttpContext.Session.Set("Cart", cart);

                int totalQuantity = cart.Sum(c => c.Quantity);
                HttpContext.Session.SetInt32("CartCount", totalQuantity);
            }
            return Json(new { success = true });
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CartCount");
            return RedirectToAction("ViewCart");
        }

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0) return RedirectToAction("Index");
            ViewBag.CustomerName = HttpContext.Session.GetString("CustomerName") ?? "Khách hàng";
            return View(cart);
        }

        [HttpPost]
        public IActionResult DoCheckout(string phone, string deliveryAddress)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0) return RedirectToAction("Index");

            string customerName = HttpContext.Session.GetString("CustomerName") ?? "Khách hàng";

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(deliveryAddress))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin!");
                return View("Checkout", cart);
            }

            // Tạo đơn hàng mới
            var orderData = new Order()
            {
                OrderId = new Random().Next(1000, 9999),
                OrderTime = DateTime.Now,
                CustomerName = customerName,
                Phone = phone,
                Address = deliveryAddress,
                TotalAmount = cart.Sum(i => i.Quantity * i.Price),
                Status = "Chờ xác nhận",
                Details = cart.Select(c => new OrderDetail
                {
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    SalePrice = c.Price
                }).ToList()
            };

            // Lưu vào lịch sử (Session)
            var history = HttpContext.Session.Get<List<Order>>("OrderHistory") ?? new List<Order>();
            history.Add(orderData);
            HttpContext.Session.Set("OrderHistory", history);

            // Xóa giỏ hàng sau khi đặt thành công
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CartCount");

            return View("OrderSuccess", orderData);
        }

        public IActionResult OrderSuccess(Order model) => View(model);

        #endregion

        #region Quản lý Đơn hàng

        public IActionResult History()
        {
            string customerName = HttpContext.Session.GetString("CustomerName");
            if (string.IsNullOrEmpty(customerName)) return RedirectToAction("Login", "Account");

            var model = HttpContext.Session.Get<List<Order>>("OrderHistory") ?? new List<Order>();
            return View(model.OrderByDescending(o => o.OrderTime).ToList());
        }

        public IActionResult OrderDetail(int id)
        {
            var history = HttpContext.Session.Get<List<Order>>("OrderHistory") ?? new List<Order>();
            var order = history.FirstOrDefault(o => o.OrderId == id);

            if (order == null) return RedirectToAction("History");

            return View(order);
        }

        #endregion
    }
}