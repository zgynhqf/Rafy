using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 一个用于保存实体列表的服务。
    /// </summary>
    public interface ISaveListService : IFlowService
    {
        /// <summary>
        /// 需要保存的列表。
        /// </summary>
        [ServiceInput, ServiceOutput]
        IEntityList EntityList { get; set; }
    }
}
