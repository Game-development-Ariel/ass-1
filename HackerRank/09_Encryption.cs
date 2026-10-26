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
     * Complete the 'encryption' function below.
     *
     * The function is expected to return a STRING.
     * The function accepts STRING s as parameter.
     */

    public static string encryption(string s)
    {
        // קודם מורידים את הרווחים
        s = s.Replace(" ", "");
        int len = s.Length;

        // מספר העמודות הוא השורש של האורך מעוגל למעלה
        int cols = (int)Math.Ceiling(Math.Sqrt(len));

        // קוראים את הטקסט עמודה אחרי עמודה, וכל עמודה היא מילה
        List<string> words = new List<string>();
        for (int c = 0; c < cols; c++)
        {
            string word = "";
            for (int index = c; index < len; index += cols)
                word += s[index];
            words.Add(word);
        }

        return string.Join(" ", words);
    }

}


class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        string s = Console.ReadLine();

        string result = Result.encryption(s);

        textWriter.WriteLine(result);

        textWriter.Flush();
        textWriter.Close();
    }
}
