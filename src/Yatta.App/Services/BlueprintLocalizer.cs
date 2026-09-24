namespace Yatta.App.Services;

using BlazorBlueprint.Components;
using Yatta.Core.Interfaces;

/// <summary>Resolves Blueprint component text from Yatta's live language resources.</summary>
public sealed class BlueprintLocalizer : DefaultBbLocalizer
{
    private readonly ILocalizationService _localization;

    /// <summary>Creates the localizer.</summary>
    public BlueprintLocalizer(ILocalizationService localization)
    {
        _localization = localization;
    }

    /// <inheritdoc />
    public override string this[string key]
    {
        get
        {
            string resourceKey = $"Blueprint_{key.Replace('.', '_')}";
            string localized = _localization.GetString(resourceKey);
            return localized == resourceKey ? base[key] : localized;
        }
    }

    /// <inheritdoc />
    public override string this[string key, params object[] arguments]
    {
        get
        {
            string resourceKey = $"Blueprint_{key.Replace('.', '_')}";
            string localized = _localization.GetString(resourceKey, arguments);
            return localized == resourceKey ? base[key, arguments] : localized;
        }
    }
}
