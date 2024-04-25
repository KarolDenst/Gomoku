﻿namespace MCST;

public class GomokuGame : IMcstGame<GomokuMove>
{
    private const int WinCount = 5;
    private readonly int[,] _board;
    private int _result = Tiles.Empty;
    private readonly Random _random = new();
    private readonly HashSet<GomokuMove> _legalMoves;
    
    public const int DefaultBoardSize = 11;
    public int NextMove = Tiles.Black;

    public GomokuGame(int size = DefaultBoardSize)
    {
        _board = new int[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                _board[i, j] = Tiles.Empty;
        _legalMoves = [];
        for (int i = 0; i < _board.GetLength(1); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                if (_board[i,j] == Tiles.Empty)
                    _legalMoves.Add(new GomokuMove(i, j));
            }
        }
    }

    private GomokuGame(int[,] board, int result, int nextMove, HashSet<GomokuMove> legalMoves)
    {
        _board = new int[board.GetLength(0), board.GetLength(1)];
        for (int i = 0; i < _board.GetLength(0); i++)
            for (int j = 0; j < _board.GetLength(1); j++)
                _board[i, j] = board[i, j];
        
        _result = result;
        NextMove = nextMove;
        _legalMoves = [];
        foreach (var move in legalMoves)
        {
            _legalMoves.Add(move);
        }
    }

    public bool IsGameOver()
    {
        return _result != Tiles.Empty || _legalMoves.Count == 0;
    }

    public List<GomokuMove> GetLegalMoves()
    {
        if (_result != Tiles.Empty)
            return [];

        return _legalMoves.ToList();
    }

    public GomokuMove GetRandomMove()
    {
        return _legalMoves.ElementAt(_random.Next(_legalMoves.Count));
    }

    public void MakeMove(GomokuMove move)
    {
        if (_result != Tiles.Empty || _legalMoves.Count == 0)
            return;
        _board[move.Row, move.Col] = NextMove;
        _legalMoves.Remove(move);
        CheckForWin(move.Row, move.Col);
        SwitchNextMove();
    }

    public void UndoMove(GomokuMove move)
    {
        _board[move.Row, move.Col] = Tiles.Empty;
        _result = Tiles.Empty;
        _legalMoves.Add(move);
        SwitchNextMove();
    }

    public double GetResult()
    {
        return _result switch
        {
            Tiles.White => 1,
            Tiles.Black => -1,
            _ => 0
        };
    }

    public IMcstGame<GomokuMove> Clone() => new GomokuGame(_board, _result, NextMove, _legalMoves);

    public int GetDesiredOutcome()
    {
        return NextMove switch
        {
            Tiles.White => 1,
            Tiles.Black => -1,
            _ => 0
        };
    }

    private void SwitchNextMove() =>
        NextMove = NextMove == Tiles.White ? Tiles.Black : Tiles.White;
    
    private void CheckForWin(int row, int col)
    {
        if (CountTiles(row, col, 1, 0) + CountTiles(row, col, -1, 0) >= WinCount - 1 ||
            CountTiles(row, col, 0, 1) + CountTiles(row, col, 0, -1) >= WinCount - 1 ||
            CountTiles(row, col, 1, 1) + CountTiles(row, col, -1, -1) >= WinCount - 1 ||
            CountTiles(row, col, 1, -1) + CountTiles(row, col, -1, 1) >= WinCount - 1)
        {
            _result = NextMove;
        }
    }

    private int CountTiles(int row, int col, int dRow, int dCol)
    {
        int count = 0;
        int currentRow = row + dRow;
        int currentCol = col + dCol;
        while (IsInBounds(currentRow, currentCol) && _board[currentRow, currentCol] == _board[row, col])
        {
            count++;
            currentRow += dRow;
            currentCol += dCol;
        }
        return count;
    }

    private bool IsInBounds(int row, int col) => row >= 0 && col >= 0 && row < _board.GetLength(0) && col < _board.GetLength(1);
}