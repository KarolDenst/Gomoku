namespace MCST;

public class Node<TMove>(IMcstGame<TMove> gameState, TMove move, Node<TMove>? parent = null, int selectionConstant = 5)
{
    public readonly Node<TMove>? Parent = parent;
    public readonly List<Node<TMove>> Children = [];
    public readonly List<TMove> UntriedMoves = [..gameState.GetLegalMoves()];
    public readonly TMove MoveMade = move;
    private double _wins = 0;
    private int _visits = 0;

    public Node<TMove> AddChild(TMove move, IMcstGame<TMove> gameState)
    {
        var childNode = new Node<TMove>(gameState, move, this);
        UntriedMoves.Remove(move);
        Children.Add(childNode);
        return childNode;
    }

    public Node<TMove> SelectChild()
    {
        // UCB1 selection policy
        return Children.OrderByDescending(c => c._wins / c._visits + Math.Sqrt(selectionConstant * Math.Log(_visits) / c._visits)).First();
    }

    public void Update(double result)
    {
        _visits++;
        _wins += result;
    }

    public TMove GetBestMove()
    {
        return Children.OrderByDescending(c => c._visits).Select(c => c.MoveMade).FirstOrDefault()!;
    }
    
    public void MergeResults(Node<TMove> other)
    {
        if (!MoveMade!.Equals(other.MoveMade)) return;
        _visits += other._visits;
        _wins += other._wins;

        foreach (var otherChild in other.Children)
        {
            var match = Children.Find(c => c.MoveMade!.Equals(otherChild.MoveMade));
            if (match != null)
            {
                match._visits += otherChild._visits;
                match._wins += otherChild._wins;
            }
            else
                Children.Add(otherChild);
        }
    }
}