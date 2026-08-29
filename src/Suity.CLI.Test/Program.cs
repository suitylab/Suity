using Suity.Editor;

const string CliPath = @"D:\Core\Suity\src\Suity.CLI\bin\Debug\net10.0\Suity.CLI.exe";
const string Args = @"open D:\Core\Test\TestCoding025";

Console.WriteLine("CLI Running Test");

using var runner = new CliRunner(CliPath);
try
{
    var resp = await runner.StartAsync(Args);
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