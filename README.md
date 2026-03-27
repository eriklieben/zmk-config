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
- default layer: cut, copy, paste, caps_word, escape (J+K)
- nav layer: reset_bluetooth

![Combos](docs/img/combos.svg)

## Low profile (Eyelash Corne)

### Default layer
![default layer key layout](docs/img/lp_qwerty_layer.svg)

### Symbols layer (1)
![symbols layer key layout](docs/img/lp_sym_layer.svg)

### Nav layer (2)
![Navigation layer key layout](docs/img/lp_nav_layer.svg)

### Combos
![Combos](docs/img/lp_combos.svg)

## Generating keymap visuals

```bash
cd scripts/generate-keymap-svg
dotnet run -- --keymap ../../config/corne.keymap --output ../../docs/img --layout corne
dotnet run -- --keymap ../../config/lpkeyb/eyelash_corne.keymap --output ../../docs/img --layout eyelash --prefix lp
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
