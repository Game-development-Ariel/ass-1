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
     * Complete the 'extraLongFactorials' function below.
     *
     * The function accepts INTEGER n as parameter.
     */

    public static void extraLongFactorials(int n)
    {
        // התוצאה גדולה מדי בשביל long, אז שומרים אותה כרשימה של ספרות
        // (במקום 0 נמצאת ספרת האחדות)
        List<int> digits = new List<int>();
        digits.Add(1);

        for (int i = 2; i <= n; i++)
        {
            int carry = 0;
            for (int j = 0; j < digits.Count; j++)
            {
                int x = digits[j] * i + carry;
                digits[j] = x % 10;
                carry = x / 10;
            }
            while (carry > 0)
            {
                digits.Add(carry % 10);
                carry /= 10;
            }
        }

        // מדפיסים מהספרה האחרונה לראשונה
        StringBuilder sb = new StringBuilder();
        for (int i = digits.Count - 1; i >= 0; i--)
            sb.Append(digits[i]);

        Console.WriteLine(sb.ToString());
    }

}


class Solution
{
    public static void Main(string[] args)
    {
        int n = Convert.ToInt32(Console.ReadLine().Trim());

        Result.extraLongFactorials(n);
    }
}
