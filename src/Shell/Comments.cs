using System.Windows;
using JPDFs.Engine.Documents;
using JPDFs.Services;

namespace JPDFs.Shell
{
    // Comments (#288) from the Document tools menu: read, edit, answer and delete the PDF's own
    // review comments, and add notes to the current page.
    public partial class MainWindow
    {
        private void EditComments() => EditComments(addNew: false);

        private void EditComments(bool addNew)
        {
            if (_doc is null || string.IsNullOrWhiteSpace(_currentFile))
            {
                JpdfsDialog.Show(this, Loc("Str_Msg_OpenFirst"));
                return;
            }

            CommitActiveTextBox();
            IReadOnlyList<PdfCommentThread> threads;
            try { threads = PdfEngineIntegration.ReadCommentThreads(_currentFile, _protection); }
            catch (Exception ex)
            {
                JpdfsDialog.Show(this, Loc("Str_Comments_ReadFailed") + "\n" + ex.Message,
                    "J-PDF's", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string author = Environment.UserName;
            var dialog = new Controls.CommentsDialog(this, threads, _currentPage, author,
                page => ActiveViewer.NavigateToPageExt(page));
            if (addNew) dialog.AddComment();
            dialog.ShowDialog();
            if (!dialog.Saved) return;
            IReadOnlyList<CommentEdit> edits = dialog.Edits;
            if (edits.Count == 0 || !EnsureEditable()) return;
            try
            {
                UndoEntry? documentUndo = CaptureDocumentUndo();
                SaveTempAndReload(
                    keepAnnotations: true,
                    finalizeSavedFile: path => PdfEngineIntegration.ApplyCommentEdits(path, edits, author),
                    documentUndo: documentUndo);
                SetStatus(Loc("Str_Comments_Updated"));
            }
            catch (Exception ex)
            {
                JpdfsDialog.Show(this, Loc("Str_Comments_Failed") + "\n" + ex.Message,
                    "J-PDF's", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
