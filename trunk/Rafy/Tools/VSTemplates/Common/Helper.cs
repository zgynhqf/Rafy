using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using RafySDK;

namespace Rafy.VSPackage
{
    internal static class Helper
    {
        public static string GetResourceContent(string key)
        {
            return GetResourceContent(typeof(Helper).Assembly, key);
        }

        public static string GetResourceContent(Assembly assembly, string key)
        {
            var templateStream = assembly.GetManifestResourceStream(key);
            using (var sr = new StreamReader(templateStream))
            {
                return sr.ReadToEnd();
            }
        }

        internal static bool IsEntity(CodeClass codeClass)
        {
            //使用 Attribute 来进行检测。
            foreach (CodeAttribute attri in codeClass.Attributes)
            {
                //RootEntity or ChildEntity
                if (attri.FullName == Consts.RootEntityAttributeClassFullName ||
                    attri.FullName == Consts.ChildEntityAttributeClassFullName)
                {
                    return true;
                }
            }

            return InheritesEntityNotCriteria(codeClass);
        }

        internal static bool IsRepository(CodeClass codeClass)
        {
            if (codeClass.Name.EndsWith(Consts.RepositorySuffix))
            {
                var baseClass = Helper.GetBaseClass(codeClass);
                if (baseClass != null && baseClass.Name.EndsWith(Consts.RepositorySuffix))
                {
                    return true;
                }
            }

            return false;
        }

        internal static string GetEntityNameForRepository(CodeClass repo)
        {
            ////使用 Attribute 来进行获取实体类名。
            //foreach (CodeAttribute attri in repo.Attributes)
            //{
            //    //RootEntity or ChildEntity
            //    if (attri.FullName == Consts.RepositoryForAttributeClassFullName)
            //    {
            //    }
            //}

            return repo.Name.Substring(0, repo.Name.Length - Consts.RepositorySuffix.Length);
        }

        private static bool InheritesEntityNotCriteria(CodeClass codeClass)
        {
            var baseClass = Helper.GetBaseClass(codeClass);
            if (baseClass != null)
            {
                if (baseClass.Name == "Entity") { return true; }

                //如果是 Criteria，则返回 False
                if (baseClass.Name == "Criteria") { return false; }
                return InheritesEntityNotCriteria(baseClass);
            }

            return false;
        }

        /// <summary>
        /// 获取某个类的基类。
        /// 有时会找不到对应的对象、或者没有基类，则返回 null。
        /// </summary>
        /// <param name="codeClass"></param>
        /// <returns></returns>
        internal static CodeClass GetBaseClass(CodeClass codeClass)
        {
            //查找基类的方式检测可能会出现以下问题，原因不详：
            //如果基类不在同一程序集时，codeClass.Bases 中的元素的类型(Kind)不是 CodeClass，而是 Other。
            //所以会查找基类失败。
            foreach (CodeElement item in codeClass.Bases)
            {
                if (item.Kind == vsCMElement.vsCMElementOther)
                {
                    throw new InvalidOperationException(string.Format("无法读取类型 {0} 的基类，请先编译程序集。", codeClass.Name));
                }
                if (item is CodeClass)
                {
                    return item as CodeClass;
                }
            }

            return null;
        }

        private static void RegisterGAC()
        {
            //if (File.Exists(vsVarsPath))
            //{
            //    Process.Start("cmd.exe", string.Concat(new string[]
            //    {
            //        "/k \"" + SanitizePath(vsVarsPath)
            //    }));
            //}
        }

        private static string GetVsVarsPath(IServiceProvider isp)
        {
            System.Windows.MessageBox.Show("DDDDDDD");

            string result;
            if (Environment.GetEnvironmentVariable("VS100COMNTOOLS") != null)
            {
                result = Path.Combine(Environment.GetEnvironmentVariable("VS100COMNTOOLS"), "vsvars32.bat");
            }
            else
            {
                var service = isp.GetService(typeof(SLocalRegistry)) as ILocalRegistry2;
                string name;
                service.GetLocalRegistryRoot(out name);
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name))
                {
                    string str = registryKey.GetValue("InstallDir").ToString();
                    result = Path.Combine(str + "\\..\\Tools\\", "vsvars32.bat");
                }
            }
            return result;
        }

        internal static string GetVsShellDir(IServiceProvider isp)
        {
            var service = isp.GetService(typeof(SLocalRegistry)) as ILocalRegistry2;
            string name;
            service.GetLocalRegistryRoot(out name);
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name))
            {
                string path = registryKey.GetValue("ShellFolder").ToString();
                return path;
            }
        }

        private static string SanitizePath(string path)
        {
            string text = path;
            if (text.IndexOf(" ") > -1)
            {
                text = "\"" + text + "\"";
            }
            return text;
        }
    }
}
