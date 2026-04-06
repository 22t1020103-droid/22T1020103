#nullable disable
using Microsoft.AspNetCore.Mvc;
using SV22T1020103.BusinessLayers;
using SV22T1020103.Models.Catalog;
using SV22T1020103.Models.Common;
using SV22T1020103.Models.Sales;

namespace SV22T1020103.Admin.Controllers
{
    public class OrderController : Controller
    {
        private const int PAGESIZE = 10;
        private const string ORDER_SEARCH = "OrderSearchInput";
        private const string SEARCH_PRODUCT = "SearchProductToSale";

        public IActionResult Index()
        {
            ViewBag.Title = "Quản lý đơn hàng";

            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    Status = (OrderStatusEnum)0,
                    DateFrom = null,
                    DateTo = null
                };
            }

            return View(input);
        }

        [HttpPost]
        public IActionResult Search(OrderSearchInput input)
        {
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    Status = (OrderStatusEnum)0,
                    DateFrom = null,
                    DateTo = null
                };
            }

            if (input.Page <= 0) input.Page = 1;
            if (input.PageSize <= 0) input.PageSize = PAGESIZE;
            if (input.SearchValue == null) input.SearchValue = "";

            ApplicationContext.SetSessionData(ORDER_SEARCH, input);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SearchResult(int page = 1, string searchValue = "", OrderStatusEnum status = (OrderStatusEnum)0, string dateFrom = "", string dateTo = "")
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
            if (input == null)
            {
                input = new OrderSearchInput();
            }

            input.Page = page;
            input.PageSize = PAGESIZE;
            input.SearchValue = searchValue ?? "";
            input.Status = status;
            input.DateFrom = ParseDateString(dateFrom);
            input.DateTo = ParseDateString(dateTo);

            ApplicationContext.SetSessionData(ORDER_SEARCH, input);

            var result = await SalesDataService.ListOrdersAsync(input);
            return View(result);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var data = await SalesDataService.GetOrderAsync(id);
            if (data == null) return RedirectToAction("Index");

            var orderDetails = await SalesDataService.ListDetailsAsync(id);
            if (orderDetails != null)
            {
                ViewBag.OrderDetails = orderDetails;
            }

            ViewBag.Title = "Chi tiết đơn hàng";
            return View(data);
        }

        public async Task<IActionResult> Create()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(SEARCH_PRODUCT);
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 3,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }

            // Gợi ý danh sách khách hàng ban đầu
            var customerInput = new PaginationSearchInput() { Page = 1, PageSize = 20, SearchValue = "" };
            var customers = await PartnerDataService.ListCustomersAsync(customerInput);
            ViewBag.InitialCustomers = customers.DataItems;

            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> SearchCustomer(string term = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 20,
                SearchValue = term ?? ""
            };

            var customers = await PartnerDataService.ListCustomersAsync(input);
            var data = customers.DataItems.Select(c => new
            {
                customerID = c.CustomerID,
                customerName = c.CustomerName ?? "",
                province = c.Province ?? "",
                address = c.Address ?? ""
            });

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerInfo(int customerID = 0)
        {
            if (customerID <= 0) return Json(null);

            var customer = await PartnerDataService.GetCustomerAsync(customerID);
            if (customer == null) return Json(null);

            return Json(new
            {
                customerID = customer.CustomerID,
                customerName = customer.CustomerName ?? "",
                province = customer.Province ?? "",
                address = customer.Address ?? ""
            });
        }

        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            if (input.Page <= 0) input.Page = 1;
            if (input.PageSize <= 0) input.PageSize = 3;
            if (input.SearchValue == null) input.SearchValue = "";

            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(SEARCH_PRODUCT, input);
            return View(result);
        }

        public IActionResult ShowCart()
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            return View(cart); // Đảm bảo bạn có file View/Order/ShowCart.cshtml (PartialView)
        }

        public async Task<IActionResult> AddCartItem(int productId = 0, int quantity = 0, decimal price = 0)
        {
            if (productId <= 0) return Json(new { code = 0, message = "Mã mặt hàng không hợp lệ" });
            if (quantity <= 0) return Json(new { code = 0, message = "Số lượng phải lớn hơn 0" });

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null) return Json(new { code = 0, message = "Mặt hàng không tồn tại" });
            if (!product.IsSelling) return Json(new { code = 0, message = "Mặt hàng này đã ngưng bán" });

            var item = new OrderDetailViewInfo()
            {
                ProductID = productId,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo,
                Quantity = quantity,
                SalePrice = price > 0 ? price : product.Price
            };

            ShoppingCartHelper.AddItemToCart(item);
            return Json(new { code = 1, message = "" });
        }

        public IActionResult EditCartItem(int productID)
        {
            var item = ShoppingCartHelper.GetCartItem(productID);
            if (item == null) return Content("Không có dữ liệu mặt hàng");
            return PartialView(item);
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int ProductID, int Quantity, decimal SalePrice)
        {
            if (Quantity <= 0) return Json(new { code = 0, message = "Số lượng không hợp lệ" });

            ShoppingCartHelper.UpdateCartItem(ProductID, Quantity, SalePrice);
            return Json(new { code = 1, message = "Cập nhật thành công" });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerID = 0, string province = "", string address = "")
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            if (cart == null || cart.Count == 0) return Json(new { code = 0, message = "Giỏ hàng đang trống" });
            if (string.IsNullOrWhiteSpace(province) || string.IsNullOrWhiteSpace(address))
                return Json(new { code = 0, message = "Vui lòng nhập đầy đủ thông tin giao hàng" });

            try
            {
                int orderID = await SalesDataService.AddOrderAsync(customerID, address, province);
                foreach (var item in cart)
                {
                    await SalesDataService.AddDetailAsync(new OrderDetail()
                    {
                        OrderID = orderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        SalePrice = item.SalePrice
                    });
                }
                ShoppingCartHelper.ClearCart();
                return Json(new { code = orderID, message = "" });
            }
            catch { return Json(new { code = 0, message = "Lỗi khi tạo đơn hàng" }); }
        }

        // --- CÁC HÀM XỬ LÝ TRẠNG THÁI (DỮ NGUYÊN LOGIC CŨ) ---

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await SalesDataService.GetOrderAsync(id);
            if (data == null) return RedirectToAction("Index");
            ViewBag.Title = "Xóa đơn hàng";
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int orderID, string returnUrl = "")
        {
            await SalesDataService.DeleteOrderAsync(orderID);
            if (!string.IsNullOrWhiteSpace(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCartItem(int productId)
        {
            if (Request.Method == "POST")
            {
                ShoppingCartHelper.RemoveItemFromCart(productId);
                return Json(new { code = 1 });
            }
            ViewBag.ProductID = productId;
            return PartialView();
        }

        public IActionResult ClearCart()
        {
            if (Request.Method == "POST")
            {
                ShoppingCartHelper.ClearCart();
                return Json(new { code = 1 });
            }
            return PartialView();
        }

        [HttpGet] public IActionResult Accept(int id) { ViewBag.OrderID = id; return PartialView(); }
        [HttpPost, ActionName("Accept")]
        public async Task<IActionResult> AcceptPost(int orderID)
        {
            await SalesDataService.AcceptOrderAsync(orderID, 1); // 1: EmployeeID giả định
            return RedirectToAction("Detail", new { id = orderID });
        }

        [HttpGet] public IActionResult Reject(int id) { ViewBag.OrderID = id; return PartialView(); }
        [HttpPost, ActionName("Reject")]
        public async Task<IActionResult> RejectPost(int orderID)
        {
            await SalesDataService.RejectOrderAsync(orderID, 1);
            return RedirectToAction("Detail", new { id = orderID });
        }

        [HttpGet] public IActionResult Cancel(int id) { ViewBag.OrderID = id; return PartialView(); }
        [HttpPost, ActionName("Cancel")]
        public async Task<IActionResult> CancelPost(int orderID)
        {
            await SalesDataService.CancelOrderAsync(orderID);
            return RedirectToAction("Detail", new { id = orderID });
        }

        [HttpGet] public IActionResult Finish(int id) { ViewBag.OrderID = id; return PartialView(); }
        [HttpPost, ActionName("Finish")]
        public async Task<IActionResult> FinishPost(int orderID)
        {
            await SalesDataService.CompleteOrderAsync(orderID);
            return RedirectToAction("Detail", new { id = orderID });
        }

        [HttpGet] public IActionResult Shipping(int id) { ViewBag.OrderID = id; return PartialView(); }
        [HttpPost, ActionName("Shipping")]
        public async Task<IActionResult> ShippingPost(int orderID, int shipperID)
        {
            await SalesDataService.ShipOrderAsync(orderID, shipperID);
            return RedirectToAction("Detail", new { id = orderID });
        }

        private DateTime? ParseDateString(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;
            if (DateTime.TryParse(dateString, out var result)) return result;
            return null;
        }
    }
}