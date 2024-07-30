
// (c) 2024 Kazuki Kohzuki

using PowerT.Config;
using PowerT.Controls;

namespace PowerT;

/// <summary>
/// The main entry point for the application.
/// </summary>
internal static class Program
{
    internal const string GITHUB_REPOSITORY = "https://github.com/IkuzakIkuzok/PowerT";

    /// <summary>
    /// Occurs when the color gradient is changed.
    /// </summary>
    internal static event EventHandler? GradientChanged;

    /// <summary>
    /// Occurs when the axis label font is changed.
    /// </summary>
    internal static event EventHandler? AxisLabelFontChanged;

    /// <summary>
    /// Occurs when the axis title font is changed.
    /// </summary>
    internal static event EventHandler? AxisTitleFontChanged;

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    internal static AppConfig Config { get; } = AppConfig.Load();

    /// <summary>
    /// Gets the main window.
    /// </summary>
    internal static MainWindow MainWindow { get; } = new();

    /// <summary>
    /// Gets or sets the gradient start color.
    /// </summary>
    internal static Color GradientStart
    {
        get => Config.AppearanceConfig.ColorGradientConfig.StartColor;
        set
        {
            Config.AppearanceConfig.ColorGradientConfig.StartColor = value;
            Config.Save();
            GradientChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the gradient end color.
    /// </summary>
    internal static Color GradientEnd
    {
        get => Config.AppearanceConfig.ColorGradientConfig.EndColor;
        set
        {
            Config.AppearanceConfig.ColorGradientConfig.EndColor = value;
            Config.Save();
            GradientChanged?.Invoke(null, EventArgs.Empty);
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
        get => Config.AppearanceConfig.GuideLineColor;
        set
        {
            Config.AppearanceConfig.GuideLineColor = value;
            Config.Save();
        }
    }

    /// <summary>
    /// Gets or sets the filename format of the A-B signal.
    /// </summary>
    internal static string AMinusBSignalFormat
    {
        get => Config.DecayLoadingConfig.AMinusBSignalFormat;
        set
        {
            Config.DecayLoadingConfig.AMinusBSignalFormat = value;
            Config.Save();
        }
    }

    /// <summary>
    /// Gets or sets the filename format of the B signal.
    /// </summary>
    internal static string BSignalFormat
    {
        get => Config.DecayLoadingConfig.BSignalFormat;
        set
        {
            Config.DecayLoadingConfig.BSignalFormat = value;
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
        Application.Run(MainWindow);
    } // private static void Main ()
} // internal static class Program
