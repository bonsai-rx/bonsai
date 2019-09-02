# Building ICO files

**Dependency**: ImageMagick

```
inkscape --export-png=build/16.png --export-dpi=2.40 --export-background-opacity=0 --without-gui icon.svg

convert build/16.png build/32.png build/48.png build/64.png build/128.png build/256.png  icon.ico
```