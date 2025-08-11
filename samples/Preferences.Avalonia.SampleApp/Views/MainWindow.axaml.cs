using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Preferences.Avalonia.Models;
using Preferences.Avalonia.SampleApp.ViewModels;
using Preferences.Avalonia.ViewModels;
using Preferences.Avalonia.Views;
using ReactiveUI;

namespace Preferences.Avalonia.SampleApp.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(action => 
            action(ViewModel!.ShowPreferencesDialog.RegisterHandler(DoShowDialogAsync)));
    }

    private async Task DoShowDialogAsync(IInteractionContext<PreferencesViewModel, PreferencesOptions> interaction)
    {
        var dialog = new PreferencesView
        {
            DataContext = interaction.Input
        };

        _ = await dialog.ShowDialog<PreferencesOptions>(this);
        interaction.SetOutput(interaction.Input.Options);
    }
}
