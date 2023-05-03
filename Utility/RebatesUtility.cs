using System;

namespace RebatesAPI.Utility
{
    public class RebatesUtility
    {

        public static bool SearchValueInSemicolonDelimitedString(string inputString, string searchValue)
        {
            // Split the input string by semicolon delimiter
            string[] values = inputString.Split(';');

            // Trim leading and trailing whitespaces from each value
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }

            // Search for the searchValue in the array of values
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == searchValue)
                {
                    return true;
                }
            }
            return false;
        }

        public static string RemoveRECAndGetNumber(string input)
        {
            string result = string.Empty;

            // Check if the input starts with "REC"
            if (input.StartsWith("REC"))
            {
                // Remove "REC" from the input
                string withoutREC = input.Substring(3);

                // Loop through each character in the remaining string
                foreach (char c in withoutREC)
                {
                    // Check if the character is a letter
                    if (char.IsLetter(c))
                    {
                        // Stop processing when a letter is encountered
                        break;
                    }
                    else
                    {
                        // Add the character to the result if it's a digit
                        result += c;
                    }
                }
            }

            return result;
        }
    }
    
    
}
