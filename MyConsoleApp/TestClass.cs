namespace MyConsoleApp
{
    public class TestClass
    {
        [Cache]
        public string TestMethod1(string someThing, int otherThings)
        {
            
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

        [Cache]
        public string TestMethod2(string someThing, int otherThings)
        {

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
