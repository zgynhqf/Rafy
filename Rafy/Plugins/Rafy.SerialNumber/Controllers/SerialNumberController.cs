/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160318
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160318 09:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain;

namespace Rafy.SerialNumber
{
    /// <summary>
    /// 自动编码逻辑控制器
    /// </summary>
    public class SerialNumberController : DomainController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialNumberController"/> class.
        /// </summary>
        protected SerialNumberController() { }

        /// <summary>
        /// 创建一个以日期进行分组生成编号的规则，存储到仓库中，并返回。
        /// 性能-仓库访问次数：1。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public SerialNumberInfo CreateDailySerialNumberInfo(string name, string format = "yyyyMMdd********")
        {
            var sni = new SerialNumberInfo
            {
                Name = name,
                TimeGroupFormat = "yyyyMMdd",
                Format = format,
                RollValueStart = 1,
                RollValueStep = 1,
            };

            var infoRepo = RF.Concrete<SerialNumberInfoRepository>();
            infoRepo.Save(sni);

            return sni;
        }

        /// <summary>
        /// 为指定名称的编码规则生成最新的编号。
        /// 性能-仓库访问次数：2（未更换组）、3-4（组不可用，需要更换新组）。
        /// </summary>
        /// <param name="name">自动编码规则的名称</param>
        /// <returns></returns>
        public string GenerateNext(string name)
        {
            var valueRepo = RF.Concrete<SerialNumberValueRepository>();
            var value = valueRepo.GetLastValue(name);
            if (value != null)
            {
                return this.GenerateNext(value);
            }

            var infoRepo = RF.Concrete<SerialNumberInfoRepository>();
            var sni = infoRepo.GetByName(name);
            if (sni == null) throw new InvalidOperationException(string.Format("没有找到名称是 {0} 的规则。", name));

            return this.GenerateNext(sni);
        }

        /// <summary>
        /// 为指定名称的编码规则生成最新的编号。
        /// 性能-仓库访问次数：2。
        /// </summary>
        /// <param name="info">自动编码规则。</param>
        /// <returns></returns>
        public string GenerateNext(SerialNumberInfo info)
        {
            if (info == null) throw new ArgumentNullException("info");

            var now = DateTime.Now;

            string timeKey = null;
            var timeKeyFormat  = info.TimeGroupFormat;
            if (!string.IsNullOrEmpty(timeKeyFormat))
            {
                timeKey = now.ToString(timeKeyFormat);
            }

            var valueRepo = RF.Concrete<SerialNumberValueRepository>();
            using (var tran = RF.TransactionScope(valueRepo))
            {
                //先找到当前的值。
                var currentValue = valueRepo.GetByKey(info.Name, timeKey);

                string res = null;

                if (currentValue != null)
                {
                    res = this.GenerateNext(currentValue, now);
                }
                else
                {
                    //如果当前值还不存在，则直接添加一个到库中。
                    currentValue = new SerialNumberValue
                    {
                        SerialNumberInfo = info,
                        TimeKey = timeKey,
                        RollValue = info.RollValueStart
                    };
                    valueRepo.Save(currentValue);

                    res = GenerateCode(now, currentValue.RollValue, info.Format);
                }

                tran.Complete();

                return res;
            }
        }

        /// <summary>
        /// 为指定的编码值对应的编码规则生成最新的编号。
        /// 性能-仓库访问次数：1（未更换组）、2-3（组不可用，需要更换新组）。
        /// </summary>
        /// <param name="currentValue">当前最新的值。</param>
        /// <returns></returns>
        public string GenerateNext(SerialNumberValue currentValue)
        {
            var now = DateTime.Now;
            string timeKey = null;
            var timeKeyFormat  = currentValue.RD_TimeKeyFormat;
            if (!string.IsNullOrEmpty(timeKeyFormat))
            {
                timeKey = now.ToString(timeKeyFormat);
            }

            //需要验证传入的 Value 是否应是当前时间对应的组。
            if (currentValue.TimeKey == timeKey)
            {
                return this.GenerateNext(currentValue, now);
            }
            else
            {
                return this.GenerateNext(currentValue.SerialNumberInfo);
            }
        }

        /// <summary>
        /// 性能-仓库访问次数：1。
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private string GenerateNext(SerialNumberValue currentValue, DateTime now)
        {
            currentValue.RollValue += currentValue.RD_RollValueStep;
            var valueRepo = RF.Concrete<SerialNumberValueRepository>();
            valueRepo.Save(currentValue);

            return GenerateCode(now, currentValue.RollValue, currentValue.RD_Format);
        }

        private static string GenerateCode(DateTime time, int rollValue, string format)
        {
            var index = format.IndexOf('*');
            var starsCount = 0;
            if (index >= 0)
            {
                for (int i = index, c = format.Length; i < c; i++)
                {
                    if (format[i] == '*')
                    {
                        starsCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            var stars = new string('*', starsCount);
            var rollStrValue = rollValue.ToString(new string('0', starsCount));
            var timeFormat  = format.Replace(stars, rollStrValue);
            var res = time.ToString(timeFormat);
            return res;
        }
    }
}
