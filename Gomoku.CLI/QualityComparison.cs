using System.Diagnostics;
using MCST;


namespace Gomoku.CLI;

public static class QualityComparison
{
    public static void CompareQuality(int size = 8, int iterations = 120_000)
    {
        var stopwatch = Stopwatch.StartNew();

        var alg1 = new BasicMcst<GomokuMove>(iterations, MCST.Enums.MctsVersion.BasicUct);
        var alg2 = new BasicMcst<GomokuMove>(120_000, MCST.Enums.MctsVersion.BasicUct);
        // var alg2 = new AlphaBeta<GomokuMove>(2);

        int alg1Wins = 0;
        int alg2Wins = 0;
        int totalGames = 20;

        for (int i = 0; i < totalGames; i++)
        {
            var game = new GomokuGame(size);
            while (!game.IsGameOver())
            {
                if (i % 2 == 0)
                {
                    game.MakeMove(alg1.FindBestMove(game));
                    game.MakeMove(alg2.FindBestMove(game));
                }
                else
                {
                    game.MakeMove(alg2.FindBestMove(game));
                    game.MakeMove(alg1.FindBestMove(game));
                }
            }

            var result = game.GetResult();
            var eps = 0.1;
            if (Math.Abs(result - 0) < eps)
            {
                if (i % 2 == 0) alg1Wins++;
                else alg2Wins++;
            }
            else if (Math.Abs(result - 1) < eps)
            {
                if (i % 2 == 0) alg2Wins++;
                else alg1Wins++;
            }
        }

        Console.WriteLine($"Number of iterations: {iterations}");
        Console.WriteLine($"Total Games: {totalGames}");
        Console.WriteLine($"Alg1 Wins: {100.0 * alg1Wins / totalGames}%");
        Console.WriteLine($"Alg2 Wins: {100.0 * alg2Wins /totalGames}%");
        Console.WriteLine($"Draws: {100.0 * (totalGames - alg1Wins - alg2Wins) / totalGames}%");
        Console.WriteLine();
        Console.WriteLine($"Total Time: {stopwatch.Elapsed}");
    }
}