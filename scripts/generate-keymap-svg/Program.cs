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
var positions = layout == "eyelash" ? Layout.Eyelash() : Layout.Corne();
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

static class Layout
{
    const int W = 64, H = 48, Pad = 4, SplitGap = 32, ThumbOffsetY = 12, Cols = 6, Rows = 3;

    public static List<(int X, int Y)> Corne()
    {
        var pos = new List<(int, int)>();
        for (var row = 0; row < Rows; row++)
        {
            for (var col = 0; col < Cols; col++)
                pos.Add((col * (W + Pad), row * (H + Pad)));
            for (var col = 0; col < Cols; col++)
                pos.Add(((Cols + col) * (W + Pad) + SplitGap, row * (H + Pad)));
        }
        var ty = Rows * (H + Pad) + ThumbOffsetY;
        for (var i = 0; i < 3; i++) pos.Add(((3 + i) * (W + Pad), ty));
        for (var i = 0; i < 3; i++) pos.Add(((Cols + i) * (W + Pad) + SplitGap, ty));
        return pos;
    }

    public static List<(int X, int Y)> Eyelash()
    {
        var pos = new List<(int, int)>();
        var extraW = (int)(W * 0.75);
        var sideW = Cols * (W + Pad);
        var centerX = sideW + SplitGap / 2;

        // Row 0: 6 + 1 + 6
        for (var c = 0; c < 6; c++) pos.Add((c * (W + Pad), 0));
        pos.Add((centerX - extraW / 2, 0));
        for (var c = 0; c < 6; c++) pos.Add((sideW + SplitGap + c * (W + Pad), 0));

        // Row 1: 6 + 3 + 6
        var y1 = H + Pad;
        for (var c = 0; c < 6; c++) pos.Add((c * (W + Pad), y1));
        for (var i = 0; i < 3; i++) pos.Add((centerX - extraW - Pad + i * (extraW + Pad), y1));
        for (var c = 0; c < 6; c++) pos.Add((sideW + SplitGap + c * (W + Pad), y1));

        // Row 2: 6 + 1 + 1 + 6
        var y2 = 2 * (H + Pad);
        for (var c = 0; c < 6; c++) pos.Add((c * (W + Pad), y2));
        pos.Add((sideW - Pad, y2));
        pos.Add((centerX - extraW / 2, y2));
        for (var c = 0; c < 6; c++) pos.Add((sideW + SplitGap + c * (W + Pad), y2));

        // Thumbs: 3 + 3
        var ty = 3 * (H + Pad) + ThumbOffsetY;
        for (var i = 0; i < 3; i++) pos.Add(((3 + i) * (W + Pad), ty));
        for (var i = 0; i < 3; i++) pos.Add((sideW + SplitGap + i * (W + Pad), ty));
        return pos;
    }
}

// ─── SVG Renderer ───────────────────────────────────────────────────────────

class SvgRenderer
{
    const int W = 64, H = 48, Rx = 6, FontSize = 11, PadSvg = 20;
    const string BgColor = "#1e1e2e", KeyColor = "#313244", KeyStroke = "#45475a";
    const string TextColor = "#cdd6f4", AccentColor = "#89b4fa", HoldColor = "#a6adc8";
    const string ComboColor = "#f38ba8", TransColor = "#585b70";
    const string Font = "Inter, SF Pro Text, Segoe UI, system-ui, sans-serif";

    public string RenderLayer(Layer layer, List<(int X, int Y)> positions)
    {
        var labels = layer.Bindings.Select(KeyLabels.ParseBinding).ToList();
        var maxX = positions.Max(p => p.X) + W + PadSvg;
        var maxY = positions.Max(p => p.Y) + H + PadSvg + 30;
        var sb = new StringBuilder();

        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{maxX + PadSvg * 2}" height="{maxY + PadSvg}" viewBox="{-PadSvg} {-PadSvg} {maxX + PadSvg * 2} {maxY + PadSvg}">""");
        sb.AppendLine($"""  <rect x="{-PadSvg}" y="{-PadSvg}" width="{maxX + PadSvg * 2}" height="{maxY + PadSvg}" fill="{BgColor}" rx="12"/>""");
        sb.AppendLine($"""  <text x="{maxX / 2}" y="-4" text-anchor="middle" fill="{AccentColor}" font-size="14px" font-weight="bold" font-family="{Font}">{Esc(layer.Name)}</text>""");

        for (var i = 0; i < positions.Count && i < labels.Count; i++)
            RenderKey(sb, positions[i].X, positions[i].Y + 16, labels[i]);

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    public string RenderCombos(List<Combo> combos, List<(int X, int Y)> positions)
    {
        var maxX = positions.Max(p => p.X) + W + PadSvg;
        var maxY = positions.Max(p => p.Y) + H + PadSvg + 30;
        var sb = new StringBuilder();

        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{maxX + PadSvg * 2}" height="{maxY + PadSvg}" viewBox="{-PadSvg} {-PadSvg} {maxX + PadSvg * 2} {maxY + PadSvg}">""");
        sb.AppendLine($"""  <rect x="{-PadSvg}" y="{-PadSvg}" width="{maxX + PadSvg * 2}" height="{maxY + PadSvg}" fill="{BgColor}" rx="12"/>""");
        sb.AppendLine($"""  <text x="{maxX / 2}" y="-4" text-anchor="middle" fill="{AccentColor}" font-size="14px" font-weight="bold" font-family="{Font}">Combos</text>""");

        foreach (var (x, y) in positions)
            sb.AppendLine($"""  <rect x="{x}" y="{y + 16}" width="{W}" height="{H}" rx="{Rx}" fill="none" stroke="{TransColor}" stroke-width="0.5" opacity="0.3"/>""");

        foreach (var combo in combos)
        {
            var centers = new List<(int X, int Y)>();
            var layerInfo = combo.Layers != null ? $" (L{string.Join(",", combo.Layers)})" : "";

            foreach (var p in combo.Positions.Where(p => p < positions.Count))
            {
                var (kx, ky) = positions[p];
                centers.Add((kx + W / 2, ky + 16 + H / 2));
                sb.AppendLine($"""  <rect x="{kx}" y="{ky + 16}" width="{W}" height="{H}" rx="{Rx}" fill="{ComboColor}" opacity="0.15" stroke="{ComboColor}" stroke-width="1.5"/>""");
            }

            for (var j = 0; j < centers.Count - 1; j++)
                sb.AppendLine($"""  <line x1="{centers[j].X}" y1="{centers[j].Y}" x2="{centers[j + 1].X}" y2="{centers[j + 1].Y}" stroke="{ComboColor}" stroke-width="1.5" opacity="0.5"/>""");

            if (centers.Count > 0)
            {
                var avgX = centers.Sum(c => c.X) / centers.Count;
                var avgY = centers.Sum(c => c.Y) / centers.Count;
                sb.AppendLine($"""  <text x="{avgX}" y="{avgY + 4}" text-anchor="middle" fill="{ComboColor}" font-size="{FontSize - 1}px" font-weight="bold" font-family="{Font}">{Esc(combo.Name)}{Esc(layerInfo)}</text>""");
            }
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    void RenderKey(StringBuilder sb, int x, int y, string label)
    {
        var isTrans = label == "\u25BD";
        var fill = isTrans ? BgColor : KeyColor;
        var stroke = isTrans ? TransColor : KeyStroke;
        var textFill = isTrans ? TransColor : TextColor;

        sb.AppendLine($"""  <rect x="{x}" y="{y}" width="{W}" height="{H}" rx="{Rx}" fill="{fill}" stroke="{stroke}" stroke-width="1.5"/>""");

        if (label.Contains('\n'))
        {
            var lines = label.Split('\n');
            var tx = x + W / 2;
            sb.AppendLine($"""  <text x="{tx}" y="{y + H / 2 - 2}" text-anchor="middle" fill="{textFill}" font-size="{FontSize}px" font-family="{Font}">{Esc(lines[0])}</text>""");
            sb.AppendLine($"""  <text x="{tx}" y="{y + H / 2 + FontSize + 1}" text-anchor="middle" fill="{HoldColor}" font-size="{FontSize - 2}px" font-family="{Font}">{Esc(lines[1])}</text>""");
        }
        else
        {
            sb.AppendLine($"""  <text x="{x + W / 2}" y="{y + H / 2 + FontSize / 3}" text-anchor="middle" fill="{textFill}" font-size="{FontSize}px" font-family="{Font}">{Esc(label)}</text>""");
        }
    }

    static string Esc(string text) => text
        .Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
