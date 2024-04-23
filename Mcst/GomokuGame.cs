namespace MCST;

public class GomokuGame : IMcstGame<GomokuMove>
{
    private const int WinCount = 5;
    private readonly int[,] _board;
    private int _result = Tiles.Empty;
    private int _moveCount = 0;
    private readonly Random _random = new();
    
    public const int DefaultBoardSize = 7;
    public int NextMove = Tiles.Black;

    public GomokuGame(int size = DefaultBoardSize)
    {
        _board = new int[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                _board[i, j] = Tiles.Empty;
    }

    private GomokuGame(int[,] board, int result, int moveCount, int nextMove)
    {
        _board = new int[board.GetLength(0), board.GetLength(1)];
        for (int i = 0; i < _board.GetLength(0); i++)
            for (int j = 0; j < _board.GetLength(1); j++)
                _board[i, j] = board[i, j];
        
        _result = result;
        _moveCount = moveCount;
        NextMove = nextMove;
    }

    public bool IsGameOver()
    {
        return _result != Tiles.Empty || _moveCount == _board.Length;
    }

    public List<GomokuMove> GetLegalMoves()
    {
        if (_result != Tiles.Empty || _moveCount == _board.Length)
            return [];
        
        var moves = new List<GomokuMove>();
        for (int i = 0; i < _board.GetLength(1); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                if (_board[i,j] == Tiles.Empty)
                    moves.Add(new GomokuMove(i, j));
            }
        }

        return moves;
    }

    public GomokuMove GetRandomMove()
    {
        while (true)
        {
            var row = _random.Next(_board.GetLength(0));
            var col = _random.Next(_board.GetLength(1));
            if (_board[row, col] == Tiles.Empty)
                return new GomokuMove(row, col);
        }
    }

    public void MakeMove(GomokuMove move)
    {
        if (_result != Tiles.Empty || _moveCount == _board.Length)
            return;
        _board[move.Row, move.Col] = NextMove;
        CheckForWin(move.Row, move.Col);
        SwitchNextMove();
        _moveCount++;
    }

    public void UndoMove(GomokuMove move)
    {
        _board[move.Row, move.Col] = Tiles.Empty;
        _result = Tiles.Empty;
        SwitchNextMove();
        _moveCount--;
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

    public IMcstGame<GomokuMove> Clone() => new GomokuGame(_board, _result, _moveCount, NextMove);
        
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