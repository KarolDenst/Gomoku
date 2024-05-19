namespace MCST.Board;

public class DataDuplicatedGomokuBoard : IGomokuBoard
{
    private ulong[] _rows;
    private ulong[] _cols;
    private ulong[] _diag1;
    private ulong[] _diag2;
    private readonly int _size;

    public DataDuplicatedGomokuBoard(int size)
    {
        throw new NotImplementedException("This does not work.");
        if (size > 21)
            throw new ArgumentException("Board size exceeds the maximum size that can be handled by a single ulong.");
        
        _size = size;
        _rows = new ulong[size];
        _cols = new ulong[size];
        _diag1 = new ulong[2 * size - 1];
        _diag2 = new ulong[2 * size - 1];
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
        
        shift = row * 3;
        mask = 0x07UL << shift;
        _cols[col] = (_cols[col] & ~mask) | ((ulong)value << shift);
        
        GetDiagonalPosition(row, col, out int diag1Index, out int diag1Shift, out int diag2Index, out int diag2Shift);
        mask = 0x07UL << diag1Shift;
        _diag1[diag1Index] = (_diag1[diag1Index] & ~mask) | ((ulong)value << diag1Shift);
        
        mask = 0x07UL << diag2Shift;
        _diag2[diag2Index] = (_diag2[diag2Index] & ~mask) | ((ulong)value << diag2Shift);
    }

    public IGomokuBoard Clone()
    {
        return new DataDuplicatedGomokuBoard(_size)
        {
            _rows = (ulong[])_rows.Clone(),
            _cols = (ulong[])_cols.Clone(),
            _diag1 = (ulong[])_diag1.Clone(),
            _diag2 = (ulong[])_diag2.Clone()
        };
    }

    public bool IsWinning(int row, int col, int winCount)
    {
        int value = GetCell(row, col);
        ulong winningMask = 0;
        for (int i = 0; i < winCount; i++)
        {
            winningMask |= ((ulong)value << (i * 3));
        }
        
        GetDiagonalPosition(row, col, out int diag1Index, out int diag1Shift, out int diag2Index, out int diag2Shift);
        return IsWinning(_rows, row, col * 3, winningMask, winCount) ||
               IsWinning(_cols, col, row * 3, winningMask, winCount) ||
               IsWinning(_diag1, diag1Index, diag1Shift, winningMask, winCount) ||
               IsWinning(_diag2, diag2Index, diag2Shift, winningMask, winCount);
    }
    
    private bool IsWinning(ulong[] collection, int index, int shift, ulong winningMask, int winCount)
    {
        int minShift = Math.Max(0, (shift - winCount + 1) * 3);
        int maxShift = Math.Min(shift * 3, (_size - winCount) * 3);

        ulong current = collection[index];

        for (int i = minShift; i <= maxShift; i += 3)
            if ((current & (winningMask << i)) == (winningMask << i))
                return true;

        return false;
    }
    
    private void GetDiagonalPosition(int row, int col, out int diag1Index, out int diag1Shift, out int diag2Index, out int diag2Shift)
    {
        // Diagonal 1
        diag1Index = row + col;
        int diag1StartCol = Math.Max(0, col - row);
        int diag1EffectiveCol = col - diag1StartCol;
        diag1Shift = diag1EffectiveCol * 3;

        // Diagonal 2
        diag2Index = row - col + _size - 1;
        int diag2StartCol = Math.Max(0, col - (_size - 1 - row));
        int diag2EffectiveCol = col - diag2StartCol;
        diag2Shift = diag2EffectiveCol * 3;
    }
}