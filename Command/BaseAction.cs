using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XatBotVisualStudioCopilot.Command;

namespace XatBotVisualStudioCopilot.Command
{
    public abstract class BaseAction
    {
        public async Task ExecuteAsync(AsyncPackage package, object sender, EventArgs e)
        {
            VsShellUtilities.ShowMessageBox(
                package,
                $"gg",
                "Error",
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            return;


            var aiRequest = GetAiRequest(sender, e);
            if (aiRequest != null)
            {
                await DoExecuteAsync(aiRequest, e);
            }
        }
        public abstract Task DoExecuteAsync(AiRequest aiRequest, EventArgs e);

        public abstract AiRequest GetAiRequest(object sender, EventArgs e);
    }
}

public class CompleteAction : BaseAction
{
    public override Task DoExecuteAsync(AiRequest aiRequest, EventArgs e)
    {
        throw new NotImplementedException();
    }

    public override AiRequest GetAiRequest(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}
