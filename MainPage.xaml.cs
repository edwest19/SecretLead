using Microsoft.UI.Xaml.Controls;
using SecretLead.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecretLead;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    public MainPage()
    {
        InitializeComponent();

        // BrowsePage is the app's home content (README.md §6, §7) — shown
        // by default rather than an empty content frame.
        ContentFrame.Navigate(typeof(BrowsePage));
    }

    // Frame navigation is a view concern, so these stay in code-behind
    // rather than routed through a ViewModel command.
    private void ImportButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ImportPage));
    }

    private void BrowseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(BrowsePage));
    }
}
