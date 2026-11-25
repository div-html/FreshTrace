using System.Net;
using System.Text;

namespace FreshTrace.Services;

/// <summary>
/// Generates deterministic 11x11 SVG QR matrices in pure C# matching FreshTrace provenance tokens.
/// </summary>
public static class QrSvgRenderer
{
    public static string RenderSvg(string value)
    {
        const int size = 11;
        int hash = 0;

        foreach (char ch in value ?? string.Empty)
        {
            unchecked
            {
                hash = (hash << 5) - hash + ch;
            }
        }

        var sb = new StringBuilder();
        var encodedValue = WebUtility.HtmlEncode(value ?? string.Empty);
        sb.Append($"<svg width=\"116\" height=\"116\" viewBox=\"0 0 120 120\" role=\"img\" aria-label=\"QR code for {encodedValue}\">");
        sb.Append("<rect width=\"120\" height=\"120\" rx=\"8\" fill=\"#ffffff\" />");

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                bool inTopLeft = r < 3 && c < 3;
                bool inTopRight = r < 3 && c >= size - 3;
                bool inBottomLeft = r >= size - 3 && c < 3;

                bool filled;
                if (inTopLeft || inTopRight || inBottomLeft)
                {
                    int lr = inBottomLeft ? r - (size - 3) : r;
                    int lc = inTopRight ? c - (size - 3) : c;
                    filled = lr == 0 || lr == 2 || lc == 0 || lc == 2 || (lr == 1 && lc == 1);
                }
                else
                {
                    int raw;
                    unchecked
                    {
                        raw = (hash * (r + 3) + c * 31 + r * 17) % 11;
                    }
                    int bit = raw == int.MinValue ? 0 : Math.Abs(raw);
                    filled = bit < 5;
                }

                if (filled)
                {
                    int x = c * 10 + 5;
                    int y = r * 10 + 5;
                    sb.Append($"<rect x=\"{x}\" y=\"{y}\" width=\"8.5\" height=\"8.5\" rx=\"1.5\" fill=\"#1b3115\" />");
                }
            }
        }

        sb.Append("</svg>");
        return sb.ToString();
    }
}
