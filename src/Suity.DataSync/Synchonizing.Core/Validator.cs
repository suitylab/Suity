using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Suity.Synchonizing.Core;

/// <summary>
/// Utility class for validating and finding objects
/// </summary>
public static class Validator
{
    public static IEnumerable<SyncPathReportItem> Find(object owner, object target, string findStr, SearchOption findOption, VisitFlag flag = VisitFlag.None)
    {
        List<SyncPathReportItem> reports = [];
        var context = new ValidationContext();

        Visitor.Visit<IValidate>(
            target,
            (validate, pathContext) =>
            {
                validate.Find(context, findStr, findOption);
                foreach (var item in context.Reports)
                {
                    string msg = item.Message ?? string.Empty;
                    if (msg.Length > 50)
                    {
                        msg = msg.Substring(0, 50) + "...";
                    }

                    var report = new SyncPathReportItem(owner, pathContext.GetPath(), item.Information, msg);
                    reports.Add(report);
                }
                context.Reports.Clear();
            },
            flag);

        return reports;
    }

    public static IEnumerable<SyncPathReportItem> Validate(object owner, object target, VisitFlag flag = VisitFlag.None)
    {
        List<SyncPathReportItem> reports = [];
        var context = new ValidationContext();

        Visitor.Visit<IValidate>(
            target,
            (validate, pathContext) =>
            {
                validate.Validate(context);
                foreach (var item in context.Reports)
                {
                    string msg = item.Message ?? string.Empty;
                    if (msg.Length > 50)
                    {
                        msg = msg.Substring(0, 50) + "...";
                    }

                    var report = new SyncPathReportItem(owner, pathContext.GetPath(), item.Information, msg);
                    reports.Add(report);
                }
                context.Reports.Clear();
            },
            flag);

        return reports;
    }

    public static bool Compare(string source, string find, SearchOption findOption)
    {
        if (string.IsNullOrEmpty(source))
        {
            return false;
        }

        if ((findOption & SearchOption.MatchCase) == 0)
        {
            source = source.ToLowerInvariant();
            find = find.ToLowerInvariant();
        }

        if ((findOption & SearchOption.MatchWholeWord) != 0)
        {
            //return source == find;
            string pattern = String.Format(@"\b{0}\b", find);

            try
            {
                var match = Regex.Match(source, pattern);

                return match.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
        else
        {
            return source.Contains(find);
        }
    }

    public static string ReplaceString(string input, string oldValue, string newValue, SearchOption option)
    {
        RegexOptions options = RegexOptions.None;

        if (!option.HasFlag(SearchOption.MatchCase))
        {
            options |= RegexOptions.IgnoreCase;
        }

        if (option.HasFlag(SearchOption.MatchWholeWord))
        {
            oldValue = "\\b" + oldValue + "\\b";
        }

        return Regex.Replace(input, oldValue, newValue, options);
    }
}