using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel.View;
using Rafy.Web;

namespace Rafy.RBAC.Old
{
    /// <summary>
    /// 查询某个客户端实体对应的显示名称。
    /// </summary>
    [JsonService]
    [Contract, ContractImpl]
    public class QueryModelLabelService : Service
    {
        public string ClientEntity { get; set; }
        [ServiceOutput]
        public string Label { get; set; }

        protected override void Execute()
        {
            var em = ClientEntities.Find(ClientEntity);
            if (em != null)
            {
                var evm = UIModel.Views.CreateBaseView(em.EntityType);
                Label = evm.Label;
            }
        }
    }
}
