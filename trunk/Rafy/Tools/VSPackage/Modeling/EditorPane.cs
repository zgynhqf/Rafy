using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using tom;
using System.Linq;

using ISysServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VSStd97CmdID = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using Rafy.DomainModeling.Controls;
using System.Collections.Specialized;
using Rafy.DomainModeling.Models;
using Rafy.VSPackage.Modeling.CodeSync;

namespace Rafy.VSPackage.Modeling
{
    /// <summary>
    /// This control host the editor (an extended RichTextBox) and is responsible for
    /// handling the commands targeted to the editor as well as saving and loading
    /// the document. This control also implement the search and replace functionalities.
    /// </summary>

    ///////////////////////////////////////////////////////////////////////////////
    // Having an entry in the new file dialog.
    //
    // For our file type should appear under "General" in the new files dialog, we need the following:-
    //     - A .vsdir file in the same directory as NewFileItems.vsdir (generally under Common7\IDE\NewFileItems).
    //       In our case the file name is Editor.vsdir but we only require a file with .vsdir extension.
    //     - An empty odml file in the same directory as NewFileItems.vsdir. In
    //       our case we chose DomainModel.odml. Note this file name appears in Editor.vsdir
    //       (see vsdir file format below)
    //     - Three text strings in our language specific resource. File Resources.resx :-
    //          - "Rich Text file" - this is shown next to our icon.
    //          - "A blank rich text file" - shown in the description window
    //             in the new file dialog.
    //          - "DomainModel" - This is the base file name. New files will initially
    //             be named as DomainModel1.odml, DomainModel2.odml... etc.
    ///////////////////////////////////////////////////////////////////////////////
    // Editor.vsdir contents:-
    //    DomainModel.odml|{3085E1D6-A938-478e-BE49-3546C09A1AB1}|#106|80|#109|0|401|0|#107
    //
    // The fields in order are as follows:-
    //    - DomainModel.odml - our empty odml file
    //    - {db16ff5e-400a-4cb7-9fde-cb3eab9d22d2} - our Editor package guid
    //    - #106 - the ID of "Rich Text file" in the resource
    //    - 80 - the display ordering priority
    //    - #109 - the ID of "A blank rich text file" in the resource
    //    - 0 - resource dll string (we don't use this)
    //    - 401 - the ID of our icon
    //    - 0 - various flags (we don't use this - se vsshell.idl)
    //    - #107 - the ID of "odml"
    ///////////////////////////////////////////////////////////////////////////////

    //This is required for Find In files scenario to work properly. This provides a connection point 
    //to the event interface
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [ComSourceInterfaces(typeof(IVsTextViewEvents))]
    [ComVisible(true)]
    public sealed class EditorPane : Microsoft.VisualStudio.Shell.WindowPane,
        IVsPersistDocData,  //to Enable persistence functionality for document data
        IPersistFileFormat, //to enable the programmatic loading or saving of an object in a format specified by the user.
        IVsFileChangeEvents,//to notify the client when file changes on disk
        IVsDocDataFileChangeControl, //to Determine whether changes to files made outside of the editor should be ignored
        IVsFileBackup,      //to support backup of files. Visual Studio File Recovery backs up all objects in the Running Document Table that support IVsFileBackup and have unsaved changes.
        IVsFindTarget,      //to implement find and replace capabilities within the editor
        IVsToolboxUser      //Sends notification about Toolbox items to the owner of these items
    {
        #region Fields

        private const uint MyFormat = 0;
        private const string MyExtension = ".odml";
        private static string[] fontSizeArray = { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" };

        private VSPackagePackage _package;

        private string _fileName = string.Empty;
        /// <summary>
        /// returns whether the contents of file have changed since the last save
        /// </summary>
        private bool _isDirty;
        private ModelingEditor _editorControl;

        private IVsFileChangeEx _vsFileChangeEx;

        private Timer _fileChangeTrigger = new Timer();
        private Timer _fnfStatusbarTrigger = new Timer();

        private bool _fileChangedTimerSet;
        private int _ignoreFileChangeLevel;
        private bool _backupObsolete = true;
        private uint _vsFileChangeCookie;

        private object _findState;
        private ArrayList _textSpanArray = new ArrayList();

        private IExtensibleObjectSite _extensibleObjectSite;

        #endregion

        #region 构造器

        /// <summary>
        /// Constructor that calls the Microsoft.VisualStudio.Shell.WindowPane constructor then
        /// our initialization functions.
        /// </summary>
        /// <param name="package">Our Package instance.</param>
        public EditorPane(VSPackagePackage package)
            : base(null)
        {
            PrivateInit(package);
        }

        /// <summary>
        /// Initialization routine for the Editor. Loads the list of properties for the odml document 
        /// which will show up in the properties window 
        /// </summary>
        /// <param name="package"></param>
        private void PrivateInit(VSPackagePackage package)
        {
            _package = package;
            _trackSel = null;
            var sp = package as System.IServiceProvider;
            _dte = sp.GetService(typeof(DTE)) as DTE;

            Control.CheckForIllegalCrossThreadCalls = false;

            // Create and initialize the editor
            _editorControl = new ModelingEditor(this);
            _editorControl.Changed += (o, e) => _isDirty = true;
            this.Content = this._editorControl;

            var resources = new ComponentResourceManager(typeof(EditorPane));
            resources.ApplyResources(this._editorControl, "editorControl", CultureInfo.CurrentUICulture);

            this.InitSelection();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose the timers
                    if (null != _fileChangeTrigger)
                    {
                        _fileChangeTrigger.Dispose();
                        _fileChangeTrigger = null;
                    }
                    if (null != _fnfStatusbarTrigger)
                    {
                        _fnfStatusbarTrigger.Dispose();
                        _fnfStatusbarTrigger = null;
                    }

                    SetFileChangeNotification(null, false);

                    if (_editorControl != null)
                    {
                        //_editorControl.RichTextBoxControl.Dispose();
                        //_editorControl.Dispose();
                        _editorControl = null;
                    }
                    if (_fileChangeTrigger != null)
                    {
                        _fileChangeTrigger.Dispose();
                        _fileChangeTrigger = null;
                    }
                    if (_extensibleObjectSite != null)
                    {
                        _extensibleObjectSite.NotifyDelete(this);
                        _extensibleObjectSite = null;
                    }
                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion

        #region 一些属性

        private DTE _dte;

        public DTE DTE
        {
            get { return _dte; }
        }

        private Project _project;

        /// <summary>
        /// 这个编辑器编辑的文档所在的项目。
        /// 如果这个属性为空，表示这个文档并不属于某个项目，或者当前文档还没有加载完成。
        /// </summary>
        public Project Project
        {
            get
            {
                if (_project == null)
                {
                    var doc = _dte.ActiveDocument;
                    if (doc != null)
                    {
                        _project = doc.ProjectItem.ContainingProject;
                    }
                }

                return _project;
            }
        }

        public ModelingDesigner Designer
        {
            get { return _editorControl.designer; }
        }

        #endregion

        #region 选择项

        private EditorProperties _editorProperties;
        private ArrayList _selectableObjects = new ArrayList();
        private ArrayList _selectedObjects = new ArrayList();

        private void InitSelection()
        {
            // Create the object that will show the document's properties
            // on the properties window.
            _editorProperties = new EditorProperties(this);
            _selectableObjects.Add(_editorProperties);
            _selectedObjects.Add(_editorProperties);

            // Create the SelectionContainer object.
            _selContainer = new Microsoft.VisualStudio.Shell.SelectionContainer(true, false);
            _selContainer.SelectableObjects = _selectableObjects;
            _selContainer.SelectedObjects = _selectedObjects;

            Designer.Blocks.CollectionChanged += Blocks_CollectionChanged;
            Designer.Relations.CollectionChanged += Relations_CollectionChanged;
            Designer.SelectionChanged += Designer_SelectionChanged;
        }

        void Blocks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetSelectableObjects();
        }

        void Relations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetSelectableObjects();
        }

        void Designer_SelectionChanged(object sender, EventArgs e)
        {
            CreateSelectedObjects();
        }

        private void ResetSelectableObjects()
        {
            _selectedObjects.Clear();
            _selectableObjects.Clear();
            _selectableObjects.Add(_editorProperties);

            foreach (var relation in Designer.Relations)
            {
                var element = relation.DataContext as ConnectionElement;
                _selectableObjects.Add(new ConnectionProperties(element));
            }

            foreach (var block in Designer.Blocks)
            {
                var element = block.DataContext as BlockElement;
                _selectableObjects.Add(new BlockProperties(element));
            }

            CreateSelectedObjects();
        }

        private void CreateSelectedObjects()
        {
            _selectedObjects.Clear();

            foreach (var item in Designer.SelectedItems)
            {
                if (item.Kind == DesignerComponentKind.Relation)
                {
                    var element = (item as BlockRelation).DataContext as ConnectionElement;
                    foreach (var itemProperties in _selectableObjects)
                    {
                        var cp = itemProperties as ConnectionProperties;
                        if (cp != null && cp.Element == element)
                        {
                            _selectedObjects.Add(cp);
                            break;
                        }
                    }
                }
                else
                {
                    var element = (item as BlockControl).DataContext as BlockElement;
                    foreach (var itemProperties in _selectableObjects)
                    {
                        var cp = itemProperties as BlockProperties;
                        if (cp != null && cp.Element == element)
                        {
                            _selectedObjects.Add(cp);
                            break;
                        }
                    }
                }
            }

            if (_selectedObjects.Count == 0)
            {
                _selectedObjects.Add(_editorProperties);
            }

            NotifySelectionChanged();
        }

        private Microsoft.VisualStudio.Shell.SelectionContainer _selContainer;

        private ITrackSelection _trackSel;

        /// <summary>
        /// returns the name of the file currently loaded
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        /// <summary>
        /// returns an instance of the ITrackSelection service object
        /// </summary>
        private ITrackSelection TrackSelection
        {
            get
            {
                if (_trackSel == null)
                {
                    _trackSel = (ITrackSelection)GetService(typeof(ITrackSelection));
                }
                return _trackSel;
            }
        }

        /// <summary>
        /// 通知选择项已经发生改变。
        /// </summary>
        /// <returns></returns>
        private int NotifySelectionChanged()
        {
            var track = TrackSelection;
            if (null != track)
            {
                var hr = track.OnSelectChange((ISelectionContainer)_selContainer);
                return hr;
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region IPersistFileFormat Members

        /// <summary>
        /// Notifies the object that it has concluded the Save transaction
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name</param>
        /// <returns>S_OK if the function succeeds</returns>
        int IPersistFileFormat.SaveCompleted(string pszFilename)
        {
            // TODO:  Add Editor.SaveCompleted implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the path to the object's current working file 
        /// </summary>
        /// <param name="ppszFilename">Pointer to the file name</param>
        /// <param name="pnFormatIndex">Value that indicates the current format of the file as a zero based index
        /// into the list of formats. Since we support only a single format, we need to return zero. 
        /// Subsequently, we will return a single element in the format list through a call to GetFormatList.</param>
        /// <returns></returns>
        int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            // We only support 1 format so return its index
            pnFormatIndex = MyFormat;
            ppszFilename = _fileName;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Initialization for the object 
        /// </summary>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the current format 
        /// of the file</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.InitNew(uint nFormatIndex)
        {
            if (nFormatIndex != MyFormat)
            {
                return VSConstants.E_INVALIDARG;
            }
            // until someone change the file, we can consider it not dirty as
            // the user would be annoyed if we prompt him to save an empty file
            _isDirty = false;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the class identifier of the editor type
        /// </summary>
        /// <param name="pClassID">pointer to the class identifier</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetClassID(out Guid pClassID)
        {
            ErrorHandler.ThrowOnFailure(((Microsoft.VisualStudio.OLE.Interop.IPersist)this).GetClassID(out pClassID));
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provides the caller with the information necessary to open the standard common "Save As" dialog box. 
        /// This returns an enumeration of supported formats, from which the caller selects the appropriate format. 
        /// Each string for the format is terminated with a newline (\n) character. 
        /// The last string in the buffer must be terminated with the newline character as well. 
        /// The first string in each pair is a display string that describes the filter, such as "Text Only 
        /// (*.txt)". The second string specifies the filter pattern, such as "*.txt". To specify multiple filter 
        /// patterns for a single display string, use a semicolon to separate the patterns: "*.htm;*.html;*.asp". 
        /// A pattern string can be a combination of valid file name characters and the asterisk (*) wildcard character. 
        /// Do not include spaces in the pattern string. The following string is an example of a file pattern string: 
        /// "HTML File (*.htm; *.html; *.asp)\n*.htm;*.html;*.asp\nText File (*.txt)\n*.txt\n."
        /// </summary>
        /// <param name="ppszFormatList">Pointer to a string that contains pairs of format filter strings</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetFormatList(out string ppszFormatList)
        {
            char Endline = (char)'\n';
            string FormatList = string.Format(CultureInfo.InvariantCulture, "My Editor (*{0}){1}*{0}{1}{1}", MyExtension, Endline);
            ppszFormatList = FormatList;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the file content into the textbox
        /// </summary>
        /// <param name="pszFilename">Pointer to the full path name of the file to load</param>
        /// <param name="grfMode">file format mode</param>
        /// <param name="fReadOnly">determines if the file should be opened as read only</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            if (pszFilename == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            int hr = VSConstants.S_OK;

            // Show the wait cursor while loading the file
            IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            if (VsUiShell != null)
            {
                // Note: we don't want to throw or exit if this call fails, so
                // don't check the return code.
                hr = VsUiShell.SetWaitCursor();
            }

            // Load the file
            _editorControl.LoadDocument(pszFilename);

            _isDirty = false;

            //Determine if the file is read only on the file system
            FileAttributes fileAttrs = File.GetAttributes(pszFilename);

            int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;

            //Set readonly if either the file is readonly for the user or on the file system
            if (0 == isReadOnly && 0 == fReadOnly)
                SetReadOnly(false);
            else
                SetReadOnly(true);

            // Notify to the property window that some of the selected objects are changed
            hr = NotifySelectionChanged();
            if (ErrorHandler.Failed(hr)) return hr;

            // Hook up to file change notifications
            if (String.IsNullOrEmpty(_fileName) || 0 != String.Compare(_fileName, pszFilename, true, CultureInfo.CurrentCulture))
            {
                _fileName = pszFilename;
                SetFileChangeNotification(pszFilename, true);

                // Notify the load or reload
                NotifyDocChanged();
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines whether an object has changed since being saved to its current file
        /// </summary>
        /// <param name="pfIsDirty">true if the document has changed</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.IsDirty(out int pfIsDirty)
        {
            if (_isDirty)
            {
                pfIsDirty = 1;
            }
            else
            {
                pfIsDirty = 0;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Save the contents of the textbox into the specified file. If doing the save on the same file, we need to
        /// suspend notifications for file changes during the save operation.
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name. If the pszFilename parameter is a null reference 
        /// we need to save using the current file
        /// </param>
        /// <param name="remember">Boolean value that indicates whether the pszFileName parameter is to be used 
        /// as the current working file.
        /// If remember != 0, pszFileName needs to be made the current file and the dirty flag needs to be cleared after the save.
        ///                   Also, file notifications need to be enabled for the new file and disabled for the old file 
        /// If remember == 0, this save operation is a Save a Copy As operation. In this case, 
        ///                   the current file is unchanged and dirty flag is not cleared
        /// </param>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the format in which 
        /// the file will be saved</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            int hr = VSConstants.S_OK;
            bool doingSaveOnSameFile = false;
            // If file is null or same --> SAVE
            if (pszFilename == null || pszFilename == _fileName)
            {
                fRemember = 1;
                doingSaveOnSameFile = true;
            }

            //Suspend file change notifications for only Save since we don't have notifications setup
            //for SaveAs and SaveCopyAs (as they are different files)
            if (doingSaveOnSameFile)
                this.SuspendFileChangeNotification(pszFilename, 1);

            try
            {
                _editorControl.SaveDocument(pszFilename);
            }
            catch (ArgumentException)
            {
                hr = VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                hr = VSConstants.E_FAIL;
            }
            finally
            {
                //restore the file change notifications
                if (doingSaveOnSameFile)
                    this.SuspendFileChangeNotification(pszFilename, 0);
            }

            if (VSConstants.E_FAIL == hr)
                return hr;

            //Save and Save as
            if (fRemember != 0)
            {
                //Save as
                if (null != pszFilename && !_fileName.Equals(pszFilename))
                {
                    SetFileChangeNotification(_fileName, false); //remove notification from old file
                    SetFileChangeNotification(pszFilename, true); //add notification for new file
                    _fileName = pszFilename;     //cache the new file name
                }
                _isDirty = false;
                SetReadOnly(false);             //set read only to false since you were successfully able
                //to save to the new file                                                    
            }

            hr = NotifySelectionChanged();

            // Since all changes are now saved properly to disk, there's no need for a backup.
            _backupObsolete = false;
            return hr;
        }

        int Microsoft.VisualStudio.OLE.Interop.IPersist.GetClassID(out Guid pClassID)
        {
            pClassID = GuidList.guidVSPackageEditorFactory;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsPersistDocData Members

        /// <summary>
        /// Used to determine if the document data has changed since the last time it was saved
        /// </summary>
        /// <param name="pfDirty">Will be set to 1 if the data has changed</param>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
        {
            return ((IPersistFileFormat)this).IsDirty(out pfDirty);
        }

        /// <summary>
        /// Saves the document data. Before actually saving the file, we first need to indicate to the environment
        /// that a file is about to be saved. This is done through the "SVsQueryEditQuerySave" service. We call the
        /// "QuerySaveFile" function on the service instance and then proceed depending on the result returned as follows:
        /// If result is QSR_SaveOK - We go ahead and save the file and the file is not read only at this point.
        /// If result is QSR_ForceSaveAs - We invoke the "Save As" functionality which will bring up the Save file name 
        ///                                dialog 
        /// If result is QSR_NoSave_Cancel - We cancel the save operation and indicate that the document could not be saved
        ///                                by setting the "pfSaveCanceled" flag
        /// If result is QSR_NoSave_Continue - Nothing to do here as the file need not be saved
        /// </summary>
        /// <param name="dwSave">Flags which specify the file save options:
        /// VSSAVE_Save        - Saves the current file to itself.
        /// VSSAVE_SaveAs      - Prompts the User for a filename and saves the file to the file specified.
        /// VSSAVE_SaveCopyAs  - Prompts the user for a filename and saves a copy of the file with a name specified.
        /// VSSAVE_SilentSave  - Saves the file without prompting for a name or confirmation.  
        /// </param>
        /// <param name="pbstrMkDocumentNew">Pointer to the path to the new document</param>
        /// <param name="pfSaveCanceled">value 1 if the document could not be saved</param>
        /// <returns></returns>
        int IVsPersistDocData.SaveDocData(Microsoft.VisualStudio.Shell.Interop.VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;
            int hr = VSConstants.S_OK;

            switch (dwSave)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                    {
                        IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                        // Call QueryEditQuerySave
                        uint result = 0;
                        hr = queryEditQuerySave.QuerySaveFile(
                                _fileName,        // filename
                                0,    // flags
                                null,            // file attributes
                                out result);    // result
                        if (ErrorHandler.Failed(hr))
                            return hr;

                        // Process according to result from QuerySave
                        switch ((tagVSQuerySaveResult)result)
                        {
                            case tagVSQuerySaveResult.QSR_NoSave_Cancel:
                                // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                                // two tags have the same value.
                                pfSaveCanceled = ~0;
                                break;

                            case tagVSQuerySaveResult.QSR_SaveOK:
                                {
                                    // Call the shell to do the save for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr)) return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_ForceSaveAs:
                                {
                                    // Call the shell to do the SaveAS for us
                                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, (IPersistFileFormat)this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr)) return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_NoSave_Continue:
                                // In this case there is nothing to do.
                                break;

                            default:
                                throw new NotSupportedException("Unsupported result from QEQS");
                        }
                        break;
                    }
                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                    {
                        // Make sure the file name as the right extension
                        if (String.Compare(MyExtension, System.IO.Path.GetExtension(_fileName), true, CultureInfo.CurrentCulture) != 0)
                        {
                            _fileName += MyExtension;
                        }
                        // Call the shell to do the save for us
                        IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                        hr = uiShell.SaveDocDataToFile(dwSave, (IPersistFileFormat)this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                        if (ErrorHandler.Failed(hr)) return hr;
                        break;
                    }
                default:
                    throw new ArgumentException("Unsupported Save flag");
            };

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the document data from the file specified
        /// </summary>
        /// <param name="pszMkDocument">Path to the document file which needs to be loaded</param>
        /// <returns>S_Ok if the method succeeds</returns>
        int IVsPersistDocData.LoadDocData(string pszMkDocument)
        {
            return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
        }

        /// <summary>
        /// Used to set the initial name for unsaved, newly created document data
        /// </summary>
        /// <param name="pszDocDataPath">String containing the path to the document. We need to ignore this parameter
        /// </param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
        {
            return ((IPersistFileFormat)this).InitNew(MyFormat);
        }

        /// <summary>
        /// Returns the Guid of the editor factory that created the IVsPersistDocData object
        /// </summary>
        /// <param name="pClassID">Pointer to the class identifier of the editor type</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
        {
            return ((IPersistFileFormat)this).GetClassID(out pClassID);
        }

        /// <summary>
        /// Close the IVsPersistDocData object
        /// </summary>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines if it is possible to reload the document data
        /// </summary>
        /// <param name="pfReloadable">set to 1 if the document can be reloaded</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
        {
            // Allow file to be reloaded
            pfReloadable = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Renames the document data
        /// </summary>
        /// <param name="grfAttribs"></param>
        /// <param name="pHierNew"></param>
        /// <param name="itemidNew"></param>
        /// <param name="pszMkDocumentNew"></param>
        /// <returns></returns>
        int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            // TODO:  Add EditorPane.RenameDocData implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Reloads the document data
        /// </summary>
        /// <param name="grfFlags">Flag indicating whether to ignore the next file change when reloading the document data.
        /// This flag should not be set for us since we implement the "IVsDocDataFileChangeControl" interface in order to 
        /// indicate ignoring of file changes
        /// </param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.ReloadDocData(uint grfFlags)
        {
            return ((IPersistFileFormat)this).Load(_fileName, grfFlags, 0);
        }

        /// <summary>
        /// Called by the Running Document Table when it registers the document data. 
        /// </summary>
        /// <param name="docCookie">Handle for the document to be registered</param>
        /// <param name="pHierNew">Pointer to the IVsHierarchy interface</param>
        /// <param name="itemidNew">Item identifier of the document to be registered from VSITEM</param>
        /// <returns></returns>
        int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsFileChangeEvents Members

        /// <summary>
        /// Notify the editor of the changes made to one or more files
        /// </summary>
        /// <param name="cChanges">Number of files that have changed</param>
        /// <param name="rgpszFile">array of the files names that have changed</param>
        /// <param name="rggrfChange">Array of the flags indicating the type of changes</param>
        /// <returns></returns>
        int IVsFileChangeEvents.FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** Inside FilesChanged ****"));

            //check the different parameters
            if (0 == cChanges || null == rgpszFile || null == rggrfChange)
                return VSConstants.E_INVALIDARG;

            //ignore file changes if we are in that mode
            if (_ignoreFileChangeLevel != 0)
                return VSConstants.S_OK;

            for (uint i = 0; i < cChanges; i++)
            {
                if (!String.IsNullOrEmpty(rgpszFile[i]) && String.Compare(rgpszFile[i], _fileName, true, CultureInfo.CurrentCulture) == 0)
                {
                    // if the readonly state (file attributes) have changed we can immediately update
                    // the editor to match the new state (either readonly or not readonly) immediately
                    // without prompting the user.
                    if (0 != (rggrfChange[i] & (int)_VSFILECHANGEFLAGS.VSFILECHG_Attr))
                    {
                        FileAttributes fileAttrs = File.GetAttributes(_fileName);
                        int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                        SetReadOnly(isReadOnly != 0);
                    }
                    // if it looks like the file contents have changed (either the size or the modified
                    // time has changed) then we need to prompt the user to see if we should reload the
                    // file. it is important to not synchronously reload the file inside of this FilesChanged
                    // notification. first it is possible that there will be more than one FilesChanged 
                    // notification being sent (sometimes you get separate notifications for file attribute
                    // changing and file size/time changing). also it is the preferred UI style to not
                    // prompt the user until the user re-activates the environment application window.
                    // this is why we use a timer to delay prompting the user.
                    if (0 != (rggrfChange[i] & (int)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size)))
                    {
                        if (!_fileChangedTimerSet)
                        {
                            _fileChangeTrigger = new Timer();
                            _fileChangedTimerSet = true;
                            _fileChangeTrigger.Interval = 1000;
                            _fileChangeTrigger.Tick += new EventHandler(this.OnFileChangeEvent);
                            _fileChangeTrigger.Enabled = true;
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notify the editor of the changes made to a directory
        /// </summary>
        /// <param name="pszDirectory">Name of the directory that has changed</param>
        /// <returns></returns>
        int IVsFileChangeEvents.DirectoryChanged(string pszDirectory)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsDocDataFileChangeControl Members

        /// <summary>
        /// Used to determine whether changes to DocData in files should be ignored or not
        /// </summary>
        /// <param name="fIgnore">a non zero value indicates that the file changes should be ignored
        /// </param>
        /// <returns></returns>
        int IVsDocDataFileChangeControl.IgnoreFileChanges(int fIgnore)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside IgnoreFileChanges ****"));

            if (fIgnore != 0)
            {
                _ignoreFileChangeLevel++;
            }
            else
            {
                if (_ignoreFileChangeLevel > 0)
                    _ignoreFileChangeLevel--;

                // We need to check here if our file has changed from "Read Only"
                // to "Read/Write" or vice versa while the ignore level was non-zero.
                // This may happen when a file is checked in or out under source
                // code control. We need to check here so we can update our caption.
                FileAttributes fileAttrs = File.GetAttributes(_fileName);
                int isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                SetReadOnly(isReadOnly != 0);
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region File Change Notification Helpers

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (_fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData = IntPtr.Zero;

            try
            {
                // Lock the document
                int hr = runningDocTable.FindAndLockDocument(
                    (uint)_VSRDTFLAGS.RDT_ReadLock,
                    _fileName,
                    out hierarchy,
                    out itemID,
                    out docData,
                    out docCookie
                );

                ErrorHandler.ThrowOnFailure(hr);

                // Send the notification
                hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

                // Unlock the document.
                // Note that we have to unlock the document even if the previous call failed.
                ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie));

                // Check ff the call to NotifyDocChanged failed.
                ErrorHandler.ThrowOnFailure(hr);
            }
            finally
            {
                if (docData != IntPtr.Zero)
                    Marshal.Release(docData);
            }
        }

        /// <summary>
        /// In this function we inform the shell when we wish to receive 
        /// events when our file is changed or we inform the shell when 
        /// we wish not to receive events anymore.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fStart">TRUE indicates advise, FALSE indicates unadvise.</param>
        /// <returns>Result of the operation</returns>
        private int SetFileChangeNotification(string pszFileName, bool fStart)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SetFileChangeNotification ****"));

            int result = VSConstants.E_FAIL;

            //Get the File Change service
            if (null == _vsFileChangeEx)
                _vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == _vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            // Setup Notification if fStart is TRUE, Remove if fStart is FALSE.
            if (fStart)
            {
                if (_vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                {
                    //Receive notifications if either the attributes of the file change or 
                    //if the size of the file changes or if the last modified time of the file changes
                    result = _vsFileChangeEx.AdviseFileChange(pszFileName,
                        (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Attr | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time),
                        (IVsFileChangeEvents)this,
                        out _vsFileChangeCookie);
                    if (_vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                        return VSConstants.E_FAIL;
                }
            }
            else
            {
                if (_vsFileChangeCookie != VSConstants.VSCOOKIE_NIL)
                {
                    result = _vsFileChangeEx.UnadviseFileChange(_vsFileChangeCookie);
                    _vsFileChangeCookie = VSConstants.VSCOOKIE_NIL;
                }
            }
            return result;
        }

        /// <summary>
        /// In this function we suspend receiving file change events for
        /// a file or we reinstate a previously suspended file depending
        /// on the value of the given fSuspend flag.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fSuspend">TRUE indicates that the events needs to be suspended</param>
        /// <returns></returns>
        private int SuspendFileChangeNotification(string pszFileName, int fSuspend)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SuspendFileChangeNotification ****"));

            if (null == _vsFileChangeEx)
                _vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == _vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            if (0 == fSuspend)
            {
                // we are transitioning from suspended to non-suspended state - so force a
                // sync first to avoid asynchronous notifications of our own change
                if (_vsFileChangeEx.SyncFile(pszFileName) == VSConstants.E_FAIL)
                    return VSConstants.E_FAIL;
            }

            //If we use the VSCOOKIE parameter to specify the file, then pszMkDocument parameter 
            //must be set to a null reference and vice versa 
            return _vsFileChangeEx.IgnoreFile(_vsFileChangeCookie, null, fSuspend);
        }

        /// <summary>
        /// This event is triggered when one of the files loaded into the environment has changed outside of the
        /// editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChangeEvent(object sender, System.EventArgs e)
        {
            //Disable the timer
            _fileChangeTrigger.Enabled = false;

            string message = this.GetResourceString("@101");    //get the message string from the resource
            IVsUIShell VsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            int result = 0;
            Guid tempGuid = Guid.Empty;
            if (VsUiShell != null)
            {
                //Show up a message box indicating that the file has changed outside of VS environment
                ErrorHandler.ThrowOnFailure(VsUiShell.ShowMessageBox(0, ref tempGuid, _fileName, message, null, 0,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_QUERY, 0, out result));
            }
            //if the user selects "Yes", reload the current file
            if (result == (int)DialogResult.Yes)
            {
                ErrorHandler.ThrowOnFailure(((IVsPersistDocData)this).ReloadDocData(0));
            }

            _fileChangedTimerSet = false;
        }

        #endregion

        #region IVsFileBackup Members

        /// <summary>
        /// This method is used to Persist the data to a single file. On a successful backup this 
        /// should clear up the backup dirty bit
        /// </summary>
        /// <param name="pszBackupFileName">Name of the file to persist</param>
        /// <returns>S_OK if the data can be successfully persisted.
        /// This should return STG_S_DATALOSS or STG_E_INVALIDCODEPAGE if there is no way to 
        /// persist to a file without data loss
        /// </returns>
        int IVsFileBackup.BackupFile(string pszBackupFileName)
        {
            throw new NotImplementedException();//huqf
            //try
            //{
            //    //editorControl.RichTextBoxControl.SaveFile(pszBackupFileName);
            //    backupObsolete = false;
            //}
            //catch (ArgumentException)
            //{
            //    return VSConstants.E_FAIL;
            //}
            //catch (IOException)
            //{
            //    return VSConstants.E_FAIL;
            //}
            //return VSConstants.S_OK;
        }

        /// <summary>
        /// Used to set the backup dirty bit. This bit should be set when the object is modified 
        /// and cleared on calls to BackupFile and any Save method
        /// </summary>
        /// <param name="pbObsolete">the dirty bit to be set</param>
        /// <returns>returns 1 if the backup dirty bit is set, 0 otherwise</returns>
        int IVsFileBackup.IsBackupFileObsolete(out int pbObsolete)
        {
            if (_backupObsolete)
                pbObsolete = 1;
            else
                pbObsolete = 0;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsToolboxUser Interface

        public int IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
        {
            // Create a OleDataObject from the input interface.
            OleDataObject oleData = new OleDataObject(pDO);
            // && editorControl.RichTextBoxControl.CanPaste(DataFormats.GetFormat(DataFormats.UnicodeText))
            // Check if the data object is of type UnicodeText.
            if (oleData.GetDataPresent(DataFormats.UnicodeText))
            {
                return VSConstants.S_OK;
            }

            // In all the other cases return S_FALSE
            return VSConstants.S_FALSE;
        }

        public int ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
        {
            // Create a OleDataObject from the input interface.
            OleDataObject oleData = new OleDataObject(pDO);

            // Check if the picked item is the one we can paste.
            if (oleData.GetDataPresent(DataFormats.UnicodeText))
            {
                throw new NotImplementedException();//huqf
                //object o = null;
                //_editorControl.TextSelection.Paste(ref o, 0);
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region IVsFindTarget Members

        /// <summary>
        /// Return the object that was requested
        /// </summary>
        /// <param name="propid">Id of the requested object</param>
        /// <param name="pvar">Object returned</param>
        /// <returns>HResult</returns>
        int IVsFindTarget.GetProperty(uint propid, out object pvar)
        {
            pvar = null;

            switch (propid)
            {
                case (uint)__VSFTPROPID.VSFTPROPID_DocName:
                    {
                        // Return a copy of the file name
                        pvar = _fileName;
                        break;
                    }
                case (uint)__VSFTPROPID.VSFTPROPID_InitialPattern:
                case (uint)__VSFTPROPID.VSFTPROPID_InitialPatternAggressive:
                    {
                        break;
                    }
                case (uint)__VSFTPROPID.VSFTPROPID_WindowFrame:
                    {
                        // Return the Window frame
                        pvar = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
                        break;
                    }
                case (uint)__VSFTPROPID.VSFTPROPID_IsDiskFile:
                    {
                        // We currently assume the file is on disk
                        pvar = true;
                        break;
                    }
                default:
                    {
                        return VSConstants.E_NOTIMPL;
                    }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grfOptions"></param>
        /// <param name="ppSpans"></param>
        /// <param name="ppTextImage"></param>
        int IVsFindTarget.GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
        {
            throw new NotImplementedException();//huqf
            ////set the IVsTextSpanSet object
            //if (null != ppSpans && ppSpans.Length > 0)
            //{
            //    ppSpans[0] = (IVsTextSpanSet)this;
            //}

            ////set the IVsTextImage object
            //ppTextImage = (IVsTextImage)this;

            ////attach this text image to the span
            //if (null != ppSpans && ppSpans.Length > 0)
            //{
            //    ErrorHandler.ThrowOnFailure(ppSpans[0].AttachTextImage(ppTextImage));
            //}

            //return VSConstants.S_OK;
        }

        /// <summary>
        /// Retrieve a previously stored object
        /// </summary>
        /// <returns>The object that is being asked</returns>
        int IVsFindTarget.GetFindState(out object ppunk)
        {
            ppunk = _findState;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Search for the string in the text of our editor.
        /// Options specify how we do the search. No need to implement this since we implement IVsTextImage
        /// </summary>
        /// <param name="pszSearch">Search string</param>
        /// <param name="grfOptions">Search options</param>
        /// <param name="fResetStartPoint">Is this a new search?</param>
        /// <param name="pHelper">We are not using it</param>
        /// <param name="pResult">True if we found the search string</param>
        int IVsFindTarget.Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
        {
            pResult = 0;

            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Bring the focus to a specific position in the document
        /// </summary>
        /// <param name="pts">Location where to move the cursor to</param>
        int IVsFindTarget.NavigateTo(TextSpan[] pts)
        {
            int hr = VSConstants.S_OK;

            // Activate the window
            IVsWindowFrame frame = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
            if (frame != null)
                hr = frame.Show();
            else
                return VSConstants.E_NOTIMPL;

            //// Now navigate to the specified location (if any)
            //if (ErrorHandler.Succeeded(hr) && (null != pts) && (pts.Length > 0))
            //{
            //    // first set start location
            //    int NewPosition = _editorControl.RichTextBoxControl.GetFirstCharIndexFromLine(pts[0].iStartLine);
            //    NewPosition += pts[0].iStartIndex;
            //    if (NewPosition > _editorControl.RichTextBoxControl.Text.Length)
            //        NewPosition = _editorControl.RichTextBoxControl.Text.Length;
            //    _editorControl.RichTextBoxControl.SelectionStart = NewPosition;

            //    // now set the length of the selection
            //    NewPosition = _editorControl.RichTextBoxControl.GetFirstCharIndexFromLine(pts[0].iEndLine);
            //    NewPosition += pts[0].iEndIndex;
            //    if (NewPosition > _editorControl.RichTextBoxControl.Text.Length)
            //        NewPosition = _editorControl.RichTextBoxControl.Text.Length;
            //    int length = NewPosition - _editorControl.RichTextBoxControl.SelectionStart;
            //    if (length >= 0)
            //        _editorControl.RichTextBoxControl.SelectionLength = length;
            //    else
            //        _editorControl.RichTextBoxControl.SelectionLength = 0;
            //}

            return hr;
        }

        /// <summary>
        /// Get current cursor location
        /// </summary>
        /// <param name="pts">Current location</param>
        /// <returns>HResult</returns>
        int IVsFindTarget.GetCurrentSpan(TextSpan[] pts)
        {
            if (null == pts || 0 == pts.Length)
                return VSConstants.E_INVALIDARG;

            //pts[0].iStartIndex = _editorControl.GetColumnFromIndex(_editorControl.RichTextBoxControl.SelectionStart);
            //pts[0].iEndIndex = _editorControl.GetColumnFromIndex(_editorControl.RichTextBoxControl.SelectionStart + _editorControl.RichTextBoxControl.SelectionLength);
            //pts[0].iStartLine = _editorControl.RichTextBoxControl.GetLineFromCharIndex(_editorControl.RichTextBoxControl.SelectionStart);
            //pts[0].iEndLine = _editorControl.RichTextBoxControl.GetLineFromCharIndex(_editorControl.RichTextBoxControl.SelectionStart + _editorControl.RichTextBoxControl.SelectionLength);

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Highlight a given text span. No need to implement
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        int IVsFindTarget.MarkSpan(TextSpan[] pts)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Replace a string in the text. No need to implement since we implement IVsTextImage
        /// </summary>
        /// <param name="pszSearch">string containing the search text</param>
        /// <param name="pszReplace">string containing the replacement text</param>
        /// <param name="grfOptions">Search options available</param>
        /// <param name="fResetStartPoint">flag to reset the search start point</param>
        /// <param name="pHelper">IVsFindHelper interface object</param>
        /// <param name="pfReplaced">returns whether replacement was successful or not</param>
        /// <returns></returns>
        int IVsFindTarget.Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
        {
            pfReplaced = 0;

            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Store an object that will later be returned
        /// </summary>
        /// <returns>The object that is being stored</returns>
        int IVsFindTarget.SetFindState(object pUnk)
        {
            _findState = pUnk;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This implementation does not use notification
        /// </summary>
        /// <param name="notification"></param>
        int IVsFindTarget.NotifyFindTarget(uint notification)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Specify which search option we support.
        /// </summary>
        /// <param name="pfImage">Do we support IVsTextImage?</param>
        /// <param name="pgrfOptions">Supported options</param>
        int IVsFindTarget.GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
        {
            // We do support IVsTextImage
            if (pfImage != null && pfImage.Length > 0)
                pfImage[0] = true;

            if (pgrfOptions != null && pgrfOptions.Length > 0)
            {
                pgrfOptions[0] = (uint)__VSFINDOPTIONS.FR_Backwards;        //Search backwards from the insertion point
                pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_MatchCase;       //Match the case while searching
                pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_WholeWord;       //Match whole word while searching
                //pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_Selection;       //Search in selected text only
                pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_ActionMask;      //Find/Replace capabilities

                //// Only support selection if something is selected
                //if (_editorControl == null || _editorControl.SelectionLength == 0)
                //    pgrfOptions[0] &= ~((uint)__VSFINDOPTIONS.FR_Selection);

                //if the file is read only, don't support replace
                if (_editorControl == null || _editorControl.ReadOnly)
                    pgrfOptions[0] &= ~((uint)__VSFINDOPTIONS.FR_Replace | (uint)__VSFINDOPTIONS.FR_ReplaceAll);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Return the Screen coordinates of the matched string. No need to implement
        /// </summary>
        /// <param name="prc"></param>
        /// <returns></returns>
        int IVsFindTarget.GetMatchRect(RECT[] prc)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region 帮助方法

        /// <summary>
        /// Used to ReadOnly property for the Rich TextBox and correspondingly update the editor caption
        /// </summary>
        /// <param name="isFileReadOnly">Indicates whether the file loaded is Read Only or not</param>
        private void SetReadOnly(bool isFileReadOnly)
        {
            _editorControl.ReadOnly = isFileReadOnly;

            //update editor caption with "[Read Only]" or "" as necessary
            IVsWindowFrame frame = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
            string editorCaption = "";
            if (isFileReadOnly) editorCaption = this.GetResourceString("@100");
            ErrorHandler.ThrowOnFailure(frame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, editorCaption));
            _backupObsolete = true;
        }

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = _package.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        #endregion
    }
}
