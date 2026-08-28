using Suity.Editor;

Console.WriteLine("CLI Running Test");

using var runner = new CliRunner(@"D:\Core\Suity\src\Suity.CLI\bin\Debug\net10.0\Suity.CLI.exe");
var resp = await runner.StartAsync(@"open D:\Core\Test\TestCoding025");

string type = resp.Node("@type").ReadString();
string text = resp.Node("text").ReadString();
Console.WriteLine(type);
Console.WriteLine(text);

var respList = await runner.SendCommandAsync("list-chat");
foreach (var subReader in respList.Nodes("Strings"))
{
    string s = subReader.ReadString();
    Console.WriteLine(s);
}

Console.ReadLine();