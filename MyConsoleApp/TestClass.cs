namespace MyConsoleApp
{
    public class TestClass
    {
        [Cache]
        public string TestMethod(string someThing, int otherThings)
        {
            CacheManager.TestCall(typeof(TestClass), nameof(TestMethod));
            if (otherThings <= 0)
            {
                return $"Just do this {someThing}";
            }

            if (otherThings == 1)
            {
                return $"you should {someThing} and one thing else";
            }

            return $"you should {someThing} and {otherThings} things else";
        }
    }
}
