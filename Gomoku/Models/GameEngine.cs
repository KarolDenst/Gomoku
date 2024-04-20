using System;
using MCST;

namespace Gomoku.Models;

public class GameEngine
{
    public Action<int, int, int> BoardChanged = null!; // row, col, tile
    public readonly int BoardSize;

    private readonly GomokuGame _game;
    private readonly BasicMcst<GomokuMove> _opponent;
    
    
    public GameEngine(int size = Constants.DefaultBoardSize)
    {
        BoardSize = size;
        _game = new GomokuGame(size);
        _opponent = new BasicMcst<GomokuMove>(10, 1);
    }

    public void MakeMove(int row, int col)
    {
        var move = new GomokuMove(row, col);
        if (!_game.GetLegalMoves().Contains(move))
            return;
        BoardChanged?.Invoke(row, col, _game.NextMove);
        _game.MakeMove(move);
        
        var response = _opponent.FindBestMove(_game);
        Console.WriteLine($"Row: {response.Row}, Col: {response.Col}");
        BoardChanged?.Invoke(response.Row, response.Col, _game.NextMove);
        _game.MakeMove(response);
    }
}