using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Broiler.CSS;

public sealed class CssDeclaration(string name, CssValue value, bool important, CssSourceRange range)
{
    public string Name { get; } = name;
    public CssValue Value { get; } = value;
    public bool Important { get; } = important;
    public CssSourceRange Range { get; } = range;
}

// MaiRat: Maybe a performance issue, because of: Linq and 'ReadOnlyCollection' <-> 'ReadOnlyCollection' conversion
public sealed class CssDeclarationBlock(IEnumerable<CssDeclaration> declarations)
{
    private readonly ReadOnlyCollection<CssDeclaration> _declarations = declarations.ToList().AsReadOnly();

    public IReadOnlyList<CssDeclaration> Declarations => _declarations;

    public CssDeclaration? GetLastDeclaration(string propertyName) =>
        _declarations.LastOrDefault(declaration => 
            string.Equals(declaration.Name, propertyName, StringComparison.OrdinalIgnoreCase));

    public string? GetPropertyValue(string propertyName) => GetLastDeclaration(propertyName)?.Value.Text;
}
