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
        public virtual SerialNumberInfo CreateDailySerialNumberInfo(string name, string format = "yyyyMMdd********")
        {
            var sni = new SerialNumberInfo
            {
                Name = name,
                TimeGroupFormat = "yyyyMMdd",
                Format = format,
                RollValueStart = 1,
                RollValueStep = 1,
            };

            var infoRepo = RF.ResolveInstance<SerialNumberInfoRepository>();
            infoRepo.Save(sni);

            return sni;
        }

        /// <summary>
        /// 为指定名称的编码规则生成当前时间对应的最新的编号。
        /// 性能-仓库访问次数：2（未更换组）、3-4（组不可用，需要更换新组）。
        /// </summary>
        /// <param name="name">自动编码规则的名称</param>
        /// <returns></returns>
        public virtual string GenerateNext(string name)
        {
            var valueRepo = RF.ResolveInstance<SerialNumberValueRepository>();
            var value = valueRepo.GetLastValue(name);
            if (value != null)
            {
                return this.GenerateNext(value);
            }

            return this.GenerateNext(name, DateTime.Now);
        }

        /// <summary>
        /// 为指定名称的编码规则生成当前时间对应的最新的编号。
        /// 性能-仓库访问次数：2。
        /// </summary>
        /// <param name="info">自动编码规则。</param>
        /// <returns></returns>
        public string GenerateNext(SerialNumberInfo info)
        {
            return this.GenerateNext(info, DateTime.Now);
        }

        /// <summary>
        /// 为指定的编码值对应的编码规则生成最新的编号。
        /// 性能-仓库访问次数：1（未更换组）、2-3（组不可用，需要更换新组）。
        /// </summary>
        /// <param name="currentGroupValue">当前最新的值。</param>
        /// <returns></returns>
        public virtual string GenerateNext(SerialNumberValue currentGroupValue)
        {
            var now = DateTime.Now;
            var timeKey = GetTimeGroupKey(now, currentGroupValue.RD_TimeKeyFormat);

            //需要验证传入的 Value 是否应是当前时间对应的组。
            if (currentGroupValue.TimeKey == timeKey)
            {
                return this.GenerateNext(currentGroupValue, now);
            }
            else
            {
                return this.GenerateNext(currentGroupValue.SerialNumberInfo);
            }
        }

        /// <summary>
        /// 为指定名称的编码规则生成最新的编号。
        /// 性能-仓库访问次数：2（未更换组）、3-4（组不可用，需要更换新组）。
        /// </summary>
        /// <param name="name">自动编码规则的名称</param>
        /// <param name="specificTime">需要生成编号的指定的时间。</param>
        /// <returns></returns>
        public string GenerateNext(string name, DateTime specificTime)
        {
            var infoRepo = RF.ResolveInstance<SerialNumberInfoRepository>();
            var sni = infoRepo.GetByName(name);
            if (sni == null) throw new InvalidOperationException(string.Format("没有找到名称是 {0} 的规则。", name));

            return this.GenerateNext(sni, specificTime);
        }

        /// <summary>
        /// 为指定名称的编码规则生成指定时间对应的最新的编号。
        /// 性能-仓库访问次数：2。
        /// </summary>
        /// <param name="info">自动编码规则。</param>
        /// <param name="specificTime">需要生成编号的指定的时间。</param>
        /// <returns></returns>
        public virtual string GenerateNext(SerialNumberInfo info, DateTime specificTime)
        {
            if (info == null) throw new ArgumentNullException("info");

            //计算 specificTime 时间对应的分组 Key
            string timeGroupKey = GetTimeGroupKey(specificTime, info.TimeGroupFormat);

            var valueRepo = RF.ResolveInstance<SerialNumberValueRepository>();
            using (var tran = RF.TransactionScope(valueRepo))
            {
                //先找到当前的值。
                var currentGroupValue = valueRepo.GetByKey(info.Name, timeGroupKey);

                string res = null;
                if (currentGroupValue != null)
                {
                    res = this.GenerateNext(currentGroupValue, specificTime);
                }
                else
                {
                    //如果当前值还不存在，则直接添加一个到库中。
                    currentGroupValue = new SerialNumberValue
                    {
                        SerialNumberInfo = info,
                        TimeKey = timeGroupKey,
                        RollValue = info.RollValueStart
                    };
                    valueRepo.Save(currentGroupValue);

                    res = this.GenerateCode(specificTime, currentGroupValue.RollValue, info.Format);
                }

                tran.Complete();

                return res;
            }
        }

        /// <summary>
        /// 性能-仓库访问次数：1。
        /// </summary>
        /// <param name="currentGroupValue"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private string GenerateNext(SerialNumberValue currentGroupValue, DateTime time)
        {
            //流动当前的值。
            currentGroupValue.RollValue += currentGroupValue.RD_RollValueStep;

            var valueRepo = RF.ResolveInstance<SerialNumberValueRepository>();
            valueRepo.Save(currentGroupValue);

            return this.GenerateCode(time, currentGroupValue.RollValue, currentGroupValue.RD_Format);
        }

        /// <summary>
        /// 在内存中生成指定时间、指定格式、指定流水值的流水号。
        /// </summary>
        /// <param name="time"></param>
        /// <param name="rollValue"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        protected virtual string GenerateCode(DateTime time, int rollValue, string format)
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

        internal static string GetTimeGroupKey(DateTime time, string timeKeyFormat)
        {
            return SerialNumberInfo.GetTimeGroupKey(time, timeKeyFormat);
        }
    }
}
