using System.Text;
using System.Text.RegularExpressions;

var keymapPath = "config/corne.keymap";
var outputDir = "docs/img";
var layout = "corne";
var prefix = "";

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--keymap" when i + 1 < args.Length: keymapPath = args[++i]; break;
        case "--output" when i + 1 < args.Length: outputDir = args[++i]; break;
        case "--layout" when i + 1 < args.Length: layout = args[++i]; break;
        case "--prefix" when i + 1 < args.Length: prefix = args[++i]; break;
    }
}

var parser = new KeymapParser();
var data = parser.Parse(keymapPath);
List<KeyPos> positions = layout == "eyelash" ? Layout.Eyelash() : Layout.Corne();
Directory.CreateDirectory(outputDir);

var filePrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";
var renderer = new SvgRenderer();

foreach (var layer in data.Layers)
{
    var safeName = Regex.Replace(layer.Name.ToLower(), @"[^a-z0-9]+", "_").Trim('_');
    if (safeName.EndsWith("_layer")) safeName = safeName[..^6];
    var path = Path.Combine(outputDir, $"{filePrefix}{safeName}_layer.svg");
    File.WriteAllText(path, renderer.RenderLayer(layer, positions));
    Console.WriteLine($"  Generated {path}");
}

if (data.Combos.Count > 0)
{
    var path = Path.Combine(outputDir, $"{filePrefix}combos.svg");
    File.WriteAllText(path, renderer.RenderCombos(data.Combos, positions));
    Console.WriteLine($"  Generated {path}");
}

// ─── Models ─────────────────────────────────────────────────────────────────

record KeymapData(List<Layer> Layers, List<Combo> Combos);
record Layer(string Name, List<string> Bindings);
record Combo(string Name, string Binding, List<int> Positions, List<int>? Layers);

// ─── Parser ─────────────────────────────────────────────────────────────────

class KeymapParser
{
    public KeymapData Parse(string path)
    {
        var text = File.ReadAllText(path);
        return new KeymapData(ParseLayers(text), ParseCombos(text));
    }

    List<Layer> ParseLayers(string text)
    {
        var layers = new List<Layer>();
        var keymapMatch = Regex.Match(text,
            @"keymap\s*\{[^}]*compatible\s*=\s*""zmk,keymap""\s*;(.+?)^\s{4}\};",
            RegexOptions.Singleline | RegexOptions.Multiline);
        if (!keymapMatch.Success) return layers;

        var keymapText = keymapMatch.Groups[1].Value;
        var layerPattern = new Regex(
            @"(\w+)\s*\{[^}]*?(?:display-name\s*=\s*""([^""]+)""[^}]*?)?bindings\s*=\s*<([^>]+)>",
            RegexOptions.Singleline);

        foreach (Match m in layerPattern.Matches(keymapText))
        {
            var name = !string.IsNullOrEmpty(m.Groups[2].Value) ? m.Groups[2].Value : m.Groups[1].Value;
            var bindings = Regex.Split(m.Groups[3].Value.Trim(), @"\s+(?=&)")
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Select(b => b.Trim())
                .ToList();
            layers.Add(new Layer(name, bindings));
        }
        return layers;
    }

    List<Combo> ParseCombos(string text)
    {
        var combos = new List<Combo>();
        var sectionMatch = Regex.Match(text,
            @"combos\s*\{[^}]*compatible[^;]*;(.*?)\n    \};",
            RegexOptions.Singleline);
        if (!sectionMatch.Success) return combos;

        var pattern = new Regex(
            @"(\w+)\s*\{\s*bindings\s*=\s*<([^>]+)>\s*;\s*key-positions\s*=\s*<([^>]+)>\s*;\s*(?:layers\s*=\s*<([^>]+)>\s*;)?",
            RegexOptions.Singleline);

        foreach (Match m in pattern.Matches(sectionMatch.Groups[1].Value))
        {
            var positions = m.Groups[3].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToList();
            List<int>? layers = m.Groups[4].Success
                ? m.Groups[4].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).ToList()
                : null;
            combos.Add(new Combo(m.Groups[1].Value, m.Groups[2].Value.Trim(), positions, layers));
        }
        return combos;
    }
}

// ─── Key label mapping ──────────────────────────────────────────────────────

static class KeyLabels
{
    static readonly Dictionary<string, string> Map = new()
    {
        ["ESCAPE"] = "Esc", ["ESC"] = "Esc", ["TAB"] = "Tab",
        ["BACKSPACE"] = "Bksp", ["BSPC"] = "Bksp", ["SPACE"] = "Space",
        ["RET"] = "Ret", ["ENTER"] = "Ret", ["DELETE"] = "Del", ["DEL"] = "Del",
        ["CAPSLOCK"] = "Caps", ["CAPS"] = "Caps",
        ["LCTRL"] = "LCtrl", ["RCTRL"] = "RCtrl", ["LALT"] = "LAlt", ["RALT"] = "RAlt",
        ["LGUI"] = "LGui", ["RGUI"] = "RGui", ["LSHIFT"] = "LShift", ["RSHIFT"] = "RShift",
        ["LEFT_SHIFT"] = "LShift", ["RIGHT_SHIFT"] = "RShift",
        ["LEFT_CONTROL"] = "LCtrl", ["RIGHT_CONTROL"] = "RCtrl",
        ["BACKSLASH"] = "\\", ["BSLH"] = "\\", ["FSLH"] = "/", ["SLASH"] = "/",
        ["SEMI"] = ";", ["SQT"] = "'", ["GRAVE"] = "`", ["TILDE"] = "~",
        ["MINUS"] = "-", ["EQUAL"] = "=", ["UNDERSCORE"] = "_", ["PLUS"] = "+",
        ["PIPE"] = "|", ["PERIOD"] = ".", ["COMMA"] = ",", ["DOT"] = ".",
        ["EXCLAMATION"] = "!", ["AT_SIGN"] = "@", ["HASH"] = "#", ["DOLLAR"] = "$",
        ["PERCENT"] = "%", ["CARET"] = "^", ["AMPERSAND"] = "&", ["ASTERISK"] = "*",
        ["LEFT_PARENTHESIS"] = "(", ["RIGHT_PARENTHESIS"] = ")",
        ["LEFT_BRACE"] = "{", ["RIGHT_BRACE"] = "}",
        ["LEFT_BRACKET"] = "[", ["RIGHT_BRACKET"] = "]",
        ["LEFT_ARROW"] = "\u2190", ["DOWN"] = "\u2193", ["UP_ARROW"] = "\u2191",
        ["UP"] = "\u2191", ["RIGHT"] = "\u2192", ["LEFT"] = "\u2190",
        ["HOME"] = "Home", ["END"] = "End",
        ["PAGE_UP"] = "PgUp", ["PAGE_DOWN"] = "PgDn", ["PG_UP"] = "PgUp", ["PG_DN"] = "PgDn",
        ["C_VOLUME_UP"] = "Vol+", ["C_VOLUME_DOWN"] = "Vol-",
        ["C_MUTE"] = "Mute", ["C_PP"] = "Play", ["C_NEXT"] = "Next", ["C_PREV"] = "Prev",
        ["PRINTSCREEN"] = "PrtSc", ["SCROLLLOCK"] = "ScrLk", ["PAUSE_BREAK"] = "Pause",
        ["INS"] = "Ins",
    };

    static KeyLabels()
    {
        for (var i = 0; i <= 24; i++) Map[$"F{i}"] = $"F{i}";
        for (var i = 0; i <= 9; i++) { Map[$"NUMBER_{i}"] = $"{i}"; Map[$"N{i}"] = $"{i}"; }
    }

    public static string ParseBinding(string binding)
    {
        binding = binding.Trim();
        if (binding == "&trans") return "\u25BD";
        if (binding == "&none") return "";
        if (binding == "&caps_word") return "CapsWd";
        if (binding == "&bootloader") return "Boot";
        if (binding == "&sys_reset") return "Reset";

        Match m;
        if ((m = Regex.Match(binding, @"^&kp\s+(.+)$")).Success)
            return ParseKey(m.Groups[1].Value);
        if ((m = Regex.Match(binding, @"^&mt\s+(\S+)\s+(\S+)$")).Success)
            return $"{ParseKey(m.Groups[2].Value)}\n({ParseKey(m.Groups[1].Value)})";
        if ((m = Regex.Match(binding, @"^&lt\s+(\d+)\s+(\S+)$")).Success)
            return $"{ParseKey(m.Groups[2].Value)}\n(L{m.Groups[1].Value})";
        if ((m = Regex.Match(binding, @"^&mo\s+(\d+)$")).Success)
            return $"MO({m.Groups[1].Value})";
        if ((m = Regex.Match(binding, @"^&bt\s+(.+)$")).Success)
            return m.Groups[1].Value switch
            {
                "BT_CLR" => "BT Clr", "BT_PRV" => "BT Prv", "BT_NXT" => "BT Nxt",
                var c when c.StartsWith("BT_SEL") => $"BT {c.Split(' ').Last()}",
                var c => c
            };
        if ((m = Regex.Match(binding, @"^&out\s+(.+)$")).Success)
            return m.Groups[1].Value switch
            {
                "OUT_TOG" => "Out Tog", "OUT_USB" => "USB", "OUT_BLE" => "BLE",
                var c => c
            };
        return binding;
    }

    public static string ParseKey(string key)
    {
        var modMatch = Regex.Match(key, @"^(L[CSAG]|R[CSAG])\((.+)\)$");
        if (modMatch.Success)
        {
            var mod = modMatch.Groups[1].Value switch
            {
                "LC" or "RC" => "C-", "LS" or "RS" => "S-",
                "LA" or "RA" => "A-", "LG" or "RG" => "G-",
                var x => x + "-"
            };
            return mod + ParseKey(modMatch.Groups[2].Value);
        }
        if (key == "MEH") return "MEH";
        if (key.Length == 1 && char.IsLetter(key[0])) return key;
        return Map.TryGetValue(key, out var label) ? label : key;
    }
}

// ─── Layout positions ───────────────────────────────────────────────────────

record KeyPos(double X, double Y, double Rotation = 0, double H = 1.0);

static class Layout
{
    // 1 unit = key pitch. We scale to pixels: 1u = 56px (key=52px + 4px gap)
    const double U = 56.0;
    const double Gap = 2.5; // gap between halves

    // Column stagger offsets (in units, relative to middle finger = 0)
    static readonly double[] ColStagger = [0.375, 0.375, 0.125, 0.0, 0.125, 0.25];

    public static List<KeyPos> Corne()
    {
        var pos = new List<KeyPos>();

        // Keys are ordered per row: left 6 + right 6
        for (var row = 0; row < 3; row++)
        {
            for (var col = 0; col < 6; col++)
                pos.Add(new KeyPos(col * U, (row + ColStagger[col]) * U));
            for (var col = 0; col < 6; col++)
                pos.Add(new KeyPos((col + 6 + Gap) * U, (row + ColStagger[5 - col]) * U));
        }

        // Thumb keys (with fan arc)
        // Left thumbs
        pos.Add(new KeyPos(3.50 * U, 3.158 * U, 0));
        pos.Add(new KeyPos(4.60 * U, 3.305 * U, 15));
        pos.Add(new KeyPos(5.77 * U, 3.255 * U, 30));

        // Right thumbs (mirrored around center = 6 + Gap/2)
        var center = 6 + Gap / 2;
        pos.Add(new KeyPos((center + (center - 5.77)) * U, 3.255 * U, -30));
        pos.Add(new KeyPos((center + (center - 4.60)) * U, 3.305 * U, -15));
        pos.Add(new KeyPos((center + (center - 3.50)) * U, 3.158 * U, 0));

        return pos;
    }

    public static List<KeyPos> Eyelash()
    {
        var pos = new List<KeyPos>();
        var extraW = 0.75;

        // Row 0: 6 left + 1 center + 6 right
        for (var c = 0; c < 6; c++)
            pos.Add(new KeyPos(c * U, ColStagger[c] * U));
        pos.Add(new KeyPos((6 + Gap / 2 - extraW / 2) * U, 0.25 * U)); // UP center
        for (var c = 0; c < 6; c++)
            pos.Add(new KeyPos((c + 6 + Gap) * U, ColStagger[5 - c] * U));

        // Row 1: 6 left + 3 center + 6 right
        for (var c = 0; c < 6; c++)
            pos.Add(new KeyPos(c * U, (1 + ColStagger[c]) * U));
        for (var i = 0; i < 3; i++)
            pos.Add(new KeyPos((5.5 + Gap / 2 - extraW + i * (extraW + 0.1)) * U, 1.25 * U));
        for (var c = 0; c < 6; c++)
            pos.Add(new KeyPos((c + 6 + Gap) * U, (1 + ColStagger[5 - c]) * U));

        // Row 2: 6 left + 1 + 1 + 6 right
        for (var c = 0; c < 6; c++)
            pos.Add(new KeyPos(c * U, (2 + ColStagger[c]) * U));
        pos.Add(new KeyPos(5.75 * U, 2.25 * U));  // extra left
        pos.Add(new KeyPos((6 + Gap / 2 - extraW / 2) * U, 2.25 * U)); // DOWN
        for (var c = 0; c < 6; c++)
            pos.Add(new KeyPos((c + 6 + Gap) * U, (2 + ColStagger[5 - c]) * U));

        // Thumbs: 3 + 3
        pos.Add(new KeyPos(3.50 * U, 3.158 * U, 0));
        pos.Add(new KeyPos(4.60 * U, 3.305 * U, 15));
        pos.Add(new KeyPos(5.77 * U, 3.255 * U, 30));

        var ctr = 6 + Gap / 2;
        pos.Add(new KeyPos((ctr + (ctr - 5.77)) * U, 3.255 * U, -30));
        pos.Add(new KeyPos((ctr + (ctr - 4.60)) * U, 3.305 * U, -15));
        pos.Add(new KeyPos((ctr + (ctr - 3.50)) * U, 3.158 * U, 0));

        return pos;
    }
}

// ─── SVG Renderer ───────────────────────────────────────────────────────────

class SvgRenderer
{
    const double W = 52, H = 52, Rx = 6, PadSvg = 30;
    const int FontSize = 11;
    const string BgColor = "#1e1e2e", KeyColor = "#313244", KeyStroke = "#45475a";
    const string TextColor = "#cdd6f4", AccentColor = "#89b4fa", HoldColor = "#a6adc8";
    const string ComboColor = "#f38ba8", TransColor = "#585b70";
    const string Font = "Inter, SF Pro Text, Segoe UI, system-ui, sans-serif";

    static string F(double v) => v.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

    public string RenderLayer(Layer layer, List<KeyPos> positions)
    {
        var labels = layer.Bindings.Select(KeyLabels.ParseBinding).ToList();
        var (vw, vh) = GetViewBox(positions);
        var sb = new StringBuilder();

        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="{F(-PadSvg)} {F(-PadSvg)} {F(vw)} {F(vh)}">""");
        sb.AppendLine($"""  <rect x="{F(-PadSvg)}" y="{F(-PadSvg)}" width="{F(vw)}" height="{F(vh)}" fill="{BgColor}" rx="12"/>""");
        sb.AppendLine($"""  <text x="{F((vw - PadSvg * 2) / 2)}" y="-8" text-anchor="middle" fill="{AccentColor}" font-size="14px" font-weight="bold" font-family="{Font}">{Esc(layer.Name)}</text>""");

        for (var i = 0; i < positions.Count && i < labels.Count; i++)
            RenderKey(sb, positions[i], labels[i]);

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    public string RenderCombos(List<Combo> combos, List<KeyPos> positions)
    {
        var (vw, vh) = GetViewBox(positions);
        var sb = new StringBuilder();

        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="{F(-PadSvg)} {F(-PadSvg)} {F(vw)} {F(vh)}">""");
        sb.AppendLine($"""  <rect x="{F(-PadSvg)}" y="{F(-PadSvg)}" width="{F(vw)}" height="{F(vh)}" fill="{BgColor}" rx="12"/>""");
        sb.AppendLine($"""  <text x="{F((vw - PadSvg * 2) / 2)}" y="-8" text-anchor="middle" fill="{AccentColor}" font-size="14px" font-weight="bold" font-family="{Font}">Combos</text>""");

        // Ghost keys
        foreach (var p in positions)
        {
            var transform = p.Rotation != 0
                ? $""" transform="rotate({F(p.Rotation)} {F(p.X + W / 2)} {F(p.Y + H / 2)})" """
                : " ";
            sb.AppendLine($"""  <rect x="{F(p.X)}" y="{F(p.Y)}" width="{F(W)}" height="{F(H)}"{transform}rx="{Rx}" fill="none" stroke="{TransColor}" stroke-width="0.5" opacity="0.3"/>""");
        }

        foreach (var combo in combos)
        {
            var centers = new List<(double X, double Y)>();
            var layerInfo = combo.Layers != null ? $" (L{string.Join(",", combo.Layers)})" : "";

            foreach (var p in combo.Positions.Where(p => p < positions.Count))
            {
                var kp = positions[p];
                centers.Add((kp.X + W / 2, kp.Y + H / 2));
                var transform = kp.Rotation != 0
                    ? $""" transform="rotate({F(kp.Rotation)} {F(kp.X + W / 2)} {F(kp.Y + H / 2)})" """
                    : " ";
                sb.AppendLine($"""  <rect x="{F(kp.X)}" y="{F(kp.Y)}" width="{F(W)}" height="{F(H)}"{transform}rx="{Rx}" fill="{ComboColor}" opacity="0.15" stroke="{ComboColor}" stroke-width="1.5"/>""");
            }

            for (var j = 0; j < centers.Count - 1; j++)
                sb.AppendLine($"""  <line x1="{F(centers[j].X)}" y1="{F(centers[j].Y)}" x2="{F(centers[j + 1].X)}" y2="{F(centers[j + 1].Y)}" stroke="{ComboColor}" stroke-width="1.5" opacity="0.5"/>""");

            if (centers.Count > 0)
            {
                var avgX = centers.Average(c => c.X);
                var avgY = centers.Average(c => c.Y);
                sb.AppendLine($"""  <text x="{F(avgX)}" y="{F(avgY + 4)}" text-anchor="middle" fill="{ComboColor}" font-size="{FontSize - 1}px" font-weight="bold" font-family="{Font}">{Esc(combo.Name)}{Esc(layerInfo)}</text>""");
            }
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    void RenderKey(StringBuilder sb, KeyPos pos, string label)
    {
        var isTrans = label == "\u25BD";
        var fill = isTrans ? BgColor : KeyColor;
        var stroke = isTrans ? TransColor : KeyStroke;
        var textFill = isTrans ? TransColor : TextColor;
        var x = pos.X;
        var y = pos.Y;
        var h = H * pos.H;

        var transformAttr = pos.Rotation != 0
            ? $""" transform="rotate({F(pos.Rotation)} {F(x + W / 2)} {F(y + h / 2)})" """
            : " ";

        sb.AppendLine($"""  <g{transformAttr}>""");
        sb.AppendLine($"""    <rect x="{F(x)}" y="{F(y)}" width="{F(W)}" height="{F(h)}" rx="{Rx}" fill="{fill}" stroke="{stroke}" stroke-width="1.5"/>""");

        if (label.Contains('\n'))
        {
            var lines = label.Split('\n');
            sb.AppendLine($"""    <text x="{F(x + W / 2)}" y="{F(y + h / 2 - 2)}" text-anchor="middle" fill="{textFill}" font-size="{FontSize}px" font-family="{Font}">{Esc(lines[0])}</text>""");
            sb.AppendLine($"""    <text x="{F(x + W / 2)}" y="{F(y + h / 2 + FontSize + 1)}" text-anchor="middle" fill="{HoldColor}" font-size="{FontSize - 2}px" font-family="{Font}">{Esc(lines[1])}</text>""");
        }
        else
        {
            sb.AppendLine($"""    <text x="{F(x + W / 2)}" y="{F(y + h / 2 + FontSize / 3.0)}" text-anchor="middle" fill="{textFill}" font-size="{FontSize}px" font-family="{Font}">{Esc(label)}</text>""");
        }

        sb.AppendLine("  </g>");
    }

    (double W, double H) GetViewBox(List<KeyPos> positions)
    {
        // Approximate bounding box (ignoring rotation for simplicity)
        var maxX = positions.Max(p => p.X) + W + PadSvg * 2;
        var maxY = positions.Max(p => p.Y + H * p.H) + PadSvg * 2;
        return (maxX, maxY);
    }

    static string Esc(string text) => text
        .Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
