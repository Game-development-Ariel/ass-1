using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;


class Result
{

    /*
     * Complete the 'climbingLeaderboard' function below.
     *
     * The function is expected to return an INTEGER_ARRAY.
     * The function accepts following parameters:
     *  1. INTEGER_ARRAY ranked
     *  2. INTEGER_ARRAY player
     */

    public static List<int> climbingLeaderboard(List<int> ranked, List<int> player)
    {
        // מורידים כפילויות מהטבלה, כי לאותו ניקוד יש אותו דירוג
        List<int> scores = new List<int>();
        for (int i = 0; i < ranked.Count; i++)
        {
            if (scores.Count == 0 || scores[scores.Count - 1] != ranked[i])
                scores.Add(ranked[i]);
        }

        List<int> result = new List<int>();

        // הניקוד של השחקן רק עולה, אז מתחילים מסוף הטבלה ועולים למעלה
        int index = scores.Count - 1;
        for (int i = 0; i < player.Count; i++)
        {
            while (index >= 0 && player[i] >= scores[index])
                index--;

            // השחקן נמצא מיד אחרי index, והדירוג מתחיל מ-1
            result.Add(index + 2);
        }

        return result;
    }

}


class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        int rankedCount = Convert.ToInt32(Console.ReadLine().Trim());

        List<int> ranked = Console.ReadLine().TrimEnd().Split(' ').ToList().Select(rankedTemp => Convert.ToInt32(rankedTemp)).ToList();

        int playerCount = Convert.ToInt32(Console.ReadLine().Trim());

        List<int> player = Console.ReadLine().TrimEnd().Split(' ').ToList().Select(playerTemp => Convert.ToInt32(playerTemp)).ToList();

        List<int> result = Result.climbingLeaderboard(ranked, player);

        textWriter.WriteLine(String.Join("\n", result));

        textWriter.Flush();
        textWriter.Close();
    }
}
