using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Broiler.CSS;

public readonly record struct CssSpecificity(int Ids, int Classes, int Types)
    : IComparable<CssSpecificity>
{
    public int Encoded => checked((Ids * 1_000_000) + (Classes * 1_000) + Types);

    public int CompareTo(CssSpecificity other)
    {
        var ids = Ids.CompareTo(other.Ids);
        if (ids != 0)
            return ids;
        var classes = Classes.CompareTo(other.Classes);
        return classes != 0 ? classes : Types.CompareTo(other.Types);
    }

    public static CssSpecificity operator +(CssSpecificity left, CssSpecificity right) =>
        new(left.Ids + right.Ids, left.Classes + right.Classes, left.Types + right.Types);
}

public sealed class CssSelector(string text, CssSpecificity specificity)
{
    public string Text { get; } = text;

    public CssSpecificity Specificity { get; } = specificity;

    public override string ToString() => Text;
}

public sealed class CssSelectorList
{
    private readonly ReadOnlyCollection<CssSelector> _selectors;

    internal CssSelectorList(IEnumerable<CssSelector> selectors)
    {
        _selectors = selectors.ToList().AsReadOnly();
    }

    public IReadOnlyList<CssSelector> Selectors => _selectors;
}
