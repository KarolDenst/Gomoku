﻿using System.Collections.ObjectModel;
using Avalonia.Media;
using Gomoku.Models;
using MCST;

namespace Gomoku.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<ObservableCollection<TileViewModel>> Grid { get; set; }

    private readonly GameEngine _engine = new();

    public MainWindowViewModel()
    {
        Grid = new ObservableCollection<ObservableCollection<TileViewModel>>();
        _engine.BoardChanged += OnGameEngineTileChanged;
        InitializeGrid();
    }

    private void InitializeGrid()
    {
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