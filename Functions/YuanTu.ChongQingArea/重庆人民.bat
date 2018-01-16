@echo off
set dir=ChongQingRM
set fileName=重庆人民医院%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%.rar
set hospitalId=1101
md %dir%

::rd /s /q %dir%

copy Terminal.exe %dir%
copy YuanTu.Default.dll %dir%
copy YuanTu.Default.Clinic.dll %dir%
copy YuanTu.Default.House.dll %dir%
copy YuanTu.Default.Theme.dll %dir%

copy YuanTu.AutoUpdater.dll %dir%
copy YuanTu.Consts.dll %dir%
copy YuanTu.Core.dll %dir%
copy YuanTu.Devices.dll %dir%
copy YuanTu.ISO8583.dll %dir%


copy YuanTu.ChongQingArea.dll %dir%


"C:\Program Files\WinRAR\rar" a  "%fileName%" %dir%
cd %dir%
del /s /f %hospitalId%.rar
::"C:\Program Files\WinRAR\rar" a %hospitalId%.rar
"C:\Program Files (x86)\Adobe\Adobe Creative Cloud\Utils\zip\7za.exe" a %hospitalId%.7z * -r
::explorer "%cd%\%dir%"