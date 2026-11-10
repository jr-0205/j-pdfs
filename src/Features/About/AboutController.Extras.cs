using System.Windows;

namespace JPDFs.Features.About
{
    /// <summary>
    /// APP-OWNED: JPDFs's side of the shared About core (Features/About/AboutController.cs).
    /// An update waits until every document is saved, and the open documents are recorded just
    /// before the installer or launcher swap starts, so the updated copy reopens them.
    /// </summary>
    internal sealed partial class AboutController
    {
        private IUpdateRestartHandoff? _restart;

        private IAboutUpdateRestart? Restart => _host as IAboutUpdateRestart;

        partial void HoldUpdate(ref bool hold)
        {
            if (Restart is not { IsDirty: true }) return;
            JpdfsDialog.Show(_host.Window, _host.Loc("Str_Dlg_SaveBeforeUpdate"),
                "J-PDF's", MessageBoxButton.OK, MessageBoxImage.Warning);
            hold = true;
        }

        partial void BeforeUpdateHandoff(ref bool proceed)
        {
            _restart = null;
            if (Restart is not { } restart || !_host.Window.IsLoaded) { proceed = false; return; }
            try { _restart = restart.PrepareUpdateRestart(); }
            catch { _restart = null; }
            if (_restart is not null) return;
            proceed = false;
            PrepareFailed();
        }

        partial void AfterUpdateHandoff(bool started)
        {
            var restart = _restart;
            _restart = null;
            if (restart is null) return;
            try
            {
                if (started) { restart.Started(); restart.Commit(); }
                else PrepareFailed();
            }
            catch { /* the session changed after the handoff began; the installer still closes the app */ }
            finally { restart.Dispose(); }
        }

        // An unsigned build has no thumbprint; the card says None rather than leaving a blank row.
        partial void ShownValue(ref string value)
        {
            if (string.IsNullOrEmpty(value)) value = _host.Loc("Str_Margin_None");
        }

        partial void UpdateNotVerified(System.Exception failure)
        {
            JPDFs.CrashReporter.Capture(failure, "Update download refused");
            JpdfsDialog.Show(_host.Window, _host.Loc("Str_About_UpdateNotVerified"),
                "J-PDF's", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void PrepareFailed()
            => JpdfsDialog.Show(_host.Window, _host.Loc("Str_UpdateRestartPrepareFailed"),
                "J-PDF's", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
