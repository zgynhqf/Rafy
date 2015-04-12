/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150318
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150318 14:05
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Rafy.Domain;

namespace iWS.Web.Http
{
    public abstract class GeneralApiController<TEntity> : ApiController
        where TEntity : Entity
    {
        private EntityRepository _repo;

        public GeneralApiController()
        {
            _repo = RF.Find<TEntity>();
        }

        public EntityRepository Repo
        {
            get { return _repo; }
        }

        //[HttpGet]
        //public object GetAll()
        //{
        //    return Repo.GetAll();
        //}

        protected object Get(ODataQueryCriteria criteria)
        {
            return _repo.GetBy(criteria);
        }

        protected bool Add(TEntity entity)
        {
            _repo.Save(entity);
            return true;
        }

        protected bool Update(TEntity entity)
        {
            _repo.Save(entity);
            return true;
        }

        protected bool Delete(int id)
        {
            var entity = _repo.GetById(id);
            if (entity != null)
            {
                entity.PersistenceStatus = PersistenceStatus.Deleted;
                _repo.Save(entity);
            }
            return true;
        }
    }
}