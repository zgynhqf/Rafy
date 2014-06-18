/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Windows;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace Rafy
{
    /// <summary>
    /// 一组 Region
    /// </summary>
    public class RegionContainer
    {
        private List<Region> _regions = new List<Region>();

        private AggtBlocks _aggt;

        public RegionContainer(AggtBlocks aggt)
        {
            if (aggt == null) throw new ArgumentNullException("aggt");
            this._aggt = aggt;
        }

        /// <summary>
        /// 对应的聚合类型界面元数据
        /// </summary>
        public AggtBlocks BlocksInfo
        {
            get { return this._aggt; }
        }

        /// <summary>
        /// 获取所有的区域
        /// </summary>
        public IList<Region> Regions
        {
            get { return this._regions; }
        }

        /// <summary>
        /// 尝试使用名字查找控件。
        /// 未找到，则返回null。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ControlResult FindControl(string name)
        {
            var region = this.Regions.FirstOrDefault(r => r.Name == name);

            if (region != null) return region.ControlResult;

            return null;
        }

        /// <summary>
        /// 获取所有的孩子区域控件。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Region> GetChildrenRegions()
        {
            foreach (var region in this._regions)
            {
                if (region.Name == TraditionalRegions.Children)
                {
                    yield return region;
                }
            }
        }

        /// <summary>
        /// 添加一个控件到区域集合中。
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="control"></param>
        public void Add(string regionName, ControlResult control)
        {
            this._regions.Add(new Region(regionName, control));
        }

        /// <summary>
        /// 添加一个控件到孩子区域集合中。
        /// </summary>
        /// <param name="control"></param>
        public void AddChildren(string label, ControlResult control)
        {
            this._regions.Add(new Region(TraditionalRegions.Children, label, control));
        }
    }
}