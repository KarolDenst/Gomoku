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
        if (mctsVersion == MctsVersion.Ucb1Normal) parallelTasks = 1; // ucb1 normal can't be run in parallel
        for(int i = 0; i < parallelTasks; i++)
        {
            var task = Task.Run(() =>
            {
                var newGame = game.Clone();
                var node = new Node<TMove>(game, default);
                for (int j = 0; j < iterations / parallelTasks; j++)
                    RunIteration(newGame, node, j+1);
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

    private void RunIteration(IMcstGame<TMove> game, Node<TMove> rootNode, int iteration)
    {
        var node = rootNode;
        var moveHistory = new Stack<TMove>();
        var scoreModifier = game.GetDesiredOutcome();

        var simGame = game.Clone();
        node = Selection(simGame, node, moveHistory, iteration);
        node = Expansion(simGame, node, moveHistory);
        Simulation(simGame, moveHistory);
        Backpropagation(simGame, node, moveHistory, scoreModifier);
    }

    private Node<TMove> Selection(IMcstGame<TMove> game, Node<TMove> node, Stack<TMove> moveHistory, int iteration)
    {
		var requiredChildVisits = mctsVersion switch
		{
			MctsVersion.BasicUct => 0,
			MctsVersion.Ucb1Tuned => 2,
			MctsVersion.Ucb1Normal => Math.Ceiling(8 * Math.Log(iteration, 10)),
			_ => throw new ArgumentException($"Invalid MCTS version: {mctsVersion}"),
		};

        while (node.UntriedMoves.Count == 0 && node.Children.Count > 0)
        {
            foreach (var child in node.Children)
            {
                if (child.Visits < requiredChildVisits)
                {
                    return child;
                }
            }
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
            if (node != null && moveHistory.Count() > 0)  // Ensure we don't undo the move of the root node as it has no parent
            {
                game.UndoMove(moveHistory.Pop());
            }
        }
    }
}