using JPDFs.Controls;

namespace JPDFs.Shell;

public partial class MainWindow
{
    private void OpenCertificateCatalog()
    {
        var dialog = new CertificateCatalogDialog(this);
        dialog.ShowDialog();
        RefreshSidebarPanel();
    }
}
