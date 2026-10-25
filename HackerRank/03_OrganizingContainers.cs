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
     * Complete the 'organizingContainers' function below.
     *
     * The function is expected to return a STRING.
     * The function accepts 2D_INTEGER_ARRAY container as parameter.
     */

    public static string organizingContainers(List<List<int>> container)
    {
        int n = container.Count;

        // capacity[i] = כמה כדורים יש בקופסה i (סכום השורה)
        // typeCount[j] = כמה כדורים יש מסוג j בסך הכל (סכום העמודה)
        // משתמשים ב-long כי הסכומים יכולים לעבור את הגבול של int
        long[] capacity = new long[n];
        long[] typeCount = new long[n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                capacity[i] += container[i][j];
                typeCount[j] += container[i][j];
            }
        }

        // ההחלפות לא משנות את כמות הכדורים בכל קופסה,
        // לכן כל סוג צריך להתאים בדיוק לקופסה עם אותה כמות.
        // אחרי מיון שני המערכים צריכים להיות זהים.
        Array.Sort(capacity);
        Array.Sort(typeCount);

        for (int i = 0; i < n; i++)
        {
            if (capacity[i] != typeCount[i])
                return "Impossible";
        }

        return "Possible";
    }

}

class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        int q = Convert.ToInt32(Console.ReadLine().Trim());

        for (int qItr = 0; qItr < q; qItr++)
        {
            int n = Convert.ToInt32(Console.ReadLine().Trim());

            List<List<int>> container = new List<List<int>>();

            for (int i = 0; i < n; i++)
            {
                container.Add(Console.ReadLine().TrimEnd().Split(' ').ToList().Select(containerTemp => Convert.ToInt32(containerTemp)).ToList());
            }

            string result = Result.organizingContainers(container);

            textWriter.WriteLine(result);
        }

        textWriter.Flush();
        textWriter.Close();
    }
}
