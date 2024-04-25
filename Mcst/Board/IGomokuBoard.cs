namespace MCST.Board;

public interface IGomokuBoard
{
    int GetCell(int row, int col);
    void SetCell(int row, int col, int value);
    IGomokuBoard Clone();
    bool IsWinning(int row, int col, int winCount);
}