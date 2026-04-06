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
        public async Task<IActionResult> Index(string searchValue = "", int categoryId = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            var categories = new List<SV22T1020103.Models.Catalog.Category>();
            var productList = ProductService.Search(searchValue ?? "", categoryId, minPrice, maxPrice);
            try
            {
                var categoryInput = new PaginationSearchInput() { Page = 1, PageSize = 100, SearchValue = "" };
                var categoryResult = await CatalogDataService.ListCategoriesAsync(categoryInput);
                if (categoryResult != null) categories = categoryResult.DataItems;
            }
            catch { }

            ViewBag.Categories = categories;
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentCategory = categoryId;
            return View(productList);
        }

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

                // BỔ SUNG: Cập nhật số lượng hiển thị trên Badge
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

                // BỔ SUNG: Cập nhật lại số lượng sau khi xóa
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

                // BỔ SUNG: Cập nhật lại số lượng sau khi thay đổi số lượng từng món
                int totalQuantity = cart.Sum(c => c.Quantity);
                HttpContext.Session.SetInt32("CartCount", totalQuantity);
            }
            return Json(new { success = true });
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            // BỔ SUNG: Xóa luôn số lượng trên Badge
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

        /// <summary>
        /// Xử lý xác nhận đặt hàng và lưu vào Lịch sử đơn hàng (Session)
        /// </summary>
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

            // Tạo đối tượng Order mới mang theo danh sách sản phẩm thực tế
            var orderData = new Order()
            {
                OrderId = new Random().Next(1000, 9999),
                OrderTime = DateTime.Now,
                CustomerName = customerName,
                Phone = phone,
                Address = deliveryAddress,
                TotalAmount = cart.Sum(i => i.Quantity * i.Price),
                Status = "Chờ xác nhận",
                // Chuyển dữ liệu từ giỏ hàng sang Details của đơn hàng
                Details = cart.Select(c => new OrderDetail
                {
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    SalePrice = c.Price
                }).ToList()
            };

            // LƯU VÀO SESSION ĐỂ XEM LẠI TRONG HISTORY/DETAIL
            var history = HttpContext.Session.Get<List<Order>>("OrderHistory") ?? new List<Order>();
            history.Add(orderData);
            HttpContext.Session.Set("OrderHistory", history);

            HttpContext.Session.Remove("Cart");
            // BỔ SUNG: Xóa số lượng Badge sau khi đặt hàng thành công
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

            // Lấy danh sách thực tế thay vì gán tĩnh
            var model = HttpContext.Session.Get<List<Order>>("OrderHistory") ?? new List<Order>();
            return View(model.OrderByDescending(o => o.OrderTime).ToList());
        }

        public IActionResult OrderDetail(int id)
        {
            // Tìm đơn hàng trong lịch sử Session dựa trên ID truyền vào
            var history = HttpContext.Session.Get<List<Order>>("OrderHistory") ?? new List<Order>();
            var order = history.FirstOrDefault(o => o.OrderId == id);

            // Nếu không thấy (do reset session) thì tạo mẫu để không lỗi giao diện
            if (order == null)
            {
                return RedirectToAction("History");
            }

            return View(order);
        }

        #endregion
    }
}