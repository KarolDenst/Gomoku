using MCST.Board;

namespace MCST;

public class GomokuGame : IMcstGame<GomokuMove>
{
    private const int WinCount = 5;
    private readonly IGomokuBoard _board;
    private int _result = Tiles.Empty;
    private readonly Random _random = new();
    private readonly HashSet<GomokuMove> _legalMoves;
    
    public const int DefaultBoardSize = 7;
    public int NextMove = Tiles.Black;

    public GomokuGame(int size = DefaultBoardSize)
    {
        _board = new UlongGomokuBoard(size);
        _legalMoves = [];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (_board.GetCell(i, j) == Tiles.Empty)
                    _legalMoves.Add(new GomokuMove(i, j));
    }

    private GomokuGame(IGomokuBoard board, int result, int nextMove, HashSet<GomokuMove> legalMoves)
    {
        _board = board.Clone();
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
        _board.SetCell(move.Row, move.Col, NextMove);
        _legalMoves.Remove(move);
        if (_board.IsWinning(move.Row, move.Col, WinCount))
            _result = NextMove;
        SwitchNextMove();
    }

    public void UndoMove(GomokuMove move)
    {
        _board.SetCell(move.Row, move.Col, Tiles.Empty);
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
}