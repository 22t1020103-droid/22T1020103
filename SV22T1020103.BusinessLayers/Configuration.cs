namespace SV22T1020103.BusinessLayers
{
    public static class Configuration
    {
        private static string _connectionString = "";

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
            Console.WriteLine(">>> INIT ConnectionString: " + _connectionString);
        }

        public static string ConnectionString => _connectionString;
    }
}