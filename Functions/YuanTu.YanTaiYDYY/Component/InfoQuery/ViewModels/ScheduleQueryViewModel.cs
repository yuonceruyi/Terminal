using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using Microsoft.Practices.Unity;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.YanTaiYDYY.Component.InfoQuery.Service;
using Prism.Commands;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class ScheduleQueryViewModel : ViewModelBase
    {
        public override string Title => "门诊排班查询";

        public ScheduleQueryViewModel()
        {
            ButtonDetailPreviewCommand = new DelegateCommand(PreviewConfirm);
            ButtonDetailNextCommand = new DelegateCommand(NextConfirm);
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            UpdateDetails();
        }

        private Uri _imgUri;
        public Uri ImgUri
        {
            get { return _imgUri; }
            set
            {
                _imgUri = value;
                OnPropertyChanged();
            }
        }

        private string _lblPage;
        public string LblPage
        {
            get { return _lblPage; }
            set
            {
                _lblPage = value;
                OnPropertyChanged();
            }
        }

        public ICommand ButtonDetailPreviewCommand { get; set; }
        public ICommand ButtonDetailNextCommand { get; set; }

        public override void OnSet()
        {
            ImgUri = GetImageResourceUri();
        }

        private static Uri GetImageResourceUri()
        {
            if (ScheduleFile.ScheduleFileList != null && ScheduleFile.ScheduleFileList.Count > 0)
            {
                var file = ScheduleFile.ScheduleFileList.FirstOrDefault();
                return string.IsNullOrWhiteSpace(file)
                    ? null
                    : new Uri(ScheduleFile.ScheduleFileListPath + "/" + file, UriKind.Absolute);
            }
            else
            {
                return null;
            }
        }

        private int _detialindex = 0;
        private int _detailcount = 0;

        private void PreviewConfirm()
        {
            if (_detialindex < 1)
            {
                return;
            }
            _detialindex--;
            UpdateDetails();
        }

        private void NextConfirm()
        {
            if (_detialindex >= _detailcount - 1)
            {
                return;
            }
            _detialindex++;
            UpdateDetails();
        }

        private void UpdateDetails()
        {
            if (ScheduleFile.ScheduleFileList != null && ScheduleFile.ScheduleFileList.Count > 0)
            {
                var file = ScheduleFile.ScheduleFileList[_detialindex];
                _detailcount = ScheduleFile.ScheduleFileList.Count;

                ImgUri = string.IsNullOrWhiteSpace(file)
                ? null : new Uri(ScheduleFile.ScheduleFileListPath + "/" + file, UriKind.Absolute);

                LblPage = $"{_detialindex + 1}/{ ScheduleFile.ScheduleFileList.Count}";

                //todo 按钮是否可见处理

            }
        }
    }
}
