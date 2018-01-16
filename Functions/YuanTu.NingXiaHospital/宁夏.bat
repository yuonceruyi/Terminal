@echo off
set dir=ningxia
set fileName=ÄþÏÄÖÐÒ½Ôº%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%.rar
set HospitalId=00
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


copy YuanTu.NingXiaHospital.dll %dir%


"C:\Program Files\WinRAR\rar" a  "%fileName%" %dir%
cd %dir%
del /s /f %HospitalId%.rar
::"C:\Program Files\WinRAR\rar" a 1004.rar
"D:\Program Files (x86)\7-Zip\7z.exe" a %HospitalId%.7z * -r
::explorer "%cd%\%dir%"