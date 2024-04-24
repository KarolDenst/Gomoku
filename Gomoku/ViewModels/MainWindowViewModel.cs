using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Media;
using Gomoku.Models;
using MCST;
using ReactiveUI;

namespace Gomoku.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<ObservableCollection<TileViewModel>> Grid { get; set; }
    public ReactiveCommand<Unit, Unit> NewGameCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; } = ReactiveCommand.Create(() => Environment.Exit(0));

    private GameEngine _engine = new();

    public MainWindowViewModel()
    {
        Grid = new ObservableCollection<ObservableCollection<TileViewModel>>();
        _engine.BoardChanged += OnGameEngineTileChanged;
        InitializeGrid();

        NewGameCommand = ReactiveCommand.Create(() =>
        {
            _engine = new GameEngine();
            _engine.BoardChanged += OnGameEngineTileChanged;
            InitializeGrid();
        });
    }

    private void InitializeGrid()
    {
        Grid.Clear();
        for (int i = 0; i < _engine.BoardSize; i++)
        {
            var row = new ObservableCollection<TileViewModel>();
            for (int j = 0; j < _engine.BoardSize; j++)
            {
                row.Add(new TileViewModel(i, j, _engine.MakeMove));
            }
            Grid.Add(row);
        }
    }
    
    private void OnGameEngineTileChanged(int row, int col, int tileValue)
    {
        Grid[row][col].Color = ConvertValueToColor(tileValue);
    }

    private static IBrush ConvertValueToColor(int value) => value switch
    {
        Tiles.Black => Constants.Player1ColorBrush,
        Tiles.White => Constants.PLayer2ColorBrush,
        _ => Constants.BoardColorBrush
    };
}