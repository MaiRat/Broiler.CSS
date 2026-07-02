using System;
using System.Collections.Generic;

namespace Broiler.CSS.Dom;

/// <summary>
/// Cascade origin of a stylesheet, ordered from weakest to strongest for
/// normal (non-important) declarations. Importance reverses the user-agent and
/// author bands per CSS Cascade Level 4 §6.
/// </summary>
public enum CssOrigin
{
    /// <summary>The browser's built-in user-agent stylesheet.</summary>
    UserAgent,

    /// <summary>User-provided styles (for example, an accessibility override sheet).</summary>
    User,

    /// <summary>Author styles from the document's own stylesheets and inline styles.</summary>
    Author,
}

/// <summary>
/// Host-provided environment used to resolve environment-dependent CSS such as
/// media queries and viewport-relative lengths. Parsing never needs this; only
/// computation does, keeping <see cref="Broiler.CSS"/> environment-free.
/// </summary>
public readonly record struct CssEnvironment(int ViewportWidth, int ViewportHeight)
{
    /// <summary>A headless 0×0 environment, matching the bridge's default scope.</summary>
    public static readonly CssEnvironment Headless = new(0, 0);
}

/// <summary>
/// An immutable snapshot of an element's computed style. Property names are
/// stored case-insensitively and values are already cascade-resolved,
/// inheritance-applied, shorthand-expanded, and backfilled with initial values.
/// </summary>
public sealed class CssComputedStyle(IReadOnlyDictionary<string, string> properties)
{
    /// <summary>The computed property name/value pairs.</summary>
    public IEnumerable<KeyValuePair<string, string>> Properties => properties;

    internal IReadOnlyDictionary<string, string> AsMap() => properties;

    /// <summary>The empty computed style, returned for missing or detached elements.</summary>
    public static readonly CssComputedStyle Empty = 
        new(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
}
