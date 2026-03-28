# My custom keyboards
ZMK firmware/config for my wireless Corne keyboards.

<!-- BEGIN KEYMAP - AUTO-GENERATED, DO NOT EDIT -->

## Keyboard layout/ keymap

### default_layer
![default_layer](docs/img/default_layer.svg)

### symbols
![symbols](docs/img/symbols_layer.svg)

### nav
![nav](docs/img/nav_layer.svg)

## Combos

### cut
![cut](docs/img/combo_cut.svg)

### copy
![copy](docs/img/combo_copy.svg)

### paste
![paste](docs/img/combo_paste.svg)

### caps_word
![caps_word](docs/img/combo_caps_word.svg)

### escape
![escape](docs/img/combo_escape.svg)

### reset_bluetooth
![reset_bluetooth](docs/img/combo_reset_bluetooth.svg)

<!-- END KEYMAP -->

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
