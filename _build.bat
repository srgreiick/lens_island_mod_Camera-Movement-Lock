@echo off
set location=F:\SteamLibrary\steamapps\common\Len's Island\BepInEx\plugins

dotnet build -c Release

IF "%location%"=="" (
	echo "path empty, we won't copy dll"
	exit;
) else (
	goto cp
)

:cp
echo "we gonna copy to %location%"
xcopy /s/y ".\bin\Release\net472\CameraMovementLock.dll" "%location%"
