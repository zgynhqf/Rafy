using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EnvDTE;
using EnvDTE80;
using Rafy.EntityObjectModel;
using Rafy.DomainModeling;
using Rafy.DomainModeling.Models;
using Rafy.VSPackage.Modeling.CodeSync;
using Rafy.DomainModeling.Controls;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Specialized;

namespace Rafy.VSPackage.Modeling
{
    /// <summary>
    /// VS 中的模型编辑器控件。
    /// </summary>
    public partial class ModelingEditor : UserControl
    {
        private EditorPane _owner;
        private DTE _dte;

        public ModelingEditor(EditorPane owner)
        {
            _owner = owner;
            _dte = owner.DTE;

            InitializeComponent();

            designer.Blocks.CollectionChanged += Blocks_CollectionChanged;

            AddChangedHandler();
            AllowDropFile();
        }

        //public ModelingDesigner Designer
        //{
        //    get
        //    {
        //        return designer;
        //    }
        //}

        #region 读取项目的实体对象模型

        private EOMGroup _eom;

        private EOMGroup EOM
        {
            get
            {
                if (_eom == null)
                {
                    _eom = ProjectEOM.Get(_owner.Project);
                }

                return _eom;
            }
        }

        private void ResetEOM()
        {
            _eom = null;
            ProjectEOM.Reset();
        }

        #endregion

        #region 交互接口

        private bool _isReadOnly = false;

        internal bool ReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        internal void LoadDocument(string fileName)
        {
            designer.LoadDocument(fileName);

            OnDesignerDocumentLoaded();
        }

        internal void SaveDocument(string fileName)
        {
            designer.SaveDocument(fileName);
        }

        #endregion

        #region 文档变更

        private void AddChangedHandler()
        {
            designer.DataContextChanged += (o, e) =>
            {
                var doc = e.NewValue as ODMLDocument;
                if (doc != null)
                {
                    //文档变更，就算变更。
                    doc.Changed += (oo, ee) => this.OnChanged();
                }
            };
        }

        public event EventHandler Changed;

        protected virtual void OnChanged()
        {
            var handler = this.Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region 允许文件拖拽

        private const string VisualStudioProjectItem = "CF_VSSTGPROJECTITEMS";

        private TypeNameFinder _typeFinder = new TypeNameFinder();

        private void AllowDropFile()
        {
            this.AllowDrop = true;
            this.DragOver += ModelingEditor_DragEnter;
            this.Drop += ModelingEditor_Drop;
        }

        void ModelingEditor_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(VisualStudioProjectItem) && _owner.Project != null)
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        void ModelingEditor_Drop(object sender, DragEventArgs e)
        {
            var fileName = e.Data.GetData(typeof(string)) as string;

            var items = _owner.Project.ProjectItems;
            foreach (ProjectItem item in ProjectHelper.EnumerateCSharpFiles(items))
            {
                var itemFileName = item.get_FileNames(0);
                if (itemFileName == fileName)
                {
                    OnItemDropped(item);
                    break;
                }
            }

            e.Handled = true;
        }

        private void OnItemDropped(ProjectItem item)
        {
            //查找该文件中的所有类型全名称
            var typeFullNames = _typeFinder.FindTypes(item);

            var document = designer.GetDocument();

            //在第一次使用 EOM 前，保存鼠标的位置。这是因为获取 EOM 可能需要比较长的时间，之后鼠标的位置就变化了。
            designer.CaptureMouse();
            var pos = Mouse.GetPosition(designer);
            designer.ReleaseMouseCapture();

            //添加所有需要添加的类型。
            var entityTypes = typeFullNames.Select(fullName => EOM.EntityTypes.Find(fullName))
                .Where(e => e != null).ToArray();
            if (entityTypes.Length == 0)
            {
                MessageBox.Show("没有找到与文件对应的实体对象，如果文件是刚添加到项目中的，请使用“从代码更新”功能更新缓存。");
                return;
            }

            var list = entityTypes.Where(type =>
            {
                //已经存在的元素，则直接忽略。
                var el = document.FindEntityType(type.FullName);
                if (el != null)
                {
                    MessageBox.Show(string.Format("{0} 已经存在！", type.FullName));
                    return false;
                }
                return true;
            }).ToArray();
            if (list.Length == 0) return;

            //把类型加入到图中。
            AddTypesInDocument(list);

            //设置新加元素的位置在鼠标处。
            foreach (var type in list)
            {
                var el = document.FindEntityType(type.FullName);
                if (el != null)
                {
                    el.Left = pos.X;
                    el.Top = pos.Y;
                }
            }
        }

        #endregion

        #region 双击打开文档

        void Blocks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OpenFileOnNewBlocks(e);
        }

        private void OnDesignerDocumentLoaded()
        {
            foreach (BlockControl item in designer.Blocks)
            {
                OpenFileOnClick(item);
            }
        }

        private void OpenFileOnNewBlocks(NotifyCollectionChangedEventArgs e)
        {
            if (_owner.Project != null)
            {
                if (e.NewItems != null)
                {
                    foreach (BlockControl item in e.NewItems)
                    {
                        OpenFileOnClick(item);
                    }
                }
            }
        }

        private void OpenFileOnClick(BlockControl block)
        {
            block.MouseDoubleClick += (o, e) =>
            {
                var finder = new TypeFileFinder();
                var item = finder.FindClassFile(_owner.Project, block.TypeFullName);
                if (item != null)
                {
                    var fileName = item.get_FileNames(0);
                    _dte.Documents.Open(fileName);
                }
            };
        }

        #endregion

        #region 按钮：添加类型到图中

        private void btnAddClasses_Click(object sender, EventArgs e)
        {
            if (_owner.Project == null) return;

            try
            {
                var ctWin = new ChooseTypesWindow(EOM);
                var res = ctWin.ShowDialog();

                if (res == true)
                {
                    var list = ctWin.SelectedEntities;
                    if (list.Count > 0)
                    {
                        AddTypesInDocument(list);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void AddTypesInDocument(IList<EntityType> list)
        {
            var document = designer.GetDocument();
            ODMLDocumentHelper.AddToDocument(new AddToDocumentArgs
            {
                Docment = document,
                TypeList = list,
                AllTypes = EOM.EntityTypes
            });

            //使用双向绑定，不再需要手动调用文档加载。
            //designer.LoadDocument(document);
        }

        #endregion

        #region 按钮：从代码更新

        private void btnRefreshClasses_Click(object sender, EventArgs e)
        {
            try
            {
                if (_owner.Project == null) return;
                ResetEOM();

                var oldDocument = designer.GetDocument();

                //创建一个新文档对象，使用它来描述最新的文档结构。
                var document = new ODMLDocument();

                //把原来显示的所有实体类型，加入到新文档对象中。
                var eom = EOM;
                var types = oldDocument.EntityTypes.Select(el => eom.EntityTypes.Find(el.FullName))
                    .Where(t => t != null).ToArray();
                ODMLDocumentHelper.AddToDocument(new AddToDocumentArgs
                {
                    Docment = document,
                    TypeList = types,
                    AllTypes = eom.EntityTypes
                });

                //新的元素的位置，还原为所有旧元素的位置
                foreach (var el in document.EntityTypes)
                {
                    var oldTypeEl = oldDocument.FindEntityType(el.FullName);
                    el.Left = oldTypeEl.Left;
                    el.Top = oldTypeEl.Top;
                    el.Width = oldTypeEl.Width;
                    el.Height = oldTypeEl.Height;
                }
                foreach (var el in document.EnumTypes)
                {
                    var oldTypeEl = oldDocument.FindEnumType(el.FullName);
                    el.Left = oldTypeEl.Left;
                    el.Top = oldTypeEl.Top;
                    el.Width = oldTypeEl.Width;
                    el.Height = oldTypeEl.Height;
                }
                foreach (var el in document.Connections)
                {
                    var old = oldDocument.FindConnection(el);
                    if (old != null)
                    {
                        el.Hidden = old.Hidden;
                        el.ConnectionType = old.ConnectionType;
                        el.FromPointPos = old.FromPointPos;
                        el.ToPointPos = old.ToPointPos;
                    }
                }

                //绑定新文档对象到设计器中，丢弃旧对象。
                designer.BindDocument(document);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        #endregion

        #region 按钮：添加关系类型到图中

        private void btnAddRealtions_Click(object sender, EventArgs e)
        {
            if (_owner.Project == null) return;

            var blocks = designer.SelectedItems
                .Where(i => i.Kind == DesignerComponentKind.Block)
                .Cast<BlockControl>().ToArray();

            //把原来显示的所有实体类型，加入到新文档对象中。
            var selectedTypes = blocks.Select(el => EOM.EntityTypes.Find(el.TypeFullName)).ToArray();

            var types = new List<EntityType>();
            foreach (var item in selectedTypes)
            {
                if (item.BaseType != null && !this.IsVeryBaseType(item.BaseType))
                {
                    types.Add(item.BaseType);
                }

                foreach (var reference in item.References)
                {
                    types.Add(reference.RefEntityType);
                }

                foreach (var child in item.Children)
                {
                    types.Add(child.ChildEntityType);
                }
            }

            AddTypesInDocument(types);
        }

        /// <summary>
        /// 判断指定的基类是否是其它所有实体类型的基类。
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        private bool IsVeryBaseType(EntityType baseType)
        {
            var count = 0;
            foreach (var type in this.EOM.EntityTypes)
            {
                if (type.IsSubclassOf(baseType))
                {
                    count++;
                }
            }
            return count == this.EOM.EntityTypes.Count - 1;
        }

        #endregion
    }
}
