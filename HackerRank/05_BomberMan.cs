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
     * Complete the 'bomberMan' function below.
     *
     * The function is expected to return a STRING_ARRAY.
     * The function accepts following parameters:
     *  1. INTEGER n
     *  2. STRING_ARRAY grid
     */

    public static List<string> bomberMan(int n, List<string> grid)
    {
        // בשנייה הראשונה לא קורה כלום
        if (n == 1)
            return grid;

        // בכל שנייה זוגית בומברמן ממלא את כל הלוח בפצצות
        if (n % 2 == 0)
        {
            List<string> full = new List<string>();
            for (int i = 0; i < grid.Count; i++)
                full.Add(new string('O', grid[i].Length));
            return full;
        }

        // בשניות האי-זוגיות המצב חוזר על עצמו כל 4 שניות:
        // 3, 7, 11, ... -> פיצוץ אחד של הלוח המקורי
        // 5, 9, 13, ... -> שני פיצוצים
        List<string> afterFirst = detonate(grid);
        if (n % 4 == 3)
            return afterFirst;

        return detonate(afterFirst);
    }

    // מחזירה את הלוח אחרי שכל הפצצות שב-grid מתפוצצות
    // (כל שאר המשבצות היו מלאות בפצצות חדשות שעוד לא התפוצצו)
    private static List<string> detonate(List<string> grid)
    {
        int rows = grid.Count;
        int cols = grid[0].Length;

        // מתחילים מלוח מלא בפצצות
        char[][] result = new char[rows][];
        for (int i = 0; i < rows; i++)
            result[i] = new string('O', cols).ToCharArray();

        int[] dr = { 0, 1, -1, 0, 0 };
        int[] dc = { 0, 0, 0, 1, -1 };

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i][j] != 'O')
                    continue;

                // הפצצה מוחקת את המשבצת שלה ואת ארבע השכנות
                for (int d = 0; d < 5; d++)
                {
                    int r = i + dr[d];
                    int c = j + dc[d];
                    if (r >= 0 && r < rows && c >= 0 && c < cols)
                        result[r][c] = '.';
                }
            }
        }

        List<string> answer = new List<string>();
        for (int i = 0; i < rows; i++)
            answer.Add(new string(result[i]));
        return answer;
    }

}

class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        string[] firstMultipleInput = Console.ReadLine().TrimEnd().Split(' ');

        int r = Convert.ToInt32(firstMultipleInput[0]);

        int c = Convert.ToInt32(firstMultipleInput[1]);

        int n = Convert.ToInt32(firstMultipleInput[2]);

        List<string> grid = new List<string>();

        for (int i = 0; i < r; i++)
        {
            string gridItem = Console.ReadLine();
            grid.Add(gridItem);
        }

        List<string> result = Result.bomberMan(n, grid);

        textWriter.WriteLine(String.Join("\n", result));

        textWriter.Flush();
        textWriter.Close();
    }
}
