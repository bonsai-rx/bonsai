using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using Bonsai.Expressions;
using Bonsai.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Bonsai.Editor.Properties;
using System.IO;
using System.Windows.Forms.Design;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Globalization;
using System.Reactive.Subjects;
using Bonsai.Editor.Scripting;
using Bonsai.Editor.Themes;
using Bonsai.Editor.GraphView;
using Bonsai.Editor.GraphModel;
using System.Threading.Tasks;
using System.Net;

namespace Bonsai.Editor
{
    public partial class EditorForm : Form
    {
        const float DefaultEditorScale = 1.0f;
        const string EditorUid = "editor";
        const string BonsaiExtension = ".bonsai";
        const string BonsaiPackageName = "Bonsai";
        const string ExtensionsDirectory = "Extensions";
        const string DefinitionsDirectory = "Definitions";
        const string WorkflowCategoryName = "Workflow";
        const string SubjectCategoryName = "Subject";
        const string DefaultWorkflowNamespace = "Unspecified";
        static readonly AttributeCollection DesignTimeAttributes = new AttributeCollection(BrowsableAttribute.Yes, DesignTimeVisibleAttribute.Yes);
        static readonly AttributeCollection RuntimeAttributes = AttributeCollection.FromExisting(DesignTimeAttributes, DesignOnlyAttribute.No);
        static readonly char[] ToolboxArgumentSeparator = new[] { ' ' };
        static readonly object ExtensionsDirectoryChanged = new object();
        static readonly object WorkflowValidating = new object();

        int version;
        int saveVersion;
        readonly Font regularFont;
        readonly Font selectionFont;
        readonly EditorSite editorSite;
        readonly HotKeyMessageFilter hotKeys;
        readonly IServiceProvider serviceProvider;
        readonly IScriptEnvironment scriptEnvironment;
        readonly IDocumentationProvider documentationProvider;
        readonly WorkflowEditorControl editorControl;
        readonly WorkflowSelectionModel selectionModel;
        readonly Dictionary<string, string> propertyAssignments;
        readonly Dictionary<string, TreeNode> toolboxCategories;
        readonly IObservable<IGrouping<string, WorkflowElementDescriptor>> toolboxElements;
        readonly IObservable<TypeVisualizerDescriptor> visualizerElements;
        readonly List<TreeNode> treeCache;
        readonly ToolStripStatusLabel statusTextLabel;
        readonly Bitmap statusRunningImage;
        readonly ThemeRenderer themeRenderer;
        readonly SvgRendererFactory iconRenderer;
        readonly ToolStripButton statusUpdateAvailableLabel;
        readonly ToolStripItem directoryToolStripItem;
        readonly BehaviorSubject<bool> updatesAvailable;
        readonly FormScheduler formScheduler;
        readonly TypeVisualizerMap typeVisualizers;
        readonly List<WorkflowElementDescriptor> workflowElements;
        readonly List<WorkflowElementDescriptor> workflowExtensions;
        readonly WorkflowRuntimeExceptionCache exceptionCache;
        readonly string definitionsPath;
        AttributeCollection browsableAttributes;
        DirectoryInfo extensionsPath;
        WorkflowBuilder workflowBuilder;
        WorkflowException workflowError;
        IDisposable running;
        bool debugging;
        bool building;
        SizeF inverseScaleFactor;
        SizeF scaleFactor;

        public EditorForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider)
            : this(elementProvider, visualizerProvider, null)
        {
        }

        public EditorForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            IServiceProvider provider)
            : this(elementProvider, visualizerProvider, provider, DefaultEditorScale)
        {
        }

        public EditorForm(
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
            statusTextLabel = new ToolStripStatusLabel();
            statusTextLabel.Spring = true;
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
            statusStrip.Items.Add(statusTextLabel);

            directoryToolStripItem = directoryToolStripTextBox;
            if (EditorSettings.IsRunningOnMono)
            {
                var directoryToolStripLabel = new ToolStripConstrainedStatusLabel();
                directoryToolStripLabel.BorderSides = ToolStripStatusLabelBorderSides.All;
                directoryToolStripLabel.MaximumSize = new Size(250, 0);
                directoryToolStripLabel.Spring = true;
                var textBoxIndex = toolStrip.Items.IndexOf(directoryToolStripTextBox);
                toolStrip.Items.RemoveAt(textBoxIndex);
                toolStrip.Items.Insert(textBoxIndex, directoryToolStripLabel);
                directoryToolStripItem = directoryToolStripLabel;
            }

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
                documentationProvider = (IDocumentationProvider)serviceProvider.GetService(typeof(IDocumentationProvider));
            }

            definitionsPath = Path.Combine(Path.GetTempPath(), DefinitionsDirectory + "." + GuidHelper.GetProcessGuid().ToString());
            editorControl = new WorkflowEditorControl(editorSite);
            editorControl.Enter += new EventHandler(editorControl_Enter);
            editorControl.Workflow = workflowBuilder.Workflow;
            editorControl.Dock = DockStyle.Fill;
            workflowSplitContainer.Panel1.Controls.Add(editorControl);
            propertyGrid.BrowsableAttributes = browsableAttributes = DesignTimeAttributes;
            propertyGrid.LineColor = SystemColors.InactiveBorder;
            propertyGrid.Site = editorSite;

            editorControl.Disposed += editorControl_Disposed;
            selectionModel.SelectionChanged += new EventHandler(selectionModel_SelectionChanged);
            toolboxElements = elementProvider;
            visualizerElements = visualizerProvider;
            Application.AddMessageFilter(hotKeys);

            if (EditorSettings.IsRunningOnMono)
            {
                editorControl.Enter += delegate { menuStrip.Enabled = false; };
                editorControl.Leave += delegate { menuStrip.Enabled = true; };
            }
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
            if (desktopBounds.Width > 0 &&
                Array.Exists(Screen.AllScreens, screen => screen.WorkingArea.IntersectsWith(desktopBounds)))
            {
                Bounds = desktopBounds;
            }

            WindowState = EditorSettings.Instance.WindowState;
            themeRenderer.ActiveTheme = EditorSettings.Instance.EditorTheme;
            editorControl.AnnotationPanelSize = (int)Math.Round(
                EditorSettings.Instance.AnnotationPanelSize * scaleFactor.Width);
        }

        void CloseEditorForm()
        {
            Application.RemoveMessageFilter(hotKeys);
            EditorSettings.Instance.AnnotationPanelSize = (int)Math.Round(
                editorControl.AnnotationPanelSize * inverseScaleFactor.Width);
            var desktopBounds = WindowState != FormWindowState.Normal ? RestoreBounds : Bounds;
            EditorSettings.Instance.DesktopBounds = ScaleBounds(desktopBounds, inverseScaleFactor);
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

            var formClosed = Observable.FromEventPattern<FormClosedEventHandler, FormClosedEventArgs>(
                handler => FormClosed += handler,
                handler => FormClosed -= handler);
            InitializeSubjectSources().TakeUntil(formClosed).Subscribe();
            InitializeWorkflowFileWatcher().TakeUntil(formClosed).Subscribe();
            updatesAvailable.TakeUntil(formClosed).ObserveOn(formScheduler).Subscribe(HandleUpdatesAvailable);

            var currentDirectory = Path.GetFullPath(Environment.CurrentDirectory).TrimEnd('\\');
            var appDomainBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
            var currentDirectoryRestricted = currentDirectory == appDomainBaseDirectory;
            if (!EditorSettings.IsRunningOnMono)
            {
                var systemPath = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.System)).TrimEnd('\\');
                var systemX86Path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)).TrimEnd('\\');
                currentDirectoryRestricted |= currentDirectory == systemPath || currentDirectory == systemX86Path;
            }

            var workflowBaseDirectory = validFileName
                ? Path.GetDirectoryName(initialFileName)
                : (!currentDirectoryRestricted ? currentDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            if (currentDirectoryRestricted)
            {
                currentDirectory = workflowBaseDirectory;
                Environment.CurrentDirectory = currentDirectory;
            }

            directoryToolStripItem.Text = currentDirectory;
            openWorkflowDialog.InitialDirectory = saveWorkflowDialog.InitialDirectory = currentDirectory;
            extensionsPath = new DirectoryInfo(Path.Combine(workflowBaseDirectory, ExtensionsDirectory));
            if (extensionsPath.Exists) OnExtensionsDirectoryChanged(EventArgs.Empty);

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
                        StartWorkflow(debugging);
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
            var workflowSplitterScale = EditorSettings.IsRunningOnMono ? 0.5f / factor.Width : 1.0f;
            var toolboxSplitterScale = EditorSettings.IsRunningOnMono ? 0.75f / factor.Height : 1.0f;
            toolboxSplitterScale *= DefaultToolboxSplitterDistance / toolboxSplitContainer.SplitterDistance;
            panelSplitContainer.SplitterDistance = (int)(panelSplitContainer.SplitterDistance * factor.Height);
            workflowSplitContainer.SplitterDistance = (int)(workflowSplitContainer.SplitterDistance * workflowSplitterScale * factor.Height);
            propertiesSplitContainer.SplitterDistance = (int)(propertiesSplitContainer.SplitterDistance * factor.Height);
            toolboxSplitContainer.SplitterDistance = (int)(toolboxSplitContainer.SplitterDistance * toolboxSplitterScale * factor.Height);
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
            if (EditorResult == EditorResult.Exit)
            {
                const int DeleteRetries = 3;
                for (int i = 0; i < DeleteRetries; i++)
                {
                    try { Directory.Delete(definitionsPath, true); break; }
                    catch { } // best effort
                }
            }
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

                    var nameProperty = TypeDescriptor.GetProperties(typeof(SubscribeSubject))[nameof(SubscribeSubject.Name)];
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
            string[] workflowFiles;
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
                        if (reader.Name == nameof(WorkflowBuilder.Description))
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
                                    select (targetType, visualizerType);

            return visualizerMapping
                .ObserveOn(formScheduler)
                .Do(typeMapping => typeVisualizers.Add(typeMapping.targetType, typeMapping.visualizerType))
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
                    if (typeCategory == ElementCategory.Nested ||
                        typeCategory == ~ElementCategory.Combinator)
                    {
                        continue;
                    }

                    if (typeCategory == ElementCategory.Workflow ||
#pragma warning disable CS0612 // Type or member is obsolete
                        typeCategory == ElementCategory.Condition ||
#pragma warning restore CS0612 // Type or member is obsolete
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
                    Resources.EnsureSavedWorkflow_Question,
                    Resources.UnsavedWorkflow_Question_Caption,
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
                    Resources.StopWorkflow_Question,
                    Resources.StopWorkflow_Question_Caption,
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
                    Resources.UnsavedWorkflow_Question,
                    Resources.UnsavedWorkflow_Question_Caption,
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

        WorkflowBuilder PrepareWorkflow(WorkflowBuilder workflowBuilder, SemanticVersion version, out bool upgraded)
        {
            upgraded = false;
            var workflow = workflowBuilder.Workflow;
            try { upgraded = UpgradeHelper.TryUpgradeWorkflow(workflow, version, out workflow); }
            catch (WorkflowBuildException)
            {
                MessageBox.Show(
                    this,
                    Resources.UpgradeWorkflow_Error,
                    Resources.UpgradeWorkflow_Warning_Caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return new WorkflowBuilder(workflow.ToInspectableGraph())
            {
                Description = workflowBuilder.Description
            };
        }

        void ClearWorkflow()
        {
            ClearWorkflowError();
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
            try { workflowBuilder = ElementStore.LoadWorkflow(fileName, out workflowVersion); }
            catch (SystemException ex) when (ex is InvalidOperationException || ex is XmlException)
            {
                var activeException = ex.InnerException ?? ex;
                var errorMessage = activeException.Message;
                if (activeException.InnerException != null)
                {
                    errorMessage += Environment.NewLine + activeException.InnerException.Message;
                }

                errorMessage = string.Format(Resources.OpenWorkflow_Error, Path.GetFileName(fileName), errorMessage);
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
                    var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    errorMessage = string.Format(Resources.OpenWorkflow_Error, Path.GetFileName(fileName), errorMessage);
                    MessageBox.Show(this, errorMessage, Resources.OpenWorkflow_Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            workflowBuilder = PrepareWorkflow(workflowBuilder, workflowVersion, out bool upgraded);
            editorControl.VisualizerLayout = null;
            editorControl.Workflow = workflowBuilder.Workflow;
            editorSite.ValidateWorkflow();

            var layoutPath = LayoutHelper.GetLayoutPath(fileName);
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
            if (upgraded)
            {
                MessageBox.Show(
                    this,
                    Resources.UpgradeWorkflow_Warning,
                    Resources.UpgradeWorkflow_Warning_Caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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
                ElementStore.SaveElement(serializer, fileName, o);
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
                var layoutPath = LayoutHelper.GetLayoutPath(fileName);
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

        void SaveWorkflowExtension(string fileName, GraphNode node)
        {
            var groupNode = node;
            var model = selectionModel.SelectedView;
            if (groupNode == null)
            {
                model.Editor.GroupGraphNodes(selectionModel.SelectedNodes);
                groupNode = selectionModel.SelectedNodes.Single();
                if (!model.CanEdit)
                {
                    //TODO: Refactor to avoid covertly modifying read-only or running workflow
                    editorSite.Undo(false);
                }
            }

            var includePath = PathConvert.GetProjectPath(fileName);
            var groupBuilder = (GroupWorkflowBuilder)ExpressionBuilder.Unwrap(groupNode.Value);
            if (groupBuilder.Workflow.Descendants().Any(builder =>
                    builder is IncludeWorkflowBuilder includeBuilder &&
                    includeBuilder.Path == includePath))
            {
                var errorMessage = string.Format("Included workflow '{0}' includes itself.", includePath);
                MessageBox.Show(this, errorMessage, Resources.SaveElement_Error_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (node == null && model.CanEdit) editorSite.Undo(false);
                return;
            }

            groupBuilder.Name = Path.GetFileNameWithoutExtension(fileName);
            var selectedElements = LayeredGraphExtensions.ToWorkflow(new[] { groupNode });
            groupBuilder = (GroupWorkflowBuilder)selectedElements.Single().Value;
            var serializerWorkflowBuilder = new WorkflowBuilder(groupBuilder.Workflow);
            serializerWorkflowBuilder.Description = groupBuilder.Description;
            if (SaveWorkflowBuilder(fileName, serializerWorkflowBuilder) && model.CanEdit)
            {
                var includeBuilder = new IncludeWorkflowBuilder { Path = includePath };
                model.Editor.ReplaceGraphNode(groupNode, includeBuilder);
                editorSite.ValidateWorkflow();
            }
        }

        void OnWorkflowValidating(EventArgs e)
        {
            (Events[WorkflowValidating] as EventHandler)?.Invoke(this, e);
        }

        void OnExtensionsDirectoryChanged(EventArgs e)
        {
            (Events[ExtensionsDirectoryChanged] as EventHandler)?.Invoke(this, e);
        }

        void UpdateWorkflowDirectory(string fileName)
        {
            UpdateWorkflowDirectory(fileName, true);
        }

        void UpdateWorkflowDirectory(string fileName, bool setWorkingDirectory)
        {
            var workflowDirectory = Path.GetDirectoryName(fileName);
            if (setWorkingDirectory && directoryToolStripItem.Text != workflowDirectory)
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

        void ExportImage(WorkflowGraphView model)
        {
            ExportImage(model, null);
        }

        void ExportImage(WorkflowGraphView model, string fileName)
        {
            var workflow = model.Workflow;
            if (model.GraphView.SelectedNodes.Count() > 0)
            {
                var selectedElements = model.GraphView.SelectedNodes.SortSelection(workflow);
                workflow = selectedElements.ToWorkflow();
            }

            var extension = Path.GetExtension(fileName);
            if (extension == ".svg")
            {
                var svg = ExportHelper.ExportSvg(workflow, iconRenderer);
                File.WriteAllText(fileName, svg);
            }
            else
            {
                using var bitmap = ExportHelper.ExportBitmap(workflow, Font, iconRenderer);
                if (string.IsNullOrEmpty(fileName))
                {
                    Clipboard.SetImage(bitmap);
                }
                else bitmap.Save(fileName);
            }
        }

        private void directoryToolStripTextBox_DoubleClick(object sender, EventArgs e)
        {
            directoryToolStripTextBox.SelectAll();
        }

        private void browseDirectoryToolStripButton_Click(object sender, EventArgs e)
        {
            EditorDialog.OpenUrl(directoryToolStripItem.Text);
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
                if (ExpressionBuilder.Unwrap(selectedNode.Value) is GroupWorkflowBuilder groupBuilder)
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

        protected override bool ProcessDialogChar(char charCode)
        {
            if ((ModifierKeys & Keys.Alt) == Keys.None)
            {
                return false;
            }

            return base.ProcessDialogChar(charCode);
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

        void StartWorkflow(bool debug)
        {
            if (running == null)
            {
                building = true;
                debugging = debug;
                ClearWorkflowError();
                LayoutHelper.SetWorkflowNotifications(workflowBuilder.Workflow, debug);
                if (!debug && editorControl.VisualizerLayout != null)
                {
                    LayoutHelper.SetLayoutNotifications(editorControl.VisualizerLayout);
                }

                running = Observable.Using(
                    () =>
                    {
                        var shutdown = ShutdownSequence();
                        try
                        {
                            var runtimeWorkflow = workflowBuilder.Workflow.BuildObservable();
                            Invoke((Action)(() =>
                            {
                                statusTextLabel.Text = Resources.RunningStatus;
                                statusImageLabel.Image = statusRunningImage;
                                editorSite.OnWorkflowStarted(EventArgs.Empty);
                                Activate();
                            }));
                            return new WorkflowDisposable(runtimeWorkflow, shutdown);
                        }
                        catch (Exception ex)
                        {
                            HandleWorkflowError(ex);
                            shutdown.Dispose();
                            throw;
                        }
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
                BeginInvoke((Action<bool>)StartWorkflow, debugging);
            };

            editorSite.WorkflowStopped += startWorkflow;
            StopWorkflow();
        }

        void RegisterWorkflowError(Exception ex)
        {
            Action registerError = () =>
            {
                if (ex is WorkflowRuntimeException workflowException)
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

            if (e.InnerException is WorkflowException nestedException)
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
                statusStrip.ContextMenuStrip = null;
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
                    throw new InvalidOperationException(Resources.ExceptionNodeNotFound_Error);
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
                var errorCaption = buildException ? Resources.BuildError_Caption : Resources.RuntimeError_Caption;
                statusTextLabel.Text = ex.Message;
                statusStrip.ContextMenuStrip = statusContextMenuStrip;
                statusImageLabel.Image = buildException ? Resources.StatusBlockedImage : Resources.StatusCriticalImage;
                if (showMessageBox)
                {
                    editorSite.ShowError(ex.Message, errorCaption);
                }
            }
        }

        bool HandleSchedulerError(Exception e)
        {
            using var shutdown = ShutdownSequence();
            HandleWorkflowError(e);
            return true;
        }

        void HandleWorkflowError(Exception e)
        {
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
        }

        void HandleWorkflowCompleted()
        {
            Action clearErrors = exceptionCache.Clear;
            if (InvokeRequired) BeginInvoke(clearErrors);
            else clearErrors();
        }

        void HighlightExpression(WorkflowGraphView workflowView, ExpressionScope scope)
        {
            if (workflowView == null)
            {
                throw new ArgumentNullException(nameof(workflowView));
            }

            var graphNode = workflowView.FindGraphNode(scope.Value);
            if (graphNode != null)
            {
                workflowView.GraphView.SelectedNode = graphNode;
                var innerScope = scope.InnerScope;
                if (innerScope != null)
                {
                    workflowView.LaunchWorkflowView(graphNode);
                    var editorLauncher = workflowView.GetWorkflowEditorLauncher(graphNode);
                    if (editorLauncher != null)
                    {
                        HighlightExpression(editorLauncher.WorkflowGraphView, innerScope);
                    }
                }
                else
                {
                    var ownerForm = workflowView.EditorControl.ParentForm;
                    if (ownerForm != null) ownerForm.Activate();
                    workflowView.SelectGraphNode(graphNode);
                }
            }
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
                model.Editor.DeleteGraphNodes(selectionModel.SelectedNodes);
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
                if (activeControl is IContainerControl container)
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
            if (e.Item is TreeNode selectedNode &&
                selectedNode.Tag is not null &&
                selectedNode.GetNodeCount(false) == 0)
            {
                toolboxTreeView.DoDragDrop(selectedNode, DragDropEffects.Copy);
            }
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

        private void GetSelectionDescription(object[] selectedObjects, out string displayName, out string description)
        {
            var displayNames = selectedObjects.Select(ElementHelper.GetElementName).Distinct().Reverse().ToArray();
            displayName = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayNames);
            var objectDescriptions = selectedObjects.Select(ElementHelper.GetElementDescription).Distinct().Reverse().ToArray();
            description = objectDescriptions.Length == 1 ? objectDescriptions[0] : string.Empty;
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

            var selectionBrowsableAttributes = canEdit ? DesignTimeAttributes : RuntimeAttributes;
            if (browsableAttributes != selectionBrowsableAttributes)
            {
                browsableAttributes = selectionBrowsableAttributes;
                propertyGrid.BrowsableAttributes = browsableAttributes;
            }

            string displayName, description;
            if (!hasSelectedObjects && selectedView != null)
            {
                // Select externalized properties
                var launcher = selectedView.Launcher;
                if (launcher != null)
                {
                    displayName = ElementHelper.GetElementName(launcher.Builder);
                    description = ElementHelper.GetElementDescription(launcher.Builder);
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
            else
            {
                GetSelectionDescription(selectedObjects, out displayName, out description);
                propertyGrid.SelectedObjects = selectedObjects;
            }
            UpdateDescriptionTextBox(displayName, description, propertiesDescriptionTextBox);
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var shift = ModifierKeys.HasFlag(Keys.Shift);
            var control = ModifierKeys.HasFlag(Keys.Control);
            if (shift)
            {
                if (control) RestartWorkflow();
                else StopWorkflow();
            }
            else StartWorkflow(!control);
        }

        private void startWithoutDebuggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartWorkflow(false);
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
            toolboxTreeView.SelectedNode = null;
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
                    case Keys.F2:
                    case Keys.F3:
                        toolboxTreeView_KeyDown(sender, e);
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
            var model = selectionModel.SelectedView;
            if (!model.CanEdit) return;

            var group = modifiers.HasFlag(WorkflowGraphView.GroupModifier);
            var branch = modifiers.HasFlag(WorkflowGraphView.BranchModifier);
            var predecessor = modifiers.HasFlag(WorkflowGraphView.PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
            var arguments = GetToolboxArguments(searchTextBox);
            var elementCategory = WorkflowGraphView.GetToolboxElementCategory(typeNode);
            model.CreateGraphNode(typeNode.Text, typeNode.Name, elementCategory, predecessor, branch, group, arguments);
        }

        void UpdateDescriptionTextBox(string name, string description, RichTextBox descriptionTextBox)
        {
            descriptionTextBox.SuspendLayout();
            descriptionTextBox.Text = name;

            var nameLength = descriptionTextBox.TextLength;
            name = descriptionTextBox.Text.Replace('\n', ' ');
            descriptionTextBox.Lines = new[] { name, description };
            if (!EditorSettings.IsRunningOnMono)
            {
                descriptionTextBox.SelectionStart = 0;
                descriptionTextBox.SelectionLength = nameLength;
                descriptionTextBox.SelectionFont = selectionFont;
                descriptionTextBox.SelectionStart = nameLength;
                descriptionTextBox.SelectionLength = descriptionTextBox.TextLength - nameLength;
                descriptionTextBox.SelectionFont = regularFont;
            }
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
                if (toolboxTreeView.Tag is TreeNode previousNode) previousNode.BackColor = Color.Empty;
                toolboxTreeView.Tag = selectedNode;
            }

            if (selectedNode == null) return;
            selectedNode.BackColor = focused ? Color.Empty : themeRenderer.ToolStripRenderer.ColorTable.InactiveCaption;
        }

        void SelectTreeViewSubjectNode(string subjectName)
        {
            var subjectCategory = toolboxCategories[SubjectCategoryName];
            var subjectNode = subjectCategory.Nodes.Find(subjectName, false);
            if (subjectNode.Length > 0)
            {
                toolboxTreeView.SelectedNode = subjectNode[0];
            }
        }

        void FindNextTypeMatch(TreeNode typeNode, bool findPrevious)
        {
            var currentNode = selectionModel.SelectedNodes.FirstOrDefault();
            var elementCategory = WorkflowGraphView.GetToolboxElementCategory(typeNode);
            Func<ExpressionBuilder, bool> predicate = elementCategory switch
            {
                ~ElementCategory.Workflow => builder => builder.MatchIncludeWorkflow(typeNode.Name),
                ~ElementCategory.Source => builder => builder.MatchSubjectReference(typeNode.Name),
                _ => builder => builder.MatchElementType(typeNode.Name),
            };

            FindNextMatch(predicate, currentNode?.Value, findPrevious);
        }

        void FindNextGraphNode(bool findPrevious)
        {
            var model = selectionModel.SelectedView;
            if (!model.GraphView.Focused) return;

            var selection = selectionModel.SelectedNodes.ToArray();
            if (selection.Length == 0) return;

            Func<ExpressionBuilder, bool> predicate;
            var currentBuilder = ExpressionBuilder.Unwrap(selection[0].Value);
            if (currentBuilder is SubjectExpressionBuilder ||
                currentBuilder is SubscribeSubject ||
                currentBuilder is MulticastSubject)
            {
                var subjectName = ((INamedElement)currentBuilder).Name;
                predicate = builder => builder.MatchSubjectReference(subjectName);
            }
            else if (currentBuilder is IncludeWorkflowBuilder includeBuilder)
            {
                predicate = builder => builder.MatchIncludeWorkflow(includeBuilder.Path);
            }
            else
            {
                var workflowElement = ExpressionBuilder.GetWorkflowElement(currentBuilder);
                var typeName = workflowElement.GetType().AssemblyQualifiedName;
                predicate = builder => builder.MatchElementType(typeName);
            }

            FindNextMatch(predicate, currentBuilder, findPrevious);
        }

        void FindNextMatch(Func<ExpressionBuilder, bool> predicate, ExpressionBuilder current, bool findPrevious)
        {
            var match = workflowBuilder.Find(predicate, current, findPrevious);
            if (match != null)
            {
                var scope = workflowBuilder.GetExpressionScope(match);
                HighlightExpression(editorControl.WorkflowGraphView, scope);
            }
        }

        private void toolboxTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            var selectedNode = toolboxTreeView.SelectedNode;
            if (e.KeyCode == Keys.Return && selectedNode?.Tag != null)
            {
                CreateGraphNode(selectedNode, e.Modifiers);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.F3 && selectedNode?.Tag != null)
            {
                var findPrevious = e.Modifiers == Keys.Shift;
                FindNextTypeMatch(selectedNode, findPrevious);
            }

            var rename = e.KeyCode == Keys.F2;
            var goToDefinition = e.KeyCode == Keys.F12;
            if ((rename || goToDefinition) && selectedNode?.Tag != null)
            {
                var elementCategory = WorkflowGraphView.GetToolboxElementCategory(selectedNode);
                if (!selectedNode.IsEditing && elementCategory == ~ElementCategory.Source)
                {
                    var currentName = selectedNode.Name;
                    var selectedView = selectionModel.SelectedView;
                    var definition = workflowBuilder.GetSubjectDefinition(selectedView.Workflow, currentName);
                    if (definition == null || rename && definition.IsReadOnly)
                    {
                        var message = definition == null
                            ? Resources.SubjectDefinitionNotFound_Error
                            : string.Format(Resources.RenameReadOnlySubjectDefinition_Error, currentName);
                        editorSite.ShowError(message);
                        return;
                    }

                    if (rename)
                    {
                        toolboxTreeView.LabelEdit = true;
                        selectedNode.BeginEdit();
                    }
                    else
                    {
                        var scope = workflowBuilder.GetExpressionScope(definition.Subject);
                        HighlightExpression(editorControl.WorkflowGraphView, scope);
                    }
                }
            }
        }

        private void toolboxTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            e.CancelEdit = true;

            var selectedView = selectionModel.SelectedView;
            if (e.Node == null || string.IsNullOrEmpty(e.Label) || selectedView == null)
            {
                return;
            }

            var newName = e.Label;
            var currentName = e.Node.Name;
            var currentLabel = e.Node.Text;
            var definition = workflowBuilder.GetSubjectDefinition(selectedView.Workflow, currentName);
            selectedView.Editor.RenameSubject(definition, newName);
            e.Node.Name = newName;
            e.Node.Text = newName + currentLabel.Substring(currentName.Length);
            toolboxTreeView.LabelEdit = false;
            searchTextBox.Clear();
            SelectTreeViewSubjectNode(newName);
            UpdatePropertyGrid();
        }

        private void toolboxTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Node.Tag != null)
            {
                toolboxTreeView_KeyDown(sender, new KeyEventArgs(Keys.Return | ModifierKeys));
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
                            renameSubjectToolStripMenuItem.Visible = true;
                            goToDefinitionToolStripMenuItem.Visible = true;
                            replaceToolStripMenuItem.Visible = true;
                            findNextToolStripMenuItem.Visible = true;
                            findPreviousToolStripMenuItem.Visible = true;
                        }
                        else
                        {
                            var allowGroup = elementCategories.Contains(ElementCategory.Nested);
                            var allowGenericSource = elementCategories.Contains(~ElementCategory.Combinator);
                            var allowSuccessor = selectedNode.Name != typeof(ExternalizedMappingBuilder).AssemblyQualifiedName;
                            var selection = allowGenericSource ? selectionModel.SelectedNodes.ToArray() : null;
                            var selectionType = selection?.Length == 1 && selection?[0].Value is InspectBuilder inspectBuilder ? inspectBuilder.ObservableType : null;
                            createGroupToolStripMenuItem.Text = selectionType != null
                                ? string.Format("{0} ({1})", Resources.CreateSourceMenuItemLabel, TypeHelper.GetTypeName(selectionType))
                                : Resources.CreateGroupAction;
                            insertAfterToolStripMenuItem.Visible = createBranchToolStripMenuItem.Visible = allowSuccessor;
                            createGroupToolStripMenuItem.Visible = allowGroup || selectionType != null;
                            subscribeSubjectToolStripMenuItem.Visible = false;
                            multicastSubjectToolStripMenuItem.Visible = false;
                            renameSubjectToolStripMenuItem.Visible = false;
                            goToDefinitionToolStripMenuItem.Visible = false;
                            insertBeforeToolStripMenuItem.Visible = true;
                            toolboxDocsToolStripMenuItem.Visible = true;
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

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolboxTreeView_KeyDown(sender, new KeyEventArgs(Keys.Control | Keys.Alt | Keys.Return));
        }

        private void renameSubjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ModifierKeys != Keys.None)
            {
                return;
            }

            if (!toolboxTreeView.Focused)
            {
                var model = selectionModel.SelectedView;
                if (!model.GraphView.Focused) return;

                var selection = selectionModel.SelectedNodes.ToArray();
                var selectedBuilder = selection.Length == 1 && selection[0].Value is InspectBuilder inspectBuilder ? inspectBuilder.Builder : null;
                if (selectedBuilder is SubjectExpressionBuilder ||
                    selectedBuilder is SubscribeSubject ||
                    selectedBuilder is MulticastSubject)
                {
                    var subjectName = ((INamedElement)selectedBuilder).Name;
                    SelectTreeViewSubjectNode(subjectName);
                }
            }

            toolboxTreeView_KeyDown(sender, new KeyEventArgs(renameSubjectToolStripMenuItem.ShortcutKeys));
        }

        private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolboxTreeView.Focused || searchTextBox.Focused)
            {
                toolboxTreeView_KeyDown(sender, new KeyEventArgs(findNextToolStripMenuItem.ShortcutKeys));
            }
            else FindNextGraphNode(findPrevious: false);
        }

        private void findPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolboxTreeView.Focused || searchTextBox.Focused)
            {
                toolboxTreeView_KeyDown(sender, new KeyEventArgs(findPreviousToolStripMenuItem.ShortcutKeys));
            }
            else FindNextGraphNode(findPrevious: true);
        }

        private void goToDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolboxTreeView_KeyDown(sender, new KeyEventArgs(goToDefinitionToolStripMenuItem.ShortcutKeys));
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
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.PasteFromClipboard();
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
                model.Editor.GroupGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.Editor.UngroupGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.Editor.EnableGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.Editor.DisableGraphNodes(selectionModel.SelectedNodes);
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

        static bool TryGetAssemblyResource(string path, out string assemblyName, out string resourceName)
        {
            if (!string.IsNullOrEmpty(path))
            {
                const char AssemblySeparator = ':';
                var separatorIndex = path.IndexOf(AssemblySeparator);
                if (separatorIndex >= 0 && !Path.IsPathRooted(path) && path.EndsWith(BonsaiExtension))
                {
                    path = Path.ChangeExtension(path, null);
                    var nameElements = path.Split(new[] { AssemblySeparator }, 2);
                    if (!string.IsNullOrEmpty(nameElements[0]))
                    {
                        assemblyName = nameElements[0];
                        resourceName = string.Join(ExpressionHelper.MemberSeparator, nameElements);
                        return true;
                    }
                }
            }

            assemblyName = default;
            resourceName = default;
            return false;
        }

        private async Task OpenDocumentationAsync(ExpressionBuilder builder)
        {
            var selectedElement = ExpressionBuilder.GetWorkflowElement(builder);
            if (selectedElement is ICustomTypeDescriptor typeDescriptor &&
                typeDescriptor.GetPropertyOwner(null) is object selectedOperator)
            {
                selectedElement = selectedOperator;
            }

            if (selectedElement is IncludeWorkflowBuilder include &&
                TryGetAssemblyResource(include.Path, out string assemblyName, out string resourceName))
            {
                await OpenDocumentationAsync(assemblyName, resourceName);
            }
            else await OpenDocumentationAsync(selectedElement.GetType());
        }

        private async Task OpenDocumentationAsync(Type type)
        {
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                type = type.GetGenericTypeDefinition();
            }

            var uid = type.FullName;
            var assemblyName = type.Assembly.GetName().Name;
            await OpenDocumentationAsync(assemblyName, uid);
        }

        private async Task OpenDocumentationAsync(string assemblyName, string uid)
        {
            if (documentationProvider == null)
            {
                EditorDialog.ShowDocs();
                return;
            }

            try
            {
                var showExternal = ModifierKeys.HasFlag(Keys.Control);
                var editorControl = selectionModel.SelectedView.EditorControl;
                var url = await documentationProvider.GetDocumentationAsync(assemblyName, uid);
                if (!showExternal && editorControl.AnnotationPanel.HasWebView)
                {
                    editorControl.AnnotationPanel.Navigate(url.AbsoluteUri);
                    var nameSeparator = uid.LastIndexOf(ExpressionHelper.MemberSeparator);
                    if (nameSeparator >= 0)
                    {
                        var name = uid.Substring(nameSeparator + 1);
                        var categoryName = GetPackageDisplayName(uid.Substring(0, nameSeparator));
                        editorControl.ExpandAnnotationPanel(label: $"{name} ({categoryName})");
                    }
                    else editorControl.ExpandAnnotationPanel(label: uid == EditorUid ? Resources.Editor_HelpLabel : uid);
                }
                else EditorDialog.OpenUrl(url);
            }
            catch (ArgumentException ex) when (ex.ParamName == nameof(assemblyName))
            {
                var message = $"Documentation for the module {assemblyName} is not available.";
                editorSite.ShowError(ex, message);
            }
            catch (KeyNotFoundException ex)
            {
                var message = $"The specified operator {uid} was not found in the documentation for {assemblyName}.";
                editorSite.ShowError(ex, message);
            }
            catch (Exception ex) when (ex is WebException or NotSupportedException or DocumentationException)
            {
                var message = $"The documentation for the module {assemblyName} is not available. {{0}}";
                editorSite.ShowError(ex, message);
            }
            catch (Exception ex)
            {
                editorSite.ShowError(ex);
            }
        }

        private async void docsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ModifierKeys != Keys.None && ModifierKeys != Keys.Control)
            {
                return;
            }

            if (toolboxTreeView.Focused || searchTextBox.Focused)
            {
                var typeNode = toolboxTreeView.SelectedNode;
                if (typeNode != null && typeNode.Tag != null)
                {
                    var elementCategory = WorkflowGraphView.GetToolboxElementCategory(typeNode);
                    if (elementCategory == ~ElementCategory.Workflow &&
                        TryGetAssemblyResource(typeNode.Name, out string assemblyName, out string resourceName))
                    {
                        await OpenDocumentationAsync(assemblyName, resourceName);
                        return;
                    }

                    var type = Type.GetType(typeNode.Name);
                    if (type != null)
                    {
                        await OpenDocumentationAsync(type);
                    }
                    return;
                }
            }

            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                var selectedNode = selectionModel.SelectedNodes.FirstOrDefault();
                if (selectedNode != null)
                {
                    await OpenDocumentationAsync(selectedNode.Value);
                    return;
                }
            }

            var editorAssemblyName = GetType().Assembly.GetName().Name;
            await OpenDocumentationAsync(editorAssemblyName, EditorUid);
        }

        private void forumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorDialog.ShowForum();
        }

        private void reportBugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorDialog.ShowReportBug();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorDialog.ShowAboutBox();
        }

        private void welcomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = EditorDialog.ShowStartScreen(out string fileName);
            if (result != EditorResult.Exit && CloseWorkflow())
            {
                if (result == EditorResult.ReloadEditor)
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        ClearWorkflow();
                    }
                    else OpenWorkflow(fileName);
                }
                else
                {
                    EditorResult = result;
                    Close();
                }
            }
        }

        #endregion

        #region EditorSite Class

        class EditorSite : ISite, IWorkflowEditorService, IWorkflowEditorState, IWorkflowToolboxService, IUIService, IDefinitionProvider
        {
            readonly EditorForm siteForm;

            public EditorSite(EditorForm form)
            {
                siteForm = form;
                Styles = new System.Collections.Hashtable();
                Styles["DialogFont"] = siteForm.Font;
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

                if (serviceType == typeof(TypeVisualizerMap))
                {
                    return siteForm.typeVisualizers;
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
                    serviceType == typeof(IUIService) ||
                    serviceType == typeof(IDefinitionProvider))
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
                HandleMenuItemShortcutKeys(e, siteForm.startWithoutDebuggingToolStripMenuItem, siteForm.startWithoutDebuggingToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.startToolStripButtonMenuItem, siteForm.startToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.restartToolStripMenuItem, siteForm.restartToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.stopToolStripMenuItem, siteForm.stopToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.renameSubjectToolStripMenuItem, siteForm.renameSubjectToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.findNextToolStripMenuItem, siteForm.findNextToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.findPreviousToolStripMenuItem, siteForm.findPreviousToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.docsToolStripMenuItem, siteForm.docsToolStripMenuItem_Click);
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
                if (!siteForm.extensionsPath.Exists)
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
                var workflow = ElementStore.LoadWorkflow(fileName, out SemanticVersion version);
                return siteForm.PrepareWorkflow(workflow, version, out _);
            }

            public void OpenWorkflow(string fileName)
            {
                siteForm.OpenWorkflow(fileName);
            }

            public string GetPackageDisplayName(string packageKey)
            {
                return EditorForm.GetPackageDisplayName(packageKey);
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
                WorkflowStarted?.Invoke(this, e);
            }

            public void OnWorkflowStopped(EventArgs e)
            {
                WorkflowStopped?.Invoke(this, e);
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

            public bool HasDefinition(object component)
            {
                return siteForm.scriptEnvironment != null;
            }

            public void ShowDefinition(object component)
            {
                if (component is INamedElement namedElement &&
                   (namedElement is SubscribeSubject || namedElement is MulticastSubject))
                {
                    var model = siteForm.selectionModel.SelectedView ?? siteForm.editorControl.WorkflowGraphView;
                    var definition = siteForm.workflowBuilder.GetSubjectDefinition(model.Workflow, namedElement.Name);
                    if (definition != null)
                    {
                        var scope = siteForm.workflowBuilder.GetExpressionScope(definition.Subject);
                        siteForm.HighlightExpression(siteForm.editorControl.WorkflowGraphView, scope);
                        return;
                    }
                }

                Type componentType;
                if (siteForm.scriptEnvironment == null) return;
                if (siteForm.scriptEnvironment.AssemblyName != null &&
                    siteForm.scriptEnvironment.AssemblyName.FullName == (componentType = component.GetType()).Assembly.FullName)
                {
                    var scriptFile = Path.Combine(siteForm.extensionsPath.FullName, componentType.Name);
                    ScriptEditorLauncher.Launch(siteForm, siteForm.scriptEnvironment.ProjectFileName, scriptFile);
                }
                else
                {
                    string source, extension;
                    var type = component.GetType();
                    if (type.IsGenericType) type = type.GetGenericTypeDefinition();
                    var typeDefinition = TypeDefinitionProvider.GetTypeDefinition(type);
                    using (var provider = new TypeDefinitionCodeProvider())
                    using (var writer = new StringWriter())
                    {
                        provider.GenerateCodeFromCompileUnit(typeDefinition, writer, null);
                        source = writer.ToString();
                        extension = provider.FileExtension;
                    }

                    var directory = Directory.CreateDirectory(Path.Combine(siteForm.definitionsPath, DefinitionsDirectory));
                    var sourceFile = Path.Combine(directory.FullName, type.FullName + "." + extension);
                    File.WriteAllText(sourceFile, source);
                    ScriptEditorLauncher.Launch(siteForm, siteForm.scriptEnvironment.ProjectFileName, sourceFile);
                }
            }

            public bool CanShowComponentEditor(object component)
            {
                var editor = TypeDescriptor.GetEditor(component, typeof(ComponentEditor));
                if (editor != null) return true;
                return false;
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
                    if (editor is WindowsFormsComponentEditor windowsFormsEditor)
                    {
                        return windowsFormsEditor.EditComponent(component, parent);
                    }

                    if (editor is WorkflowComponentEditor workflowEditor)
                    {
                        return workflowEditor.EditComponent(component, this, parent);
                    }

                    return editor.EditComponent(component);
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

            public System.Collections.IDictionary Styles { get; }
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

        private void propertyGrid_Refreshed(object sender, EventArgs e)
        {
            var selectedObjects = propertyGrid.SelectedObjects;
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                var selectedView = selectionModel.SelectedView;
                if (selectedObjects.Length > 1 || selectedObjects[0] != selectedView.Workflow)
                {
                    GetSelectionDescription(selectedObjects, out string displayName, out string description);
                    UpdateDescriptionTextBox(displayName, description, propertiesDescriptionTextBox);
                }
            }
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

        private void statusCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (workflowError != null)
            {
                Clipboard.SetText(workflowError.ToString());
            }
        }

        #endregion

        #region Color Themes

        static ColorTheme InvertTheme(ColorTheme theme)
        {
            return theme == ColorTheme.Light ? ColorTheme.Dark : ColorTheme.Light;
        }

        private void InitializeBorderTheme()
        {
            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            var panelColor = colorTable.ContentPanelBackColor;
            propertyGrid.CategorySplitterColor = colorTable.SeparatorDark;
            propertyGrid.CommandsBorderColor = panelColor;
            propertyGrid.ViewBorderColor = panelColor;
            propertyGrid.CanShowVisualStyleGlyphs = false;
        }

        private void InitializeTheme()
        {
            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            var panelColor = colorTable.ContentPanelBackColor;
            var windowText = colorTable.WindowText;
            ForeColor = colorTable.ControlForeColor;
            BackColor = colorTable.ControlBackColor;
            propertiesSplitContainer.BackColor = panelColor;
            propertiesLabel.BackColor = colorTable.SeparatorDark;
            propertiesLabel.ForeColor = ForeColor;
            propertyGrid.BackColor = panelColor;
            propertyGrid.LineColor = colorTable.SeparatorDark;
            propertyGrid.CommandsBackColor = panelColor;
            propertyGrid.HelpBackColor = panelColor;
            propertyGrid.HelpForeColor = ForeColor;
            propertyGrid.ViewBackColor = panelColor;
            propertyGrid.ViewForeColor = windowText;
            if (!EditorSettings.IsRunningOnMono)
            {
                propertyGrid.CategoryForeColor = ForeColor;
                InitializeBorderTheme();
            }

            toolboxTableLayoutPanel.BackColor = panelColor;
            toolboxSplitContainer.BackColor = panelColor;
            toolboxLabel.BackColor = colorTable.SeparatorDark;
            toolboxLabel.ForeColor = ForeColor;
            toolboxTreeView.BackColor = panelColor;
            toolboxTreeView.ForeColor = windowText;
            toolboxDescriptionTextBox.BackColor = panelColor;
            toolboxDescriptionTextBox.ForeColor = ForeColor;
            propertiesDescriptionTextBox.BackColor = panelColor;
            propertiesDescriptionTextBox.ForeColor = ForeColor;
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
