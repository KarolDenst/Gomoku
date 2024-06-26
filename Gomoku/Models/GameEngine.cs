﻿using System;
using MCST;
using MCST.Enums;

namespace Gomoku.Models;

public class GameEngine(int size = GomokuGame.DefaultBoardSize, int iterations = 300_000, MctsVersion version = MctsVersion.BasicUct)
{
    public Action<int, int, int> BoardChanged = null!; // row, col, tile
    public readonly int BoardSize = size;

    private readonly GomokuGame _game = new(size);
    private readonly BasicMcst<GomokuMove> _opponent = new(iterations, version);


    public void MakeMove(int row, int col)
    {
        var move = new GomokuMove(row, col);
        if (!_game.GetLegalMoves().Contains(move))
            return;
        BoardChanged?.Invoke(row, col, _game.NextMove);
        _game.MakeMove(move);
        if (_game.IsGameOver())
            return;
        
        var response = _opponent.FindBestMove(_game);
        BoardChanged?.Invoke(response.Row, response.Col, _game.NextMove);
        _game.MakeMove(response);
    }
}