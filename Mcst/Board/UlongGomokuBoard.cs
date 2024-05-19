namespace MCST.Board;

public class UlongGomokuBoard : IGomokuBoard
{
    private ulong[] _rows;
    private readonly int _size;

    public UlongGomokuBoard(int size)
    {
        if (size > 21)
            throw new ArgumentException("Board size exceeds the maximum size that can be handled by a single ulong.");

        _size = size;
        _rows = new ulong[size];
    }

    public int GetCell(int row, int col)
    {
        int shift = col * 3;
        return (int)((_rows[row] >> shift) & 0x07);
    }

    public void SetCell(int row, int col, int value)
    {
        int shift = col * 3;
        ulong mask = 0x07UL << shift;
        _rows[row] = (_rows[row] & ~mask) | ((ulong)value << shift);
    }

    public IGomokuBoard Clone() => new UlongGomokuBoard(_size) { _rows = (ulong[])_rows.Clone() };

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
        int value = GetCell(row, col);
        int currentRow = row + dRow;
        int currentCol = col + dCol;

        while (IsInBounds(currentRow, currentCol) && GetCell(currentRow, currentCol) == value)
        {
            count++;
            currentRow += dRow;
            currentCol += dCol;
        }
        return count;
    }

    private bool IsInBounds(int row, int col) => row >= 0 && col >= 0 && row < _size && col < _size;
}
