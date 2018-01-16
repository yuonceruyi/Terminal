@echo off
set dir=TongXiang1Yuan
set fileName=Í©Ïç1Ôº%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%.rar
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


copy YuanTu.TongXiangHospitals.dll %dir%
copy YuanTu.TongXiangFirstHospital.dll %dir%


"C:\Program Files\WinRAR\rar" a  "%fileName%" %dir%
::cd %dir%
