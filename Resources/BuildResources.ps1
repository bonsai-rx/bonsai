# Builds ICO files
# Dependencies (must be in current PATH): Inkscape 0.92+, ImageMagick

md -Force build | Out-Null
$icons = Get-ChildItem -Path .\* -Include *.svg -Exclude Bonsai.svg,BonsaiGallery.svg
ForEach ($icon in $icons)
{
    $icon.Name | Out-Default
	$output = [System.IO.Path]::ChangeExtension($icon.Name,".ico")
    &inkscape $icon.Name --export-png=build/16.png --export-dpi=2.40 --export-background-opacity=0 --without-gui | Out-Default
	&inkscape $icon.Name --export-png=build/32.png --export-dpi=4.80 --export-background-opacity=0 --without-gui | Out-Default
	&inkscape $icon.Name --export-png=build/48.png --export-dpi=7.20 --export-background-opacity=0 --without-gui | Out-Default
	&inkscape $icon.Name --export-png=build/64.png --export-dpi=9.60 --export-background-opacity=0 --without-gui | Out-Default
	&inkscape $icon.Name --export-png=build/256.png --export-dpi=38.40 --export-background-opacity=0 --without-gui | Out-Default
	&magick build/16.png build/32.png build/48.png build/64.png build/256.png $output
	$output | Out-Default; ""
}
rm -Recurse -Force build