﻿using System.Collections.Generic;
using System.Linq;

namespace YuanTu.BJArea
{
    /// <summary>
    /// 地址(省市区)
    /// </summary>
    public class AddressItem
    {
        public string Code { get; set; }
        public string Flag { get; set; }
        public int Level { get; set; }
        public string ParentCode { get; set; }
        public string Name { get; set; }
        public string SimpleCode { get; set; }

        public List<AddressItem> Children { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AddressInfo
    {
        public AddressInfo(List<AddressItem> list)
        {
            var level0 = list.Where(i => i.Level == 0).ToList();
            var level1 = list.Where(i => i.Level == 1).ToList();
            var level2 = list.Where(i => i.Level == 2).ToList();
            var level3 = list.Where(i => i.Level == 3).ToList();

            Link(level0, level1);
            Link(level1, level2);
            Link(level2, level3);

            Level0 = level0;
            Level1 = level1;
            Level2 = level2;
            Level3 = level3;
        }

        public IReadOnlyList<AddressItem> Level0 { get; }
        public IReadOnlyList<AddressItem> Level1 { get; }
        public IReadOnlyList<AddressItem> Level2 { get; }
        public IReadOnlyList<AddressItem> Level3 { get; }

        private static void Link(IEnumerable<AddressItem> parents, IEnumerable<AddressItem> children)
        {
            var groups = children.GroupBy(i => i.ParentCode).ToList();
            foreach (var info in parents)
            {
                var match = groups.FirstOrDefault(g => g.Key == info.Code);
                if (match != null)
                    info.Children = match.ToList();
            }
        }
    }

    /// <summary>
    /// 监护人关系 
    /// </summary>
    public class RelationItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    public class RelationInfo
    {
        public List<RelationItem> List { get; set; }
    }
    /// <summary>
    /// 宗教信仰
    /// </summary>
    public class ReligionItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    public class ReligionInfo
    {
        public List<ReligionItem> List { get; set; }
    }
    /// <summary>
    /// 教育程度
    /// </summary>
    public class EducationItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    public class EducationInfo
    {
        public List<EducationItem> List { get; set; }
    }
    /// <summary>
    /// 教育程度
    /// </summary>
    public class NationItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    public class NationInfo
    {
        public List<NationItem> List { get; set; }
    }
}