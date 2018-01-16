using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Services
{
    public interface IRecognitionService:IService
    {
        string StartAnalyze(StrokeCollection strocks);
    }

    public class RecognitionService : IRecognitionService
    {
        #region Implementation of IService

        public string ServiceName => "基于Ink的手写识别";

        public string StartAnalyze(StrokeCollection strocks)
        {
            var asm = Assembly.LoadFrom(@"External\Ink\IAWinFX.dll");
            var typ = asm.GetType("System.Windows.Ink.InkAnalyzer");
            var theInkAnalyzer = asm.CreateInstance("System.Windows.Ink.InkAnalyzer");
            var addStrokes = typ.GetMethod("AddStrokes", new[] {typeof (StrokeCollection), typeof (int)});
            addStrokes.Invoke(theInkAnalyzer, new object[] {strocks, 0x0804});

            var analyze = typ.GetMethod("Analyze");
            var status = analyze.Invoke(theInkAnalyzer, null);

            var getRecognizedString= typ.GetMethod("GetRecognizedString");
            var rest = getRecognizedString.Invoke(theInkAnalyzer, null);
            (theInkAnalyzer as IDisposable)?.Dispose();
            return rest as string;

        }

        #endregion
    }
}
 