/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211027
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211027 09:46
 * 
*******************************************************/

using Rafy.ManagedProperty;
using Rafy.MetaModel.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.UI
{
    public class UIEnvironmentProvider : EnvironmentProvider
    {
        private static EntityViewMeta _lastViewMeta;

        public override string GetLabelForDisplay(IManagedProperty property, Type entityType)
        {
            //以线程安全的方式获取最后一次缓存的 View。
            EntityViewMeta safeView = _lastViewMeta;
            if (safeView == null || safeView.EntityType != entityType)
            {
                safeView = UIModel.Views.CreateBaseView(entityType);
                _lastViewMeta = safeView;
            }

            string res = null;

            var pvm = safeView.Property(property);
            if (pvm != null) res = pvm.Label;

            //如果是引用 Id 属性没有配置 Label，则尝试使用它对应的引用实体属性的 Label 来显示。
            if (string.IsNullOrEmpty(res))
            {
                var refMP = property as IRefIdProperty;
                if (refMP != null)
                {
                    pvm = safeView.Property(refMP.RefEntityProperty);
                    if (pvm != null) res = pvm.Label;
                }
            }

            if (!string.IsNullOrEmpty(res))
            {
                return res.Translate();
            }

            return base.GetLabelForDisplay(property, entityType);
        }

        public override string GetLabelForDisplay(Type entityType)
        {
            //以线程安全的方式获取最后一次缓存的 View。
            EntityViewMeta safeView = _lastViewMeta;
            var ownerType = entityType;
            if (safeView == null || safeView.EntityType != ownerType)
            {
                safeView = UIModel.Views.CreateBaseView(ownerType);
                _lastViewMeta = safeView;
            }

            string res = safeView.Label;
            if (!string.IsNullOrEmpty(res))
            {
                return res.Translate();
            }

            return base.GetLabelForDisplay(entityType);
        }
    }
}
