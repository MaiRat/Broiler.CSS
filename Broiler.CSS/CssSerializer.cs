using System;
using System.Linq;
using System.Text;

namespace Broiler.CSS;

public static class CssSerializer
{
    public static string Serialize(CssStyleSheet styleSheet)
    {
        ArgumentNullException.ThrowIfNull(styleSheet);
        var builder = new StringBuilder();
        for (var index = 0; index < styleSheet.Rules.Count; index++)
        {
            if (index > 0)
                builder.AppendLine();
            WriteRule(builder, styleSheet.Rules[index], 0);
        }
        return builder.ToString();
    }

    public static string Serialize(CssRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        var builder = new StringBuilder();
        WriteRule(builder, rule, 0);
        return builder.ToString();
    }

    public static string Serialize(CssDeclarationBlock declarations)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        var builder = new StringBuilder();
        WriteDeclarations(builder, declarations, 0);
        return builder.ToString();
    }

    private static void WriteRule(StringBuilder builder, CssRule rule, int indent)
    {
        var padding = new string(' ', indent);
        switch (rule)
        {
            case CssStyleRule styleRule:
                builder.Append(padding);
                builder.Append(string.Join(", ", styleRule.Selectors.Selectors.Select(static selector => selector.Text)));
                builder.AppendLine(" {");
                WriteDeclarations(builder, styleRule.Declarations, indent + 2);
                builder.Append(padding);
                builder.Append('}');
                break;
            case CssAtRule atRule:
                builder.Append(padding);
                builder.Append('@');
                builder.Append(atRule.Name);
                if (atRule.Prelude.Length > 0)
                {
                    builder.Append(' ');
                    builder.Append(atRule.Prelude);
                }
                if (!atRule.HasBlock)
                {
                    builder.Append(';');
                    break;
                }
                builder.AppendLine(" {");
                if (atRule.Declarations is not null)
                {
                    WriteDeclarations(builder, atRule.Declarations, indent + 2);
                }
                else if (atRule.Rules.Count > 0)
                {
                    for (var index = 0; index < atRule.Rules.Count; index++)
                    {
                        WriteRule(builder, atRule.Rules[index], indent + 2);
                        builder.AppendLine();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(atRule.BlockText))
                {
                    builder.Append(new string(' ', indent + 2));
                    builder.AppendLine(atRule.BlockText.Trim());
                }
                builder.Append(padding);
                builder.Append('}');
                break;
        }
    }

    private static void WriteDeclarations(StringBuilder builder, CssDeclarationBlock declarations, int indent)
    {
        var padding = new string(' ', indent);
        foreach (var declaration in declarations.Declarations)
        {
            builder.Append(padding);
            builder.Append(declaration.Name);
            builder.Append(": ");
            builder.Append(declaration.Value.Text);
            if (declaration.Important)
                builder.Append(" !important");
            builder.AppendLine(";");
        }
    }
}
