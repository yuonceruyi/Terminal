@echo off
set fileName=.\Versions\Õ©œÁ∆§∑¿‘∫%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%.rar

"C:\Program Files\WinRAR\rar" a "%fileName%" Terminal.exe YuanTu.Default.dll YuanTu.Default.Clinic.dll YuanTu.Default.House.dll YuanTu.Default.Theme.dll  YuanTu.AutoUpdater.dll YuanTu.Consts.dll YuanTu.Core.dll YuanTu.Devices.dll YuanTu.ISO8583.dll YuanTu.TongXiangHospitals.dll YuanTu.TongXiangPiFangHospital.dll 

;PAUSE