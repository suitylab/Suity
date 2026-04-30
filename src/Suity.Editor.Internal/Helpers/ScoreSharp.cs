using Suity.Editor;
using Suity.Selecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Helpers;

/// <summary>
/// Provides fuzzy scoring and sorting utilities for matching abbreviations against words.
/// Used for implementing quick search and autocomplete functionality.
/// </summary>
public static class ScoreSharp
{
    /// <summary>
    /// Calculates a fuzzy matching score between a word and an abbreviation.
    /// </summary>
    /// <param name="word">The full word to match against.</param>
    /// <param name="abbrv">The abbreviation or search query.</param>
    /// <param name="fuzzines">Fuzziness tolerance (0 = strict, higher values allow more leniency). Default is 0.</param>
    /// <returns>A score between 0 and 1, where 1 indicates a perfect match.</returns>
    public static double score(string word, string abbrv, double fuzzines = 0)
    {
        double total_char_score = 0, abbrv_size = abbrv.Length,
            fuzzies = 1, final_score, abbrv_score;
        int word_size = word.Length;
        bool start_of_word_bonus = false;

        //If strings are equal, return 1.0
        if (word == abbrv) return 1.0;

        int index_in_string,
            index_char_lowercase,
            index_char_uppercase,
            min_index;
        double char_score;
        string c;
        for (int i = 0; i < abbrv_size; i++)
        {
            c = abbrv[i].ToString();
            index_char_uppercase = word.IndexOf(c.ToUpper());
            index_char_lowercase = word.IndexOf(c.ToLower());
            min_index = Math.Min(index_char_lowercase, index_char_uppercase);

            //Finds first valid occurrence
            //In upper or lowercase
            index_in_string = min_index > -1 ?
                min_index : Math.Max(index_char_lowercase, index_char_uppercase);

            //If no value is found
            //Check if fuzzines is allowed
            if (index_in_string == -1)
            {
                if (fuzzines > 0)
                {
                    fuzzies += 1 - fuzzines;
                    continue;
                }
                else return 0;
            }
            else
                char_score = 0.1;

            //Check if current char is the same case
            //Then add bonus
            if (word[index_in_string].ToString() == c) char_score += 0.1;

            //Check if char matches the first letter
            //And add bonnus for consecutive letters
            if (index_in_string == 0)
            {
                char_score += 0.6;

                //Check if the abbreviation
                //is in the start of the word
                start_of_word_bonus = i == 0;
            }
            else
            {
                // Acronym Bonus
                // Weighing Logic: Typing the first character of an acronym is as if you
                // preceded it with two perfect character matches.
                if (word.ElementAtOrDefault(index_in_string - 1).ToString() == " ") char_score += 0.8;
            }

            //Remove the start of string, so we don't reprocess it
            word = word.Substring(index_in_string + 1);

            //sum chars scores
            total_char_score += char_score;
        }

        abbrv_score = total_char_score / abbrv_size;

        //Reduce penalty for longer words
        final_score = ((abbrv_score * (abbrv_size / word_size)) + abbrv_score) / 2;

        //Reduce using fuzzies;
        final_score = final_score / fuzzies;

        //Process start of string bonus
        if (start_of_word_bonus && final_score <= 0.85)
            final_score += 0.15;

        return final_score;
    }

    /// <summary>
    /// Sorts an array of strings by their fuzzy match score against the given match string, in descending order.
    /// </summary>
    /// <param name="arr">The array of strings to sort.</param>
    /// <param name="match">The string to match against for scoring.</param>
    /// <returns>The sorted array.</returns>
    public static string[] sorter(string[] arr, string match)
    {
        Array.Sort(arr, new CompareScore(match));
        return arr;
    }

    /// <summary>
    /// Sorts an array of <see cref="ISelectionItem"/> by their fuzzy match score against the given match string, in descending order.
    /// </summary>
    /// <param name="arr">The array of selection items to sort.</param>
    /// <param name="match">The string to match against for scoring.</param>
    /// <returns>The sorted array.</returns>
    public static ISelectionItem[] sorter(ISelectionItem[] arr, string match)
    {
        Array.Sort(arr, new SelectionItemScoreComparer(match));
        return arr;
    }
}

/// <summary>
/// A comparer that sorts strings based on their fuzzy match score against a reference string.
/// </summary>
public class CompareScore : IComparer<string>
{
    private readonly string match;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompareScore"/> class.
    /// </summary>
    /// <param name="match">The reference string to compare against.</param>
    public CompareScore(string match)
    {
        this.match = match;
    }

    /// <inheritdoc/>
    public int Compare(string obj1, string obj2)
    {
        int retorno;
        double comparison = ScoreSharp.score(obj2, this.match) - ScoreSharp.score(obj1, this.match);
        if (comparison > 0)
            retorno = 1;
        else if (comparison < 0)
            retorno = -1;
        else
            retorno = 0;
        return retorno;
    }
}

/// <summary>
/// A comparer that sorts <see cref="ISelectionItem"/> objects based on their fuzzy match score against a reference string.
/// </summary>
public class SelectionItemScoreComparer : IComparer<ISelectionItem>
{
    private readonly string match;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionItemScoreComparer"/> class.
    /// </summary>
    /// <param name="match">The reference string to compare against.</param>
    public SelectionItemScoreComparer(string match)
    {
        this.match = match;
    }

    /// <inheritdoc/>
    public int Compare(ISelectionItem obj1, ISelectionItem obj2)
    {
        if (string.IsNullOrEmpty(obj1.SelectionKey))
        {
            return -1;
        }
        if (string.IsNullOrEmpty(obj2.SelectionKey))
        {
            return 1;
        }

        int retorno;

        double fuzz = 0;

        string s1 = $"{obj1.ToDisplayText()} {obj1.SelectionKey}";
        string s2 = $"{obj2.ToDisplayText()} {obj2.SelectionKey}";

        double comparison = ScoreSharp.score(s2, this.match, fuzz) - ScoreSharp.score(s1, this.match, fuzz);
        if (comparison > 0)
        {
            retorno = 1;
        }
        else if (comparison < 0)
        {
            retorno = -1;
        }
        else
        {
            retorno = 0;
        }

        return retorno;
    }
}
