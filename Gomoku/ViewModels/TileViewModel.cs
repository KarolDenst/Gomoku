using System;
using System.Reactive;
using Avalonia.Media;
using ReactiveUI;

namespace Gomoku.ViewModels;

public class TileViewModel(int row, int col, Action<int, int> onClicked) : ReactiveObject
{
    public ReactiveCommand<Unit, Unit> TileClickedCommand { get; } = ReactiveCommand.Create(() => onClicked(row, col));

    private IBrush _color = Constants.BoardColorBrush;

    public IBrush Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }
}