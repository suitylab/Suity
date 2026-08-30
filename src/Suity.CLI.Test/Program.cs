using Suity.Editor;

const string CliPath = @"D:\Core\Suity\src\Suity.CLI\bin\Debug\net10.0\Suity.CLI.exe";
const string Args = @"new D:\Core\Test\TestCoding026 -template D:\Core\Test\TestPack.suitypackage";

Console.WriteLine("CLI Running Test");

using var runner = new CliRunner(CliPath);
runner.NotificationMessage += (msg) =>
{
    Console.WriteLine($"[Notify Message] {msg}");
};

try
{
    var resp = await runner.StartAsync(Args);
    Console.WriteLine(resp.ToString());

    var respList = await runner.SendCommandAsync("list-startup") as string[];
    if (respList != null)
    {
        foreach (var s in respList)
        {
            Console.WriteLine(s);
        }
    }

    resp = await runner.SendCommandAsync("test-notify");
    Console.WriteLine(resp.ToString());

    runner.Dispose();

    Console.ReadLine();
}
finally
{
    runner.Dispose();
}