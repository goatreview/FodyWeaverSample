namespace MyConsoleApp
{
    internal static class CacheManager
    {
        static Dictionary<string, object?> _cache = new();

        private static string GetKey(params object[] parameters)
        {
            var values = parameters.Select(obj => obj ?? string.Empty)
                .Select(obj => (obj ?? string.Empty).ToString());

            var key = string.Join("/", values);
            return key;
        }
        
        internal static object? GetCacheValue(params object[] parameters)
        {
            var key = GetKey(parameters);
            if (_cache.ContainsKey(key))
            {
                Console.WriteLine($"Cache hit for key: {key}");
            }
            var value = _cache.GetValueOrDefault(GetKey(key));
            return value;
        }

        internal static void SetCacheValue(object value, params object[] parameters)
        {
            var key = GetKey(parameters);
            _cache[key] = value;
            Console.WriteLine($"Cache set for key: {key}");
        }

        internal static void CaptureMethodResult(object value)
        {
            Console.WriteLine($"Captured value: {value}");
        }

        
        internal static void TestCall(Type type, string methodName)
        {
            return;
        }
    }
}
