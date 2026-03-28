# My custom keyboards
ZMK firmware/config for my wireless Corne keyboards.

## Keyboard layout/ keymap

### Default layer
![default layer key layout](docs/img/default_layer.svg)

### Symbols layer (1)
![symbols layer key layout](docs/img/symbols_layer.svg)

### Nav layer (2)
![Navigation layer key layout](docs/img/nav_layer.svg)

- below F3: CTRL+SHIFT+B - build solution (Visual Studio)
- below F4: CTRL+. - suggestions (Visual Studio/ VSCode)

## Combos

| Combo | Keys | Layer |
|-------|------|-------|
| ![cut](docs/img/combo_cut.svg) | Z + X | Default |
| ![copy](docs/img/combo_copy.svg) | X + C | Default |
| ![paste](docs/img/combo_paste.svg) | C + V | Default |
| ![caps_word](docs/img/combo_caps_word.svg) | T + Y | Default |
| ![escape](docs/img/combo_escape.svg) | J + K | Default |
| ![reset_bluetooth](docs/img/combo_reset_bluetooth.svg) | E+R+T+Y+U+I | Nav |

## Generating keymap visuals

```bash
cd scripts/generate-keymap-svg
dotnet run -- --keymap ../../config/corne.keymap --output ../../docs/img --layout corne
```

## Keyboards

### Black case (thocky)
firmware: blk_corne_left, blk_corne_right
![Keyboard black](docs/img/key_black.jpg)

### White case (silent)
firmware: wh_corne_left, wh_corne_right
![Keyboard black](docs/img/keyb_white.jpg)

### Gray case (silent)
firmware: grs_corne_left, grs_corne_right
![Keyboard black](docs/img/keyb_grayblue.jpg)

### Low profile
firmware: lp_corne_left, lp_corne_right
![Keyboard black](docs/img/low_profile.jpg)
