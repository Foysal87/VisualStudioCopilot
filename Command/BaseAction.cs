using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetCopilot.Models;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using OpenAI.Chat;
using XatBotVisualStudioCopilot.Models;
using XatBotVisualStudioCopilot.Services;

namespace XatBotVisualStudioCopilot.Command
{
    public abstract class BaseAction
    {
        public async Task ExecuteAsync(AsyncPackage package, object sender, EventArgs e)
        {
            try
            {
                var metaData = await GetSelectedMetaData(package, sender, e);
                if (string.IsNullOrEmpty(metaData.SelectionText))
                {
                    throw new Exception("No Selected Text Found");
                }
                var aiRequest = await GetAiRequest(metaData);
                if (aiRequest != null)
                {
                    var chatMessages =
                        XatBotAIService.Instance.GetChatMessages(aiRequest.SystemMessage, aiRequest.PromptContents);
                    var response = await XatBotAIService.Instance.ChatAsync(chatMessages);
                    var content = response.Text;
                    if (string.IsNullOrEmpty(content) || string.Equals(content, "\n") || string.Equals(content, "\r"))
                    {
                        return;
                    }

                    await DoAfterExecution(metaData, content);
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    $"{ex.Message}",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                return;
            }

        }

        private async Task DoAfterExecution(SelectionMetaData metaData, string content)
        {
            if (GetCommandType() == SelectionCommandType.InsertAfter)
            {
                if (metaData.Selection != null)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    metaData.Selection.MoveToPoint(metaData.TopPoint);
                    metaData.Selection.MoveToPoint(metaData.BottomPoint,true);
                    var selectedC = metaData.Selection.Text;

                    //metaData.Selection.Delete();
                    metaData.Selection.Insert(content);
                }
            }
        }

        public abstract Task<AiRequest> GetAiRequest(SelectionMetaData selectionMetaData);
        public abstract string GetType();
        public abstract string GetCommandType();
        public async Task<string> ReadFileContentAsync(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync();
            }
        }

        protected async Task<SelectionMetaData> GetSelectedMetaData(AsyncPackage package, object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            if (dte == null)
            {
                throw new Exception("DTE is null");
            }
            // Get the selected file in Solution Explorer
            string selectedFilePath = null;
            UIHierarchy solutionExplorer = dte.ToolWindows.SolutionExplorer;
            Array selectedItems = (Array)solutionExplorer.SelectedItems;

            if (selectedItems != null && selectedItems.Length > 0)
            {
                UIHierarchyItem hierarchyItem = (UIHierarchyItem)selectedItems.GetValue(0);
                ProjectItem projectItem = hierarchyItem.Object as ProjectItem;

                if (projectItem != null)
                {
                    // Get the full path of the selected item
                    selectedFilePath = projectItem.FileNames[1];
                }
            }
            var metaData = new SelectionMetaData();
            // Get the cursor position and all text above it in the active document
            string textAboveCursor = null;
            int cursorLine = 0, cursorColumn = 0;
            Document activeDocument = dte.ActiveDocument;

            var selectedText = "";
            TextSelection selection = null;
            if (activeDocument != null)
            {
                selection = (TextSelection)activeDocument.Selection;
                selectedText = selection.Text;
                metaData.PositionStart = selection.TopLine;
                metaData.PositionEnd = selection.BottomLine;
                metaData.SelectionText = selectedText;
                metaData.CursorLine = selection.CurrentLine;
                metaData.CursorColumn = selection.CurrentColumn;
                metaData.TopPoint = selection.TopPoint;
                metaData.BottomPoint = selection.BottomPoint;
                metaData.Selection = selection;
                // Get the cursor line and column position
                cursorLine = selection.CurrentLine;
                cursorColumn = selection.CurrentColumn;

                // Move the selection to the start of the document
                selection.StartOfDocument(false);

                // Select text up to the current cursor position
                selection.MoveToLineAndOffset(cursorLine, cursorColumn, true);
                textAboveCursor = selection.Text;
                metaData.SelectionAboveCursorContent = textAboveCursor;

                // Deselect text and move the cursor back to its original position
                selection.MoveToLineAndOffset(cursorLine, cursorColumn, false);
            }

            var selectedFileContent = await ReadFileContentAsync(selectedFilePath);
            metaData.SelectedFileContent = selectedFileContent;
            return metaData;


            /*
            var chatMessages = GetPromptContents(selectedFileContent, textAboveCursor, selectedText);

            var response = await XatBotAIService.Instance.ChatAsync(chatMessages);
            var (replacedSelectedText, generatedCode) = GetTextAndCode(response.Text);


            generatedCode = selectedFileContent.Replace(replacedSelectedText, generatedCode);

            if (selection != null)
            {
                selection.Delete();
                // Insert the new text at the cursor position with a newline
                selection.Insert(Environment.NewLine + generatedCode);
            }
            */
        }

        protected abstract List<PromptContent> GetPromptContents(SelectionMetaData selectionMetaData);
    }
}