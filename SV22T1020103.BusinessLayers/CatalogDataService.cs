using SV22T1020103.DataLayers.Interfaces;
using SV22T1020103.DataLayers.SQLServer;
using SV22T1020103.Models.Catalog;
using SV22T1020103.Models.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020103.BusinessLayers
{
    public static class CatalogDataService
    {
        private static IProductRepository? productDB;
        private static IGenericRepository<Category>? categoryDB;

        public static void Initialize(string connectionString)
        {
            categoryDB = new CategoryRepository(connectionString);
            productDB = new ProductRepository(connectionString);
        }

        #region Category (Loại hàng)
        public static async Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput { Page = 1, PageSize = 0, SearchValue = "" };
            if (categoryDB == null) throw new InvalidOperationException("Service chưa khởi tạo");
            return await categoryDB.ListAsync(input);
        }

        public static async Task<Category?> GetCategoryAsync(int id) => await categoryDB!.GetAsync(id);
        public static async Task<int> AddCategoryAsync(Category data) => await categoryDB!.AddAsync(data);
        public static async Task<bool> UpdateCategoryAsync(Category data) => await categoryDB!.UpdateAsync(data);
        public static async Task<bool> DeleteCategoryAsync(int id) => await categoryDB!.DeleteAsync(id);
        public static async Task<bool> IsUsedCategoryAsync(int id) => await categoryDB!.IsUsedAsync(id);
        #endregion

        #region Product (Sản phẩm)
        public static async Task<PagedResult<Product>> ListProductsAsync(ProductSearchInput input)
        {
            input ??= new ProductSearchInput { Page = 1, PageSize = 0, SearchValue = "" };
            if (productDB == null) throw new InvalidOperationException("Service chưa khởi tạo");
            return await productDB.ListAsync(input);
        }

        public static async Task<Product?> GetProductAsync(int id) => await productDB!.GetAsync(id);
        public static async Task<int> AddProductAsync(Product data) => await productDB!.AddAsync(data);
        public static async Task<bool> UpdateProductAsync(Product data) => await productDB!.UpdateAsync(data);
        public static async Task<bool> DeleteProductAsync(int id) => await productDB!.DeleteAsync(id);
        public static async Task<bool> IsUsedProductAsync(int id) => await productDB!.IsUsedAsync(id);
        #endregion

        #region Product Photo (Ảnh sản phẩm)
        public static async Task<List<ProductPhoto>> ListPhotosAsync(int productID) => await productDB!.ListPhotosAsync(productID);
        public static async Task<ProductPhoto?> GetPhotoAsync(long photoID) => await productDB!.GetPhotoAsync(photoID);
        public static async Task<long> AddPhotoAsync(ProductPhoto data) => await productDB!.AddPhotoAsync(data);
        public static async Task<bool> UpdatePhotoAsync(ProductPhoto data) => await productDB!.UpdatePhotoAsync(data);
        public static async Task<bool> DeletePhotoAsync(long photoID) => await productDB!.DeletePhotoAsync(photoID);
        #endregion

        #region Product Attribute (Thuộc tính sản phẩm)
        public static async Task<List<ProductAttribute>> ListAttributesAsync(int productID) => await productDB!.ListAttributesAsync(productID);
        public static async Task<ProductAttribute?> GetAttributeAsync(long attributeID) => await productDB!.GetAttributeAsync(attributeID);
        public static async Task<long> AddAttributeAsync(ProductAttribute data) => await productDB!.AddAttributeAsync(data);
        public static async Task<bool> UpdateAttributeAsync(ProductAttribute data) => await productDB!.UpdateAttributeAsync(data);
        public static async Task<bool> DeleteAttributeAsync(long attributeID) => await productDB!.DeleteAttributeAsync(attributeID);
        #endregion
    }
}