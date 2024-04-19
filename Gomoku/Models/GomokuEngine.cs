using System;

namespace Gomoku.Models;

public class GomokuEngine
{
    public Action<int, int, int> BoardChanged = null!; // row, col, tile
    public readonly int BoardSize;
    
    private readonly int[,] _board;
    private int _nextMove = Tiles.Black;
    
    public GomokuEngine(int size = Constants.DefaultBoardSize)
    {
        BoardSize = size;
        _board = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                _board[i, j] = Tiles.Empty;
            }
        }
    }

    public void MakeMove(int row, int col)
    {
        if (_board[row, col] != Tiles.Empty)
            return;
        
        _board[row, col] = _nextMove;
        BoardChanged?.Invoke(row, col, _nextMove);
        _nextMove = _nextMove == Tiles.Black ? Tiles.White : Tiles.Black;
    }
}