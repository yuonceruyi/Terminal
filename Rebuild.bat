
if exist "C:\Program Files\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initialize2k8on64Dev14
if exist "C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initialize2k8Dev14
if exist "D:\Program Files\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initialize2k8on64Dev14D
if exist "D:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" goto initialize2k8Dev14D

echo "Unable to detect suitable environment. Build may not succeed."
goto build


:initialize2k8Dev14
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" amd64
goto build

:initialize2k8on64Dev14
call "C:\Program Files\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" amd64
goto build

:initialize2k8Dev14D
call "D:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" amd64
goto build

:initialize2k8on64Dev14D
call "D:\Program Files\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" amd64
goto build

:build
if "%~1"=="" msbuild Terminal.sln /t:Build /p:Configuration=Release /p:Platform="Any CPU"
msbuild Terminal.sln /t:%~1 /p:Configuration=Release /p:Platform="Any CPU"


pause
goto end


:end

