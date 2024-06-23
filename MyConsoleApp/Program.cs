// See https://aka.ms/new-console-template for more information

using MyConsoleApp;

var testClass = new TestClass();

var methodResult0 = testClass.TestMethod("do this",1);
Console.WriteLine(methodResult0);
var methodResult1 = testClass.TestMethod("do this", 1);
Console.WriteLine(methodResult1);

Console.ReadKey();

