# Fody Weaving Example for .NET Projects

Welcome to the GitHub repository for showcasing the implementation of Fody for Aspect-Oriented Programming (AOP) in .NET projects. This sample project provides a practical demonstration of using Fody to inject caching logic into a .NET application, ensuring core business logic remains clean and maintainable.

## Overview

This repository contains a sample implementation of Fody, a .NET weaver that facilitates AOP by weaving custom aspects, such as caching, into your codebase. This example demonstrates how to automatically add caching functionality to methods using Fody, without cluttering the core business logic.

## Use Case

Imagine a scenario where you want to implement caching across multiple methods to optimize performance. Traditionally, you would need to add repetitive caching logic to each method. With Fody weaving, you can define this caching logic once and apply it across your methods using a simple attribute.

### Key Concepts

- **Aspect-Oriented Programming (AOP):** A programming paradigm that aims to increase modularity by allowing the separation of cross-cutting concerns.
- **Fody Weaving:** Fody is a .NET library that allows you to inject custom aspects into your code at compile-time, automating the incorporation of cross-cutting concerns.

## Implementation Details

This example centers around annotating methods with a custom `CacheAttribute` to automatically apply caching logic during compilation.

### Step-by-Step Guide

1. **Define the Cache Attribute:**
    ```csharp
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheAttribute : Attribute
    {
        public CacheAttribute() { }
    }
    ```

2. **Implement Cache Manager:**
    ```csharp
    internal static class CacheManager
    {
        static Dictionary<string, object?> _cache = new();

        private static string GetKey(string declaringType, string methodName, params object[] parameters)
        {
            var values = (new[] { declaringType, methodName })
                         .Concat(parameters.Select(obj => obj ?? string.Empty).ToString());
            return string.Join("/", values);
        }

        internal static object? GetCacheValue(string declaringType, string methodName, params object[] parameters)
        {
            var key = GetKey(declaringType, methodName, parameters);
            Console.WriteLine(_cache.ContainsKey(key) ? $"[Cache hit for key: {key}]" : $"[No cached value for key: {key}]");
            return _cache.GetValueOrDefault(key);
        }

        internal static void SetCacheValue(object value, string declaringType, string methodName, params object[] parameters)
        {
            var key = GetKey(declaringType, methodName, parameters);
            _cache[key] = value;
            Console.WriteLine($"[Cache set for key: {key}]");
        }
    }
    ```

3. **Implement the Weaver:**
    ```csharp
    public class ModuleWeaver : BaseModuleWeaver
    {
        private const string CacheAttributeName = "CacheAttribute";

        public override void Execute()
        {
            var methods = ModuleDefinition.Types
                .SelectMany(t => t.Methods)
                .Where(m => m.CustomAttributes.Any(attr => attr.AttributeType.Name == CacheAttributeName));

            foreach (var method in methods)
            {
                if (!method.HasBody || method.ReturnType.FullName == typeof(void).FullName) continue;

                InjectCache(method);
            }
        }

        private void InjectCache(MethodDefinition method)
        {
            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            // Insert the caching logic
            foreach (var returnInstr in instructions.Where(i => i.OpCode == OpCodes.Ret).ToList())
            {
                processor.InsertInstructions(returnInstr, SetCacheValue(method));
            }

            var firstInstr = instructions.First();
            processor.InsertInstructions(firstInstr, ReturnGetValueCacheIfAny(method));
        }

        // Helper methods to add instructions for caching
    }
    ```

4. **Apply Cache Attribute in Your Business Logic:**
    ```csharp
    public class TestClass
    {
        [Cache]
        public string TestMethod1(string someThing, int otherThings)
        {
            return otherThings <= 1 ? $"Just do this {someThing}" : $"you should {someThing} and {otherThings} things else";
        }

        [Cache]
        public string TestMethod2(string someThing, int otherThings)
        {
            return otherThings <= 1 ? $"Just do this {someThing}" : $"you should {someThing} and {otherThings} things else";
        }
    }
    ```

5. **Run the Sample Application:**
    ```csharp
    var testClass = new TestClass();

    var result1 = testClass.TestMethod1("do this", 1);
    Console.WriteLine(result1);

    var result2 = testClass.TestMethod1("do this", 1);
    Console.WriteLine(result2);

    var result3 = testClass.TestMethod2("do this", 1);
    Console.WriteLine(result3);

    var result4 = testClass.TestMethod2("do this", 1);
    Console.WriteLine(result4);
    ```

## Conclusion

This Fody Weaving example illustrates the practical implementation of AOP for injecting caching logic in a .NET application. By using Fody, you can modularize and simplify the management of cross-cutting concerns, allowing your core business logic to remain clean and focused.

Feel free to explore the repository and adapt the implementation to suit your specific needs. The provided sample demonstrates how you can leverage Fody to enhance the maintainability and modularity of your .NET projects.

## Resources
- [Fody Documentation](https://github.com/Fody/Home)
