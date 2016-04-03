/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：Rafy中使用的版本号检测器。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rafy.Threading;
using Rafy.Utils.Caching;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// Rafy中使用的版本号检测器。
    /// </summary>
    [DebuggerDisplay("{_version.ClassRegion} {_version.ScopeClass} {_version.ScopeId}")]
    public class VersionChecker : ChangeChecker
    {
        private EntityListVersion _version;

        public VersionChecker(Type regionType, Type scopeClass = null, string scopeId = null)
        {
            this._version = VersionSyncMgr.Repository.GetOrNew(regionType, scopeClass, scopeId);
        }

        public override void Check()
        {
            var currentVersion = VersionSyncMgr.Repository.Get(
                this._version.ClassRegion,
                this._version.ScopeClass,
                this._version.ScopeId
                );

            //有可能已经被清空。
            if (currentVersion == null ||
                currentVersion.Value != this._version.Value)
            {
                this.NotifyChanged();
            }
        }

        public override CheckerMemoto GetMemoto()
        {
            return VCM.Create(this);
        }

        /// <summary>
        /// Used by Memoto
        /// </summary>
        private VersionChecker() { }

        public override bool Equals(object obj)
        {
            var vc = obj as VersionChecker;
            if (vc != null)
            {
                return vc._version.ClassRegion == this._version.ClassRegion &&
                    vc._version.ScopeClass == this._version.ScopeClass &&
                    vc._version.ScopeId == this._version.ScopeId &&
                    vc._version.Value == this._version.Value;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this._version.ClassRegion.GetHashCode() ^
                this._version.ScopeClass.GetHashCode() ^
                this._version.ScopeId.GetHashCode() ^
                this._version.Value.GetHashCode();
        }

        /// <summary>
        /// VersionChecker的Memoto
        /// “VersionCheckerMemoto”
        /// </summary>
        [Serializable]
        private class VCM : CheckerMemoto
        {
            private string ClassRegion;

            private string ScopeClass;

            private string ScopeId;

            private DateTime Value;

            public override ChangeChecker Restore()
            {
                Type regionType = Type.GetType(ClassRegion);
                Type scopeClass = null;
                if (!string.IsNullOrWhiteSpace(ScopeClass))
                {
                    scopeClass = Type.GetType(ScopeClass);
                }

                return new VersionChecker()
                {
                    _version = new EntityListVersion()
                    {
                        ClassRegion = regionType,
                        ScopeClass = scopeClass,
                        ScopeId = ScopeId,
                        Value = Value
                    }
                };
            }

            public static VCM Create(VersionChecker owner)
            {
                var version = owner._version;

                var result = new VCM()
                {
                    ClassRegion = version.ClassRegion.AssemblyQualifiedName,
                    ScopeId = version.ScopeId,
                    Value = version.Value
                };
                if (version.ScopeClass != null)
                {
                    result.ScopeClass = version.ScopeClass.AssemblyQualifiedName;
                }

                return result;
            }
        }
    }
}