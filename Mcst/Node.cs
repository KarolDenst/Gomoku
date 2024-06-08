using MCST.Enums;

namespace MCST;

public class Node<TMove>(IMcstGame<TMove> gameState, TMove move, Node<TMove>? parent = null, int selectionConstant = 5) where TMove : IMove
{
    public readonly Node<TMove>? Parent = parent;
    public List<Node<TMove>> Children = new();
    public readonly List<TMove> UntriedMoves = new(gameState.GetLegalMoves()); // wszystkie wolne pola?
	public readonly TMove MoveMade = move;
    private double _wins = 0;
    public int Visits = 0;

    public Node<TMove> AddChild(TMove move, IMcstGame<TMove> gameState)
    {
        var childNode = new Node<TMove>(gameState, move, this);
        UntriedMoves.Remove(move);
        Children.Add(childNode);
        return childNode;
    }

    public Node<TMove> SelectChild(MctsVersion mctsVersion, IMcstGame<TMove> game)
    {
        if (mctsVersion == MctsVersion.Heuristic)
        {
            var sortedChildren = Children.OrderByDescending(c =>
                c._wins / c.Visits + Math.Sqrt(selectionConstant * Math.Log(Visits) / c.Visits));
            foreach (var child in sortedChildren)
            {
                var neighbors = game.GetNeighbors(child.MoveMade.Row, child.MoveMade.Col);
                foreach (var neighbor in neighbors)
                {
                    if (game.GetBoard().GetCell(neighbor.Row, neighbor.Col) != Tiles.Empty)
                    {
                        return child;
                    }
                }
            }
            return sortedChildren.First();
        }

        return mctsVersion switch
        {
            MctsVersion.BasicUct => Children.MaxBy(c =>
                c._wins / c.Visits + Math.Sqrt(selectionConstant * Math.Log(Visits) / c.Visits)),
            MctsVersion.Ucb1Tuned => Children.MaxBy(c =>
            {
                double averageReward = c._wins / c.Visits;
                double variance = (c._wins - c.Visits * averageReward * averageReward) / (c.Visits - 1);
                double ucbTunedValue = averageReward + Math.Sqrt(Math.Log(Visits) / c.Visits *
                                                                 Math.Min(0.25,
                                                                     variance + Math.Sqrt(2 * Math.Log(Visits) /
                                                                         c.Visits)));

                return ucbTunedValue;
            }),
            MctsVersion.Ucb1Normal => Children.MaxBy(c =>
                c._wins / c.Visits + Math.Sqrt(16 *
                                               ((c._wins - c.Visits * (c._wins / c.Visits) * (c._wins / c.Visits)) /
                                                (c.Visits - 1)) * (Math.Log(Visits - 1) / c.Visits))),
            _ => throw new ArgumentException($"Invalid MCTS version: {mctsVersion}"),
        };
    }

    public void Update(double result)
    {
        Visits++;
        _wins += result;
    }

    public TMove GetBestMove(MctsVersion mctsVersion, IMcstGame<TMove> game)
    {
        if (Children.Count == 0 && mctsVersion == MctsVersion.Heuristic)
        {
            return game.GetMiddleOfBoard();
        }

        return Children.MaxBy(c => c.Visits).MoveMade;
	}
    
    public void MergeResults(Node<TMove> other)
    {
        if (!MoveMade!.Equals(other.MoveMade)) return;
        Visits += other.Visits;
        _wins += other._wins;

        foreach (var otherChild in other.Children)
        {
            var match = Children.Find(c => c.MoveMade!.Equals(otherChild.MoveMade));
            if (match != null)
            {
                match.Visits += otherChild.Visits;
                match._wins += otherChild._wins;
            }
            else
                Children.Add(otherChild);
        }
    }
}