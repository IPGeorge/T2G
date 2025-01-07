using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Utilities
{
    public static float CalculateStringsSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
        {
            return 0.0f;
        }

        var words1 = str1.Split(' ');
        var words2 = str2.Split(' ');

        int matchCount = words1.Intersect(words2).Count();
        int maxLength = Math.Max(words1.Length, words2.Length);

        return (float)matchCount / maxLength;
    }

    public static bool StringsAreSimilar(string str1, string str2, double percentThreshold)
    {
        float matchPercent = CalculateStringsSimilarity(str1, str2);
        return matchPercent >= percentThreshold;
    }

    public static bool FindTopMatches(string prompt, string[] standardPrompts, int topN, float similarity, ref List<string> foundStandardPrompts)
    {
        if (string.IsNullOrEmpty(prompt) || standardPrompts == null || standardPrompts.Length == 0)
        {
            foundStandardPrompts.Clear();
            return false;
        }

        prompt = prompt.ToLower();
        foundStandardPrompts = standardPrompts
            .Select(str => new { String = str, Similarity = CalculateStringsSimilarity(prompt, str) })
            .Where(x => x.Similarity >= similarity)
            .OrderByDescending(x => x.Similarity)
            .Take(topN)
            .Select(x => x.String)
            .ToList();
        return (foundStandardPrompts.Count > 0);
    }

}

