using MCST.Enums;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MCST;

public class BasicMcst<TMove>(int iterations, MctsVersion mctsVersion)
{
    public TMove FindBestMove(IMcstGame<TMove> game)
    {
        var tasks = new List<Task>();
        var sharedResults = new ConcurrentBag<Node<TMove>>();
        int parallelTasks = Environment.ProcessorCount;
        // int parallelTasks = 1;
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
        var scoreModifier = game.GetDesiredOutcome();

        var simGame = game.Clone();
        node = Selection(simGame, node, moveHistory);
        node = Expansion(simGame, node, moveHistory);
        Simulation(simGame, moveHistory);
        Backpropagation(simGame, node, moveHistory, scoreModifier);
    }

    private Node<TMove> Selection(IMcstGame<TMove> game, Node<TMove> node, Stack<TMove> moveHistory)
    {
        while (node.UntriedMoves.Count == 0 && node.Children.Count > 0)
        {
            node = node.SelectChild(mctsVersion);
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
            var randomMove = game.GetRandomMove();
            game.MakeMove(randomMove);
            moveHistory.Push(randomMove);
        }
    }

    private void Backpropagation(IMcstGame<TMove> game, Node<TMove> node, Stack<TMove> moveHistory, int scoreModifier)
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
}