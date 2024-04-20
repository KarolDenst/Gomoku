namespace MCST;

public class Node<TMove>(IMcstGame<TMove> gameState, TMove move, Node<TMove>? parent = null)
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
        return Children.OrderByDescending(c => c._wins / c._visits + Math.Sqrt(2 * Math.Log(_visits) / c._visits)).First();
    }

    public void Update(double result)
    {
        _visits++;
        _wins += result;
    }

    public TMove GetBestMove()
    {
        return Children.OrderByDescending(c => c._wins / c._visits).Select(c => c.MoveMade).FirstOrDefault()!;
    }
}