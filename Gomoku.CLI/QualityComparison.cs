using System.Diagnostics;
using MCST;


namespace Gomoku.CLI;

public static class QualityComparison
{
    public static void CompareQuality()
    {
        var stopwatch = Stopwatch.StartNew();
        int iterations = 10_000;
        int size = 7;

        var alg1 = new BasicMcst<GomokuMove>(iterations, MCST.Enums.MctsVersion.BasicUct);
        var alg2 = new BasicMcst<GomokuMove>(iterations, MCST.Enums.MctsVersion.BasicUct);

        int alg1Wins = 0;
        int alg2Wins = 0;
        int totalGames = 10;

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
            if (i % 2 != 0) result *= -1;
            if (result > 0) alg1Wins++;
            if (result < 0) alg2Wins++;
        }

        Console.WriteLine($"Alg1 Wins: {100.0 * alg1Wins / totalGames}%");
        Console.WriteLine($"Alg2 Wins: {100.0 * alg2Wins /totalGames}%");
        Console.WriteLine($"Draws: {100.0 * (totalGames - alg1Wins - alg2Wins) / totalGames}%");
        Console.WriteLine();
        Console.WriteLine($"Total Time: {stopwatch.Elapsed}");
    }
}