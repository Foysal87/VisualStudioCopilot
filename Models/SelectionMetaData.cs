using EnvDTE;

namespace XatBotVisualStudioCopilot.Models
{
    public class SelectionMetaData
    {
        public int CursorLine { get; set; }
        public string SelectionText { get; set; }
        public string SelectionFilePath { get; set; }
        public string SelectedFileContent { get; set; }
        public string SelectionAboveCursorContent { get; set; }
        public string SelectionDownCursorContent { get; set; }
        public int PositionStart { get; set; }
        public int PositionEnd { get; set; }
        public int Position { get; set; }
        public int CursorColumn { get; set; }
        public TextSelection Selection { get; set; }
        public VirtualPoint TopPoint { get; set; }
        public VirtualPoint BottomPoint { get; set; }
    }
}