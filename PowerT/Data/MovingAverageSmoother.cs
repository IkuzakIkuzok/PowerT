
// (c) 2024 Kazuki Kohzuki

using PowerT.Plugin;
using System.Runtime.CompilerServices;

namespace PowerT.Data;

internal class MovingAverageSmoother : ISmoother
{
    [ModuleInitializer]
    internal static void Register()
    {
        PluginManager.AddPlugin(new MovingAverageSmoother());
    } // internal static void Register()

    private int width = 30;

    public string Name => "Moving Average";

    public string Description => "Smoothes the data using the moving average method.";

    public bool HasOption => true;

    public void Initialize() { }

    public void Dispose() { }

    public IEnumerable<double> Smooth(IEnumerable<double> data)
    {
        var source = data.ToArray();
        var result = new double[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            var sum = 0.0;
            var count = 0;
            for (var j = i - this.width / 2; j <= i + this.width / 2; j++)
            {
                if (j < 0 || j >= source.Length) continue;
                sum += source[j];
                ++count;
            }
            result[i] = sum / count;
        }
        return result;
    } // public IEnumerable<double> Smooth(IEnumerable<double>)

    public bool SetOption(string option)
    {
        if (int.TryParse(option, out var width) && width > 0)
        {
            this.width = width;
            return true;
        }
        return false;
    } // public bool SetOption(string)

    public string GetOption() => this.width.ToString();
} // internal class MovingAverageSmoother : ISmoother
