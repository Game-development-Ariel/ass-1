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
     * Complete the 'nonDivisibleSubset' function below.
     *
     * The function is expected to return an INTEGER.
     * The function accepts following parameters:
     *  1. INTEGER k
     *  2. INTEGER_ARRAY s
     */

    public static int nonDivisibleSubset(int k, List<int> s)
    {
        // סופרים כמה מספרים יש מכל שארית בחלוקה ל-k
        int[] count = new int[k];
        foreach (int num in s)
            count[num % k]++;

        // ממספרים עם שארית 0 אפשר לקחת רק אחד
        int result = Math.Min(count[0], 1);

        // שארית i ושארית k-i ביחד מתחלקות ב-k, אז לוקחים רק את הקבוצה הגדולה מביניהן
        for (int i = 1; i <= k / 2; i++)
        {
            if (i == k - i)
                result += Math.Min(count[i], 1); // כש-k זוגי, שארית k/2 מתנהגת כמו שארית 0
            else
                result += Math.Max(count[i], count[k - i]);
        }

        return result;
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

        List<int> s = Console.ReadLine().TrimEnd().Split(' ').ToList().Select(sTemp => Convert.ToInt32(sTemp)).ToList();

        int result = Result.nonDivisibleSubset(k, s);

        textWriter.WriteLine(result);

        textWriter.Flush();
        textWriter.Close();
    }
}
