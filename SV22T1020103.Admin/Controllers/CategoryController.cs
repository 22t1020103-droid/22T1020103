using Microsoft.AspNetCore.Mvc;
using SV22T1020103.BusinessLayers;
using SV22T1020103.Models.Common;
using SV22T1020103.Models.Catalog;

namespace SV22T1020103.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string SESSION_INPUT = "CategorySearchInput";

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SESSION_INPUT);
            input ??= new PaginationSearchInput { Page = 1, PageSize = PAGE_SIZE, SearchValue = "" };
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput { Page = 1, PageSize = PAGE_SIZE, SearchValue = "" };

            var result = await CatalogDataService.ListCategoriesAsync(input);
            ApplicationContext.SetSessionData(SESSION_INPUT, input);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(data.CategoryName), "Tên không được để trống");

            if (!ModelState.IsValid) return View("Edit", data);

            if (data.CategoryID == 0) await CatalogDataService.AddCategoryAsync(data);
            else await CatalogDataService.UpdateCategoryAsync(data);

            return RedirectToAction("Index");
        }
    }
}