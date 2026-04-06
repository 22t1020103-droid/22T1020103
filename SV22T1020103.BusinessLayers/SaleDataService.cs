using SV22T1020103.BusinessLayers;
using SV22T1020103.DataLayers.Interfaces;
using SV22T1020103.DataLayers.SQLServer;
using SV22T1020103.Models.Common;
using SV22T1020103.Models.Sales;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020103.BusinessLayers
{
    public static class SalesDataService
    {
        // Bỏ 'readonly' để có thể gán giá trị trong hàm Initialize
        private static IOrderRepository? orderDB;

        /// <summary>
        /// Khởi tạo Service (Phải được gọi từ Program.cs)
        /// </summary>
        public static void Initialize(string connectionString)
        {
            orderDB = new OrderRepository(connectionString);
        }

        private static void CheckInitialized()
        {
            if (orderDB == null) throw new InvalidOperationException("SalesDataService chưa được khởi tạo.");
        }

        #region Order (Đơn hàng)

        public static async Task<PagedResult<OrderSearchInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            CheckInitialized();
            input ??= new OrderSearchInput()
            {
                Page = 1,
                PageSize = 10,
                SearchValue = "",
                Status = 0
            };

            if (input.Page <= 0) input.Page = 1;
            if (input.PageSize <= 0) input.PageSize = 10;
            input.SearchValue ??= "";

            return await orderDB!.ListAsync(input);
        }

        public static async Task<PagedResult<OrderSearchInfo>> ListOrdersByCustomerAsync(int customerID, OrderSearchInput input)
        {
            CheckInitialized();
            if (customerID <= 0)
                return new PagedResult<OrderSearchInfo>() { Page = 1, RowCount = 0 };

            input ??= new OrderSearchInput() { Page = 1, PageSize = 10 };
            input.SearchValue ??= "";

            return await orderDB!.ListByCustomerAsync(customerID, input);
        }

        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            CheckInitialized();
            if (orderID <= 0) return null;
            return await orderDB!.GetAsync(orderID);
        }

        public static async Task<int> AddOrderAsync(int customerID, string deliveryAddress, string deliveryProvince)
        {
            CheckInitialized();
            if (string.IsNullOrWhiteSpace(deliveryAddress) || string.IsNullOrWhiteSpace(deliveryProvince))
                return 0;

            var order = new Order()
            {
                CustomerID = customerID == 0 ? null : customerID,
                DeliveryAddress = deliveryAddress.Trim(),
                DeliveryProvince = deliveryProvince.Trim(),
                Status = OrderStatusEnum.New,
                OrderTime = DateTime.Now
            };

            return await orderDB!.AddAsync(order);
        }

        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            CheckInitialized();
            if (data == null || data.OrderID <= 0) return false;

            var oldOrder = await orderDB!.GetAsync(data.OrderID);
            if (oldOrder == null || oldOrder.Status != OrderStatusEnum.New)
                return false;

            data.OrderTime = oldOrder.OrderTime;
            data.Status = oldOrder.Status;

            return await orderDB!.UpdateAsync(data);
        }

        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            CheckInitialized();
            if (orderID <= 0) return false;
            return await orderDB!.DeleteAsync(orderID);
        }

        #endregion

        #region Xử lý trạng thái đơn hàng (Duyệt, Giao hàng, Hoàn tất...)

        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            CheckInitialized();
            var order = await orderDB!.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New) return false;

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.Status = OrderStatusEnum.Accepted;

            return await orderDB!.UpdateAsync(order);
        }

        public static async Task<bool> RejectOrderAsync(int orderID, int employeeID)
        {
            CheckInitialized();
            var order = await orderDB!.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New) return false;

            order.EmployeeID = employeeID;
            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Rejected;

            return await orderDB!.UpdateAsync(order);
        }

        public static async Task<bool> CancelOrderAsync(int orderID)
        {
            CheckInitialized();
            var order = await orderDB!.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Cancelled;

            return await orderDB!.UpdateAsync(order);
        }

        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            CheckInitialized();
            var order = await orderDB!.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Accepted) return false;

            order.ShipperID = shipperID;
            order.ShippedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Shipping;

            return await orderDB!.UpdateAsync(order);
        }

        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            CheckInitialized();
            var order = await orderDB!.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.Shipping) return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Completed;

            return await orderDB!.UpdateAsync(order);
        }

        #endregion

        #region Order Detail (Chi tiết đơn hàng)

        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            CheckInitialized();
            return orderID <= 0 ? new List<OrderDetailViewInfo>() : await orderDB!.ListDetailsAsync(orderID);
        }

        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            CheckInitialized();
            if (data == null || data.Quantity <= 0 || data.SalePrice < 0) return false;

            var order = await orderDB!.GetAsync(data.OrderID);
            if (order == null || order.Status != OrderStatusEnum.New) return false;

            var oldDetail = await orderDB!.GetDetailAsync(data.OrderID, data.ProductID);
            if (oldDetail != null)
            {
                data.Quantity += oldDetail.Quantity;
                return await orderDB!.UpdateDetailAsync(data);
            }

            return await orderDB!.AddDetailAsync(data);
        }

        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            CheckInitialized();
            var order = await orderDB!.GetAsync(orderID);
            if (order == null || order.Status != OrderStatusEnum.New) return false;

            return await orderDB!.DeleteDetailAsync(orderID, productID);
        }

        #endregion
    }
}