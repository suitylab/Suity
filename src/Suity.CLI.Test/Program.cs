using Suity.Editor;

Console.WriteLine("CLI Running Test");

var runner = new CliRunner(@"D:\Core\Suity\src\Suity.CLI\bin\Debug\net10.0\Suity.CLI.exe");
runner.Start(@"open D:\Core\Test\TestCoding025", resp =>
{
    string type = resp.Node("@type").ReadString();
    string text = resp.Node("text").ReadString();
    Console.WriteLine(type);
    Console.WriteLine(text);
});

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();

runner.Dispose();