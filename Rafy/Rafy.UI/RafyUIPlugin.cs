/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211027
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211027 09:28
 * 
*******************************************************/

using Rafy.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.UI
{
    public class RafyUIPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            //RafyEnvironment.Provider.Translator = new EmptyTranslator();
        }
    }
}
