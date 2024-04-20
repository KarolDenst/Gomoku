namespace MCST;

public class BasicMcst<TMove>(int iterations, int scoreModifier = 1)
{
    private Random _random = new();
    
    public TMove FindBestMove(IMcstGame<TMove> game)
    {
        var rootNode = new Node<TMove>(game, default);

        for (int i = 0; i < iterations; i++)
        {
            var node = rootNode;
            var moveHistory = new Stack<TMove>();

            // Selection
            while (node.UntriedMoves.Count == 0 && node.Children.Count > 0)
            {
                node = node.SelectChild();
                game.MakeMove(node.MoveMade);
                moveHistory.Push(node.MoveMade);
            }

            // Expansion
            if (node.UntriedMoves.Count > 0)
            {
                var move = node.UntriedMoves.First();
                game.MakeMove(move);
                moveHistory.Push(move);
                node = node.AddChild(move, game);
            }

            // Simulation
            while (!game.IsGameOver())
            {
                var legalMoves = game.GetLegalMoves();
                Console.WriteLine(1);
                Console.WriteLine($"moves: {legalMoves.Count}");
                var randomMove = legalMoves[_random.Next(legalMoves.Count)];
                Console.WriteLine(2);
                game.MakeMove(randomMove);
                moveHistory.Push(randomMove);
            }

            // Backpropagation
            double result = scoreModifier * game.GetResult();
            while (node != null)
            {
                node.Update(result);
                node = node.Parent;
                if (node != null)  // Ensure we don't undo the move of the root node as it has no parent
                {
                    game.UndoMove(moveHistory.Pop());
                }
            }
            
            // Cleanup
            while (moveHistory.Count > 0)
                game.UndoMove(moveHistory.Pop());
        }

        // Return the best move based on the most visited node
        return rootNode.GetBestMove();
    }
}