@ECHO OFF
cls

echo Removing old builds
wsl rm -r ./Builds/*
echo Done!

echo Building Android
wsl cp ./BuildAndroid/Vulnus_Beta_Android.apk ./Builds/Vulnus_Beta_Android.apk
echo Done!

echo Building Windows
cd ./BuildWin/
@REM wsl zip -q -r -Z bzip2 ../Builds/Vulnus_Beta_Win.zip ./* -x ./Vulnus_BackUpThisFolder_ButDontShipItWithYourGame/\* ./settings.json ./maps/\* ./assets/\*
"C:\Program Files\7-Zip\7z.exe" a -tzip ../Builds/Vulnus_Beta_Win.zip . -r -x!Vulnus_BackUpThisFolder_ButDontShipItWithYourGame -x!settings.json -x!maps -x!assets
cd ../
echo Done!

echo Building MacOS
cd ./BuildMac/
wsl zip -q -r -Z bzip2 ../Builds/Vulnus_Beta_Mac.zip Vulnus.app
cd ../
echo Done!

echo Building Linux
cd ./BuildLinux/
wsl zip -q -r -Z bzip2 ../Builds/Vulnus_Beta_Linux.zip ./* -x ./Vulnus_BackUpThisFolder_ButDontShipItWithYourGame/\* ./settings.json ./maps/\* ./assets/\*
cd ../
echo Done!

echo Nothing left to build!
timeout /T 5
