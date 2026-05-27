using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Suity.Editor.AIGC.Tools;

internal static class StringUtility
{
    public struct MatchResult
    {
        public int Index;
        public int Length;
        public bool Found => Index >= 0;
        public static MatchResult NotFound => new() { Index = -1, Length = 0 };
    }

    public static MatchResult IndexOfContent(string content, string pattern, int startIndex = 0)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(pattern))
            return MatchResult.NotFound;

        int patternLen = pattern.Length;
        int contentLen = content.Length;

        for (int i = startIndex; i <= contentLen - patternLen; i++)
        {
            int pi = 0;
            int ci = i;
            int matchStart = i;

            while (pi < patternLen && ci < contentLen)
            {
                char pc = pattern[pi];
                char cc = content[ci];

                if (cc == '\r' && pi < patternLen - 1 && pattern[pi + 1] == '\n')
                    continue;

                if (pc == '\r' && cc == '\n')
                {
                    if (pi + 1 < patternLen && pattern[pi + 1] == '\n')
                    {
                        pi += 2;
                        ci += 2;
                        continue;
                    }
                    if (ci + 1 < contentLen && content[ci + 1] == '\n')
                    {
                        pi++;
                        ci += 2;
                        continue;
                    }
                }

                if (pc == '\n')
                {
                    if (cc == '\r')
                    {
                        if (ci + 1 < contentLen && content[ci + 1] == '\n')
                        {
                            ci++;
                        }
                        else if (pi + 1 < patternLen && pattern[pi + 1] == '\n')
                        {
                            pi++;
                            ci++;
                            continue;
                        }
                    }
                    if (cc == '\n' || cc == '\r')
                    {
                        pi++;
                        ci++;
                        continue;
                    }
                }

                if (pc == cc)
                {
                    pi++;
                    ci++;
                    continue;
                }

                break;
            }

            if (pi >= patternLen)
                return new MatchResult { Index = matchStart, Length = ci - matchStart };
        }

        return MatchResult.NotFound;
    }

    /// <summary>
    /// 在源文本中从指定索引开始宽松搜索目标模式，忽略所有空白字符差异。
    /// </summary>
    /// <param name="source">源文本</param>
    /// <param name="pattern">要查找的代码段落（可含任意空白）</param>
    /// <param name="startIndex">起始搜索索引（包含），默认为 0。负数视为 0，超出长度则返回 NotFound</param>
    /// <returns>匹配结果（包含索引和长度），未找到则返回 NotFound</returns>
    public static MatchResult FuzzyMatch(this string source, string pattern, int startIndex = 0)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
            return MatchResult.NotFound;

        // 规范化 startIndex
        if (startIndex < 0)
            startIndex = 0;
        if (startIndex >= source.Length)
            return MatchResult.NotFound;

        // 将 pattern 按连续空白分割成非空 token，并转义正则特殊字符
        var tokens = Regex.Split(pattern, @"\s+")
                          .Where(t => !string.IsNullOrEmpty(t))
                          .Select(Regex.Escape)
                          .ToArray();

        if (tokens.Length == 0)
            return MatchResult.NotFound;

        // 用 \s+ 连接各 token，\s+ 可匹配任意空白（跨行、跨 \r/\n/\r\n）
        string regexPattern = string.Join(@"\s+", tokens);

        // 从 startIndex 开始匹配
        var match = Regex.Match(source, regexPattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        // 如果匹配成功但匹配位置小于 startIndex，则继续查找下一个
        while (match.Success && match.Index < startIndex)
        {
            match = match.NextMatch();
        }

        if (!match.Success || match.Index < startIndex)
            return MatchResult.NotFound;

        return new MatchResult
        {
            Index = match.Index,
            Length = match.Length
        };
    }

    public static string ReplaceContent(string content, int index, int length, string newContent)
    {
        if (index < 0 || index >= content.Length || index + length > content.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return content.Substring(0, index) + newContent + content.Substring(index + length);
    }
}