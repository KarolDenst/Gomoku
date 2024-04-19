using Avalonia.Media;

namespace Gomoku;

public static class Constants
{
    public const int DefaultBoardSize = 15;
    public static readonly IImmutableSolidColorBrush BoardColorBrush = Brushes.Sienna;
    public static readonly IImmutableSolidColorBrush Player1ColorBrush = Brushes.Black;
    public static readonly IImmutableSolidColorBrush PLayer2ColorBrush = Brushes.White;
}