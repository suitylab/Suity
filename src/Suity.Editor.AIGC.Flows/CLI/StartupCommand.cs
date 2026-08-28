using System;

namespace Suity.Editor.CLI;

[CliCommandKey("startup")]
public class StartupCommand : CliCommand
{
    public override string Description => "Start a new chat";

    public override string Usage => "startup <user-input>";

    public override void DoCommand(ICliArguments args)
    {
        throw new NotImplementedException();
    }
}
