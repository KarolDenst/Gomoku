namespace MCST;

public interface IMove
{
	int Row { get; }
	int Col { get; }
}

public struct GomokuMove(int row, int col) : IMove
{
	public int Row { get; } = row;
	public int Col { get; } = col;
}