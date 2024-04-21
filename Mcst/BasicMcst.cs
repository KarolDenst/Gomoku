using System.Collections.Concurrent;

namespace MCST;

public class BasicMcst<TMove>(int iterations, int scoreModifier = 1)
{
    private readonly Random _random = new();
    
    public TMove FindBestMove(IMcstGame<TMove> game)
    {
        var tasks = new List<Task>();
        var sharedResults = new ConcurrentBag<Node<TMove>>();
        int parallelTasks = Environment.ProcessorCount;
        for(int i = 0; i < parallelTasks; i++)
        {
            var task = Task.Run(() =>
            {
                var newGame = game.Clone();
                var node = new Node<TMove>(game, default);
                for (int i = 0; i < iterations / parallelTasks; i++)
                    RunIteration(newGame, node);
                sharedResults.Add(node);
            });
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());

        var rootNode = new Node<TMove>(game, default);
        foreach (var result in sharedResults)
            rootNode.MergeResults(result);

        return rootNode.GetBestMove();
    }

    private void RunIteration(IMcstGame<TMove> game, Node<TMove> rootNode)
    {
        var node = rootNode;
        var moveHistory = new Stack<TMove>();

        node = Selection(game, node, moveHistory);
        node = Expansion(game, node, moveHistory);
        Simulation(game, moveHistory);
        Backpropagation(game, node, moveHistory);
        Cleanup(game, moveHistory);
    }

    private Node<TMove> Selection(IMcstGame<TMove> game, Node<TMove> node, Stack<TMove> moveHistory)
    {
        while (node.UntriedMoves.Count == 0 && node.Children.Count > 0)
        {
            node = node.SelectChild();
            game.MakeMove(node.MoveMade);
            moveHistory.Push(node.MoveMade);
        }

        return node;
    }

    private Node<TMove> Expansion(IMcstGame<TMove> game, Node<TMove> node, Stack<TMove> moveHistory)
    {
        if (node.UntriedMoves.Count <= 0) return node;
        var move = node.UntriedMoves.First();
        game.MakeMove(move);
        moveHistory.Push(move);
        node = node.AddChild(move, game);

        return node;
    }

    private void Simulation(IMcstGame<TMove> game, Stack<TMove> moveHistory)
    {
        while (!game.IsGameOver())
        {
            // var legalMoves = game.GetLegalMoves();
            // var randomMove = legalMoves[_random.Next(legalMoves.Count)];
            var randomMove = game.GetRandomMove();
            game.MakeMove(randomMove);
            moveHistory.Push(randomMove);
        }
    }

    private void Backpropagation(IMcstGame<TMove> game, Node<TMove> node, Stack<TMove> moveHistory)
    {
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
    }

    private void Cleanup(IMcstGame<TMove> game, Stack<TMove> moveHistory)
    {
        while (moveHistory.Count > 0)
            game.UndoMove(moveHistory.Pop());
    }
}