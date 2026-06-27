using System;
using System.Globalization;

namespace Broiler.CSS;

/// <summary>
/// Parses a single CSS length token (e.g. <c>"12px"</c>, <c>"1.5em"</c>,
/// <c>"50%"</c>) into its numeric value and unit. Promoted into the
/// <c>Broiler.CSS</c> kernel for the layout extraction (see
/// <c>docs/roadmap/broiler-layout-component.md</c> §3, Phase 3.3); ported from the
/// renderer's <c>Broiler.HTML.CSS.CssLength</c>, with the unit enum remapped onto
/// <see cref="CssUnit"/> (<c>Px</c>/<c>Em</c>/… in place of the renderer's
/// <c>Pixels</c>/<c>Ems</c>/…). The renderer keeps its own copy until the Phase 7
/// CSS cleanup dedups.
/// </summary>
public sealed class CssLength
{
    private readonly double _number;

    public CssLength(string length)
    {
        Length = length;
        _number = 0f;
        Unit = CssUnit.None;
        IsPercentage = false;

        //Return zero if no length specified, zero specified
        if (string.IsNullOrEmpty(length) || length == "0")
            return;

        //If percentage, use ParseNumber
        if (length.EndsWith('%'))
        {
            _number = CssLengthParser.ParseNumber(length, 1);
            IsPercentage = true;
            return;
        }

        //If no units, has error
        if (length.Length < 3)
        {
            _ = double.TryParse(length, out _number);
            HasError = true;
            return;
        }

        // Check for 4-character units (e.g. "vmin", "vmax")
        if (length.Length >= 5)
        {
            var last4 = length.Substring(length.Length - 4, 4);
            if (last4.Equals(CssConstants.Vmin, StringComparison.OrdinalIgnoreCase))
            {
                Unit = CssUnit.Vmin;
                IsRelative = true;
                string vmNumber = length[..^4];
                if (!double.TryParse(vmNumber, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _number))
                    HasError = true;
                return;
            }
            if (last4.Equals(CssConstants.Vmax, StringComparison.OrdinalIgnoreCase))
            {
                Unit = CssUnit.Vmax;
                IsRelative = true;
                string vmNumber = length[..^4];
                if (!double.TryParse(vmNumber, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _number))
                    HasError = true;
                return;
            }
        }

        // Check for 3-character units first (e.g. "rem")
        if (length.Length >= 4 && length.EndsWith(CssConstants.Rem, StringComparison.Ordinal))
        {
            Unit = CssUnit.Rem;
            IsRelative = true;
            string remNumber = length[..^3];
            if (!double.TryParse(remNumber, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _number))
                HasError = true;
            return;
        }

        //Get units of the length
        string u = length.Substring(length.Length - 2, 2);

        //Number of the length
        string number = length[..^2];

        switch (u)
        {
            case CssConstants.Em:
                Unit = CssUnit.Em;
                IsRelative = true;
                break;
            case CssConstants.Ex:
                Unit = CssUnit.Ex;
                IsRelative = true;
                break;
            case CssConstants.Ch:
                Unit = CssUnit.Ch;
                IsRelative = true;
                break;
            case CssConstants.Ic:
                Unit = CssUnit.Ic;
                IsRelative = true;
                break;
            case CssConstants.Px:
                Unit = CssUnit.Px;
                IsRelative = true;
                break;
            case CssConstants.Mm:
                Unit = CssUnit.Mm;
                break;
            case CssConstants.Cm:
                Unit = CssUnit.Cm;
                break;
            case CssConstants.In:
                Unit = CssUnit.In;
                break;
            case CssConstants.Pt:
                Unit = CssUnit.Pt;
                break;
            case CssConstants.Pc:
                Unit = CssUnit.Pc;
                break;
            default:
                // Check for viewport units (case-insensitive)
                if (u.Equals(CssConstants.Vh, StringComparison.OrdinalIgnoreCase))
                {
                    Unit = CssUnit.Vh;
                    IsRelative = true;
                    break;
                }
                if (u.Equals(CssConstants.Vw, StringComparison.OrdinalIgnoreCase))
                {
                    Unit = CssUnit.Vw;
                    IsRelative = true;
                    break;
                }
                HasError = true;
                return;
        }

        if (!double.TryParse(number, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _number))
            HasError = true;
    }


    public double Number => _number;
    public bool HasError { get; }
    public bool IsPercentage { get; }
    public bool IsRelative { get; }
    public CssUnit Unit { get; }
    public string Length { get; }

    public CssLength ConvertEmToPoints(double emSize)
    {
        if (HasError)
            throw new InvalidOperationException("Invalid length");

        if (Unit != CssUnit.Em)
            throw new InvalidOperationException("Length is not in ems");

        return new CssLength($"{Convert.ToSingle(Number * emSize).ToString("0.0", NumberFormatInfo.InvariantInfo)}pt");
    }

    public CssLength ConvertEmToPixels(double pixelFactor)
    {
        if (HasError)
            throw new InvalidOperationException("Invalid length");

        if (Unit != CssUnit.Em)
            throw new InvalidOperationException("Length is not in ems");

        return new CssLength($"{Convert.ToSingle(Number * pixelFactor).ToString("0.0", NumberFormatInfo.InvariantInfo)}px");
    }

    public override string ToString()
    {
        if (HasError)
        {
            return string.Empty;
        }
        else if (IsPercentage)
        {
            return $"{Number}%";
        }
        else
        {
            string u = string.Empty;

            switch (Unit)
            {
                case CssUnit.None:
                    break;
                case CssUnit.Em:
                    u = "em";
                    break;
                case CssUnit.Px:
                    u = "px";
                    break;
                case CssUnit.Ex:
                    u = "ex";
                    break;
                case CssUnit.Ch:
                    u = "ch";
                    break;
                case CssUnit.Ic:
                    u = "ic";
                    break;
                case CssUnit.In:
                    u = "in";
                    break;
                case CssUnit.Cm:
                    u = "cm";
                    break;
                case CssUnit.Mm:
                    u = "mm";
                    break;
                case CssUnit.Pt:
                    u = "pt";
                    break;
                case CssUnit.Pc:
                    u = "pc";
                    break;
                case CssUnit.Rem:
                    u = "rem";
                    break;
                case CssUnit.Vh:
                    u = "vh";
                    break;
                case CssUnit.Vw:
                    u = "vw";
                    break;
                case CssUnit.Vmin:
                    u = "vmin";
                    break;
                case CssUnit.Vmax:
                    u = "vmax";
                    break;
            }

            return $"{Number:0.0}{u}".Replace(',', '.');
        }
    }
}
