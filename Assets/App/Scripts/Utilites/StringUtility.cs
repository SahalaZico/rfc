using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class StringUtility
{
    public static List<int> ConvertStringToIntList(string input)
    {
        List<int> numbers = new List<int>();
        try
        {
            numbers = input
                .Split(',')
                .Select(int.Parse)
                .ToList();
        }
        catch {
            return new List<int>();
        }

        return numbers;
    }
    public static string ConvertToFormatNumber(float number)
    {
        if (number >= 1000000)
            return (number / 1000000f).ToString("0.##") + "M";
        else if (number >= 1000)
            return (number / 1000f).ToString("0.##") + "K";
        else
            return number.ToString("0.##");
    }

    public static string ConvertDoubleToString(double input)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        string formattedNumber = input.ToString("N0", cultureInfo);
        return formattedNumber;
    }

    public static int ConvertStringToInteger(string numberString, int falloutValue = 0)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        int number;

        if (Int32.TryParse(numberString, NumberStyles.AllowThousands, cultureInfo, out number))
        {
            return number;
        }
        else
        {
            return falloutValue;
        }
    }

    public static int[] ConvertStringToIntArray(string input)
    {
        return input.Split(',')
                    .Select(int.Parse)
                    .ToArray();
    }
}
