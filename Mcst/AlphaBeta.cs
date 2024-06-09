namespace MCST;

public class AlphaBeta<TMove>(int maxDepth = 10) where TMove : IMove
{
    public TMove FindBestMove(IMcstGame<TMove> game)
    {
        bool isMaximizingPlayer = game.GetDesiredOutcome() == 1;
        TMove bestMove = default;
        double bestValue = isMaximizingPlayer ? double.NegativeInfinity : double.PositiveInfinity;
        double alpha = double.NegativeInfinity;
        double beta = double.PositiveInfinity;

        foreach (var move in game.GetLegalMoves())
        {
            game.MakeMove(move);
            double moveValue = AlphaBetaSearch(game, 1, alpha, beta, !isMaximizingPlayer);
            game.UndoMove(move);

            if (isMaximizingPlayer)
            {
                if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    bestMove = move;
                    alpha = Math.Max(alpha, moveValue);
                }
            }
            else
            {
                if (moveValue < bestValue)
                {
                    bestValue = moveValue;
                    bestMove = move;
                    beta = Math.Min(beta, moveValue);
                }
            }
        }

        return bestMove;
    }

    private double AlphaBetaSearch(IMcstGame<TMove> game, int depth, double alpha, double beta, bool maximizingPlayer)
    {
        if (game.IsGameOver() || depth == maxDepth)
        {
            return (game.GetResult() - 0.5) * 1000000;
        }

        if (maximizingPlayer)
            return Search(game, depth, alpha, beta, maximizingPlayer);
        else
            return Search(game, depth, alpha, beta, maximizingPlayer);
    }

    private double Search(IMcstGame<TMove> game, int depth, double alpha, double beta, bool maximizingPlayer)
    {
        double bestEval = maximizingPlayer ? double.NegativeInfinity : double.PositiveInfinity;

        foreach (var move in game.GetLegalMoves())
        {
            game.MakeMove(move);
            double eval = AlphaBetaSearch(game, depth + 1, alpha, beta, !maximizingPlayer);
            game.UndoMove(move);

            if (maximizingPlayer)
            {
                bestEval = Math.Max(bestEval, eval);
                alpha = Math.Max(alpha, eval);
            }
            else
            {
                bestEval = Math.Min(bestEval, eval);
                beta = Math.Min(beta, eval);
            }

            if (beta <= alpha)
            {
                break;
            }
        }

        return bestEval;
    }
}