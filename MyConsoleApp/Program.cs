// See https://aka.ms/new-console-template for more information

using MyConsoleApp;

var testClass = new TestClass();

var methodResult0 = testClass.TestMethod1("do this",1);
Console.WriteLine(methodResult0);
var methodResult1 = testClass.TestMethod1("do this", 1);
Console.WriteLine(methodResult1);
var methodResult3 = testClass.TestMethod2("do this", 1);
Console.WriteLine(methodResult3);
var methodResult4 = testClass.TestMethod2("do this", 1);
Console.WriteLine(methodResult4);

Console.ReadKey();

