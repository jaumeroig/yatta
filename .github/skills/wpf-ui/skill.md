---
name: wpf-ui
description: Build modern Fluent Design WPF applications using WPF UI library
model: haiku
agent: coder-haiku
version: '2.1'
allowed-tools:
  - Read
  - Write
  - Edit
  - Bash
sources:
  - 'Claude Desktop WPF UI Skill'
  - 'https://wpfui.lepo.co/documentation/'
  - 'https://github.com/lepoco/wpfui'
  - 'WPF UI Gallery App (src/Wpf.Ui.Gallery)'
---

# WPF UI - Modern Fluent Design for WPF

## Overview

WPF UI (https://github.com/lepoco/wpfui) provides Windows 11 Fluent Design styling for WPF applications. This skill contains patterns and templates for building modern desktop apps with real-world examples.

**NuGet Package**: `WPF-UI` (current version 4.x)
**Documentation**: https://wpfui.lepo.co/
**Gallery App**: Available in Microsoft Store and GitHub (src/Wpf.Ui.Gallery)

### Key Features
- Modern Fluent Design System
- Windows 11 Mica and Acrylic backdrops
- NavigationView for app shell
- 50+ styled controls
- Dark/Light/HighContrast theme support
- Fluent System Icons
- MVVM and Dependency Injection ready

---

## Quick Start Checklist

When starting a WPF UI project:

1. ✅ Install NuGet: `WPF-UI` (4.x)
2. ✅ Configure App.xaml with theme dictionaries
3. ✅ Use `ui:FluentWindow` instead of `Window`
4. ✅ Add `xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"` namespace
5. ✅ Set up NavigationView for app shell (if multi-page)
6. ✅ Choose theme and backdrop (Mica/Acrylic)

---

## Essential App.xaml Setup

**CRITICAL**: This must be configured first or controls won't style correctly.

```xml
<Application x:Class="YourApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ui:ThemesDictionary Theme="Dark" />
                <ui:ControlsDictionary />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Theme Options**: `Light`, `Dark`, `HighContrast`

**Alternative**: Apply theme programmatically in App.xaml.cs:
```csharp
protected override void OnStartup(StartupEventArgs e)
{
    ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica);
    base.OnStartup(e);
}
```

---

## Theme Management

### Runtime Theme Switching

```csharp
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

// Apply theme with backdrop effect
ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica);

// Check current theme
var currentTheme = ApplicationThemeManager.GetAppTheme();
if (currentTheme == ApplicationTheme.Dark)
{
    ApplicationThemeManager.Apply(ApplicationTheme.Light);
}

// Get system theme
var systemTheme = ApplicationThemeManager.GetSystemTheme();

// Listen for theme changes
ApplicationThemeManager.Changed += (theme, backdrop) =>
{
    Debug.WriteLine($"Theme changed to {theme} with {backdrop}");
};
```

### Auto-Sync with Windows System Theme

```csharp
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        // Automatically follow Windows dark/light mode
        SystemThemeWatcher.Watch(this);
        InitializeComponent();
    }
}
```

Use `SystemThemeWatcher.UnWatch(this)` to stop watching.

### Backdrop Effects (Windows 11)

```csharp
// Mica - Subtle tinted background (default)
ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica);

// Acrylic - More transparent/blurred
ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Acrylic);

// Tabbed - Blurred wallpaper effect (Windows 11+)
ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Tabbed);

// None - Solid background
ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.None);
```

**Note**: Backdrop effects require Windows 11. Gracefully degrades on Windows 10.

### Custom Accent Color

```csharp
using Wpf.Ui.Appearance;

// Set custom accent color
ApplicationAccentColorManager.Apply(Color.FromRgb(0, 120, 212));

// Apply system accent color
ApplicationAccentColorManager.ApplySystemAccent();
```

---

## FluentWindow (Modern Window)

Always use `ui:FluentWindow` instead of `Window` for proper styling:

```xml
<ui:FluentWindow x:Class="YourApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
    Title="My App" Height="600" Width="900"
    ExtendsContentIntoTitleBar="True"
    WindowBackdropType="Mica"
    WindowCornerPreference="Round"
    WindowStartupLocation="CenterScreen">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <ui:TitleBar Title="My Application" Grid.Row="0">
            <ui:TitleBar.Icon>
                <ui:ImageIcon Source="pack://application:,,,/Assets/app-icon.png" />
            </ui:TitleBar.Icon>
        </ui:TitleBar>

        <!-- Content goes here -->
        <Grid Grid.Row="1">
            <!-- Your content -->
        </Grid>
    </Grid>
</ui:FluentWindow>
```

**Code-behind**:
```csharp
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

**Properties**:
- `ExtendsContentIntoTitleBar` - Extends content into title bar area (modern look)
- `WindowBackdropType` - Mica, Acrylic, Tabbed, or None
- `WindowCornerPreference` - Round, RoundSmall, DoNotRound, Default
- `CloseWindowByDoubleClickOnIcon` - Enable/disable double-click close

---

## NavigationView App Shell

The standard pattern for multi-page apps. This is the production-quality pattern from the Gallery app.

### Complete MainWindow with NavigationView

```xml
<ui:FluentWindow
    x:Class="YourApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
    xmlns:pages="clr-namespace:YourApp.Views.Pages"
    Title="{Binding ViewModel.ApplicationTitle}"
    Width="1200" Height="700"
    MinWidth="900" MinHeight="600"
    ExtendsContentIntoTitleBar="True"
    WindowBackdropType="Mica"
    WindowStartupLocation="CenterScreen">

    <Grid>
        <ui:NavigationView x:Name="RootNavigation"
                           Padding="42,0,42,0"
                           PaneDisplayMode="Left"
                           OpenPaneLength="280"
                           IsBackButtonVisible="Visible"
                           IsPaneToggleVisible="True"
                           MenuItemsSource="{Binding ViewModel.MenuItems}"
                           FooterMenuItemsSource="{Binding ViewModel.FooterMenuItems}"
                           FrameMargin="0"
                           Transition="FadeInWithSlide">

            <!-- Search box in navigation pane -->
            <ui:NavigationView.AutoSuggestBox>
                <ui:AutoSuggestBox x:Name="AutoSuggestBox"
                                   PlaceholderText="Search">
                    <ui:AutoSuggestBox.Icon>
                        <ui:IconSourceElement>
                            <ui:SymbolIconSource Symbol="Search24" />
                        </ui:IconSourceElement>
                    </ui:AutoSuggestBox.Icon>
                </ui:AutoSuggestBox>
            </ui:NavigationView.AutoSuggestBox>

            <!-- Breadcrumb header -->
            <ui:NavigationView.Header>
                <StackPanel Margin="42,32,42,20">
                    <ui:BreadcrumbBar x:Name="BreadcrumbBar" />
                </StackPanel>
            </ui:NavigationView.Header>

            <!-- Main navigation items -->
            <ui:NavigationView.MenuItems>
                <ui:NavigationViewItem Content="Dashboard"
                                       TargetPageType="{x:Type pages:DashboardPage}">
                    <ui:NavigationViewItem.Icon>
                        <ui:SymbolIcon Symbol="Home24" />
                    </ui:NavigationViewItem.Icon>
                </ui:NavigationViewItem>

                <ui:NavigationViewItem Content="Data"
                                       TargetPageType="{x:Type pages:DataPage}">
                    <ui:NavigationViewItem.Icon>
                        <ui:SymbolIcon Symbol="DataHistogram24" />
                    </ui:NavigationViewItem.Icon>
                </ui:NavigationViewItem>

                <ui:NavigationViewItem Content="Tasks"
                                       TargetPageType="{x:Type pages:TasksPage}">
                    <ui:NavigationViewItem.Icon>
                        <ui:SymbolIcon Symbol="TaskListSquareLtr24" />
                    </ui:NavigationViewItem.Icon>
                </ui:NavigationViewItem>
            </ui:NavigationView.MenuItems>

            <!-- Footer items (settings, about) -->
            <ui:NavigationView.FooterMenuItems>
                <ui:NavigationViewItem Content="Settings"
                                       TargetPageType="{x:Type pages:SettingsPage}">
                    <ui:NavigationViewItem.Icon>
                        <ui:SymbolIcon Symbol="Settings24" />
                    </ui:NavigationViewItem.Icon>
                </ui:NavigationViewItem>
            </ui:NavigationView.FooterMenuItems>

            <!-- Snackbar for notifications -->
            <ui:NavigationView.ContentOverlay>
                <Grid>
                    <ui:SnackbarPresenter x:Name="SnackbarPresenter" />
                </Grid>
            </ui:NavigationView.ContentOverlay>
        </ui:NavigationView>

        <!-- Title bar -->
        <ui:TitleBar x:Name="TitleBar"
                     Title="{Binding ViewModel.ApplicationTitle}"
                     Grid.Row="0"
                     CloseWindowByDoubleClickOnIcon="True">
            <ui:TitleBar.Icon>
                <ui:ImageIcon Source="pack://application:,,,/Assets/app-icon.png" />
            </ui:TitleBar.Icon>
        </ui:TitleBar>
    </Grid>
</ui:FluentWindow>
```

### NavigationView Properties

- `PaneDisplayMode` - Left, LeftCompact, Top, LeftMinimal
- `OpenPaneLength` - Width when pane is open (default 320)
- `CompactPaneLength` - Width when compact (default 48)
- `IsBackButtonVisible` - Auto, Visible, Collapsed
- `IsPaneToggleVisible` - Show/hide hamburger menu
- `Transition` - None, FadeIn, FadeInWithSlide, SlideLeft, SlideBottom

---

## Dashboard Layout Patterns

### Stats Cards Grid (Real-World Pattern)

From WPF UI Gallery and Claude Desktop skill - this is a production-ready pattern for dashboard stats:

```xml
<ui:Page x:Class="YourApp.Views.Pages.DashboardPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
    Title="Dashboard">

    <ScrollViewer VerticalScrollBarVisibility="Auto"
                  HorizontalScrollBarVisibility="Disabled">
        <StackPanel Margin="24,0,24,24">

            <!-- Page Header -->
            <TextBlock Text="Dashboard"
                       FontSize="28"
                       FontWeight="SemiBold"
                       Margin="0,0,0,24" />

            <!-- Stats Cards Row (4-column grid) -->
            <UniformGrid Rows="1" Columns="4" Margin="0,0,0,24">

                <!-- Stat Card 1: With trend indicator -->
                <ui:Card Margin="0,0,8,0">
                    <StackPanel>
                        <ui:SymbolIcon Symbol="People24"
                                       FontSize="24"
                                       Foreground="{ui:ThemeResource SystemAccentColorPrimaryBrush}"
                                       Margin="0,0,0,8" />
                        <TextBlock Text="Total Users"
                                   FontSize="12"
                                   Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
                        <TextBlock Text="1,234"
                                   FontSize="32"
                                   FontWeight="Bold" />
                        <StackPanel Orientation="Horizontal" Margin="0,4,0,0">
                            <ui:SymbolIcon Symbol="ArrowUp24"
                                           FontSize="12"
                                           Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}" />
                            <TextBlock Text="12% from last month"
                                       FontSize="11"
                                       Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}"
                                       Margin="4,0,0,0" />
                        </StackPanel>
                    </StackPanel>
                </ui:Card>

                <!-- Stat Card 2: Simple -->
                <ui:Card Margin="4,0">
                    <StackPanel>
                        <ui:SymbolIcon Symbol="DocumentBulletList24"
                                       FontSize="24"
                                       Foreground="{ui:ThemeResource SystemAccentColorPrimaryBrush}"
                                       Margin="0,0,0,8" />
                        <TextBlock Text="Active Projects"
                                   FontSize="12"
                                   Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
                        <TextBlock Text="42"
                                   FontSize="32"
                                   FontWeight="Bold" />
                        <TextBlock Text="8 completed this week"
                                   FontSize="11"
                                   Foreground="{ui:ThemeResource TextFillColorTertiaryBrush}"
                                   Margin="0,4,0,0" />
                    </StackPanel>
                </ui:Card>

                <!-- Stat Card 3: Success status -->
                <ui:Card Margin="4,0">
                    <StackPanel>
                        <ui:SymbolIcon Symbol="CheckmarkCircle24"
                                       FontSize="24"
                                       Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}"
                                       Margin="0,0,0,8" />
                        <TextBlock Text="Success Rate"
                                   FontSize="12"
                                   Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
                        <TextBlock Text="97.7%"
                                   FontSize="32"
                                   FontWeight="Bold"
                                   Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}" />
                        <TextBlock Text="Above target"
                                   FontSize="11"
                                   Foreground="{ui:ThemeResource TextFillColorTertiaryBrush}"
                                   Margin="0,4,0,0" />
                    </StackPanel>
                </ui:Card>

                <!-- Stat Card 4: Warning status -->
                <ui:Card Margin="8,0,0,0">
                    <StackPanel>
                        <ui:SymbolIcon Symbol="Warning24"
                                       FontSize="24"
                                       Foreground="{ui:ThemeResource SystemFillColorCautionBrush}"
                                       Margin="0,0,0,8" />
                        <TextBlock Text="Open Issues"
                                   FontSize="12"
                                   Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
                        <TextBlock Text="23"
                                   FontSize="32"
                                   FontWeight="Bold" />
                        <TextBlock Text="5 high priority"
                                   FontSize="11"
                                   Foreground="{ui:ThemeResource SystemFillColorCautionBrush}"
                                   Margin="0,4,0,0" />
                    </StackPanel>
                </ui:Card>

            </UniformGrid>

            <!-- Two Column Layout: Main content + Sidebar -->
            <Grid Margin="0,0,0,24">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="2*" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>

                <!-- Main Content Area: Activity Feed -->
                <ui:Card Grid.Column="0" Margin="0,0,12,0">
                    <StackPanel>
                        <Grid Margin="0,0,0,16">
                            <TextBlock Text="Recent Activity"
                                       FontSize="18"
                                       FontWeight="SemiBold"
                                       VerticalAlignment="Center" />
                            <ui:Button Content="View All"
                                       Appearance="Transparent"
                                       HorizontalAlignment="Right" />
                        </Grid>

                        <!-- Activity List -->
                        <StackPanel>
                            <!-- Activity Item 1 -->
                            <Border Padding="12"
                                    Margin="0,0,0,8"
                                    CornerRadius="4"
                                    Background="{ui:ThemeResource CardBackgroundFillColorSecondaryBrush}">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto" />
                                        <ColumnDefinition Width="*" />
                                        <ColumnDefinition Width="Auto" />
                                    </Grid.ColumnDefinitions>
                                    <ui:SymbolIcon Symbol="Checkmark24"
                                                   Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}"
                                                   Margin="0,0,12,0" />
                                    <StackPanel Grid.Column="1">
                                        <TextBlock Text="Project Alpha completed" FontWeight="SemiBold" />
                                        <TextBlock Text="All tasks finished successfully"
                                                   Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}"
                                                   FontSize="12" />
                                    </StackPanel>
                                    <TextBlock Grid.Column="2"
                                               Text="2 hours ago"
                                               Foreground="{ui:ThemeResource TextFillColorTertiaryBrush}"
                                               FontSize="12"
                                               VerticalAlignment="Center" />
                                </Grid>
                            </Border>

                            <!-- More activity items... -->

                        </StackPanel>
                    </StackPanel>
                </ui:Card>

                <!-- Sidebar -->
                <StackPanel Grid.Column="1">

                    <!-- Quick Actions Card -->
                    <ui:Card Margin="0,0,0,12">
                        <StackPanel>
                            <TextBlock Text="Quick Actions"
                                       FontSize="16"
                                       FontWeight="SemiBold"
                                       Margin="0,0,0,16" />

                            <ui:Button Content="New Project"
                                       Icon="{ui:SymbolIcon Add24}"
                                       Appearance="Primary"
                                       HorizontalAlignment="Stretch"
                                       Margin="0,0,0,8" />

                            <ui:Button Content="Create Report"
                                       Icon="{ui:SymbolIcon DocumentAdd24}"
                                       HorizontalAlignment="Stretch"
                                       Margin="0,0,0,8" />

                            <ui:Button Content="Invite Team"
                                       Icon="{ui:SymbolIcon PersonAdd24}"
                                       HorizontalAlignment="Stretch" />
                        </StackPanel>
                    </ui:Card>

                    <!-- System Status Card (Expandable) -->
                    <ui:CardExpander Header="System Status" IsExpanded="True">
                        <StackPanel>
                            <Grid Margin="0,0,0,8">
                                <TextBlock Text="Database" VerticalAlignment="Center" />
                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                                    <Ellipse Width="8" Height="8"
                                             Fill="{ui:ThemeResource SystemFillColorSuccessBrush}"
                                             Margin="0,0,6,0" />
                                    <TextBlock Text="Online"
                                               Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}"
                                               FontSize="12" />
                                </StackPanel>
                            </Grid>

                            <Grid Margin="0,0,0,8">
                                <TextBlock Text="API Server" VerticalAlignment="Center" />
                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                                    <Ellipse Width="8" Height="8"
                                             Fill="{ui:ThemeResource SystemFillColorSuccessBrush}"
                                             Margin="0,0,6,0" />
                                    <TextBlock Text="Connected"
                                               Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}"
                                               FontSize="12" />
                                </StackPanel>
                            </Grid>

                            <Grid>
                                <TextBlock Text="Background Jobs" VerticalAlignment="Center" />
                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                                    <Ellipse Width="8" Height="8"
                                             Fill="{ui:ThemeResource SystemFillColorCautionBrush}"
                                             Margin="0,0,6,0" />
                                    <TextBlock Text="3 pending"
                                               Foreground="{ui:ThemeResource SystemFillColorCautionBrush}"
                                               FontSize="12" />
                                </StackPanel>
                            </Grid>
                        </StackPanel>
                    </ui:CardExpander>

                </StackPanel>
            </Grid>

            <!-- InfoBar for notifications -->
            <ui:InfoBar Title="Tip of the day"
                        Message="You can press Ctrl+K to quickly search across all projects."
                        IsOpen="True"
                        Severity="Informational"
                        IsClosable="True" />

        </StackPanel>
    </ScrollViewer>
</ui:Page>
```

---

## Common Controls Reference

### Buttons

```xml
<!-- Primary action -->
<ui:Button Content="Save" Appearance="Primary" Icon="{ui:SymbolIcon Save24}" />

<!-- Secondary action -->
<ui:Button Content="Cancel" Appearance="Secondary" />

<!-- Danger action (destructive) -->
<ui:Button Content="Delete" Appearance="Danger" Icon="{ui:SymbolIcon Delete24}" />

<!-- Transparent (ghost button) -->
<ui:Button Content="Learn More" Appearance="Transparent" />

<!-- Icon-only button -->
<ui:Button Icon="{ui:SymbolIcon Settings24}" ToolTip="Settings" />

<!-- Button with command -->
<ui:Button Content="Save"
           Appearance="Primary"
           Command="{Binding SaveCommand}"
           CommandParameter="{Binding CurrentItem}" />
```

### Text Input

```xml
<!-- Basic text input -->
<ui:TextBox PlaceholderText="Enter your name"
            Icon="{ui:SymbolIcon Person24}" />

<!-- Password input -->
<ui:PasswordBox PlaceholderText="Password" />

<!-- Number input with spinner -->
<ui:NumberBox Value="{Binding Quantity}"
              Minimum="0"
              Maximum="100"
              SpinButtonPlacementMode="Inline" />

<!-- Auto-suggest search -->
<ui:AutoSuggestBox PlaceholderText="Search..."
                   ItemsSource="{Binding Suggestions}"
                   TextChanged="OnSearchTextChanged" />

<!-- Multi-line text -->
<ui:RichTextBox AcceptsReturn="True"
                MinHeight="100" />
```

### Cards

```xml
<!-- Simple card -->
<ui:Card Padding="16">
    <TextBlock Text="Card content here" />
</ui:Card>

<!-- Card with header and icon -->
<ui:CardControl Header="Settings"
                Description="Configure your preferences"
                Icon="{ui:SymbolIcon Settings24}">
    <StackPanel>
        <ui:Button Content="Open Settings" />
    </StackPanel>
</ui:CardControl>

<!-- Expandable card -->
<ui:CardExpander Header="Advanced Options" IsExpanded="False">
    <StackPanel>
        <ui:ToggleSwitch Content="Enable feature A" />
        <ui:ToggleSwitch Content="Enable feature B" />
    </StackPanel>
</ui:CardExpander>

<!-- Clickable card (navigation) -->
<ui:CardAction Command="{Binding OpenDetailsCommand}"
               CommandParameter="{Binding Item}">
    <StackPanel>
        <TextBlock Text="View Details" FontWeight="SemiBold" />
        <TextBlock Text="Opens the details page" Opacity="0.7" />
    </StackPanel>
</ui:CardAction>
```

### Data Display

```xml
<!-- DataGrid with WPF UI styling -->
<ui:DataGrid ItemsSource="{Binding Items}"
             AutoGenerateColumns="False"
             IsReadOnly="True">
    <ui:DataGrid.Columns>
        <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*" />
        <DataGridTextColumn Header="Status" Binding="{Binding Status}" Width="120" />
        <DataGridTextColumn Header="Date" Binding="{Binding Date, StringFormat=d}" Width="100" />
    </ui:DataGrid.Columns>
</ui:DataGrid>

<!-- ListView with custom template -->
<ui:ListView ItemsSource="{Binding Items}">
    <ui:ListView.ItemTemplate>
        <DataTemplate>
            <Grid Margin="0,4">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                <ui:SymbolIcon Symbol="Document24" Margin="0,0,12,0" />
                <StackPanel Grid.Column="1">
                    <TextBlock Text="{Binding Name}" FontWeight="SemiBold" />
                    <TextBlock Text="{Binding Description}"
                               FontSize="12"
                               Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
                </StackPanel>
                <TextBlock Grid.Column="2"
                           Text="{Binding Date, StringFormat=d}"
                           VerticalAlignment="Center"
                           Foreground="{ui:ThemeResource TextFillColorTertiaryBrush}" />
            </Grid>
        </DataTemplate>
    </ui:ListView.ItemTemplate>
</ui:ListView>
```

### Dialogs and Notifications

```xml
<!-- Content Dialog in XAML -->
<ui:ContentDialog x:Name="ConfirmDialog"
                  Title="Confirm Action"
                  Content="Are you sure you want to proceed?"
                  PrimaryButtonText="Yes"
                  SecondaryButtonText="No"
                  CloseButtonText="Cancel" />

<!-- Snackbar for toast notifications -->
<ui:Snackbar x:Name="NotificationSnackbar" Timeout="3000" />
```

```csharp
// Show dialog
var result = await ConfirmDialog.ShowAsync();
if (result == ContentDialogResult.Primary)
{
    // User clicked Yes
}

// Show snackbar notification
NotificationSnackbar.Show("Success", "Item saved successfully.", ControlAppearance.Success);

// Snackbar with icon
NotificationSnackbar.Show(
    "Error",
    "Failed to save item.",
    ControlAppearance.Danger,
    new SymbolIcon(SymbolRegular.ErrorCircle24)
);
```

### InfoBar

```xml
<!-- Informational message -->
<ui:InfoBar Title="Information"
            Message="This is an informational message."
            IsOpen="True"
            Severity="Informational" />

<!-- Error message -->
<ui:InfoBar Title="Error"
            Message="Something went wrong."
            IsOpen="{Binding HasError}"
            Severity="Error"
            IsClosable="True" />

<!-- Success message -->
<ui:InfoBar Title="Success"
            Message="Operation completed successfully."
            IsOpen="True"
            Severity="Success" />

<!-- Warning message -->
<ui:InfoBar Title="Warning"
            Message="This action cannot be undone."
            Severity="Warning" />
```

**Severity Options**: Informational, Success, Warning, Error

### Toggle and Selection

```xml
<!-- Toggle switch -->
<ui:ToggleSwitch Content="Enable notifications"
                 IsChecked="{Binding NotificationsEnabled}" />

<!-- Checkbox -->
<CheckBox Content="I agree to terms"
          IsChecked="{Binding AgreeToTerms}" />

<!-- Radio buttons -->
<StackPanel>
    <RadioButton Content="Option A" GroupName="Options" />
    <RadioButton Content="Option B" GroupName="Options" />
    <RadioButton Content="Option C" GroupName="Options" />
</StackPanel>

<!-- ComboBox -->
<ui:ComboBox ItemsSource="{Binding Options}"
             SelectedItem="{Binding SelectedOption}"
             PlaceholderText="Select an option" />
```

---

## Icons (Fluent System Icons)

WPF UI uses Fluent System Icons. Browse icons at: https://github.com/microsoft/fluentui-system-icons

```xml
<!-- Basic usage -->
<ui:SymbolIcon Symbol="Home24" />
<ui:SymbolIcon Symbol="Settings24" />
<ui:SymbolIcon Symbol="Person24" />
<ui:SymbolIcon Symbol="Search24" />

<!-- Common icons -->
<ui:SymbolIcon Symbol="Add24" />
<ui:SymbolIcon Symbol="Delete24" />
<ui:SymbolIcon Symbol="Save24" />
<ui:SymbolIcon Symbol="Edit24" />
<ui:SymbolIcon Symbol="CheckmarkCircle24" />
<ui:SymbolIcon Symbol="ErrorCircle24" />
<ui:SymbolIcon Symbol="Warning24" />
<ui:SymbolIcon Symbol="Info24" />
<ui:SymbolIcon Symbol="Document24" />
<ui:SymbolIcon Symbol="Folder24" />
<ui:SymbolIcon Symbol="Calendar24" />
<ui:SymbolIcon Symbol="Mail24" />

<!-- With button -->
<ui:Button Icon="{ui:SymbolIcon Add24}" Content="Add New" />

<!-- Colored icon -->
<ui:SymbolIcon Symbol="CheckmarkCircle24"
               Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}"
               FontSize="32" />
```

**Icon Sizes**: 12, 16, 20, 24, 28, 32, 48 (e.g., `Home24`, `Home32`)

**Filled variants**: Many icons have `Filled` suffix (e.g., `HomeFilled24`)

---

## MVVM with Dependency Injection

For production apps, use the MVVM pattern with .NET Generic Host and DI:

### App.xaml.cs Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Wpf.Ui;

public partial class App : Application
{
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            // WPF UI Services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddSingleton<IThemeService, ThemeService>();

            // Windows
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            // Pages
            services.AddTransient<DashboardPage>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<SettingsViewModel>();

            // Your services
            services.AddSingleton<IDataService, DataService>();
        })
        .Build();

    public static T GetService<T>() where T : class
        => _host.Services.GetRequiredService<T>();

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = GetService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        base.OnExit(e);
    }
}
```

### MainWindow with DI

```csharp
public partial class MainWindow : FluentWindow
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        IPageService pageService,
        ISnackbarService snackbarService)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        // Set up navigation
        navigationService.SetNavigationControl(RootNavigation);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);

        // Navigate to initial page
        navigationService.Navigate(typeof(DashboardPage));
    }
}
```

### Page with ViewModel

```csharp
public partial class DashboardPage : INavigableView<DashboardViewModel>
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ISnackbarService _snackbarService;

    public DashboardViewModel(IDataService dataService, ISnackbarService snackbarService)
    {
        _dataService = dataService;
        _snackbarService = snackbarService;
    }

    [ObservableProperty]
    private int _totalUsers;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            var data = await _dataService.GetDashboardDataAsync();
            TotalUsers = data.TotalUsers;
        }
        catch (Exception ex)
        {
            _snackbarService.Show("Error", ex.Message, ControlAppearance.Danger);
        }
    }
}
```

---

## Theme Resources

Access theme colors in XAML:

```xml
<!-- Text colors -->
<TextBlock Foreground="{ui:ThemeResource TextFillColorPrimaryBrush}" />
<TextBlock Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
<TextBlock Foreground="{ui:ThemeResource TextFillColorTertiaryBrush}" />
<TextBlock Foreground="{ui:ThemeResource TextFillColorDisabledBrush}" />

<!-- Accent colors -->
<Border Background="{ui:ThemeResource SystemAccentColorPrimaryBrush}" />
<Border Background="{ui:ThemeResource SystemAccentColorSecondaryBrush}" />

<!-- Status colors -->
<TextBlock Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}" />
<TextBlock Foreground="{ui:ThemeResource SystemFillColorCautionBrush}" />
<TextBlock Foreground="{ui:ThemeResource SystemFillColorCriticalBrush}" />
<TextBlock Foreground="{ui:ThemeResource SystemFillColorAttentionBrush}" />

<!-- Background colors -->
<Border Background="{ui:ThemeResource ApplicationBackgroundBrush}" />
<Border Background="{ui:ThemeResource CardBackgroundFillColorDefaultBrush}" />
<Border Background="{ui:ThemeResource CardBackgroundFillColorSecondaryBrush}" />
<Border Background="{ui:ThemeResource SubtleFillColorSecondaryBrush}" />

<!-- Stroke/Border colors -->
<Border BorderBrush="{ui:ThemeResource CardStrokeColorDefaultBrush}" />
<Border BorderBrush="{ui:ThemeResource ControlStrokeColorDefaultBrush}" />
```

---

## Improving Claude Family Manager v2

Based on the current claude-family-manager-v2 UI, here are specific WPF UI improvements using patterns from this skill:

### 1. Replace Plain Stats with Card Grid

**Current**: Plain text stats (Pending Messages: 1, Open Feedback: 8)

**Improved**: Use the stats card pattern:

```xml
<UniformGrid Rows="1" Columns="3" Margin="16,0,16,16">
    <ui:Card Margin="0,0,8,0">
        <StackPanel>
            <ui:SymbolIcon Symbol="Mail24"
                           Foreground="{ui:ThemeResource SystemAccentColorPrimaryBrush}"
                           FontSize="24" Margin="0,0,0,8" />
            <TextBlock Text="Pending Messages"
                       FontSize="12"
                       Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
            <TextBlock Text="{Binding PendingMessagesCount}"
                       FontSize="32"
                       FontWeight="Bold" />
        </StackPanel>
    </ui:Card>

    <ui:Card Margin="4,0">
        <StackPanel>
            <ui:SymbolIcon Symbol="ChatBubblesQuestion24"
                           Foreground="{ui:ThemeResource SystemFillColorCautionBrush}"
                           FontSize="24" Margin="0,0,0,8" />
            <TextBlock Text="Open Feedback"
                       FontSize="12"
                       Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
            <TextBlock Text="{Binding OpenFeedbackCount}"
                       FontSize="32"
                       FontWeight="Bold" />
        </StackPanel>
    </ui:Card>

    <ui:Card Margin="8,0,0,0">
        <StackPanel>
            <ui:SymbolIcon Symbol="Clock24"
                           Foreground="{ui:ThemeResource SystemFillColorSuccessBrush}"
                           FontSize="24" Margin="0,0,0,8" />
            <TextBlock Text="Last Activity"
                       FontSize="12"
                       Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
            <TextBlock Text="{Binding LastActivity}"
                       FontSize="20"
                       FontWeight="SemiBold" />
        </StackPanel>
    </ui:Card>
</UniformGrid>
```

### 2. Use Activity Feed Pattern for Sessions Tab

```xml
<!-- Instead of plain list, use activity feed -->
<ui:Card>
    <StackPanel>
        <Grid Margin="0,0,0,16">
            <TextBlock Text="Recent Sessions" FontSize="18" FontWeight="SemiBold" />
            <ui:Button Content="View All" Appearance="Transparent" HorizontalAlignment="Right" />
        </Grid>

        <ItemsControl ItemsSource="{Binding RecentSessions}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Padding="12" Margin="0,0,0,8" CornerRadius="4"
                            Background="{ui:ThemeResource CardBackgroundFillColorSecondaryBrush}">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto" />
                                <ColumnDefinition Width="*" />
                                <ColumnDefinition Width="Auto" />
                            </Grid.ColumnDefinitions>
                            <ui:SymbolIcon Symbol="History24" Margin="0,0,12,0" />
                            <StackPanel Grid.Column="1">
                                <TextBlock Text="{Binding SessionName}" FontWeight="SemiBold" />
                                <TextBlock Text="{Binding Summary}"
                                           FontSize="12"
                                           Foreground="{ui:ThemeResource TextFillColorSecondaryBrush}" />
                            </StackPanel>
                            <TextBlock Grid.Column="2"
                                       Text="{Binding TimeAgo}"
                                       FontSize="12"
                                       Foreground="{ui:ThemeResource TextFillColorTertiaryBrush}"
                                       VerticalAlignment="Center" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </StackPanel>
</ui:Card>
```

### 3. Improve Quick Actions with Sidebar Card

```xml
<ui:Card>
    <StackPanel>
        <TextBlock Text="Quick Actions"
                   FontSize="16"
                   FontWeight="SemiBold"
                   Margin="0,0,0,16" />

        <ui:Button Content="Open in VS Code"
                   Icon="{ui:SymbolIcon Code24}"
                   Appearance="Primary"
                   HorizontalAlignment="Stretch"
                   Margin="0,0,0,8"
                   Command="{Binding OpenInVSCodeCommand}" />

        <ui:Button Content="Open in Explorer"
                   Icon="{ui:SymbolIcon Folder24}"
                   HorizontalAlignment="Stretch"
                   Margin="0,0,0,8"
                   Command="{Binding OpenInExplorerCommand}" />

        <ui:Button Content="Refresh"
                   Icon="{ui:SymbolIcon ArrowClockwise24}"
                   HorizontalAlignment="Stretch"
                   Command="{Binding RefreshCommand}" />
    </StackPanel>
</ui:Card>
```

### 4. Add InfoBar for Status Messages

```xml
<!-- Show tips or important messages -->
<ui:InfoBar Title="Tip"
            Message="You can press F5 to refresh the project list."
            IsOpen="True"
            Severity="Informational"
            IsClosable="True"
            Margin="16,0,16,16" />
```

### 5. Use CardExpander for Collapsible Sections

```xml
<ui:CardExpander Header="Project Details" IsExpanded="True" Margin="16">
    <StackPanel>
        <Grid Margin="0,0,0,8">
            <TextBlock Text="Type" />
            <TextBlock Text="{Binding ProjectType}" HorizontalAlignment="Right" FontWeight="SemiBold" />
        </Grid>
        <Grid Margin="0,0,0,8">
            <TextBlock Text="Status" />
            <TextBlock Text="{Binding ProjectStatus}" HorizontalAlignment="Right" FontWeight="SemiBold" />
        </Grid>
        <Grid>
            <TextBlock Text="Path" />
            <TextBlock Text="{Binding ProjectPath}"
                       HorizontalAlignment="Right"
                       FontFamily="Consolas"
                       FontSize="11" />
        </Grid>
    </StackPanel>
</ui:CardExpander>
```

---

## Project Structure

Recommended folder structure for WPF UI apps:

```
YourApp/
├── App.xaml                    # Theme dictionaries, DI setup
├── App.xaml.cs
├── Assets/
│   ├── app-icon.png
│   └── images/
├── ViewModels/
│   ├── MainWindowViewModel.cs
│   ├── DashboardViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── Windows/
│   │   ├── MainWindow.xaml
│   │   └── MainWindow.xaml.cs
│   └── Pages/
│       ├── DashboardPage.xaml
│       ├── DashboardPage.xaml.cs
│       ├── SettingsPage.xaml
│       └── SettingsPage.xaml.cs
├── Models/
│   └── ...
├── Services/
│   ├── Interfaces/
│   └── Implementations/
└── Helpers/
    └── ...
```

---

## Resources and References

**Official**:
- [WPF UI Documentation](https://wpfui.lepo.co/documentation/)
- [API Reference](https://wpfui.lepo.co/api/)
- [GitHub Repository](https://github.com/lepoco/wpfui)
- [NuGet Package](https://www.nuget.org/packages/WPF-UI/)

**Examples**:
- [Gallery App Source](https://github.com/lepoco/wpfui/tree/main/src/Wpf.Ui.Gallery)
- [MVVM Demo](https://github.com/lepoco/wpfui/tree/main/samples/Wpf.Ui.Demo.Mvvm)
- [Simple Demo](https://github.com/lepoco/wpfui/tree/main/samples/Wpf.Ui.Demo.Simple)

**Design Resources**:
- [Fluent System Icons](https://github.com/microsoft/fluentui-system-icons)
- [Windows 11 Design Principles](https://learn.microsoft.com/en-us/windows/apps/design/)

**Community**:
- [GitHub Issues](https://github.com/lepoco/wpfui/issues)
- [GitHub Discussions](https://github.com/lepoco/wpfui/discussions)
- Discord (link in GitHub README)

---

## Best Practices

1. **Always use FluentWindow** - Not standard Window
2. **Set up App.xaml dictionaries** - Required for control styling
3. **NavigationView for main navigation** - Not TabControl
4. **Cards for content grouping** - Modern container pattern
5. **Snackbar for notifications** - Not MessageBox
6. **InfoBar for persistent messages** - Warnings, tips, errors
7. **Mica/Acrylic backdrops** - Windows 11 integration
8. **Symbol icons** - Consistent with Windows 11
9. **Theme resources** - Use dynamic resources, not hardcoded colors
10. **MVVM with DI** - For maintainable apps

---

**Version**: 2.0
**Created**: 2025-12-26
**Updated**: 2025-12-26
**Lines**: ~1050
**Status**: Comprehensive - Desktop skill + Web docs + Gallery examples
