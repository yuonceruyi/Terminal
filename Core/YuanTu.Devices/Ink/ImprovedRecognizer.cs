using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Ink;
using System.Windows.Input;
using YuanTu.Consts;

namespace YuanTu.Devices.Ink
{
    /// <summary>
    /// 改进的识别器
    /// </summary>
    public class ImprovedRecognizer : ICharactorRecognizer
    {
        /// <summary>
        /// Get 识别器名称
        /// </summary>
        public string Name
        {
            get { return "改进的识别器"; }
        }

        /// <summary>
        /// 识别
        /// </summary>
        /// <param name="strokes">笔迹集合</param>
        /// <returns>候选词数组</returns>
        public string[] Recognize(StrokeCollection strokes)
        {
            if (strokes == null || strokes.Count == 0)
                return Constants.EmptyAlternates;

            var stroke = GetCombinedStore(strokes);

            var analyzer = ReflectGetObj("IAWinFX", "System.Windows.Ink.InkAnalyzer"); 
            var strokeTypeTp = ReflectGetType("IAWinFX", "System.Windows.Ink.StrokeType");
            var kb = Enum.Parse(strokeTypeTp, "Writing");

            var analyzerType = analyzer.GetType();
            var addStroke = analyzerType.GetMethod("AddStroke", new[] { typeof(Stroke),typeof(int) });
            var setStrokeType = analyzerType.GetMethod("SetStrokeType", new[] { typeof(Stroke), strokeTypeTp });
            var analyze = analyzerType.GetMethod("Analyze");
            var getAlternates = analyzerType.GetMethod("GetAlternates",new Type[0]);
            var dispose = analyzerType.GetMethod("Dispose");

            addStroke.Invoke(analyzer, new object[] { stroke, Constants.ChsLanguageId });
            setStrokeType.Invoke(analyzer, new object[] { stroke, kb });


            var status = analyze.Invoke(analyzer,null);
            var successful = status.GetType().GetProperty("Successful");
            var issuccess = (bool)successful.GetValue(status);
            if (issuccess)
            {
                var collections = (IEnumerable)getAlternates.Invoke(analyzer, null);
                var rest =new List<string>();
               
                PropertyInfo recognizedString = null;
                foreach (var collection in collections)
                {
                    if (recognizedString==null)
                    {
                       var tp = collection.GetType();
                        recognizedString = tp.GetProperty("RecognizedString");
                    }
                    rest.Add(recognizedString.GetValue(collection).ToString());
                }
                return rest.ToArray();
            }

            dispose.Invoke(analyzer, null);
            return Constants.EmptyAlternates;
        }

        private static Stroke GetCombinedStore(StrokeCollection strokes)
        {
            var points = new StylusPointCollection();
            foreach (var stroke in strokes)
            {
                points.Add(stroke.StylusPoints);
            }
            return new Stroke(points);
        }

        private object ReflectGetObj(string assembly, string fullName)
        {
            return System.Reflection.Assembly.LoadFrom(Path.Combine(FrameworkConst.RootDirectory, "External\\Ink\\" + assembly + ".dll") ).CreateInstance(fullName, false);
        }
        private Type ReflectGetType(string assembly, string fullName)
        {
            return System.Reflection.Assembly.LoadFrom(Path.Combine(FrameworkConst.RootDirectory, "External\\Ink\\" + assembly + ".dll")).GetType(fullName, false);
        }
    }
}
