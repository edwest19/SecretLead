using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SecretLead.Services;
using SecretLead.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecretLead;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The main application window. Use <c>App.Window</c> from any class that needs
    /// the window reference (for dialogs, pickers, interop, etc.).
    /// </summary>
    public static Window Window { get; private set; } = null!;

    /// <summary>
    /// The UI thread dispatcher. Use <c>App.DispatcherQueue</c> to marshal calls
    /// to the UI thread. Fully qualified to avoid CS0104 ambiguity with
    /// <see cref="Windows.System.DispatcherQueue"/>.
    /// </summary>
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;

    /// <summary>
    /// The native window handle (HWND). Use for file pickers,
    /// <c>DataTransferManager</c>, and any WinRT interop that requires
    /// <c>InitializeWithWindow</c>.
    /// </summary>
    public static nint WindowHandle =>
        WinRT.Interop.WindowNative.GetWindowHandle(Window);

    /// <summary>
    /// Shared status/validation messages shown in the nav rail's Status
    /// flyout (README.md §3). Use <c>App.Status</c> from any page that
    /// needs to report validation results or errors.
    /// </summary>
    public static StatusService Status { get; } = new();

    /// <summary>
    /// AI-generated (Claude): the one record store, used by every page. It lives in SecretLead's
    /// own MSIX package data folder (<c>ApplicationData.Current.LocalFolder</c>, which is
    /// <c>%LOCALAPPDATA%\Packages\&lt;PackageFamilyName&gt;\LocalState</c>), so Windows keeps it
    /// private to SecretLead and deletes it when SecretLead is uninstalled (README.md §5).
    /// </summary>
    public static EntityStore Store => _store ??= new EntityStore(Windows.Storage.ApplicationData.Current.LocalFolder.Path);

    private static EntityStore? _store;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // AI-generated (Claude): on first run, with no entities in AppData yet, add the bundled
        // Construction demo before the browse screen lists entities (README.md §6, §11).
        try
        {
            ConstructionDemo.SeedIfEmpty(Store);
        }
        catch (Exception ex)
        {
            Status.SetMessages([$"The Construction demo couldn't be added: {ex.Message}"]);
        }

        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();
    }
}
