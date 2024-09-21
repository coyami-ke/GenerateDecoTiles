SETLOCAL ENABLEDELAYEDEXPANSION
set /p version=<VERSION.txt
mkdir tmp
cd tmp
mkdir GenerateDecoTiles
cp ../Info.json GenerateDecotiles
cp ../MyAdofaiMod/bin/Release/GenerateDecoTiles.dll GenerateDecoTiles

cd GenerateDecoTiles
for /f "delims=" %%a in (Info.json) do (
    SET s=%%a
    SET s=!s:$VERSION=%version%!
    echo !s!
)>>"InfoChanged.json"
rm Info.json
mv InfoChanged.json Info.json
cd ..

tar -a -c -f GenerateDecoTiles-%version%.zip GenerateDecoTiles
mv GenerateDecoTiles-%version%.zip ..
cd ..
rm -rf tmp
pause