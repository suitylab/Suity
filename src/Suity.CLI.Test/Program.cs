using Suity.Editor;

Console.WriteLine("CLI Running Test");

using var runner = new CliRunner(@"D:\Core\Suity\src\Suity.CLI\bin\Debug\net10.0\Suity.CLI.exe");
var resp = await runner.StartAsync(@"open D:\Core\Test\TestCoding025");

string type = resp.Node("@type").ReadString();
string text = resp.Node("text").ReadString();
Console.WriteLine(type);
Console.WriteLine(text);

Console.ReadLine();