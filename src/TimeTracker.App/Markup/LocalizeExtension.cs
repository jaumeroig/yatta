namespace TimeTracker.App.Markup;

using System;
using System.Windows.Markup;
using TimeTracker.App.Resources;

/// <summary>
/// MarkupExtension per accedir als recursos localitzats des de XAML.
/// Ús: {local:Localize Nav_Activities}
/// </summary>
public class LocalizeExtension : MarkupExtension
{
    public string Key { get; set; }

    public LocalizeExtension()
    {
        Key = string.Empty;
    }

    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
        {
            return string.Empty;
        }

        var value = Resources.ResourceManager.GetString(Key, Resources.Culture);
        return value ?? Key;
    }
}