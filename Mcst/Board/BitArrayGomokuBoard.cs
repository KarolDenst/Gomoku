using System.Collections;

namespace MCST.Board;

public class BitArrayGomokuBoard: IGomokuBoard
{
    private readonly BitArray _board;
    private readonly int _size;

    public BitArrayGomokuBoard(int size)
    {
        _size = size;
        _board = new BitArray(2 * size * size);
    }

    private BitArrayGomokuBoard(BitArray board, int size)
    {
        _size = size;
        _board = new BitArray(board);
    }

    public int GetCell(int row, int col)
    {
        int index = 2 * (row * _size + col);
        bool bit1 = _board.Get(index);
        bool bit2 = _board.Get(index + 1);

        return (bit2 ? 2 : 0) + (bit1 ? 1 : 0);
    }

    public void SetCell(int row, int col, int value)
    {
        int index = 2 * (row * _size + col);
        _board.Set(index, (value & 1) != 0);
        _board.Set(index + 1, (value & 2) != 0);
    }

    public IGomokuBoard Clone() => new BitArrayGomokuBoard(_board, _size);

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