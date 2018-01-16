using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using System.Xml.Linq;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.LocalResourceEngine;

// ReSharper disable once CheckNamespace
namespace YuanTu.ChongQingArea.Clinic
{
    public class MainWindowViewModel : YuanTu.Default.MainWindowViewModel
    {
        private List<string> _imageNameList = null;
        private int _imageLoc = 0;

        public override void OnViewInit()
        {
            base.OnViewInit();
            //ADImageUrl = ResourceEngine.GetImageResourceUri("规范建设图.png");
            _imageNameList = ResourceEngine.GetResourceFullPathWithRegex("规范建设图.*").OrderBy(p => p).ToList();
            (new Thread(Timer_Tick) { IsBackground = true, Priority = ThreadPriority.Lowest }).Start();
        }

        private void Timer_Tick()
        {
            while (true)
            {
                if (_imageNameList == null || !_imageNameList.Any())
                {
                    return;
                }
                if (_imageLoc > _imageNameList.Count - 1)
                {
                    _imageLoc = 0;
                }

                ADImageUrl = new Uri(_imageNameList[_imageLoc++]);
                Thread.Sleep(60 * 1000);
            }
           
        }
    }
}