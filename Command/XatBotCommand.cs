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
        public const int CompleteId = 0x0200;
        public const int FindBugsId = 0x0300;
        public const int ExplainId = 0x0400;
        public const int OptimizeId = 0x0500;
        public const int RefactorId = 0x0600;
        public const int AddCommentId = 0x0700;
        public const int AddSummaryId = 0x0800;

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

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(async (s, e) => await ExecuteAsync(s, e), menuCommandID);
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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async Task ExecuteAsync(object sender, EventArgs e)
        {
            try
            {
                DTE2 dte = (DTE2) Package.GetGlobalService(typeof(DTE));
                if (dte == null)
                {
                    return;
                }

                // Get the selected file in Solution Explorer
                string selectedFilePath = null;
                UIHierarchy solutionExplorer = dte.ToolWindows.SolutionExplorer;
                Array selectedItems = (Array) solutionExplorer.SelectedItems;

                if (selectedItems != null && selectedItems.Length > 0)
                {
                    UIHierarchyItem hierarchyItem = (UIHierarchyItem) selectedItems.GetValue(0);
                    ProjectItem projectItem = hierarchyItem.Object as ProjectItem;

                    if (projectItem != null)
                    {
                        // Get the full path of the selected item
                        selectedFilePath = projectItem.FileNames[1];
                    }
                }

                // Get the cursor position and all text above it in the active document
                string textAboveCursor = null;
                int cursorLine = 0, cursorColumn = 0;
                Document activeDocument = dte.ActiveDocument;

                var selectedText = "";
                TextSelection selection = null;
                if (activeDocument != null)
                {
                    selection = (TextSelection) activeDocument.Selection;
                    selectedText = selection.Text;
                    // Get the cursor line and column position
                    cursorLine = selection.CurrentLine;
                    cursorColumn = selection.CurrentColumn;

                    // Move the selection to the start of the document
                    selection.StartOfDocument(false);

                    // Select text up to the current cursor position
                    selection.MoveToLineAndOffset(cursorLine, cursorColumn, true);
                    textAboveCursor = selection.Text;

                    // Deselect text and move the cursor back to its original position
                    selection.MoveToLineAndOffset(cursorLine, cursorColumn, false);
                }
                var selectedFileContent = SelectedFileContent(selectedFilePath);
                var chatMessages = GetChatMessages(selectedFileContent, textAboveCursor, selectedText);

                var response = await XatBotAIService.Instance.ChatAsync(chatMessages);
                var (replacedSelectedText, generatedCode) = GetTextAndCode(response.Text);

                

                generatedCode = selectedFileContent.Replace(replacedSelectedText, generatedCode);

                if (selection != null)
                {

                    selection.Delete();
                    // Insert the new text at the cursor position with a newline
                    selection.Insert(Environment.NewLine + generatedCode);

                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"{ex.Message}",
                    "Error",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            //ThreadHelper.ThrowIfNotOnUIThread();

            // Get the DTE service (Development Tools Environment)
           
        }

        private List<ChatMessage> GetChatMessages(string selectedFileContent, string textAboveCursor, string selectedText)
        {
            var chatMessages = new List<ChatMessage>();
            var stringBuilder = new StringBuilder();
            var allContents = ContentService.GetAllContents();

            stringBuilder.AppendLine("[Platform Content]:");
            stringBuilder.AppendLine($"{allContents}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"[Full File code] :");
            stringBuilder.AppendLine($"{selectedFileContent}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("[Upper part of the code, where currently user cursor is located] : ");
            stringBuilder.AppendLine($"{textAboveCursor}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"[Selected Text]:");
            stringBuilder.AppendLine($"{selectedText}");
            stringBuilder.AppendLine();

            var systemMessage = SystemMessageService.GetSystemMessage();
            chatMessages.Add(new SystemChatMessage(systemMessage));
            chatMessages.Add(new UserChatMessage(stringBuilder.ToString()));

            return chatMessages;


        }

        private static string SelectedFileContent(string selectedFilePath)
        {
            var selectedFileContent = string.Empty;
            if (string.IsNullOrEmpty(selectedFilePath) == false) selectedFileContent = File.ReadAllText(selectedFilePath);
            return selectedFileContent;
        }

        public static (string SelectedText, string GeneratedCode) GetTextAndCode(string jsonString)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<Dictionary<string,string> >(jsonString);
                return (obj["SelectedText"], obj["GeneratedCode"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing JSON: " + ex.Message);
                return (string.Empty, string.Empty);
            }
        }
    }
}
