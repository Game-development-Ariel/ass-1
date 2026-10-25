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
     * Complete the 'queensAttack' function below.
     *
     * The function is expected to return an INTEGER.
     * The function accepts following parameters:
     *  1. INTEGER n
     *  2. INTEGER k
     *  3. INTEGER r_q
     *  4. INTEGER c_q
     *  5. 2D_INTEGER_ARRAY obstacles
     */

    public static int queensAttack(int n, int k, int r_q, int c_q, List<List<int>> obstacles)
    {
        // שומרים את כל המכשולים ב-HashSet כדי לבדוק מהר אם יש מכשול במשבצת
        HashSet<string> blocked = new HashSet<string>();
        for (int i = 0; i < k; i++)
        {
            blocked.Add(obstacles[i][0] + "," + obstacles[i][1]);
        }

        // שמונה הכיוונים: למעלה, למטה, ימינה, שמאלה וארבעת האלכסונים
        int[] dr = { 1, -1, 0, 0, 1, 1, -1, -1 };
        int[] dc = { 0, 0, 1, -1, 1, -1, 1, -1 };

        int count = 0;

        for (int d = 0; d < 8; d++)
        {
            int r = r_q + dr[d];
            int c = c_q + dc[d];

            // מתקדמים בכיוון עד שיוצאים מהלוח או נתקלים במכשול
            while (r >= 1 && r <= n && c >= 1 && c <= n)
            {
                if (blocked.Contains(r + "," + c))
                    break;

                count++;
                r += dr[d];
                c += dc[d];
            }
        }

        return count;
    }

}

class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        string[] firstMultipleInput = Console.ReadLine().TrimEnd().Split(' ');

        int n = Convert.ToInt32(firstMultipleInput[0]);

        int k = Convert.ToInt32(firstMultipleInput[1]);

        string[] secondMultipleInput = Console.ReadLine().TrimEnd().Split(' ');

        int r_q = Convert.ToInt32(secondMultipleInput[0]);

        int c_q = Convert.ToInt32(secondMultipleInput[1]);

        List<List<int>> obstacles = new List<List<int>>();

        for (int i = 0; i < k; i++)
        {
            obstacles.Add(Console.ReadLine().TrimEnd().Split(' ').ToList().Select(obstaclesTemp => Convert.ToInt32(obstaclesTemp)).ToList());
        }

        int result = Result.queensAttack(n, k, r_q, c_q, obstacles);

        textWriter.WriteLine(result);

        textWriter.Flush();
        textWriter.Close();
    }
}
