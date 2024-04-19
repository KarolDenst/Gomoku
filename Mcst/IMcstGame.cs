namespace MCST;

public interface IMcstGame<TMove>
{
    bool IsGameOver();
    List<TMove> GetLegalMoves();
    void MakeMove(TMove move);
    void UndoMove(TMove move);
    double GetResult();
}