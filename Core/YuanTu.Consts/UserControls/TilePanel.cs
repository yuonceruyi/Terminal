using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace YuanTu.Consts.UserControls
{
    public class TilePanel : Panel
    {
        private static readonly Dictionary<int, ItemRowPackage> DefaultRuleDictionary;

        #region DependencyProperty

        public static readonly DependencyProperty LayoutRuleProperty = DependencyProperty.Register(
            nameof(LayoutRule), typeof(string), typeof(TilePanel), new FrameworkPropertyMetadata((l, e) =>
            {
                var p = l as TilePanel;
                p.ItemRuleDictionary = RuleBuilder.Build(e.NewValue as string);
            }));

        public static readonly DependencyProperty GapProperty = DependencyProperty.Register(
            nameof(Gap), typeof(Size), typeof(TilePanel), new FrameworkPropertyMetadata(Size.Empty));

        public static readonly DependencyProperty ItemVerticalAlignmentProperty = DependencyProperty.Register(
            nameof(ItemVerticalAlignment), typeof(VerticalAlignment), typeof(TilePanel),
            new FrameworkPropertyMetadata(VerticalAlignment.Center));

        public string LayoutRule
        {
            get => (string)GetValue(LayoutRuleProperty);
            set => SetValue(LayoutRuleProperty, value);
        }

        public Size Gap
        {
            get => (Size)GetValue(GapProperty);
            set => SetValue(GapProperty, value);
        }

        public VerticalAlignment ItemVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(ItemVerticalAlignmentProperty);
            set => SetValue(ItemVerticalAlignmentProperty, value);
        }

        #endregion

        static TilePanel()
        {
            DefaultRuleDictionary =
                RuleBuilder.Build(
                    "[L,1],[L,1,1],[L,1,1,1],[L,1,1,1,1],[L,1,1,1,1 L,1],[L,1,1,1,1 L,1,1],[L,1,1,1,1 L,1,1,1],[L,1,1,1,1 L,1,1,1,1],[L,1,1,1,1 L,1,1,1,1 L,1],[L,1,1,1,1 L,1,1,1,1 L,1,1]");
        }

        /*
        规则设计：
            [C,1,1,0 C,0,1,1],[L,1,1,1 R,1,1,1]
                四个图标          六个图标
        */
        public Dictionary<int, ItemRowPackage> ItemRuleDictionary { get; set; }


        protected override Size ArrangeOverride(Size finalSize)
        {
            var count = InternalChildren.Count;
            if (count == 0)
                return finalSize;

            var rule = (ItemRuleDictionary?.ContainsKey(InternalChildren.Count) ?? false)
                ? ItemRuleDictionary[InternalChildren.Count]
                : GetRuleFromDefault(InternalChildren.Count);

            var index = 0;
            foreach (UIElement child in InternalChildren)
            {
                var size = child.DesiredSize;
                var pt = rule.GetPoint(index, size, Gap, finalSize, ItemVerticalAlignment);
                child.Arrange(new Rect(pt, size));
                index++;
            }

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var count = InternalChildren.Count;
            if (count == 0)
                return availableSize;

            var size = new Size();
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                var curSize = child.DesiredSize;
                if (size.Width < curSize.Width)
                    size.Width = curSize.Width;
                if (size.Height < curSize.Height)
                    size.Height = curSize.Height;
            }
            return size;
        }

        private ItemRowPackage GetRuleFromDefault(int itemcount)
        {
            if (!DefaultRuleDictionary.ContainsKey(itemcount))
            {
                var rowItemCount = 5;
                //构建一个适配的
                //var ruleStr = new StringBuilder();
                var ruleArr=new List<string>();
                for (int i = 0; i < itemcount; i+= rowItemCount)
                {
                    var min = Math.Min(itemcount - i, rowItemCount);
                    if (min>0)
                    {
                        var tmp = "".PadRight(min, '1').ToArray();
                        ruleArr.Add($"L,{string.Join(",",tmp)}");
                    }
                }
                var bd = RuleBuilder.ItemRowPackage($"[{string.Join(" ", ruleArr)}]");
                DefaultRuleDictionary[bd.Item1] = bd.Item2;
            }
           
            return DefaultRuleDictionary[itemcount];
        }
    }

    public class ItemRowPackage
    {
        public List<ItemRow> ItemRows { get; set; }

        private int GetInRow(int index) //从0开始
        {
            var total = 0;
            for (var i = 0; i < ItemRows.Count; i++)
            {
                total += ItemRows[i].ItemData.Count(p => !p.IsPlaceholder);
                if (index < total)
                    return i;
            }
            return -1;
        }

        private int GetInColumn(int rowIndex, int totalIndex) //从0开始
        {
            var rowdata = ItemRows[rowIndex];
            var lastRealCount = ItemRows.Take(rowIndex).Sum(p => p.ItemData.Count(q => !q.IsPlaceholder));
            var realcurrentIndex = totalIndex - lastRealCount + 1;
            var startIndex = 0;
            for (var i = 0; i < rowdata.ItemData.Length; i++)
                if (!rowdata.ItemData[i].IsPlaceholder)
                {
                    startIndex++;
                    if (startIndex == realcurrentIndex)
                        return i;
                }
            return -1;
        }

        public Point GetPoint(int index, Size baseSize, Size gap, Size totalSize, VerticalAlignment valignment)
        {
            var rowIndex = GetInRow(index); //所在行，0表示第一行
            var colIndex = GetInColumn(rowIndex, index); //所在列，0表示第一列 占位列也会计入
            var rule = ItemRows[rowIndex];
            double py = 0;
            switch (valignment)
            {
                case VerticalAlignment.Top:
                    py = (baseSize.Height + gap.Height) * rowIndex;
                    break;

                case VerticalAlignment.Center:
                    var rows = ItemRows.Count;
                    var startY = (totalSize.Height - (baseSize.Height * rows + gap.Height * (rows - 1))) / 2;
                    py = startY + (baseSize.Height + gap.Height) * rowIndex;
                    break;

                case VerticalAlignment.Bottom:
                    var relIndex = ItemRows.Count;
                    py = totalSize.Height - (baseSize.Height * relIndex + gap.Height * (relIndex - 1));
                    break;

                case VerticalAlignment.Stretch:
                default:
                    throw new ArgumentOutOfRangeException(nameof(valignment), valignment, null);
            }

            switch (rule.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                {
                    var px = (baseSize.Width + gap.Width) * colIndex;
                    return new Point(px, py);
                }
                case HorizontalAlignment.Center:
                {
                    var count = rule.ItemData.Length;
                    var startX = (totalSize.Width - (baseSize.Width * count + gap.Width * (count - 1))) / 2;
                    var px = startX + (baseSize.Width + gap.Width) * colIndex;
                    return new Point(px, py);
                }
                case HorizontalAlignment.Right:
                {
                    var relIndex = rule.ItemData.Length - colIndex;
                    var px = totalSize.Width - (baseSize.Width * relIndex + gap.Width * (relIndex - 1));
                    return new Point(px, py);
                }
                case HorizontalAlignment.Stretch:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //先一律居左
            return new Point();
        }
    }

    public class ItemRow
    {
        public int RowIndex { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public Item[] ItemData { get; set; }
    }

    public class Item
    {
        public bool IsPlaceholder { get; set; }
    }

    public static class RuleBuilder
    {
        private static readonly Dictionary<string, HorizontalAlignment> HorizontalAlignmentDict =
            new Dictionary<string, HorizontalAlignment>
            {
                ["L"] = HorizontalAlignment.Left,
                ["C"] = HorizontalAlignment.Center,
                ["R"] = HorizontalAlignment.Right
            };

        public static Dictionary<int, ItemRowPackage> Build(string msg)
        {
            // [C,1,1,0 C,0,1,1],[L,1,1,1 R,1,1,1]
            var ruleDict = new Dictionary<int, ItemRowPackage>();
            var tmpdata = msg.Trim(' ', '\r', '\n'); //去掉中括号
            var spit = Regex.Split(tmpdata, "(?<=\\])\\s*.\\s*(?=\\[)"); //(?<=\s*\]\s*),(?=\s*\[\s*)
            foreach (var rule in spit)
            {
                try
                {
                    var tpu = ItemRowPackage(rule);
                    ruleDict[tpu.Item1] =tpu.Item2;
                }
                catch
                {
                    //
                }
            }
            return ruleDict;
        }

        public static Tuple<int, ItemRowPackage> ItemRowPackage(string rule)
        {
            //去掉中括号 并分割出每行的数据
            var tmpdata = rule.Trim(' ', '[', ']','\r','\n'); //去掉中括号
            var tmp2data = Regex.Replace(tmpdata, "\\s*,\\s*", ","); //将分隔符在存在空格的去掉
            var ruleData = tmp2data.Split(' '); //分割
            var rows = new List<ItemRow>();
            for (var i = 0; i < ruleData.Length; i++)
            {
                rows.Add(BuildItemRow(i, ruleData[i]));
            }
            var itmscount = rows.Sum(p => p.ItemData.Count(q => !q.IsPlaceholder));
            return new Tuple<int, ItemRowPackage>(itmscount, new ItemRowPackage { ItemRows = rows });
        }

        private static ItemRow BuildItemRow(int index, string rowstr)
        {
            var row = new ItemRow {RowIndex = index};
            var arr = rowstr.Split(',');
            if (!HorizontalAlignmentDict.ContainsKey(arr[0]))
                row.HorizontalAlignment = HorizontalAlignment.Left;
            else
                row.HorizontalAlignment = HorizontalAlignmentDict[arr[0]];
            row.ItemData = arr.Skip(1).SelectMany(BuildItem).ToArray();
            return row;
        }

        private static Item[] BuildItem(string chr)
        {
           
            if (int.TryParse(chr, out int realVal))
            {
                //结果若是0，则为占位
                if (realVal==0)
                {
                    return new[]
                    {
                        new Item { IsPlaceholder = true }
                    };
                }
                var itemarr = new Item[realVal];
                for (int i = 0; i < realVal; i++)
                {
                    itemarr[i] = new Item { IsPlaceholder = false };
                }
                return itemarr;
            }
            return new Item[0];
        }
    }
}