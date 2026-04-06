using SV22T1020103.BusinessLayers;
using SV22T1020103.DataLayers.Interfaces;
using SV22T1020103.DataLayers.SQLServer;
using SV22T1020103.Models.DataDictionary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1020103.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến từ điển dữ liệu
    /// </summary>
    public static class DictionaryDataService
    {
        // Bỏ 'readonly' và dùng kiểu nullable để kiểm soát việc khởi tạo
        private static IDataDictionaryRepository<Province>? provinceDB;

        /// <summary>
        /// Khởi tạo Service (Phải được gọi từ Program.cs)
        /// </summary>
        public static void Initialize(string connectionString)
        {
            provinceDB = new ProvinceRepository(connectionString);
        }

        /// <summary>
        /// Kiểm tra xem service đã được khởi tạo chưa
        /// </summary>
        private static void CheckInitialized()
        {
            if (provinceDB == null)
                throw new InvalidOperationException("DictionaryDataService chưa được khởi tạo chuỗi kết nối.");
        }

        /// <summary>
        /// Lấy danh sách tỉnh thành
        /// </summary>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            CheckInitialized();
            return await provinceDB!.ListAsync();
        }
    }
}