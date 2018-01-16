@echo off
set dir=YiWuBYUpdate
set fileName=ÒåÎÚ±±Ô·%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%.rar
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


copy YuanTu.YiWuArea.dll %dir%
copy YuanTu.YiWuBeiYuan.dll %dir%


"C:\Program Files\WinRAR\rar" a  "%fileName%" %dir%
cd %dir%
del /s /f 1015.rar
::"C:\Program Files\WinRAR\rar" a 1015.rar
"C:\Program Files (x86)\Adobe\Adobe Creative Cloud\Utils\zip\7za.exe" a 1015.7z * -r
::explorer "%cd%\%dir%"