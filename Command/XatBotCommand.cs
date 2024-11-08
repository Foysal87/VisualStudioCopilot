using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI.Chat;
using XatBotVisualStudioCopilot.Services;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json.Linq;

namespace XatBotVisualStudioCopilot.Command
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class XatBotCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        public const int CompleteId = 0x0101;
        public const int FindBugsId = 0x0102;
        public const int ExplainId = 0x0103;
        public const int OptimizeId = 0x0104;
        public const int RefactorId = 0x0105;
        public const int AddCommentId = 0x0106;
        public const int AddSummaryId = 0x0107;
        public const int AddTests = 0x0108;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("fbfec63b-b25f-492f-a605-d1b264cb82d7");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="XatBotCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private XatBotCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            AddCompleteAction(commandService);
        }

        private void AddCompleteAction(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(CommandSet, CompleteId);
            var completeAction = new CompleteAction();
            var menuItem = new MenuCommand(async (s, e) => await completeAction.ExecuteAsync(this.package, s, e), menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static XatBotCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in XatBotCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new XatBotCommand(package, commandService);
        }
    }
}
