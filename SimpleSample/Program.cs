// See https://aka.ms/new-console-template for more information
using SimpleSample;

Console.WriteLine("Hello, World!");
var defServ = new ServiceProImpl();
var anotherServ = new AnotherServicePro();
Console.WriteLine(defServ.DoAnother());
Console.WriteLine(anotherServ.DoAnother());
