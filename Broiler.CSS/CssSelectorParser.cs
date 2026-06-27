using System;
using System.Collections.Generic;
using System.Linq;

namespace Broiler.CSS;

public static class CssSelectorParser
{
    public static CssSelectorList Parse(string? source)
    {
        var selectors = CssSyntax.SplitTopLevel(source ?? string.Empty, ',')
            .Select(static selector => selector.Trim())
            .Where(static selector => selector.Length > 0)
            .Select(static selector => new CssSelector(selector, CalculateSpecificity(selector)));
        return new CssSelectorList(selectors);
    }

    public static CssSpecificity CalculateSpecificity(string? selector)
    {
        var maximum = default(CssSpecificity);
        foreach (var candidate in CssSyntax.SplitTopLevel(selector ?? string.Empty, ','))
        {
            var specificity = CalculateComplexSpecificity(candidate);
            if (specificity.CompareTo(maximum) > 0)
                maximum = specificity;
        }
        return maximum;
    }

    private static CssSpecificity CalculateComplexSpecificity(string selector)
    {
        var total = default(CssSpecificity);
        foreach (var compound in SplitCompounds(selector))
            total += CalculateCompoundSpecificity(compound);
        return total;
    }

    private static CssSpecificity CalculateCompoundSpecificity(string compound)
    {
        var ids = 0;
        var classes = 0;
        var types = 0;
        var index = 0;
        var typeAllowed = true;

        while (index < compound.Length)
        {
            var character = compound[index];
            if (char.IsWhiteSpace(character) || character is '>' or '+' or '~')
            {
                index++;
                typeAllowed = true;
                continue;
            }

            switch (character)
            {
                case '#':
                    ids++;
                    index = ConsumeName(compound, index + 1);
                    typeAllowed = false;
                    break;
                case '.':
                    classes++;
                    index = ConsumeName(compound, index + 1);
                    typeAllowed = false;
                    break;
                case '[':
                    classes++;
                    index = CssSyntax.FindMatching(compound, index, '[', ']') + 1;
                    typeAllowed = false;
                    break;
                case ':':
                    var pseudoElement = index + 1 < compound.Length && compound[index + 1] == ':';
                    index += pseudoElement ? 2 : 1;
                    var nameStart = index;
                    index = ConsumeName(compound, index);
                    var name = compound[nameStart..index].ToLowerInvariant();
                    string? argument = null;
                    if (index < compound.Length && compound[index] == '(')
                    {
                        var close = CssSyntax.FindMatching(compound, index, '(', ')');
                        argument = close > index ? compound[(index + 1)..close] : compound[(index + 1)..];
                        index = close >= index ? close + 1 : compound.Length;
                    }

                    if (pseudoElement || name is "before" or "after" or "first-line" or "first-letter")
                    {
                        types++;
                    }
                    else
                    {
                        var pseudoSpecificity = name switch
                        {
                            "where" => default,
                            "is" or "not" or "has" => CalculateSpecificity(argument),
                            "nth-child" or "nth-last-child" =>
                                new CssSpecificity(0, 1, 0) + CalculateSpecificity(ExtractNthOfSelector(argument)),
                            _ => new CssSpecificity(0, 1, 0),
                        };
                        ids += pseudoSpecificity.Ids;
                        classes += pseudoSpecificity.Classes;
                        types += pseudoSpecificity.Types;
                    }
                    typeAllowed = false;
                    break;
                case '*':
                    index++;
                    typeAllowed = false;
                    break;
                case '|':
                    index++;
                    break;
                case '\\':
                    if (typeAllowed)
                        types++;
                    index = ConsumeEscape(compound, index);
                    typeAllowed = false;
                    break;
                default:
                    if (typeAllowed && IsNameStart(character))
                    {
                        types++;
                        index = ConsumeName(compound, index);
                        typeAllowed = false;
                    }
                    else
                    {
                        index++;
                    }
                    break;
            }
        }

        return new CssSpecificity(ids, classes, types);
    }

    private static IEnumerable<string> SplitCompounds(string selector)
    {
        var start = 0;
        var bracketDepth = 0;
        var parenthesisDepth = 0;
        char quote = '\0';
        for (var index = 0; index < selector.Length; index++)
        {
            var character = selector[index];
            if (quote != '\0')
            {
                if (character == '\\')
                    index++;
                else if (character == quote)
                    quote = '\0';
                continue;
            }

            if (character is '"' or '\'')
            {
                quote = character;
                continue;
            }
            if (character == '[') bracketDepth++;
            else if (character == ']') bracketDepth--;
            else if (character == '(') parenthesisDepth++;
            else if (character == ')') parenthesisDepth--;
            else if (bracketDepth == 0 && parenthesisDepth == 0 &&
                     (char.IsWhiteSpace(character) || character is '>' or '+' or '~'))
            {
                if (index > start)
                    yield return selector[start..index];
                start = index + 1;
            }
        }

        if (start < selector.Length)
            yield return selector[start..];
    }

    private static string? ExtractNthOfSelector(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return null;

        var lower = argument.ToLowerInvariant();
        var depth = 0;
        for (var index = 0; index <= lower.Length - 4; index++)
        {
            if (lower[index] == '(') depth++;
            else if (lower[index] == ')') depth--;
            else if (depth == 0 &&
                     lower.AsSpan(index, 4).Equals(" of ", StringComparison.Ordinal))
            {
                return argument[(index + 4)..].Trim();
            }
        }
        return null;
    }

    private static int ConsumeName(string text, int index)
    {
        while (index < text.Length)
        {
            if (text[index] == '\\')
            {
                index = ConsumeEscape(text, index);
                continue;
            }
            if (!IsNameCharacter(text[index]))
                break;
            index++;
        }
        return index;
    }

    private static int ConsumeEscape(string text, int index)
    {
        index++;
        var digits = 0;
        while (index < text.Length && digits < 6 && Uri.IsHexDigit(text[index]))
        {
            index++;
            digits++;
        }
        if (digits > 0 && index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
        else if (digits == 0 && index < text.Length)
            index++;
        return index;
    }

    private static bool IsNameStart(char character) =>
        char.IsLetter(character) || character is '_' or '-' || character >= 0x80;

    private static bool IsNameCharacter(char character) =>
        IsNameStart(character) || char.IsDigit(character);
}
