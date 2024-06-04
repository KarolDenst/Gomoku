﻿using MCST.Enums;

namespace MCST;

public class Node<TMove>(IMcstGame<TMove> gameState, TMove move, Node<TMove>? parent = null, int selectionConstant = 5) where TMove : IMove
{
    public readonly Node<TMove>? Parent = parent;
    public readonly List<Node<TMove>> Children = [];
    public readonly List<TMove> UntriedMoves = [..gameState.GetLegalMoves()]; // wszystkie wolne pola?
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
        // UCB1 selection policy
        var sortedChildren = mctsVersion switch
        {
            MctsVersion.BasicUct => Children.OrderByDescending(c => c._wins / c.Visits + Math.Sqrt(selectionConstant * Math.Log(Visits) / c.Visits)),
            MctsVersion.Heuristic => Children.OrderByDescending(c => c._wins / c.Visits + Math.Sqrt(selectionConstant * Math.Log(Visits) / c.Visits)),
			MctsVersion.Ucb1Tuned => Children.OrderByDescending(c =>
			{
				double averageReward = c._wins / c.Visits;
				double variance = (c._wins - c.Visits * averageReward * averageReward) / (c.Visits - 1);
				double ucbTunedValue = averageReward + Math.Sqrt(Math.Log(Visits) / c.Visits * Math.Min(0.25, variance + Math.Sqrt(2 * Math.Log(Visits) / c.Visits)));

				return ucbTunedValue;
			}),
			MctsVersion.Ucb1Normal => Children.
			 OrderByDescending(c => c._wins / c.Visits + Math.Sqrt(16*((c._wins - c.Visits * (c._wins / c.Visits) * (c._wins / c.Visits))/(c.Visits - 1)) * (Math.Log(Visits-1)/c.Visits))),
			_ => throw new ArgumentException($"Invalid MCTS version: {mctsVersion}"),
        };

        if (mctsVersion == MctsVersion.Heuristic)
        {
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
        }

		return sortedChildren.First();
	}

    public void Update(double result)
    {
        Visits++;
        _wins += result;
    }

    public TMove GetBestMove()
    {
		return Children.OrderByDescending(c => c.Visits).First().MoveMade;
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