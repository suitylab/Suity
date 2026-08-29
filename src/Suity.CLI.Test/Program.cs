using Suity.Editor;

Console.WriteLine("CLI Running Test");

using var runner = new CliRunner(@"D:\Core\Suity\src\Suity.CLI\bin\Debug\net10.0\Suity.CLI.exe");

try
{
    var resp = await runner.StartAsync(@"open D:\Core\Test\TestCoding025");
    Console.WriteLine(resp.ToString());

    var respList = await runner.SendCommandAsync("list-chat") as string[];
    if (respList != null)
    {
        foreach (var s in respList)
        {
            Console.WriteLine(s);
        }
    }

    Console.ReadLine();
}
finally
{
    runner.Dispose();
}