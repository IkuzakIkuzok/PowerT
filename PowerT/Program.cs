
// (c) 2024 Kazuki Kohzuki

using PowerT.Config;
using PowerT.Controls;

namespace PowerT;

/// <summary>
/// The main entry point for the application.
/// </summary>
internal static class Program
{
    internal static event EventHandler? AxisLabelFontChanged;

    internal static event EventHandler? AxisTitleFontChanged;

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    internal static AppConfig Config { get; } = AppConfig.Load();

    /// <summary>
    /// Gets or sets the gradient start color.
    /// </summary>
    internal static Color GradientStart
    {
        get => Config.AppearanceConfig.ColorGradientConfig.StartColor.Color;
        set
        {
            Config.AppearanceConfig.ColorGradientConfig.StartColor.Color = value;
            Config.Save();
        }
    }

    /// <summary>
    /// Gets or sets the gradient end color.
    /// </summary>
    internal static Color GradientEnd
    {
        get => Config.AppearanceConfig.ColorGradientConfig.EndColor.Color;
        set
        {
            Config.AppearanceConfig.ColorGradientConfig.EndColor.Color = value;
            Config.Save();
        }
    }

    /// <summary>
    /// Gets or sets the line widdh of the fitted lines.
    /// </summary>
    internal static int FittedWidth
    {
        get => Config.AppearanceConfig.FittedWidth;
        set
        {
            Config.AppearanceConfig.FittedWidth = value;
            Config.Save();
        }
    }

    /// <summary>
    /// Get or set the font of the axis labels.
    /// </summary>
    internal static Font AxisLabelFont
    {
        get => Config.AppearanceConfig.AxisLabelFont.Font;
        set
        {
            Config.AppearanceConfig.AxisLabelFont.Font = value;
            Config.Save();
            AxisLabelFontChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Get or set the font of the axis titles.
    /// </summary>
    internal static Font AxisTitleFont
    {
        get => Config.AppearanceConfig.AxisTitleFont.Font;
        set
        {
            Config.AppearanceConfig.AxisTitleFont.Font = value;
            Config.Save();
            AxisTitleFontChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the color of a guide line.
    /// </summary>
    internal static Color GuideLineColor
    {
        get => Config.AppearanceConfig.GuideLineColor.Color;
        set
        {
            Config.AppearanceConfig.GuideLineColor.Color = value;
            Config.Save();
        }
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
    } // private static void Main ()
} // internal static class Program
