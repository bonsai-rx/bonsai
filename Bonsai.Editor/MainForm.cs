using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Xml.Serialization;
using System.Xml;
using Bonsai.Expressions;
using Bonsai.Dag;
using Bonsai.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Linq.Expressions;
using Bonsai.Editor.Properties;
using System.IO;
using System.Windows.Forms.Design;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.Reactive.Subjects;
using Bonsai.Editor.Scripting;
using Bonsai.Editor.Themes;
using Bonsai.Editor.GraphView;

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        const float DefaultEditorScale = 1.0f;
        const string BonsaiExtension = ".bonsai";
        const string LayoutExtension = ".layout";
        const string BonsaiPackageName = "Bonsai";
        const string ExtensionsDirectory = "Extensions";
        const string WorkflowCategoryName = "Workflow";
        const string SubjectCategoryName = "Subject";
        const string VersionAttributeName = "Version";
        const string DefaultWorkflowNamespace = "Unspecified";
        static readonly char[] ToolboxArgumentSeparator = new[] { ' ' };
        static readonly object ExtensionsDirectoryChanged = new object();
        static readonly object WorkflowValidating = new object();
        static readonly XmlWriterSettings DefaultWriterSettings = new XmlWriterSettings
        {
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            Indent = true
        };

        int version;
        int saveVersion;
        Font regularFont;
        Font selectionFont;
        EditorSite editorSite;
        HotKeyMessageFilter hotKeys;
        IServiceProvider serviceProvider;
        IScriptEnvironment scriptEnvironment;
        WorkflowEditorControl editorControl;
        WorkflowBuilder workflowBuilder;
        WorkflowSelectionModel selectionModel;
        Dictionary<string, string> propertyAssignments;
        Dictionary<string, TreeNode> toolboxCategories;
        List<TreeNode> treeCache;
        Label statusTextLabel;
        Bitmap statusRunningImage;
        ThemeRenderer themeRenderer;
        SvgRendererFactory iconRenderer;
        ToolStripButton statusUpdateAvailableLabel;
        BehaviorSubject<bool> updatesAvailable;
        DirectoryInfo extensionsPath;
        FormScheduler formScheduler;

        TypeVisualizerMap typeVisualizers;
        List<WorkflowElementDescriptor> workflowElements;
        List<WorkflowElementDescriptor> workflowExtensions;
        WorkflowRuntimeExceptionCache exceptionCache;
        WorkflowException workflowError;
        IDisposable running;
        bool debugging;
        bool building;

        IObservable<IGrouping<string, WorkflowElementDescriptor>> toolboxElements;
        IObservable<TypeVisualizerDescriptor> visualizerElements;
        SizeF inverseScaleFactor;
        SizeF scaleFactor;

        public MainForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider)
            : this(elementProvider, visualizerProvider, null)
        {
        }

        public MainForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            IServiceProvider provider)
            : this(elementProvider, visualizerProvider, provider, DefaultEditorScale)
        {
        }

        public MainForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            IServiceProvider provider,
            float editorScale)
        {
            if (editorScale != DefaultEditorScale)
            {
                Font = new Font(Font.FontFamily, Font.SizeInPoints * editorScale);
            }

            InitializeComponent();
            statusTextLabel = new Label();
            statusTextLabel.AutoSize = true;
            statusTextLabel.Text = Resources.ReadyStatus;
            formScheduler = new FormScheduler(this);
            themeRenderer = new ThemeRenderer();
            themeRenderer.ThemeChanged += themeRenderer_ThemeChanged;
            iconRenderer = new SvgRendererFactory();
            updatesAvailable = new BehaviorSubject<bool>(false);
            statusUpdateAvailableLabel = new ToolStripButton();
            statusUpdateAvailableLabel.Click += packageManagerToolStripMenuItem_Click;
            statusUpdateAvailableLabel.ToolTipText = Resources.PackageUpdatesAvailable_Notification;
            statusUpdateAvailableLabel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            statusUpdateAvailableLabel.Image = Resources.StatusUpdateAvailable;
            statusRunningImage = Resources.StatusRunningImage;
            searchTextBox.CueBanner = Resources.SearchModuleCueBanner;
            statusStrip.Items.Add(new ToolStripControlHost(statusTextLabel));
            statusStrip.SizeChanged += new EventHandler(statusStrip_SizeChanged);
            UpdateStatusLabelSize();

            toolboxCategories = new Dictionary<string, TreeNode>();
            foreach (TreeNode node in toolboxTreeView.Nodes)
            {
                toolboxCategories.Add(node.Name, node);
            }

            serviceProvider = provider;
            treeCache = new List<TreeNode>();
            editorSite = new EditorSite(this);
            hotKeys = new HotKeyMessageFilter();
            workflowBuilder = new WorkflowBuilder();
            regularFont = new Font(toolboxDescriptionTextBox.Font, FontStyle.Regular);
            selectionFont = new Font(toolboxDescriptionTextBox.Font, FontStyle.Bold);
            typeVisualizers = new TypeVisualizerMap();
            workflowElements = new List<WorkflowElementDescriptor>();
            workflowExtensions = new List<WorkflowElementDescriptor>();
            exceptionCache = new WorkflowRuntimeExceptionCache();
            selectionModel = new WorkflowSelectionModel();
            propertyAssignments = new Dictionary<string, string>();
            if (serviceProvider != null)
            {
                scriptEnvironment = (IScriptEnvironment)serviceProvider.GetService(typeof(IScriptEnvironment));
            }

            editorControl = new WorkflowEditorControl(editorSite);
            editorControl.Enter += new EventHandler(editorControl_Enter);
            editorControl.Workflow = workflowBuilder.Workflow;
            editorControl.Dock = DockStyle.Fill;
            workflowSplitContainer.Panel1.Controls.Add(editorControl);
            propertyGrid.BrowsableAttributes = new AttributeCollection(BrowsableAttribute.Yes, DesignTimeVisibleAttribute.Yes);
            propertyGrid.LineColor = SystemColors.InactiveBorder;
            propertyGrid.Site = editorSite;

            editorControl.Disposed += editorControl_Disposed;
            selectionModel.SelectionChanged += new EventHandler(selectionModel_SelectionChanged);
            toolboxElements = elementProvider;
            visualizerElements = visualizerProvider;
            Application.AddMessageFilter(hotKeys);
            components.Add(editorControl);
        }

        #region Loading

        [Obsolete]
        public bool LaunchPackageManager
        {
            get { return EditorResult == EditorResult.ManagePackages; }
            set
            {
                if (value) EditorResult = EditorResult.ManagePackages;
                else EditorResult = EditorResult.Exit;
            }
        }

        [Obsolete]
        public string InitialFileName
        {
            get { return FileName; }
            set { FileName = value; }
        }

        [Obsolete]
        public bool StartOnLoad
        {
            get { return LoadAction != LoadAction.None; }
            set { LoadAction = value ? LoadAction.Start : LoadAction.None; }
        }

        public EditorResult EditorResult { get; set; }

        public LoadAction LoadAction { get; set; }

        public string FileName
        {
            get { return saveWorkflowDialog.FileName; }
            set { saveWorkflowDialog.FileName = value; }
        }

        public bool UpdatesAvailable
        {
            get { return updatesAvailable.Value; }
            set { updatesAvailable.OnNext(value); }
        }

        public IDictionary<string, string> PropertyAssignments
        {
            get { return propertyAssignments; }
        }

        static Rectangle ScaleBounds(Rectangle bounds, SizeF scaleFactor)
        {
            bounds.Location = Point.Round(new PointF(bounds.X * scaleFactor.Width, bounds.Y * scaleFactor.Height));
            bounds.Size = Size.Round(new SizeF(bounds.Width * scaleFactor.Width, bounds.Height * scaleFactor.Height));
            return bounds;
        }

        void RestoreEditorSettings()
        {
            var desktopBounds = ScaleBounds(EditorSettings.Instance.DesktopBounds, scaleFactor);
            if (desktopBounds.Width > 0)
            {
                DesktopBounds = desktopBounds;
            }

            WindowState = EditorSettings.Instance.WindowState;
            themeRenderer.ActiveTheme = EditorSettings.Instance.EditorTheme;
        }

        void CloseEditorForm()
        {
            Application.RemoveMessageFilter(hotKeys);
            var desktopBounds = EditorSettings.Instance.DesktopBounds;
            if (WindowState != FormWindowState.Normal)
            {
                desktopBounds.Size = RestoreBounds.Size;
            }
            else desktopBounds = ScaleBounds(DesktopBounds, inverseScaleFactor);
            EditorSettings.Instance.DesktopBounds = desktopBounds;
            if (WindowState == FormWindowState.Minimized)
            {
                EditorSettings.Instance.WindowState = FormWindowState.Normal;
            }
            else EditorSettings.Instance.WindowState = WindowState;
            EditorSettings.Instance.Save();
        }

        void HandleUpdatesAvailable(bool value)
        {
            if (value) toolStrip.Items.Add(statusUpdateAvailableLabel);
            else toolStrip.Items.Remove(statusUpdateAvailableLabel);
        }

        protected override void OnLoad(EventArgs e)
        {
            RestoreEditorSettings();
            var initialFileName = FileName;
            var validFileName =
                !string.IsNullOrEmpty(initialFileName) &&
                Path.GetExtension(initialFileName) == BonsaiExtension &&
                File.Exists(initialFileName);

            var currentDirectory = Path.GetFullPath(Environment.CurrentDirectory).TrimEnd('\\');
            var appDomainBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
            var systemPath = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.System)).TrimEnd('\\');
            var systemX86Path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)).TrimEnd('\\');
            var currentDirectoryRestricted = currentDirectory == appDomainBaseDirectory || currentDirectory == systemPath || currentDirectory == systemX86Path;
            var formClosed = Observable.FromEventPattern<FormClosedEventHandler, FormClosedEventArgs>(
                handler => FormClosed += handler,
                handler => FormClosed -= handler);

            InitializeSubjectSources().TakeUntil(formClosed).Subscribe();
            InitializeWorkflowFileWatcher().TakeUntil(formClosed).Subscribe();
            updatesAvailable.TakeUntil(formClosed).ObserveOn(formScheduler).Subscribe(HandleUpdatesAvailable);
            directoryToolStripTextBox.Text = !currentDirectoryRestricted ? currentDirectory : (validFileName ? Path.GetDirectoryName(initialFileName) : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            InitializeEditorToolboxTypes();
            var shutdown = ShutdownSequence();
            var initialization = InitializeToolbox().Merge(InitializeTypeVisualizers()).TakeLast(1).Finally(shutdown.Dispose).ObserveOn(Scheduler.Default);
            if (validFileName && OpenWorkflow(initialFileName, false))
            {
                foreach (var assignment in propertyAssignments)
                {
                    workflowBuilder.Workflow.SetWorkflowProperty(assignment.Key, assignment.Value);
                }

                var loadAction = LoadAction;
                if (loadAction != LoadAction.None)
                {
                    initialization = initialization.Do(xs => BeginInvoke((Action)(() =>
                    {
                        var debugging = loadAction == LoadAction.Start;
                        editorSite.StartWorkflow(debugging);
                    })));
                }
            }
            else FileName = null;

            initialization.TakeUntil(formClosed).Subscribe();
            base.OnLoad(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            scaleFactor = factor;
            inverseScaleFactor = new SizeF(1f / factor.Width, 1f / factor.Height);

            const float DefaultToolboxSplitterDistance = 245f;
            panelSplitContainer.SplitterDistance = (int)(panelSplitContainer.SplitterDistance * factor.Height);
            workflowSplitContainer.SplitterDistance = (int)(workflowSplitContainer.SplitterDistance * factor.Height);
            propertiesSplitContainer.SplitterDistance = (int)(propertiesSplitContainer.SplitterDistance * factor.Height);
            var splitterScale = DefaultToolboxSplitterDistance / toolboxSplitContainer.SplitterDistance;
            toolboxSplitContainer.SplitterDistance = (int)(toolboxSplitContainer.SplitterDistance * splitterScale * factor.Height);
            workflowSplitContainer.Panel1.Padding = new Padding(0, 6, 0, 2);

            var imageSize = toolStrip.ImageScalingSize;
            var scalingFactor = ((int)(factor.Height * 100) / 50 * 50) / 100f;
            if (scalingFactor > 1)
            {
                startToolStripSplitButton.DropDownButtonWidth = (int)(startToolStripSplitButton.DropDownButtonWidth * scalingFactor);
                toolStrip.ImageScalingSize = new Size((int)(imageSize.Width * scalingFactor), (int)(imageSize.Height * scalingFactor));
                menuStrip.ImageScalingSize = toolStrip.ImageScalingSize;
                statusStrip.ImageScalingSize = toolStrip.ImageScalingSize;
                propertyGrid.LargeButtons = scalingFactor >= 2;
            }
            base.ScaleControl(factor, specified);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Action closeEditor = CloseEditorForm;
            if (InvokeRequired) Invoke(closeEditor);
            else closeEditor();
            base.OnFormClosed(e);
        }

        void editorControl_Disposed(object sender, EventArgs e)
        {
            iconRenderer.Dispose();
        }

        #endregion

        #region Toolbox

        static readonly Type[] CSharpScriptTypes = new[]
        {
            typeof(CSharpSource),
            typeof(CSharpTransform),
            typeof(CSharpSink),
            typeof(CSharpCombinator)
        };

        void InitializeEditorToolboxTypes()
        {
            foreach (var group in EditorToolboxTypes().GroupBy(descriptor => descriptor.Namespace))
            {
                InitializeToolboxCategory(group.Key, group);
            }
        }

        IEnumerable<WorkflowElementDescriptor> EditorToolboxTypes()
        {
            foreach (var scriptType in CSharpScriptTypes)
            {
                var attributes = TypeDescriptor.GetAttributes(scriptType);
                var descriptionAttribute = (DescriptionAttribute)attributes[typeof(DescriptionAttribute)];
                var categoryAttribute = (WorkflowElementCategoryAttribute)attributes[typeof(WorkflowElementCategoryAttribute)];
                yield return new WorkflowElementDescriptor
                {
                    Name = ExpressionBuilder.GetElementDisplayName(scriptType),
                    Namespace = scriptType.Namespace,
                    FullyQualifiedName = scriptType.AssemblyQualifiedName,
                    Description = descriptionAttribute.Description,
                    ElementTypes = new[] { categoryAttribute.Category }
                };
            }
        }

        IObservable<Unit> InitializeSubjectSources()
        {
            var selectionChanged = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => selectionModel.SelectionChanged += handler,
                handler => selectionModel.SelectionChanged -= handler)
                .Select(evt => selectionModel.SelectedView)
                .DistinctUntilChanged();
            var workflowValidating = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => Events.AddHandler(WorkflowValidating, handler),
                handler => Events.RemoveHandler(WorkflowValidating, handler))
                .Select(evt => selectionModel.SelectedView);
            return Observable
                .Merge(selectionChanged, workflowValidating)
                .Do(view =>
                {
                    toolboxTreeView.BeginUpdate();
                    var subjectCategory = toolboxCategories[SubjectCategoryName];
                    var isEmpty = subjectCategory.Nodes.Count == 0;
                    subjectCategory.Nodes.Clear();

                    var nameProperty = TypeDescriptor.GetProperties(typeof(SubscribeSubjectBuilder))["Name"];
                    var subjects = nameProperty.Converter.GetStandardValues(new TypeDescriptorContext(workflowBuilder, nameProperty, editorSite));
                    if (subjects != null && subjects.Count > 0)
                    {
                        var elementCategories = new[] { ~ElementCategory.Source };
                        foreach (string entry in subjects)
                        {
                            var subjectNode = subjectCategory.Nodes.Add(entry, entry);
                            subjectNode.Tag = elementCategories;
                        }

                        if (isEmpty)
                        {
                            subjectCategory.Expand();
                        }
                    }

                    toolboxTreeView.EndUpdate();
                })
                .IgnoreElements()
                .Select(xs => Unit.Default);
        }

        IObservable<Unit> InitializeWorkflowFileWatcher()
        {
            var extensionsDirectoryChanged = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => Events.AddHandler(ExtensionsDirectoryChanged, handler),
                handler => Events.RemoveHandler(ExtensionsDirectoryChanged, handler));
            return extensionsDirectoryChanged.Select(evt => Observable.Defer(RefreshWorkflowExtensions)).Switch();
        }

        IObservable<Unit> RefreshWorkflowExtensions()
        {
            try
            {
                workflowFileWatcher.Path = extensionsPath.FullName;
                workflowFileWatcher.EnableRaisingEvents = true;
            }
            catch (ArgumentException)
            {
                workflowFileWatcher.EnableRaisingEvents = false;
            }

            var start = Observable.Return(new EventPattern<EventArgs>(this, EventArgs.Empty));
            var changed = Observable.FromEventPattern<FileSystemEventHandler, EventArgs>(
                handler => workflowFileWatcher.Changed += handler,
                handler => workflowFileWatcher.Changed -= handler);
            var created = Observable.FromEventPattern<FileSystemEventHandler, EventArgs>(
                handler => workflowFileWatcher.Created += handler,
                handler => workflowFileWatcher.Created -= handler);
            var deleted = Observable.FromEventPattern<FileSystemEventHandler, EventArgs>(
                handler => workflowFileWatcher.Deleted += handler,
                handler => workflowFileWatcher.Deleted -= handler);
            var renamed = Observable.FromEventPattern<RenamedEventHandler, EventArgs>(
                handler => workflowFileWatcher.Renamed += handler,
                handler => workflowFileWatcher.Renamed -= handler);
            return Observable
                .Merge(start, changed, created, deleted, renamed)
                .Throttle(TimeSpan.FromSeconds(1), Scheduler.Default)
                .Select(evt => workflowExtensions
                    .Concat(FindWorkflows(ExtensionsDirectory))
                    .GroupBy(x => x.Namespace)
                    .ToList())
                .ObserveOn(formScheduler)
                .Do(elements =>
                {
                    toolboxTreeView.BeginUpdate();
                    var workflowCategory = toolboxCategories[WorkflowCategoryName];
                    foreach (TreeNode node in workflowCategory.Nodes)
                    {
                        node.Nodes.Clear();
                    }

                    foreach (var package in elements)
                    {
                        InitializeToolboxCategory(package.Key, package);
                    }

                    var sortedNodes = new TreeNode[workflowCategory.Nodes.Count];
                    workflowCategory.Nodes.CopyTo(sortedNodes, 0);
                    Array.Sort(sortedNodes, (n1, n2) =>
                    {
                        if (n1.Nodes.Count == 0) return n2.Nodes.Count == 0 ? 0 : 1;
                        else if (n2.Nodes.Count == 0) return -1;
                        else return string.Compare(n1.Text, n2.Text);
                    });

                    workflowCategory.Nodes.Clear();
                    for (int i = 0; i < sortedNodes.Length; i++)
                    {
                        var node = sortedNodes[i];
                        if (node.Nodes.Count == 0) break;
                        workflowCategory.Nodes.Add(node);
                    }
                    toolboxTreeView.EndUpdate();
                })
                .IgnoreElements()
                .Select(xs => Unit.Default);
        }

        static IEnumerable<WorkflowElementDescriptor> FindWorkflows(string basePath)
        {
            int basePathLength;
            var workflowFiles = default(string[]);
            if (!Path.IsPathRooted(basePath))
            {
                var currentDirectory = Environment.CurrentDirectory;
                basePath = Path.Combine(currentDirectory, basePath);
                basePathLength = currentDirectory.Length;
            }
            else basePathLength = basePath.Length;

            try { workflowFiles = Directory.GetFiles(basePath, "*" + BonsaiExtension, SearchOption.AllDirectories); }
            catch (UnauthorizedAccessException) { yield break; }
            catch (DirectoryNotFoundException) { yield break; }

            foreach (var fileName in workflowFiles)
            {
                var description = string.Empty;
                try
                {
                    using (var reader = XmlReader.Create(fileName, new XmlReaderSettings { IgnoreWhitespace = true }))
                    {
                        reader.ReadStartElement(typeof(WorkflowBuilder).Name);
                        if (reader.Name == "Description")
                        {
                            reader.ReadStartElement();
                            description = reader.Value;
                        }
                    }
                }
                catch (SystemException) { continue; }
                
                var relativePath = fileName.Substring(basePathLength)
                                           .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var fileNamespace = Path.GetDirectoryName(relativePath)
                                        .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                                        .Replace(Path.DirectorySeparatorChar, ExpressionHelper.MemberSeparator.First());
                if (string.IsNullOrEmpty(fileNamespace)) fileNamespace = DefaultWorkflowNamespace;

                yield return new WorkflowElementDescriptor
                {
                    Name = Path.GetFileNameWithoutExtension(relativePath),
                    Namespace = fileNamespace,
                    FullyQualifiedName = relativePath,
                    Description = description,
                    ElementTypes = new[] { ~ElementCategory.Workflow }
                };
            }
        }

        IObservable<Unit> InitializeTypeVisualizers()
        {
            var visualizerMapping = from typeVisualizer in visualizerElements
                                    let targetType = Type.GetType(typeVisualizer.TargetTypeName)
                                    let visualizerType = Type.GetType(typeVisualizer.VisualizerTypeName)
                                    where targetType != null && visualizerType != null
                                    select Tuple.Create(targetType, visualizerType);

            return visualizerMapping
                .ObserveOn(formScheduler)
                .Do(typeMapping => typeVisualizers.Add(typeMapping.Item1, typeMapping.Item2))
                .SubscribeOn(Scheduler.Default)
                .TakeLast(1)
                .Select(xs => Unit.Default);
        }

        IObservable<Unit> InitializeToolbox()
        {
            return toolboxElements
                .ObserveOn(formScheduler)
                .Do(package => InitializeToolboxCategory(
                    package.Key,
                    InitializeWorkflowExtensions(package)))
                .SubscribeOn(Scheduler.Default)
                .TakeLast(1)
                .Select(xs => Unit.Default);
        }

        IEnumerable<WorkflowElementDescriptor> InitializeWorkflowExtensions(IEnumerable<WorkflowElementDescriptor> types)
        {
            foreach (var type in types)
            {
                foreach (var elementType in type.ElementTypes)
                {
                    if (elementType == ~ElementCategory.Workflow)
                    {
                        workflowExtensions.Add(type);
                    }
                }

                yield return type;
            }
        }

        static string GetPackageDisplayName(string packageKey)
        {
            if (packageKey == null) return ExtensionsDirectory;
            if (packageKey == BonsaiPackageName) return packageKey;
            return packageKey.Replace(BonsaiPackageName + ".", string.Empty);
        }

        void InitializeToolboxCategory(string categoryName, IEnumerable<WorkflowElementDescriptor> types)
        {
            foreach (var type in types.OrderBy(type => type.Name))
            {
                workflowElements.Add(type);
                foreach (var elementType in type.ElementTypes)
                {
                    var typeCategory = elementType;
                    if (typeCategory == ElementCategory.Nested) continue;
                    if (typeCategory == ElementCategory.Workflow ||
                        typeCategory == ElementCategory.Condition ||
                        typeCategory == ElementCategory.Property)
                    {
                        typeCategory = ElementCategory.Combinator;
                    }
                    else if (typeCategory < 0)
                    {
                        typeCategory = ~typeCategory;
                    }

                    var typeCategoryName = typeCategory.ToString();
                    var elementTypeNode = toolboxCategories[typeCategoryName];
                    var category = elementTypeNode.Nodes[categoryName];
                    if (category == null)
                    {
                        category = elementTypeNode.Nodes.Add(categoryName, GetPackageDisplayName(categoryName));
                    }

                    var node = category.Nodes.Add(type.FullyQualifiedName, type.Name);
                    node.Tag = type.ElementTypes;
                    node.ToolTipText = type.Description;
                }
            }
        }

        #endregion

        #region File Menu

        void ResetProjectStatus()
        {
            commandExecutor.Clear();
            version = 0;
            saveVersion = 0;
            UpdatePropertyGrid();
        }

        bool EnsureWorkflowFile(bool force = false)
        {
            if (string.IsNullOrEmpty(FileName) || force)
            {
                var result = MessageBox.Show(
                    this,
                    "The workflow needs to be saved before proceeding. Do you want to save the workflow?",
                    "Unsaved Workflow",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
                if (result != DialogResult.Yes) return false;
                if (string.IsNullOrEmpty(FileName))
                {
                    saveAsToolStripMenuItem_Click(this, EventArgs.Empty);
                    if (string.IsNullOrEmpty(FileName)) return false;
                }
                else return SaveWorkflow(FileName);
            }

            return true;
        }

        bool CloseWorkflow(CloseReason reason = CloseReason.UserClosing)
        {
            if (editorSite.WorkflowRunning)
            {
                var result = reason == CloseReason.UserClosing
                    ? MessageBox.Show(
                    this,
                    "Do you want to stop the workflow?",
                    "Workflow Running",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1)
                    : DialogResult.Yes;
                if (result == DialogResult.Yes)
                {
                    StopWorkflow();
                }
                else return false;
            }

            if (saveVersion != version)
            {
                var result = MessageBox.Show(
                    this,
                    "Workflow has unsaved changes. Save project file?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(this, EventArgs.Empty);
                    return saveVersion == version;
                }
                else return result == DialogResult.No;
            }

            return true;
        }

        string GetLayoutPath(string fileName)
        {
            return Path.ChangeExtension(fileName, Path.GetExtension(fileName) + LayoutExtension);
        }

        WorkflowBuilder LoadWorkflow(string fileName, out SemanticVersion version)
        {
            using (var reader = XmlReader.Create(fileName))
            {
                version = null;
                reader.MoveToContent();
                var versionName = reader.GetAttribute(VersionAttributeName);
                SemanticVersion.TryParse(versionName, out version);
                var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
                return (WorkflowBuilder)serializer.Deserialize(reader);
            }
        }

        WorkflowBuilder UpdateWorkflow(WorkflowBuilder workflowBuilder, SemanticVersion version)
        {
            var workflow = workflowBuilder.Workflow;
            if (version == null || UpgradeHelper.IsDeprecated(version))
            {
                MessageBox.Show(
                    this,
                    Resources.UpdateWorkflow_Warning,
                    Resources.UpdateWorkflow_Warning_Caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                try
                {
                    UpgradeHelper.UpgradeEnumerableUnfoldingRules(workflowBuilder, version);
                    workflow = UpgradeHelper.UpgradeBuilderNodes(workflow, version);
                }
                catch (WorkflowBuildException)
                {
                    MessageBox.Show(
                        this,
                        Resources.UpdateWorkflow_Error,
                        Resources.UpdateWorkflow_Warning_Caption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1);
                }
            }

            workflowBuilder = new WorkflowBuilder(workflow.ToInspectableGraph());
            return workflowBuilder;
        }

        void ClearWorkflow()
        {
            saveWorkflowDialog.FileName = null;
            workflowBuilder.Workflow.Clear();
            editorControl.VisualizerLayout = null;
            editorControl.Workflow = workflowBuilder.Workflow;
            ResetProjectStatus();
            UpdateTitle();
        }

        bool OpenWorkflow(string fileName)
        {
            return OpenWorkflow(fileName, true);
        }

        bool OpenWorkflow(string fileName, bool setWorkingDirectory)
        {
            SemanticVersion workflowVersion;
            try { workflowBuilder = LoadWorkflow(fileName, out workflowVersion); }
            catch (InvalidOperationException ex)
            {
                var activeException = ex.InnerException != null ? ex.InnerException : ex;
                var errorMessage = activeException.Message;
                if (activeException.InnerException != null)
                {
                    errorMessage += Environment.NewLine + activeException.InnerException.Message;
                }

                errorMessage = string.Format(Resources.OpenWorkflow_Error, errorMessage);
                MessageBox.Show(this, errorMessage, Resources.OpenWorkflow_Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            UpdateWorkflowDirectory(fileName, setWorkingDirectory);
            if (EditorResult == EditorResult.ReloadEditor) return false;

            editorControl.Workflow = workflowBuilder.Workflow;
            if (workflowBuilder.Workflow.Count > 0 && !editorControl.WorkflowGraphView.GraphView.Nodes.Any())
            {
                try { workflowBuilder.Workflow.Build(); }
                catch (WorkflowBuildException ex)
                {
                    var errorMessage = string.Format(Resources.OpenWorkflow_Error, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    MessageBox.Show(this, errorMessage, Resources.OpenWorkflow_Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            workflowBuilder = UpdateWorkflow(workflowBuilder, workflowVersion);
            editorControl.VisualizerLayout = null;
            editorControl.Workflow = workflowBuilder.Workflow;
            editorSite.ValidateWorkflow();

            var layoutPath = GetLayoutPath(fileName);
            if (File.Exists(layoutPath))
            {
                using (var reader = XmlReader.Create(layoutPath))
                {
                    try { editorControl.VisualizerLayout = (VisualizerLayout)VisualizerLayout.Serializer.Deserialize(reader); }
                    catch (InvalidOperationException) { }
                }
            }

            saveWorkflowDialog.FileName = fileName;
            ResetProjectStatus();
            if (UpgradeHelper.IsDeprecated(workflowVersion))
            {
                saveWorkflowDialog.FileName = null;
                version++;
            }

            UpdateTitle();
            return true;
        }

        bool SaveElement(XmlSerializer serializer, string fileName, object o, string error)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                using (var writer = XmlnsIndentedWriter.Create(memoryStream, DefaultWriterSettings))
                {
                    serializer.Serialize(writer, o);
                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        memoryStream.WriteTo(fileStream);
                    }
                }

                return true;
            }
            catch (IOException ex)
            {
                var errorMessage = string.Format(error, ex.Message);
                MessageBox.Show(this, errorMessage, Resources.SaveElement_Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                // Unwrap XML exceptions when serializing individual workflow elements
                var writerException = ex.InnerException as InvalidOperationException;
                if (writerException != null) ex = writerException;

                var errorMessage = string.Format(error, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                MessageBox.Show(this, errorMessage, Resources.SaveElement_Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        bool SaveWorkflow(string fileName)
        {
            var serializerWorkflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.FromInspectableGraph());
            if (!SaveWorkflowBuilder(fileName, serializerWorkflowBuilder)) return false;
            saveVersion = version;

            editorControl.UpdateVisualizerLayout();
            if (editorControl.VisualizerLayout != null)
            {
                var layoutPath = GetLayoutPath(fileName);
                SaveVisualizerLayout(layoutPath, editorControl.VisualizerLayout);
            }

            UpdateWorkflowDirectory(fileName);
            UpdateTitle();
            return true;
        }

        bool SaveWorkflowBuilder(string fileName, WorkflowBuilder workflowBuilder)
        {
            return SaveElement(WorkflowBuilder.Serializer, fileName, workflowBuilder, Resources.SaveWorkflow_Error);
        }

        void SaveVisualizerLayout(string fileName, VisualizerLayout layout)
        {
            SaveElement(VisualizerLayout.Serializer, fileName, layout, Resources.SaveLayout_Error);
        }

        void SaveWorkflowExtension(string fileName, GraphNode groupNode)
        {
            var model = selectionModel.SelectedView;
            if (groupNode == null)
            {
                model.GroupGraphNodes(selectionModel.SelectedNodes);
                groupNode = selectionModel.SelectedNodes.Single();
                if (!model.CanEdit)
                {
                    //TODO: Refactor to avoid covertly modifying read-only or running workflow
                    editorSite.Undo(false);
                }
            }

            var groupBuilder = (GroupWorkflowBuilder)ExpressionBuilder.Unwrap(groupNode.Value);
            groupBuilder.Name = Path.GetFileNameWithoutExtension(fileName);

            var serializerWorkflowBuilder = LayeredGraphExtensions.ToWorkflowBuilder(new[] { groupNode });
            groupBuilder = (GroupWorkflowBuilder)serializerWorkflowBuilder.Workflow.Single().Value;
            serializerWorkflowBuilder = new WorkflowBuilder(groupBuilder.Workflow);
            serializerWorkflowBuilder.Description = groupBuilder.Description;
            if (SaveWorkflowBuilder(fileName, serializerWorkflowBuilder) && model.CanEdit)
            {
                var includeBuilder = new IncludeWorkflowBuilder();
                includeBuilder.Path = PathConvert.GetProjectPath(fileName);
                model.ReplaceGraphNode(groupNode, includeBuilder);
                editorSite.ValidateWorkflow();
            }
        }

        void OnWorkflowValidating(EventArgs e)
        {
            var handler = Events[WorkflowValidating] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void OnExtensionsDirectoryChanged(EventArgs e)
        {
            var handler = Events[ExtensionsDirectoryChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void UpdateWorkflowDirectory(string fileName)
        {
            UpdateWorkflowDirectory(fileName, true);
        }

        void UpdateWorkflowDirectory(string fileName, bool setWorkingDirectory)
        {
            var workflowDirectory = Path.GetDirectoryName(fileName);
            openWorkflowDialog.InitialDirectory = saveWorkflowDialog.InitialDirectory = workflowDirectory;
            if (setWorkingDirectory && directoryToolStripTextBox.Text != workflowDirectory)
            {
                Environment.CurrentDirectory = workflowDirectory;
                saveWorkflowDialog.FileName = fileName;
                EditorResult = EditorResult.ReloadEditor;
                ResetProjectStatus();
                Close();
            }
            else
            {
                EditorSettings.Instance.RecentlyUsedFiles.Add(fileName);
                EditorSettings.Instance.Save();
            }
        }

        void UpdateCurrentDirectory()
        {
            if (Directory.Exists(directoryToolStripTextBox.Text))
            {
                Environment.CurrentDirectory = directoryToolStripTextBox.Text;
                extensionsPath = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ExtensionsDirectory));
                OnExtensionsDirectoryChanged(EventArgs.Empty);
            }
            else directoryToolStripTextBox.Text = Environment.CurrentDirectory;
        }

        void ExportImage(WorkflowGraphView model)
        {
            ExportImage(model, null);
        }

        void ExportImage(WorkflowGraphView model, string fileName)
        {
            var bounds = model.GraphView.GetLayoutSize();
            var extension = Path.GetExtension(fileName);
            if (extension == ".svg")
            {
                var graphics = new SvgNet.SvgGdi.SvgGraphics();
                model.GraphView.DrawGraphics(graphics, true);
                var svg = graphics.WriteSVGString();
                var attributes = string.Format(
                    "<svg width=\"{0}\" height=\"{1}\" ",
                    bounds.Width, bounds.Height);
                svg = svg.Replace("<svg ", attributes);
                File.WriteAllText(fileName, svg);
            }
            else
            {
                using (var bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var gdi = new SvgNet.SvgGdi.GdiGraphics(graphics);
                    model.GraphView.DrawGraphics(gdi, false);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        Clipboard.SetImage(bitmap);
                    }
                    else bitmap.Save(fileName);
                }
            }
        }

        private void directoryToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!directoryToolStripTextBox.Focused)
            {
                UpdateCurrentDirectory();
            }
        }

        private void directoryToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ProcessTabKey(true);
            }
        }

        private void directoryToolStripTextBox_Leave(object sender, EventArgs e)
        {
            UpdateCurrentDirectory();
        }

        private void directoryToolStripTextBox_DoubleClick(object sender, EventArgs e)
        {
            directoryToolStripTextBox.SelectAll();
        }

        private void browseDirectoryToolStripButton_Click(object sender, EventArgs e)
        {
            Process.Start(directoryToolStripTextBox.Text);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CloseWorkflow()) return;
            ClearWorkflow();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CloseWorkflow()) return;

            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                OpenWorkflow(openWorkflowDialog.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(saveWorkflowDialog.FileName)) saveAsToolStripMenuItem_Click(this, e);
            else SaveWorkflow(saveWorkflowDialog.FileName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentFileName = saveWorkflowDialog.FileName;
            if (saveWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                if (!SaveWorkflow(saveWorkflowDialog.FileName))
                {
                    saveWorkflowDialog.FileName = currentFileName;
                }
                else if (!IsDisposed)
                {
                    UpdatePropertyGrid();
                }
            }
        }

        private void saveAsWorkflowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorSite.EnsureExtensionsDirectory();
            if (!extensionsPath.Exists) return;

            string fileName = null;
            GraphNode groupNode = null;
            var selectedNodes = selectionModel.SelectedNodes;
            if (selectedNodes.Count() == 1)
            {
                var selectedNode = selectedNodes.Single();
                var groupBuilder = ExpressionBuilder.Unwrap(selectedNode.Value) as GroupWorkflowBuilder;
                if (groupBuilder != null)
                {
                    fileName = groupBuilder.Name;
                    groupNode = selectedNode;
                }
            }

            var currentFileName = saveWorkflowDialog.FileName;
            try
            {
                saveWorkflowDialog.FileName = fileName;
                saveWorkflowDialog.InitialDirectory = workflowFileWatcher.Path;
                if (saveWorkflowDialog.ShowDialog() != DialogResult.OK) return;
                fileName = saveWorkflowDialog.FileName;
            }
            finally
            {
                saveWorkflowDialog.FileName = currentFileName;
                saveWorkflowDialog.InitialDirectory = openWorkflowDialog.InitialDirectory;
            }

            SaveWorkflowExtension(fileName, groupNode);
        }

        private void exportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.Workflow.Count > 0)
            {
                if (exportImageDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportImage(model, exportImageDialog.FileName);
                }
            }
        }

        private void exportPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EnsureWorkflowFile(force: true))
            {
                EditorResult = EditorResult.ExportPackage;
                Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Action closeWorkflow = () => e.Cancel = !CloseWorkflow(e.CloseReason);
            if (InvokeRequired) Invoke(closeWorkflow);
            else closeWorkflow();

            if (e.Cancel) EditorResult = EditorResult.Exit;
            base.OnFormClosing(e);
        }

        #endregion

        #region Workflow Model

        IDisposable ShutdownSequence()
        {
            return new ScheduledDisposable(new ControlScheduler(this), Disposable.Create(() =>
            {
                editorSite.OnWorkflowStopped(EventArgs.Empty);
                undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
                redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
                toolboxTreeView.Enabled = true;
                searchTextBox.Enabled = true;
                toolboxSplitContainer.Selectable = true;
                deleteToolStripMenuItem.Enabled = true;
                groupToolStripMenuItem.Enabled = true;
                cutToolStripMenuItem.Enabled = true;
                pasteToolStripMenuItem.Enabled = true;
                startToolStripSplitButton.Enabled = startToolStripMenuItem.Enabled = startWithoutDebuggingToolStripMenuItem.Enabled = true;
                stopToolStripButton.Visible = stopToolStripMenuItem.Visible = stopToolStripButton.Enabled = stopToolStripMenuItem.Enabled = false;
                restartToolStripButton.Visible = restartToolStripMenuItem.Visible = restartToolStripButton.Enabled = restartToolStripMenuItem.Enabled = false;
                if (statusImageLabel.Image == statusRunningImage)
                {
                    statusTextLabel.Text = Resources.ReadyStatus;
                    statusImageLabel.Image = Resources.StatusReadyImage;
                }

                running = null;
                building = false;
                editorControl.UpdateVisualizerLayout();
                UpdateTitle();
            }));
        }

        static void SetWorkflowNotifications(ExpressionBuilderGraph source, bool publishNotifications)
        {
            foreach (var builder in from node in source
                                    let inspectBuilder = node.Value as InspectBuilder
                                    where inspectBuilder != null
                                    select inspectBuilder)
            {
                var inspectBuilder = builder;
                inspectBuilder.PublishNotifications = publishNotifications;
                var workflowExpression = inspectBuilder.Builder as IWorkflowExpressionBuilder;
                if (workflowExpression != null && workflowExpression.Workflow != null)
                {
                    SetWorkflowNotifications(workflowExpression.Workflow, publishNotifications);
                }
            }
        }

        static void SetLayoutNotifications(VisualizerLayout root)
        {
            foreach (var settings in root.DialogSettings)
            {
                var inspectBuilder = settings.Tag as InspectBuilder;
                while (inspectBuilder != null && !inspectBuilder.PublishNotifications)
                {
                    inspectBuilder.PublishNotifications = !string.IsNullOrEmpty(settings.VisualizerTypeName);
                    var visualizerElement = ExpressionBuilder.GetVisualizerElement(inspectBuilder);
                    if (inspectBuilder.PublishNotifications && visualizerElement != inspectBuilder)
                    {
                        inspectBuilder = visualizerElement;
                    }
                    else inspectBuilder = null;
                }

                var editorSettings = settings as WorkflowEditorSettings;
                if (editorSettings != null && editorSettings.EditorVisualizerLayout != null)
                {
                    SetLayoutNotifications(editorSettings.EditorVisualizerLayout);
                }
            }
        }

        void StartWorkflow()
        {
            if (running == null)
            {
                building = true;
                ClearWorkflowError();
                SetWorkflowNotifications(workflowBuilder.Workflow, debugging);
                if (!debugging && editorControl.VisualizerLayout != null)
                {
                    SetLayoutNotifications(editorControl.VisualizerLayout);
                }

                running = Observable.Using(
                    () =>
                    {
                        var runtimeWorkflow = workflowBuilder.Workflow.BuildObservable();
                        var shutdown = ShutdownSequence();
                        try
                        {
                            Invoke((Action)(() =>
                            {
                                statusTextLabel.Text = Resources.RunningStatus;
                                statusImageLabel.Image = statusRunningImage;
                                editorSite.OnWorkflowStarted(EventArgs.Empty);
                                Activate();
                            }));
                        }
                        catch
                        {
                            shutdown.Dispose();
                            throw;
                        }

                        return new WorkflowDisposable(runtimeWorkflow, shutdown);
                    },
                    resource => resource.Workflow.TakeUntil(workflowBuilder.Workflow
                        .InspectErrorsEx()
                        .Do(RegisterWorkflowError)
                        .IgnoreElements()))
                    .SubscribeOn(NewThreadScheduler.Default.Catch<Exception>(HandleSchedulerError))
                    .Subscribe(unit => { }, HandleWorkflowError, HandleWorkflowCompleted);
            }

            UpdateTitle();
            undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = false;
            redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = false;
            toolboxTreeView.Enabled = false;
            searchTextBox.Enabled = false;
            toolboxSplitContainer.Selectable = false;
            deleteToolStripMenuItem.Enabled = false;
            groupToolStripMenuItem.Enabled = false;
            cutToolStripMenuItem.Enabled = false;
            pasteToolStripMenuItem.Enabled = false;
            startToolStripSplitButton.Enabled = startToolStripMenuItem.Enabled = startWithoutDebuggingToolStripMenuItem.Enabled = false;
            stopToolStripButton.Visible = stopToolStripMenuItem.Visible = stopToolStripButton.Enabled = stopToolStripMenuItem.Enabled = true;
            restartToolStripButton.Visible = restartToolStripMenuItem.Visible = restartToolStripButton.Enabled = restartToolStripMenuItem.Enabled = true;
        }

        void StopWorkflow()
        {
            if (running != null)
            {
                running.Dispose();
            }
        }

        void RestartWorkflow()
        {
            EventHandler startWorkflow = null;
            startWorkflow = delegate
            {
                editorSite.WorkflowStopped -= startWorkflow;
                BeginInvoke((Action)StartWorkflow);
            };

            editorSite.WorkflowStopped += startWorkflow;
            StopWorkflow();
        }

        void RegisterWorkflowError(Exception ex)
        {
            Action registerError = () =>
            {
                var workflowException = ex as WorkflowRuntimeException;
                if (workflowException != null)
                {
                    exceptionCache.TryAdd(workflowException);
                }
            };

            if (InvokeRequired) BeginInvoke(registerError);
            else registerError();
        }

        void ClearWorkflowError()
        {
            if (workflowError != null)
            {
                ClearExceptionBuilderNode(editorControl.WorkflowGraphView, workflowError);
            }

            exceptionCache.Clear();
            workflowError = null;
        }

        void HighlightWorkflowError()
        {
            if (workflowError != null)
            {
                HighlightExceptionBuilderNode(workflowError, false);
            }
        }

        void ClearExceptionBuilderNode(WorkflowGraphView workflowView, WorkflowException e)
        {
            GraphNode graphNode = null;
            if (workflowView != null)
            {
                graphNode = workflowView.FindGraphNode(e.Builder);
                if (graphNode != null)
                {
                    workflowView.GraphView.Invalidate(graphNode);
                    graphNode.Highlight = false;
                }
            }

            var nestedException = e.InnerException as WorkflowException;
            if (nestedException != null)
            {
                WorkflowGraphView nestedEditor = null;
                if (workflowView != null)
                {
                    var editorLauncher = workflowView.GetWorkflowEditorLauncher(graphNode);
                    nestedEditor = editorLauncher != null && editorLauncher.Visible ? editorLauncher.WorkflowGraphView : null;
                }

                ClearExceptionBuilderNode(nestedEditor, nestedException);
            }
            else
            {
                statusTextLabel.Text = Resources.ReadyStatus;
                statusImageLabel.Image = Resources.StatusReadyImage;
            }
        }

        void HighlightExceptionBuilderNode(WorkflowException ex, bool showMessageBox)
        {
            HighlightExceptionBuilderNode(editorControl.WorkflowGraphView, ex, showMessageBox);
        }

        void HighlightExceptionBuilderNode(WorkflowGraphView workflowView, WorkflowException ex, bool showMessageBox)
        {
            GraphNode graphNode = null;
            if (workflowView != null)
            {
                graphNode = workflowView.FindGraphNode(ex.Builder);
                if (graphNode == null)
                {
                    throw new InvalidOperationException("Exception builder node not found in active workflow editor.");
                }

                workflowView.GraphView.Invalidate(graphNode);
                if (showMessageBox) workflowView.GraphView.SelectedNode = graphNode;
                graphNode.Highlight = true;
            }

            var nestedException = ex.InnerException as WorkflowException;
            if (nestedException != null)
            {
                WorkflowGraphView nestedEditor = null;
                if (workflowView != null)
                {
                    var editorLauncher = workflowView.GetWorkflowEditorLauncher(graphNode);
                    if (editorLauncher != null)
                    {
                        if (building && editorLauncher.Visible) workflowView.LaunchWorkflowView(graphNode);
                        nestedEditor = editorLauncher.WorkflowGraphView;
                    }
                }

                HighlightExceptionBuilderNode(nestedEditor, nestedException, showMessageBox);
            }
            else
            {
                if (workflowView != null)
                {
                    workflowView.GraphView.Select();
                }

                var buildException = ex is WorkflowBuildException;
                var errorCaption = buildException ? "Build Error" : "Runtime Error";
                statusTextLabel.Text = ex.Message;
                statusImageLabel.Image = buildException ? Resources.StatusBlockedImage : Resources.StatusCriticalImage;
                if (showMessageBox)
                {
                    editorSite.ShowError(ex.Message, errorCaption);
                }
            }
        }

        bool HandleSchedulerError(Exception e)
        {
            HandleWorkflowError(e);
            return true;
        }

        void HandleWorkflowError(Exception e)
        {
            var shutdown = ShutdownSequence();
            Action selectExceptionNode = () =>
            {
                var workflowException = e as WorkflowException;
                if (workflowException != null && workflowException.Builder != null || exceptionCache.TryGetValue(e, out workflowException))
                {
                    workflowError = workflowException;
                    HighlightExceptionBuilderNode(workflowException, building);
                }
                else editorSite.ShowError(e.Message, Name);
            };

            if (InvokeRequired) BeginInvoke(selectExceptionNode);
            else selectExceptionNode();
            shutdown.Dispose();
        }

        void HandleWorkflowCompleted()
        {
            Action clearErrors = exceptionCache.Clear;
            if (InvokeRequired) BeginInvoke(clearErrors);
            else clearErrors();
        }

        #endregion

        #region Workflow Controller

        void UpdateTitle()
        {
            var modified = saveVersion != version;
            var workflowRunning = running != null;
            var fileName = Path.GetFileName(saveWorkflowDialog.FileName);
            var emptyFileName = string.IsNullOrEmpty(fileName);
            var title = new StringBuilder(emptyFileName ? Resources.BonsaiTitle : fileName);
            if (modified) title.Append('*');
            if (workflowRunning) title.AppendFormat(" ({0})", Resources.RunningStatus);
            if (!emptyFileName) title.AppendFormat(" - {0}", Resources.BonsaiTitle);
            Text = title.ToString();
        }

        static IEnumerable<TreeNode> GetTreeViewLeafNodes(System.Collections.IEnumerable nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Nodes.Count == 0 && node.Tag != null) yield return node;
                else
                {
                    foreach (var child in GetTreeViewLeafNodes(node.Nodes))
                    {
                        yield return child;
                    }
                }
            }
        }

        private void DeleteSelectedNode()
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.DeleteGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private static int IndexOfMatch(TreeNode node, string text)
        {
            return node.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsActiveControl(Control control)
        {
            var activeControl = ActiveControl;
            while (activeControl != control)
            {
                var container = activeControl as IContainerControl;
                if (container != null)
                {
                    activeControl = container.ActiveControl;
                }
                else return false;
            }

            return true;
        }

        private void RefreshSelection()
        {
            var view = selectionModel.SelectedView;
            if (view != null)
            {
                view.RefreshSelection();
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (IsActiveControl(propertyGrid)) RefreshSelection();
            if (editorControl.Focused)
            {
                ActiveControl = propertyGrid;
            }
            base.OnDeactivate(e);
        }

        private void toolboxTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var selectedNode = e.Item as TreeNode;
            if (selectedNode != null && selectedNode.GetNodeCount(false) == 0)
            {
                toolboxTreeView.DoDragDrop(selectedNode, DragDropEffects.Copy);
            }
        }

        static string GetElementName(object component)
        {
            var name = ExpressionBuilder.GetElementDisplayName(component);
            var workflowProperty = component as ExternalizedProperty;
            if (workflowProperty != null && !string.IsNullOrWhiteSpace(workflowProperty.Name) && workflowProperty.Name != workflowProperty.MemberName)
            {
                return name + " (" + workflowProperty.MemberName + ")";
            }

            var binaryOperator = component as BinaryOperatorBuilder;
            if (binaryOperator != null && binaryOperator.Operand != null)
            {
                return name + " (" + ExpressionBuilder.GetElementDisplayName(binaryOperator.Operand.GetType()) + ")";
            }
            else
            {
                var namedExpressionBuilder = component as INamedElement;
                if (namedExpressionBuilder != null && !string.IsNullOrWhiteSpace(namedExpressionBuilder.Name))
                {
                    var elementType = component.GetType();
                    name += " (" + ExpressionBuilder.GetElementDisplayName(elementType) + ")";
                }

                return name;
            }
        }

        static string GetElementDescription(object component)
        {
            var workflowExpressionBuilder = component as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                var description = workflowExpressionBuilder.Description;
                if (!string.IsNullOrEmpty(description)) return description;
            }

            var descriptionAttribute = (DescriptionAttribute)TypeDescriptor.GetAttributes(component)[typeof(DescriptionAttribute)];
            return descriptionAttribute.Description;
        }

        void editorControl_Enter(object sender, EventArgs e)
        {
            var selectedView = selectionModel.SelectedView;
            if (selectedView != null && selectedView.Launcher != null)
            {
                var container = selectedView.EditorControl;
                if (container != null && container != editorControl && hotKeys.TabState)
                {
                    container.ParentForm.Activate();
                    var forward = Form.ModifierKeys.HasFlag(Keys.Shift);
                    container.SelectNextControl(container.ActiveControl, forward, true, true, false);
                }
            }
        }

        private void selectionModel_SelectionChanged(object sender, EventArgs e)
        {
            UpdatePropertyGrid();
        }

        private void UpdatePropertyGrid()
        {
            var selectedObjects = selectionModel.SelectedNodes.Select(node =>
            {
                var builder = ExpressionBuilder.Unwrap(node.Value);
                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                var instance = workflowElement ?? builder;
                return instance;
            }).ToArray();

            var displayNames = selectedObjects.Select(GetElementName).Distinct().Reverse().ToArray();
            var displayName = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayNames);
            var objectDescriptions = selectedObjects.Select(GetElementDescription).Distinct().Reverse().ToArray();
            var description = objectDescriptions.Length == 1 ? objectDescriptions[0] : string.Empty;
            var selectedView = selectionModel.SelectedView;
            var canEdit = selectedView != null && selectedView.CanEdit;
            var hasSelectedObjects = selectedObjects.Length > 0;
            saveAsWorkflowToolStripMenuItem.Enabled = hasSelectedObjects;
            pasteToolStripMenuItem.Enabled = canEdit;
            copyToolStripMenuItem.Enabled = hasSelectedObjects;
            cutToolStripMenuItem.Enabled = canEdit && hasSelectedObjects;
            deleteToolStripMenuItem.Enabled = canEdit && hasSelectedObjects;
            groupToolStripMenuItem.Enabled = canEdit && hasSelectedObjects;
            ungroupToolStripMenuItem.Enabled = canEdit && hasSelectedObjects;
            enableToolStripMenuItem.Enabled = canEdit && hasSelectedObjects;
            disableToolStripMenuItem.Enabled = canEdit && hasSelectedObjects;
            if (!hasSelectedObjects)
            {
                // Select externalized properties
                if (selectedView != null)
                {
                    var launcher = selectedView.Launcher;
                    if (launcher != null)
                    {
                        displayName = GetElementName(launcher.Builder);
                        description = GetElementDescription(launcher.Builder);
                    }
                    else
                    {
                        description = workflowBuilder.Description ?? Resources.WorkflowPropertiesDescription;
                        displayName = !string.IsNullOrEmpty(saveWorkflowDialog.FileName)
                            ? Path.GetFileNameWithoutExtension(saveWorkflowDialog.FileName)
                            : editorControl.ActiveTab.TabPage.Text;
                    }

                    propertyGrid.SelectedObject = selectedView.Workflow;
                }
            }
            else propertyGrid.SelectedObjects = selectedObjects;
            UpdateDescriptionTextBox(displayName, description, propertiesDescriptionTextBox);
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var shift = Control.ModifierKeys.HasFlag(Keys.Shift);
            var control = Control.ModifierKeys.HasFlag(Keys.Control);
            if (shift)
            {
                if (control) RestartWorkflow();
                else StopWorkflow();
            }
            else editorSite.StartWorkflow(!control);
        }

        private void startWithoutDebuggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorSite.StartWorkflow(false);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopWorkflow();
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestartWorkflow();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedNode();
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            toolboxTreeView.BeginUpdate();
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                if (treeCache.Count > 0)
                {
                    toolboxTreeView.Nodes.Clear();
                    foreach (var node in treeCache)
                    {
                        toolboxTreeView.Nodes.Add(node);
                    }
                    treeCache.Clear();
                    toolboxTreeView.ShowRootLines = true;
                    toolboxTreeView.SelectedNode = null;
                }
            }
            else
            {
                if (treeCache.Count == 0)
                {
                    toolboxTreeView.ShowRootLines = false;
                    foreach (TreeNode node in toolboxTreeView.Nodes)
                    {
                        treeCache.Add(node);
                    }
                }

                int closestMatchIndex = -1;
                TreeNode closestMatch = null;
                toolboxTreeView.Nodes.Clear();
                var searchFilter = searchTextBox.Text.Split(ToolboxArgumentSeparator, 2)[0];
                foreach (var entry in from node in GetTreeViewLeafNodes(treeCache)
                                      let nameMatch = IndexOfMatch(node, searchFilter)
                                      let matchIndex = nameMatch < 0 && node.Parent != null ? IndexOfMatch(node.Parent, searchFilter) : nameMatch
                                      where matchIndex >= 0
                                      orderby nameMatch >= 0 descending, matchIndex, node.Text ascending
                                      select new { category = node.Parent.Text, node = (TreeNode)node.Clone(), matchIndex })
                {
                    entry.node.Text += string.Format(" ({0})", entry.category);
                    toolboxTreeView.Nodes.Add(entry.node);
                    if (closestMatch == null || entry.matchIndex < closestMatchIndex)
                    {
                        closestMatch = entry.node;
                        closestMatchIndex = entry.matchIndex;
                    }
                }

                if (toolboxTreeView.Nodes.Count > 0)
                {
                    toolboxTreeView.SelectedNode = closestMatch;
                }
            }
            UpdateTreeViewDescription();
            toolboxTreeView.EndUpdate();
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (toolboxTreeView.SelectedNode != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        toolboxTreeView.SelectedNode = toolboxTreeView.SelectedNode.PrevVisibleNode ?? toolboxTreeView.SelectedNode;
                        e.Handled = true;
                        break;
                    case Keys.Down:
                        toolboxTreeView.SelectedNode = toolboxTreeView.SelectedNode.NextVisibleNode ?? toolboxTreeView.SelectedNode;
                        e.Handled = true;
                        break;
                    case Keys.Return:
                        toolboxTreeView_KeyDown(sender, e);
                        searchTextBox.Clear();
                        var selectedView = selectionModel.SelectedView;
                        if (selectedView != null)
                        {
                            selectedView.Focus();
                        }
                        break;
                }
            }
        }

        static string GetToolboxArguments(CueBannerTextBox searchTextBox)
        {
            if (searchTextBox.CueBannerVisible || string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                return string.Empty;
            }

            var arguments = searchTextBox.Text.Split(ToolboxArgumentSeparator, 2);
            return arguments.Length == 2 ? arguments[1] : string.Empty;
        }

        void CreateGraphNode(TreeNode typeNode, Keys modifiers)
        {
            const string ErrorCaption = "Type Error";
            const string ErrorMessage = "Failed to create {0}:\n{1}";
            var model = selectionModel.SelectedView;
            if (!model.CanEdit) return;

            var group = modifiers.HasFlag(WorkflowGraphView.GroupModifier);
            var branch = modifiers.HasFlag(WorkflowGraphView.BranchModifier);
            var predecessor = modifiers.HasFlag(WorkflowGraphView.PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
            var arguments = GetToolboxArguments(searchTextBox);
            try { model.InsertGraphNode(typeNode, predecessor, branch, group, arguments); }
            catch (TargetInvocationException e)
            {
                var message = string.Format(ErrorMessage, typeNode.Text, e.InnerException.Message);
                editorSite.ShowError(message, ErrorCaption);
            }
            catch (SystemException e)
            {
                var message = string.Format(ErrorMessage, typeNode.Text, e.Message);
                editorSite.ShowError(message, ErrorCaption);
            }
        }

        void UpdateDescriptionTextBox(string name, string description, RichTextBox descriptionTextBox)
        {
            descriptionTextBox.SuspendLayout();
            descriptionTextBox.Text = name;

            var nameLength = descriptionTextBox.TextLength;
            name = descriptionTextBox.Text.Replace('\n', ' ');
            descriptionTextBox.Lines = new[] { name, description };
            descriptionTextBox.SelectionStart = 0;
            descriptionTextBox.SelectionLength = nameLength;
            descriptionTextBox.SelectionFont = selectionFont;
            descriptionTextBox.SelectionStart = nameLength;
            descriptionTextBox.SelectionLength = descriptionTextBox.TextLength - nameLength;
            descriptionTextBox.SelectionFont = regularFont;
            descriptionTextBox.ResumeLayout();
        }

        void UpdateTreeViewDescription()
        {
            var selectedNode = toolboxTreeView.SelectedNode;
            if (selectedNode != null &&
                (!toolboxTreeView.ShowRootLines ||
                selectedNode.Parent != null && selectedNode.GetNodeCount(false) == 0))
            {
                UpdateDescriptionTextBox(selectedNode.Text, selectedNode.ToolTipText, toolboxDescriptionTextBox);
            }
            else UpdateDescriptionTextBox(string.Empty, string.Empty, toolboxDescriptionTextBox);
        }

        void UpdateTreeViewSelection(bool focused)
        {
            var selectedNode = toolboxTreeView.SelectedNode;
            if (toolboxTreeView.Tag != selectedNode)
            {
                var previousNode = toolboxTreeView.Tag as TreeNode;
                if (previousNode != null) previousNode.BackColor = Color.Empty;
                toolboxTreeView.Tag = selectedNode;
            }

            if (selectedNode == null) return;
            selectedNode.BackColor = focused ? Color.Empty : themeRenderer.ToolStripRenderer.ColorTable.InactiveCaption;
        }

        private void toolboxTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return &&
                toolboxTreeView.SelectedNode != null &&
                toolboxTreeView.SelectedNode.Tag != null)
            {
                var typeNode = toolboxTreeView.SelectedNode;
                CreateGraphNode(typeNode, e.Modifiers);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void toolboxTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Node.Tag != null)
            {
                var typeNode = e.Node;
                CreateGraphNode(typeNode, Control.ModifierKeys);
            }
        }

        private void toolboxTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateTreeViewDescription();
            UpdateTreeViewSelection(toolboxTreeView.Focused);
        }

        private void toolboxTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selectedNode = toolboxTreeView.GetNodeAt(e.X, e.Y);
                if (selectedNode != null)
                {
                    toolboxTreeView.SelectedNode = selectedNode;
                    if (selectedNode.Tag != null)
                    {
                        var elementCategories = (ElementCategory[])selectedNode.Tag;
                        if (elementCategories.Contains(~ElementCategory.Source))
                        {
                            foreach (ToolStripItem item in toolboxContextMenuStrip.Items)
                            {
                                item.Visible = false;
                            }
                            subscribeSubjectToolStripMenuItem.Visible = true;
                            multicastSubjectToolStripMenuItem.Visible = true;
                        }
                        else
                        {
                            var allowSuccessor = selectedNode.Name != typeof(ExternalizedMappingBuilder).AssemblyQualifiedName;
                            insertAfterToolStripMenuItem.Visible = createBranchToolStripMenuItem.Visible = allowSuccessor;
                            createGroupToolStripMenuItem.Visible = elementCategories.Contains(ElementCategory.Nested);
                            subscribeSubjectToolStripMenuItem.Visible = false;
                            multicastSubjectToolStripMenuItem.Visible = false;
                            insertBeforeToolStripMenuItem.Visible = true;
                        }
                        toolboxContextMenuStrip.Show(toolboxTreeView, e.X, e.Y);
                    }
                }
            }
        }

        private void toolboxTreeView_Enter(object sender, EventArgs e)
        {
            UpdateTreeViewSelection(true);
        }

        private void toolboxTreeView_Leave(object sender, EventArgs e)
        {
            UpdateTreeViewSelection(false);
        }

        private void insertAfterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolboxTreeView_KeyDown(sender, new KeyEventArgs(Keys.Return));
        }

        private void insertBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolboxTreeView_KeyDown(sender, new KeyEventArgs(Keys.Shift | Keys.Return));
        }

        private void createBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolboxTreeView_KeyDown(sender, new KeyEventArgs(Keys.Alt | Keys.Return));
        }

        private void createGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolboxTreeView_KeyDown(sender, new KeyEventArgs(Keys.Control | Keys.Return));
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            editorSite.OnKeyDown(e);
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            editorSite.OnKeyPress(e);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.CutToClipboard();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (directoryToolStripTextBox.Focused)
            {
                directoryToolStripTextBox.Copy();
            }
            else
            {
                var model = selectionModel.SelectedView;
                if (model.GraphView.Focused)
                {
                    model.CopyToClipboard();
                }
            }
        }

        private void copyAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportImage(selectionModel.SelectedView);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (directoryToolStripTextBox.Focused)
            {
                directoryToolStripTextBox.Paste();
            }
            else
            {
                var model = selectionModel.SelectedView;
                if (model.GraphView.Focused)
                {
                    model.PasteFromClipboard();
                }
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.SelectAllGraphNodes();
            }
        }

        private void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.GroupGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.UngroupGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.EnableGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.DisableGraphNodes(selectionModel.SelectedNodes);
            }
        }

        #endregion

        #region Undo/Redo

        private void commandExecutor_StatusChanged(object sender, EventArgs e)
        {
            undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
            redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
            version++;
            UpdateTitle();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorSite.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorSite.Redo();
        }

        #endregion

        #region Package Manager

        private void packageManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorResult = EditorResult.ManagePackages;
            Close();
        }

        #endregion

        #region Gallery

        private void galleryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorResult = EditorResult.OpenGallery;
            Close();
        }

        #endregion

        #region Reload Extensions

        private void editExtensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EnsureWorkflowFile() && scriptEnvironment != null && !string.IsNullOrEmpty(scriptEnvironment.ProjectFileName))
            {
                ScriptEditorLauncher.Launch(this, scriptEnvironment.ProjectFileName);
            }
        }

        private void reloadExtensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EnsureWorkflowFile())
            {
                EditorResult = EditorResult.ReloadEditor;
                Close();
            }
        }

        private void reloadExtensionsDebugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scriptEnvironment != null) scriptEnvironment.DebugScripts = true;
            reloadExtensionsToolStripMenuItem_Click(sender, e);
        }

        #endregion

        #region Help Menu

        private void StartBrowser(string url)
        {
            Uri result;
            var validUrl = Uri.TryCreate(url, UriKind.Absolute, out result) &&
                (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            if (!validUrl)
            {
                throw new ArgumentException("The URL is malformed.");
            }

            try
            {
                Cursor = Cursors.AppStarting;
                Process.Start(url);
            }
            catch { } //best effort
            finally
            {
                Cursor = null;
            }
        }

        private void docsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartBrowser("http://bonsai-rx.org/docs/editor/");
        }

        private void forumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartBrowser("https://groups.google.com/forum/#!forum/bonsai-users");
        }

        private void reportBugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartBrowser("https://bitbucket.org/horizongir/bonsai/issues/new");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var about = new AboutBox())
            {
                about.ShowDialog();
            }
        }

        private void welcomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var start = new StartScreen())
            {
                start.ShowDialog();
                if (start.EditorResult != EditorResult.Exit && CloseWorkflow())
                {
                    if (start.EditorResult == EditorResult.ReloadEditor)
                    {
                        if (string.IsNullOrEmpty(start.FileName))
                        {
                            ClearWorkflow();
                        }
                        else OpenWorkflow(start.FileName);
                    }
                    else
                    {
                        EditorResult = start.EditorResult;
                        Close();
                    }
                }
            }
        }

        #endregion

        #region EditorSite Class

        class EditorSite : ISite, IWorkflowEditorService, IWorkflowEditorState, IWorkflowToolboxService, IUIService
        {
            System.Collections.IDictionary styles;
            MainForm siteForm;

            public EditorSite(MainForm form)
            {
                siteForm = form;
                styles = new System.Collections.Hashtable();
                styles["DialogFont"] = siteForm.Font;
            }

            public IComponent Component
            {
                get { return null; }
            }

            public IContainer Container
            {
                get { return null; }
            }

            public bool DesignMode
            {
                get { return false; }
            }

            public string Name { get; set; }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ExpressionBuilderGraph))
                {
                    return siteForm.selectionModel.SelectedView.Workflow;
                }

                if (serviceType == typeof(WorkflowBuilder))
                {
                    return siteForm.workflowBuilder;
                }

                if (serviceType == typeof(CommandExecutor))
                {
                    return siteForm.commandExecutor;
                }

                if (serviceType == typeof(WorkflowSelectionModel))
                {
                    return siteForm.selectionModel;
                }

                if (serviceType == typeof(ThemeRenderer))
                {
                    return siteForm.themeRenderer;
                }

                if (serviceType == typeof(SvgRendererFactory))
                {
                    return siteForm.iconRenderer;
                }

                if (serviceType == typeof(DialogTypeVisualizer))
                {
                    var selectedView = siteForm.selectionModel.SelectedView;
                    var selectedNode = siteForm.selectionModel.SelectedNodes.FirstOrDefault();
                    if (selectedNode != null)
                    {
                        var visualizerDialog = selectedView.GetVisualizerDialogLauncher(selectedNode);
                        var visualizer = visualizerDialog.Visualizer;
                        if (visualizer.IsValueCreated)
                        {
                            return visualizer.Value;
                        }
                    }
                }

                if (serviceType == typeof(IWorkflowEditorService) ||
                    serviceType == typeof(IWorkflowEditorState) ||
                    serviceType == typeof(IWorkflowToolboxService) ||
                    serviceType == typeof(IUIService))
                {
                    return this;
                }

                if (serviceType == typeof(IScriptEnvironment))
                {
                    return siteForm.scriptEnvironment;
                }

                if (siteForm.serviceProvider != null)
                {
                    return siteForm.serviceProvider.GetService(serviceType);
                }

                return null;
            }

            void HandleMenuItemShortcutKeys(KeyEventArgs e, ToolStripMenuItem menuItem, EventHandler onShortcut)
            {
                if (menuItem.Enabled && menuItem.ShortcutKeys == e.KeyData)
                {
                    onShortcut(this, e);
                    e.Handled = true;
                }
            }

            public void OnKeyDown(KeyEventArgs e)
            {
                if (!e.Shift && e.Control && e.KeyCode == Keys.E)
                {
                    siteForm.searchTextBox.Focus();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }

                HandleMenuItemShortcutKeys(e, siteForm.newToolStripMenuItem, siteForm.newToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.openToolStripMenuItem, siteForm.openToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.saveToolStripMenuItem, siteForm.saveToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.undoToolStripMenuItem, siteForm.undoToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.redoToolStripMenuItem, siteForm.redoToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.saveAsWorkflowToolStripMenuItem, siteForm.saveAsWorkflowToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.exportImageToolStripMenuItem, siteForm.exportImageToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.copyAsImageToolStripMenuItem, siteForm.copyAsImageToolStripMenuItem_Click);
            }

            public void OnKeyPress(KeyPressEventArgs e)
            {
                var model = siteForm.selectionModel.SelectedView ?? siteForm.editorControl.WorkflowGraphView;
                if (model.CanEdit && model.GraphView.Focused)
                {
                    if (char.IsLetter(e.KeyChar))
                    {
                        siteForm.searchTextBox.Focus();
                        siteForm.searchTextBox.Clear();
                        siteForm.searchTextBox.AppendText(e.KeyChar.ToString());
                        e.Handled = true;
                    }
                }
            }

            public void OnContextMenuOpening(EventArgs e)
            {
                siteForm.hotKeys.SuppressSystemKeyPress = true;
            }

            public void OnContextMenuClosed(EventArgs e)
            {
                siteForm.hotKeys.SuppressSystemKeyPress = false;
            }

            public DirectoryInfo EnsureExtensionsDirectory()
            {
                if (!siteForm.workflowFileWatcher.EnableRaisingEvents && !siteForm.extensionsPath.Exists)
                {
                    if (string.IsNullOrEmpty(siteForm.FileName))
                    {
                        MessageBox.Show(
                            Resources.CreateExtensionsWorkflow_Warning,
                            Resources.CreateExtensionsWorkflow_Warning_Caption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                    else
                    {
                        var createExtensions = MessageBox.Show(
                            Resources.CreateExtensionsFolder_Question,
                            Resources.CreateExtensionsFolder_Question_Caption,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);
                        if (createExtensions == DialogResult.Yes)
                        {
                            siteForm.extensionsPath.Create();
                            siteForm.extensionsPath.Refresh();
                            siteForm.OnExtensionsDirectoryChanged(EventArgs.Empty);
                        }
                    }
                }

                return siteForm.extensionsPath;
            }

            public WorkflowBuilder LoadWorkflow(string fileName)
            {
                SemanticVersion version;
                var workflow = siteForm.LoadWorkflow(fileName, out version);
                return siteForm.UpdateWorkflow(workflow, version);
            }

            public void OpenWorkflow(string fileName)
            {
                siteForm.OpenWorkflow(fileName);
            }

            public string StoreWorkflowElements(WorkflowBuilder builder)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException("builder");
                }

                if (builder.Workflow.Count > 0)
                {
                    var stringBuilder = new StringBuilder();
                    using (var writer = XmlnsIndentedWriter.Create(stringBuilder, DefaultWriterSettings))
                    {
                        WorkflowBuilder.Serializer.Serialize(writer, builder);
                    }

                    return stringBuilder.ToString();
                }

                return string.Empty;
            }

            public WorkflowBuilder RetrieveWorkflowElements(string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var stringReader = new StringReader(text);
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        try
                        {
                            reader.MoveToContent();
                            var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
                            if (serializer.CanDeserialize(reader))
                            {
                                return (WorkflowBuilder)serializer.Deserialize(reader);
                            }
                        }
                        catch (XmlException) { }
                    }
                }

                return new WorkflowBuilder();
            }

            public IEnumerable<Type> GetTypeVisualizers(Type targetType)
            {
                return siteForm.typeVisualizers.GetTypeVisualizers(targetType);
            }

            public string GetPackageDisplayName(string packageKey)
            {
                return MainForm.GetPackageDisplayName(packageKey);
            }

            public IEnumerable<WorkflowElementDescriptor> GetToolboxElements()
            {
                return siteForm.workflowElements;
            }

            public void SelectNextControl(bool forward)
            {
                if (forward) siteForm.SelectNextControl(siteForm.editorControl, true, true, true, true);
                else siteForm.ActiveControl = siteForm.searchTextBox;
                siteForm.Activate();
            }

            public void StartWorkflow(bool debugging)
            {
                siteForm.debugging = debugging;
                siteForm.StartWorkflow();
            }

            public void StopWorkflow()
            {
                siteForm.StopWorkflow();
            }

            public void RestartWorkflow()
            {
                siteForm.RestartWorkflow();
            }

            public bool ValidateWorkflow()
            {
                if (siteForm.running == null)
                {
                    try
                    {
                        siteForm.OnWorkflowValidating(EventArgs.Empty);
                        siteForm.ClearWorkflowError();
                        siteForm.workflowBuilder.Workflow.Build();
                    }
                    catch (WorkflowBuildException ex)
                    {
                        siteForm.HandleWorkflowError(ex);
                        return false;
                    }
                }

                return true;
            }

            public void RefreshEditor()
            {
                siteForm.HighlightWorkflowError();
            }

            public bool WorkflowRunning
            {
                get { return siteForm.running != null; }
            }

            public event EventHandler WorkflowStarted;

            public event EventHandler WorkflowStopped;

            public void OnWorkflowStarted(EventArgs e)
            {
                var handler = WorkflowStarted;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public void OnWorkflowStopped(EventArgs e)
            {
                var handler = WorkflowStopped;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public void Undo()
            {
                Undo(true);
            }

            public void Undo(bool allowRedo)
            {
                siteForm.version -= 2;
                siteForm.commandExecutor.Undo(allowRedo);
            }

            public void Redo()
            {
                siteForm.commandExecutor.Redo();
            }

            public bool CanShowComponentEditor(object component)
            {
                var editor = TypeDescriptor.GetEditor(component, typeof(ComponentEditor));
                if (editor != null) return true;

                return siteForm.scriptEnvironment != null && siteForm.scriptEnvironment.AssemblyName != null &&
                       siteForm.scriptEnvironment.AssemblyName.FullName == component.GetType().Assembly.FullName;
            }

            public IWin32Window GetDialogOwnerWindow()
            {
                return siteForm;
            }

            public void SetUIDirty()
            {
                siteForm.propertyGrid.Refresh();
            }

            public bool ShowComponentEditor(object component, IWin32Window parent)
            {
                var editor = (ComponentEditor)TypeDescriptor.GetEditor(component, typeof(ComponentEditor));
                if (editor != null)
                {
                    var windowsFormsEditor = editor as WindowsFormsComponentEditor;
                    if (windowsFormsEditor != null)
                    {
                        return windowsFormsEditor.EditComponent(component, parent);
                    }

                    var workflowEditor = editor as WorkflowComponentEditor;
                    if (workflowEditor != null)
                    {
                        return workflowEditor.EditComponent(component, this, parent);
                    }

                    return editor.EditComponent(component);
                }
                else if (siteForm.scriptEnvironment != null && siteForm.scriptEnvironment.AssemblyName != null)
                {
                    var componentType = component.GetType();
                    if (siteForm.scriptEnvironment.AssemblyName.FullName == componentType.Assembly.FullName)
                    {
                        var scriptFile = Path.Combine(siteForm.extensionsPath.FullName, componentType.Name);
                        ScriptEditorLauncher.Launch(siteForm, siteForm.scriptEnvironment.ProjectFileName, scriptFile);
                    }
                }

                return false;
            }

            public DialogResult ShowDialog(Form form)
            {
                return form.ShowDialog(siteForm);
            }

            public void ShowError(Exception ex, string message)
            {
                if (ex != null) message = string.Format(message, ex.Message);
                MessageBox.Show(siteForm, message, Resources.Editor_Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public void ShowError(Exception ex)
            {
                ShowError(ex.Message);
            }

            public void ShowError(string message)
            {
                ShowError(message, Resources.Editor_Error_Caption);
            }

            public void ShowError(string message, string caption)
            {
                MessageBox.Show(siteForm, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons)
            {
                return MessageBox.Show(siteForm, message, caption, buttons);
            }

            public void ShowMessage(string message, string caption)
            {
                MessageBox.Show(siteForm, message, caption);
            }

            public void ShowMessage(string message)
            {
                MessageBox.Show(siteForm, message);
            }

            public bool ShowToolWindow(Guid toolWindow)
            {
                return false;
            }

            public System.Collections.IDictionary Styles
            {
                get { return styles; }
            }
        }

        #endregion

        #region PropertyGrid Controller

        private void propertyGridContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var item = propertyGrid.SelectedGridItem;
            resetToolStripMenuItem.Enabled =
                item != null &&
                item.PropertyDescriptor != null &&
                item.Parent.Value != null &&
                item.PropertyDescriptor.CanResetValue(item.Parent.Value);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = propertyGrid.SelectedGridItem;
            if (item != null && item.PropertyDescriptor.CanResetValue(item.Parent.Value))
            {
                propertyGrid.ResetSelectedProperty();
            }
        }

        private void descriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertyGrid.HelpVisible = descriptionToolStripMenuItem.Checked;
        }

        private void propertyGrid_Validated(object sender, EventArgs e)
        {
            editorSite.ValidateWorkflow();
            RefreshSelection();
        }

        private void propertyGrid_DragEnter(object sender, DragEventArgs e)
        {
            var gridItem = propertyGrid.SelectedGridItem;
            if (gridItem != null && gridItem.PropertyDescriptor != null &&
                !gridItem.PropertyDescriptor.IsReadOnly &&
                gridItem.PropertyDescriptor.Converter.CanConvertFrom(typeof(string)) &&
                (e.Data.GetDataPresent(typeof(string)) ||
                 e.Data.GetDataPresent(DataFormats.FileDrop)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void propertyGrid_DragDrop(object sender, DragEventArgs e)
        {
            var gridItem = propertyGrid.SelectedGridItem;
            if (gridItem != null && gridItem.PropertyDescriptor != null)
            {
                string text = null;
                if (e.Data.GetDataPresent(typeof(string)))
                {
                    text = (string)e.Data.GetData(typeof(string));
                }
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    if (path.Length > 0)
                    {
                        text = PathConvert.GetProjectPath(path[0]);
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    object component;
                    if (propertyGrid.SelectedObjects != null && propertyGrid.SelectedObjects.Length > 1)
                    {
                        component = propertyGrid.SelectedObjects;
                    }
                    else component = propertyGrid.SelectedObject;
                    var value = gridItem.PropertyDescriptor.Converter.ConvertFrom(text);
                    gridItem.PropertyDescriptor.SetValue(component, value);
                    propertyGrid.Refresh();
                }
            }
        }

        #endregion

        #region StatusStrip Controller

        void UpdateStatusLabelSize()
        {
            var statusSize = statusStrip.Size;
            statusTextLabel.MaximumSize = new Size(statusSize.Width - statusImageLabel.Width * 2, 0);
        }

        void statusStrip_SizeChanged(object sender, EventArgs e)
        {
            UpdateStatusLabelSize();
        }

        #endregion

        #region Color Themes

        static ColorTheme InvertTheme(ColorTheme theme)
        {
            return theme == ColorTheme.Light ? ColorTheme.Dark : ColorTheme.Light;
        }

        private void InitializeTheme()
        {
            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            var foreColor = colorTable.ControlForeColor;
            var backColor = colorTable.ControlBackColor;
            var panelColor = colorTable.ContentPanelBackColor;
            var windowBackColor = colorTable.WindowBackColor;
            var windowText = colorTable.WindowText;
            ForeColor = foreColor;
            BackColor = backColor;
            propertiesSplitContainer.BackColor = panelColor;
            propertiesLabel.BackColor = colorTable.SeparatorDark;
            propertiesLabel.ForeColor = foreColor;
            propertyGrid.BackColor = panelColor;
            propertyGrid.LineColor = colorTable.SeparatorDark;
            propertyGrid.CategoryForeColor = foreColor;
            propertyGrid.CategorySplitterColor = colorTable.SeparatorDark;
            propertyGrid.CommandsBackColor = panelColor;
            propertyGrid.CommandsBorderColor = panelColor;
            propertyGrid.HelpBackColor = panelColor;
            propertyGrid.HelpForeColor = foreColor;
            propertyGrid.ViewBackColor = panelColor;
            propertyGrid.ViewForeColor = windowText;
            propertyGrid.ViewBorderColor = panelColor;
            propertyGrid.CanShowVisualStyleGlyphs = false;
            toolboxTableLayoutPanel.BackColor = panelColor;
            toolboxSplitContainer.BackColor = panelColor;
            toolboxLabel.BackColor = colorTable.SeparatorDark;
            toolboxLabel.ForeColor = foreColor;
            toolboxTreeView.BackColor = panelColor;
            toolboxTreeView.ForeColor = windowText;
            toolboxDescriptionTextBox.BackColor = panelColor;
            toolboxDescriptionTextBox.ForeColor = foreColor;
            propertiesDescriptionTextBox.BackColor = panelColor;
            propertiesDescriptionTextBox.ForeColor = foreColor;
            menuStrip.ForeColor = SystemColors.ControlText;
            toolStrip.Renderer = themeRenderer.ToolStripRenderer;
            statusStrip.Renderer = themeRenderer.ToolStripRenderer;

            var searchLayoutTop = propertiesLabel.Height + searchTextBox.Top + 1;
            var labelOffset = searchLayoutTop - editorControl.ItemHeight;
            toolboxSplitContainer.Margin -= new Padding(0, 0, 0, editorControl.Bottom - toolboxSplitContainer.Bottom);
            propertiesSplitContainer.Margin -= new Padding(0, 0, 0, editorControl.Bottom - propertiesSplitContainer.Bottom);
            if (themeRenderer.ActiveTheme == ColorTheme.Light && labelOffset < 0)
            {
                labelOffset += 1;
            }
            propertiesLayoutPanel.RowStyles[0].Height -= labelOffset;
            toolboxLayoutPanel.RowStyles[0].Height -= labelOffset;
            UpdateTreeViewSelection(toolboxTreeView.Focused);
            propertyGrid.Refresh();
        }

        private void themeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            themeRenderer.ActiveTheme = InvertTheme(themeRenderer.ActiveTheme);
        }

        private void themeRenderer_ThemeChanged(object sender, EventArgs e)
        {
            InitializeTheme();
            themeToolStripMenuItem.Text = InvertTheme(themeRenderer.ActiveTheme) + " &Theme";
            EditorSettings.Instance.EditorTheme = themeRenderer.ActiveTheme;
        }

        #endregion
    }
}
