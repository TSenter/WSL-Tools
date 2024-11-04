using System.Windows.Controls;

namespace Flow.Launcher.Plugin.WSLTools.UI
{
  public partial class SettingsView : UserControl
  {
    private readonly SettingsViewModel viewModel;
    public SettingsView(SettingsViewModel viewModel)
    {
      DataContext = this.viewModel = viewModel;
      InitializeComponent();
    }
  }
}
