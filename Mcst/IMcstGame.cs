using MCST.Board;

namespace MCST;

public interface IMcstGame<TMove>
{
    bool IsGameOver();
    List<TMove> GetLegalMoves();
	TMove GetRandomMove();
    void MakeMove(TMove move);
    void UndoMove(TMove move);
    double GetResult();
    IMcstGame<TMove> Clone();
    int GetDesiredOutcome();
	IGomokuBoard GetBoard();
    TMove GetMiddleOfBoard();
	List<GomokuMove> GetNeighbors(int row, int col);
	double EvaluateBoard();
}