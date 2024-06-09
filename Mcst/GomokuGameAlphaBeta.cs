namespace MCST;

public partial class GomokuGame
{
    private const int FiveInARow = 1000000;
    private const int FourInARow = 10000;
    private const int ThreeInARow = 1000;
    private const int TwoInARow = 100;

    private readonly int[][] directions = [[1, 0], [0, 1], [1, 1], [1, -1]];

    public double EvaluateBoard()
    {
        int score = 0;

        for (int row = 0; row < _size; row++)
        {
            for (int col = 0; col < _size; col++)
            {
                if (_board.GetCell(row, col) != Tiles.Empty)
                {
                    score += EvaluatePosition(row, col);
                }
            }
        }

        return score;
    }

    private int EvaluatePosition(int row, int col)
    {
        int score = 0;
        int player = _board.GetCell(row, col);

        foreach (var direction in directions)
        {
            int count = 1;
            int r = row;
            int c = col;

            // Check forward direction
            while (true)
            {
                r += direction[0];
                c += direction[1];
                if (IsInBounds(r, c) && _board.GetCell(r, c) == player)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            // Check backward direction
            r = row;
            c = col;
            while (true)
            {
                r -= direction[0];
                c -= direction[1];
                if (IsInBounds(r, c) && _board.GetCell(r, c) == player)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            score += GetScore(count);
        }

        return score;
    }

    private int GetScore(int count)
    {
        switch (count)
        {
            case 5: return FiveInARow;
            case 4: return FourInARow;
            case 3: return ThreeInARow;
            case 2: return TwoInARow;
            default: return 0;
        }
    }
    
    private bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < _size && col >= 0 && col < _size;
    }
}