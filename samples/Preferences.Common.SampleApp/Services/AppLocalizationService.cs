using System.Globalization;
using System.Resources;

namespace Preferences.Common.SampleApp.Services;

public class AppLocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager = 
        new ResourceManager("Preferences.Avalonia.SampleApp.Resources.General", 
            typeof(AppLocalizationService).Assembly);
            
    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;
    
    public event EventHandler? LocaleChanged;

    public string GetLocalizedString(string resourceKey)
    {
        try
        {
            string? value = _resourceManager.GetString(resourceKey, _currentCulture);
            return !string.IsNullOrEmpty(value) ? value : resourceKey;
        }
        catch
        {
            return resourceKey;
        }
    }
    
    public void SetLocale(string cultureName)
    {
        try
        {
            var newCulture = new CultureInfo(cultureName);
            if (!_currentCulture.Equals(newCulture))
            {
                _currentCulture = newCulture;
                // Raise the event to notify subscribers
                LocaleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception)
        {
            // Handle invalid culture
        }
    }
    
    public void SetLocale(CultureInfo cultureInfo)
    {
        if (!_currentCulture.Equals(cultureInfo))
        {
            _currentCulture = cultureInfo;
            // Raise the event to notify subscribers
            LocaleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
