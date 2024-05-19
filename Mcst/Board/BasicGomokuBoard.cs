namespace MCST.Board;

public class BasicGomokuBoard : IGomokuBoard
{
    private readonly int[,] _board;
    
    public BasicGomokuBoard(int size)
    {
        _board = new int[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                _board[i, j] = Tiles.Empty;
    }

    private BasicGomokuBoard(int[,] board)
    {
        _board = new int[board.GetLength(0), board.GetLength(1)];
        for (int i = 0; i < _board.GetLength(0); i++)
            for (int j = 0; j < _board.GetLength(1); j++)
                _board[i, j] = board[i, j];
    }


    public int GetCell(int row, int col) => _board[row, col];

    public void SetCell(int row, int col, int value) => _board[row, col] = value;

    public IGomokuBoard Clone() => new BasicGomokuBoard(_board);
    
    public bool IsWinning(int row, int col, int winCount)
    {
        return CountTiles(row, col, 1, 0) + CountTiles(row, col, -1, 0) >= winCount - 1 ||
               CountTiles(row, col, 0, 1) + CountTiles(row, col, 0, -1) >= winCount - 1 ||
               CountTiles(row, col, 1, 1) + CountTiles(row, col, -1, -1) >= winCount - 1 ||
               CountTiles(row, col, 1, -1) + CountTiles(row, col, -1, 1) >= winCount - 1;
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