namespace MyConsoleApp
{
    internal static class CacheManager
    {
        static Dictionary<string, object?> _cache = new();

        private static string GetKey(
            string declaringType,
            string methodName, params object[] parameters)
        {
            var values =
                (new[] { declaringType, methodName })
                .Concat(
                    parameters.Select(obj => obj ?? string.Empty)
                        .Select(obj => (obj ?? string.Empty).ToString()));

            var key = string.Join("/", values);
            return key;
        }
        
        internal static object? GetCacheValue(
            string declaringType,
            string methodName,
            params object[] parameters)
        {
            
            var key = GetKey(declaringType, methodName,parameters);
            if (_cache.ContainsKey(key))
            {
                Console.WriteLine($"[Cache hit :) for key: {key}]");
            }
            else
            {
                Console.WriteLine($"[No cached value :( for key: {key} ]");
            }
            var value = _cache.GetValueOrDefault(key);
            
            return value;
        }

        internal static void SetCacheValue(object value,
            string declaringType, string methodName,
            params object[] parameters)
        {
            var key = GetKey(declaringType, methodName, parameters);
            _cache[key] = value;
            Console.WriteLine($"[Cache set for key: {key}]");
        }

        internal static void CaptureMethodResult(object value)
        {
            Console.WriteLine($"Captured value: {value}");
        }

    }
}
