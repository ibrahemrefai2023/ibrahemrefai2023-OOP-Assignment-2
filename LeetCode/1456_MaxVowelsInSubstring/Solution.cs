public class Solution
{
    public int MaxVowels(string s, int k)
    {
        var currentVowels = 0;
        var maxVowels = 0;

        for (var i = 0; i < k; i++)
        {
            if (IsVowel(s[i]))
            {
                currentVowels++;
            }
        }

        maxVowels = currentVowels;

        for (var i = k; i < s.Length; i++)
        {
            if (IsVowel(s[i - k]))
            {
                currentVowels--;
            }

            if (IsVowel(s[i]))
            {
                currentVowels++;
            }

            if (currentVowels > maxVowels)
            {
                maxVowels = currentVowels;
            }
        }

        return maxVowels;
    }

    private bool IsVowel(char character)
    {
        return character == 'a' || character == 'e' || character == 'i' || character == 'o' || character == 'u';
    }
}
