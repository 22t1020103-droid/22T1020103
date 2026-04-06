using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SV22T1020103.BusinessLayers;
using SV22T1020103.Models.Catalog;

namespace SV22T1020103.Admin.Controllers
{
    /// <summary>
    /// Quản lý mặt hàng
    /// </summary>
    //[Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.DataManager}")]
    public class ProductController : Controller
    {
        private const int PAGESIZE = 10;
        private const string PRODUCT_SEARCH_INPUT = "ProductSearchInput";
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Giao diện danh sách mặt hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_INPUT);
            if (input == null)
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };

            return View(input);
        }

        /// <summary>
        /// Tìm kiếm mặt hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_INPUT, input);
            return View(result);
        }

        /// <summary>
        /// Xem chi tiết mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id)
        {
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.ProductID = id;
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            return View(model);
        }

        /// <summary>
        /// Giao diện bổ sung mặt hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            var model = new Product()
            {
                ProductID = 0,
                IsSelling = true,
                Price = 0
            };
            ViewBag.ProductID = 0;
            ViewBag.ProductAttributes = new List<ProductAttribute>();
            ViewBag.ProductPhotos = new List<ProductPhoto>();
            return View("Edit", model);
        }

        /// <summary>
        /// Giao diện cập nhật mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin mặt hàng";
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.ProductID = id;
            ViewBag.ProductAttributes = await CatalogDataService.ListAttributesAsync(id);
            ViewBag.ProductPhotos = await CatalogDataService.ListPhotosAsync(id);
            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uploadPhoto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto)
        {
            ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
            try
            {
                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName), "Vui lòng nhập tên mặt hàng");
                if (data.CategoryID is null or <= 0)
                    ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
                if (data.SupplierID is null or <= 0)
                    ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");
                if (string.IsNullOrWhiteSpace(data.Unit))
                    ModelState.AddModelError(nameof(data.Unit), "Vui lòng nhập đơn vị tính");
                if (data.Price < 0)
                    ModelState.AddModelError(nameof(data.Price), "Giá bán không hợp lệ");

                if (string.IsNullOrEmpty(data.ProductDescription))
                    data.ProductDescription = "";

                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, "images", "products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }
                else if (data.ProductID > 0 && string.IsNullOrWhiteSpace(data.Photo))
                {
                    var oldData = await CatalogDataService.GetProductAsync(data.ProductID);
                    data.Photo = oldData?.Photo;
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.ProductID = data.ProductID;
                    ViewBag.ProductAttributes = data.ProductID > 0 ? await CatalogDataService.ListAttributesAsync(data.ProductID) : new List<ProductAttribute>();
                    ViewBag.ProductPhotos = data.ProductID > 0 ? await CatalogDataService.ListPhotosAsync(data.ProductID) : new List<ProductPhoto>();
                    return View("Edit", data);
                }

                if (data.ProductID == 0)
                    await CatalogDataService.AddProductAsync(data);
                else
                    await CatalogDataService.UpdateProductAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống đang bận, Vui lòng thử lại sau!");
                ViewBag.ProductID = data.ProductID;
                ViewBag.ProductAttributes = data.ProductID > 0 ? await CatalogDataService.ListAttributesAsync(data.ProductID) : new List<ProductAttribute>();
                ViewBag.ProductPhotos = data.ProductID > 0 ? await CatalogDataService.ListPhotosAsync(data.ProductID) : new List<ProductPhoto>();
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteProductAsync(id);
                return RedirectToAction("Index");
            }

            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await CatalogDataService.IsUsedProductAsync(id));
            return View(model);
        }

        /// <summary>
        /// Danh sách thuộc tính mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ListAttributes(int id)
        {
            var data = await CatalogDataService.ListAttributesAsync(id);
            ViewBag.ProductID = id;
            return View(data);
        }

        /// <summary>
        /// Giao diện bổ sung thuộc tính
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CreateAttribute(int id)
        {
            if (id <= 0)
            {
                TempData["Message"] = "Vui lòng lưu mặt hàng trước khi thêm thuộc tính.";
                return RedirectToAction("Create");
            }
            var model = new ProductAttribute()
            {
                ProductID = id,
                DisplayOrder = 1
            };
            return View("EditAttribute", model);
        }

        /// <summary>
        /// Giao diện cập nhật thuộc tính
        /// </summary>
        /// <param name="id"></param>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditAttribute(int id, long attributeId)
        {
            var model = await CatalogDataService.GetAttributeAsync(attributeId);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View("EditAttribute", model);
        }

        /// <summary>
        /// Lưu thuộc tính mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Vui lòng nhập tên thuộc tính");
            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Vui lòng nhập giá trị thuộc tính");
            if (data.DisplayOrder < 1)
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");
            if (data.ProductID <= 0)
                ModelState.AddModelError(nameof(data.ProductID), "Mã mặt hàng không hợp lệ.");

            if (!ModelState.IsValid)
                return View("EditAttribute", data);

            try
            {
                if (data.AttributeID == 0)
                    await CatalogDataService.AddAttributeAsync(data);
                else
                    await CatalogDataService.UpdateAttributeAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch (SqlException)
            {
                ModelState.AddModelError("Error", "Không lưu được thuộc tính.");
                return View("EditAttribute", data);
            }
        }

        /// <summary>
        /// Xóa thuộc tính mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteAttribute(int id, long attributeId)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteAttributeAsync(attributeId);
                return RedirectToAction("Edit", new { id });
            }

            var model = await CatalogDataService.GetAttributeAsync(attributeId);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View("DeleteListAttributes", model);
        }

        /// <summary>
        /// Danh sách ảnh của mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ListPhotos(int id)
        {
            var data = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.ProductID = id;
            return View(data);
        }

        /// <summary>
        /// Giao diện bổ sung ảnh
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CreatePhoto(int id)
        {
            if (id <= 0)
            {
                TempData["Message"] = "Vui lòng lưu mặt hàng trước khi thêm ảnh.";
                return RedirectToAction("Create");
            }
            var model = new ProductPhoto()
            {
                ProductID = id,
                DisplayOrder = 1,
                Description = string.Empty
            };
            return View("EditPhoto", model);
        }

        /// <summary>
        /// Giao diện cập nhật ảnh
        /// </summary>
        /// <param name="id"></param>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditPhoto(int id, int photoId)
        {
            var model = await CatalogDataService.GetPhotoAsync(photoId);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View("EditPhoto", model);
        }

        /// <summary>
        /// Lưu ảnh của mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uploadPhoto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            if (uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, "images", "products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            data.Description = string.IsNullOrWhiteSpace(data.Description) ? string.Empty : data.Description.Trim();

            if (string.IsNullOrWhiteSpace(data.Photo))
                ModelState.AddModelError(nameof(data.Photo), "Vui lòng chọn ảnh cho mặt hàng");
            if (data.DisplayOrder < 1)
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");

            if (!ModelState.IsValid)
                return View("EditPhoto", data);

            try
            {
                if (data.PhotoID == 0)
                    await CatalogDataService.AddPhotoAsync(data);
                else
                    await CatalogDataService.UpdatePhotoAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch (SqlException)
            {
                ModelState.AddModelError("Error", "Không lưu được ảnh vào CSDL.");
                return View("EditPhoto", data);
            }
        }

        /// <summary>
        /// Xóa ảnh mặt hàng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeletePhoto(int id, int photoId)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeletePhotoAsync(photoId);
                return RedirectToAction("Edit", new { id });
            }

            var model = await CatalogDataService.GetPhotoAsync(photoId);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View(model);
        }
    }
}