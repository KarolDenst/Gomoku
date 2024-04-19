namespace MCST;

public class BasicMcst<TMove>(IMcstGame<TMove> game, int iterations)
{
    public TMove FindBestMove()
    {
        var rootNode = new Node<TMove>(game, default);

        for (int i = 0; i < iterations; i++)
        {
            var node = rootNode;
            var moveHistory = new List<TMove>();

            // Selection
            while (node.UntriedMoves.Count == 0 && node.Children.Count > 0)
            {
                node = node.SelectChild();
                game.MakeMove(node.MoveMade);
                moveHistory.Add(node.MoveMade);
            }

            // Expansion
            if (node.UntriedMoves.Count > 0)
            {
                var move = node.UntriedMoves.First();
                game.MakeMove(move);
                moveHistory.Add(move);
                node = node.AddChild(move, game);
            }

            // Simulation
            while (!game.IsGameOver())
            {
                var legalMoves = game.GetLegalMoves();
                var randomMove = legalMoves[new Random().Next(legalMoves.Count)];
                game.MakeMove(randomMove);
                moveHistory.Add(randomMove);
            }

            // Backpropagation
            double result = game.GetResult();
            while (node != null)
            {
                node.Update(result);
                node = node.Parent;
                if (node != null)  // Ensure we don't undo the move of the root node as it has no parent
                {
                    game.UndoMove(moveHistory.Last());
                    moveHistory.RemoveAt(moveHistory.Count - 1);
                }
            }
        }

        // Return the best move based on the most visited node
        return rootNode.GetBestMove();
    }
}