﻿using System;
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
using System.Reactive.Threading.Tasks;
using Bonsai.Editor.Scripting;
using Bonsai.Editor.Themes;
using Bonsai.Editor.GraphView;
using Bonsai.Editor.GraphModel;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace Bonsai.Editor
{
    public partial class EditorForm : Form
    {
        const float DefaultEditorScale = 1.0f;
        const string EditorUid = "editor";
        const string WorkflowCategoryName = "Workflow";
        const string SubjectCategoryName = "Subject";
        static readonly AttributeCollection DesignTimeAttributes = new AttributeCollection(BrowsableAttribute.Yes, DesignTimeVisibleAttribute.Yes);
        static readonly AttributeCollection RuntimeAttributes = AttributeCollection.FromExisting(DesignTimeAttributes, DesignOnlyAttribute.No);
        static readonly char[] ToolboxArgumentSeparator = new[] { ' ' };
        static readonly object ExtensionsDirectoryChanged = new object();
        static readonly object WorkflowValidating = new object();
        static readonly object WorkflowValidated = new object();

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
        readonly VisualizerLayoutMap visualizerSettings;
        readonly List<WorkflowElementDescriptor> workflowElements;
        readonly List<WorkflowElementDescriptor> workflowExtensions;
        readonly WorkflowRuntimeExceptionCache exceptionCache;
        readonly CancellationTokenSource formCancellation;
        readonly string definitionsPath;
        AttributeCollection browsableAttributes;
        DirectoryInfo extensionsPath;
        WorkflowBuilder workflowBuilder;
        VisualizerWindowMap visualizerWindows;
        WorkflowException workflowError;
        Task initialization;
        IDisposable building;
        IDisposable running;
        bool requireValidation;
        bool debugging;
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
            formCancellation = new CancellationTokenSource();
            statusTextLabel = new ToolStripStatusLabel();
            statusTextLabel.Spring = true;
            statusTextLabel.Text = Resources.ReadyStatus;
            formScheduler = new FormScheduler(this);
            themeRenderer = new ThemeRenderer();
            themeRenderer.LabelHeight = searchTextBox.Height;
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
            visualizerSettings = new VisualizerLayoutMap(typeVisualizers);
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

            definitionsPath = Project.GetDefinitionsTempPath();
            editorControl = new WorkflowEditorControl(editorSite);
            editorControl.Enter += new EventHandler(editorControl_Enter);
            editorControl.Dock = DockStyle.Fill;
            workflowSplitContainer.Panel1.Controls.Add(editorControl);
            propertyGrid.BrowsableAttributes = browsableAttributes = DesignTimeAttributes;
            propertyGrid.LineColor = SystemColors.InactiveBorder;
            propertyGrid.Site = editorSite;

            editorControl.ActiveContentChanged += editorControl_ActiveContentChanged;
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

            UpdateTitle();
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
            explorerSplitContainer.SplitterDistance = (int)Math.Round(
                EditorSettings.Instance.ExplorerSplitterDistance * scaleFactor.Width);
            var toolboxBottomMargin = toolboxSplitContainer.Margin.Bottom;
            toolboxSplitContainer.SplitterDistance = toolboxSplitContainer.Height - propertiesSplitContainer.SplitterDistance - toolboxBottomMargin;
        }

        void CloseEditorForm()
        {
            Application.RemoveMessageFilter(hotKeys);
            EditorSettings.Instance.AnnotationPanelSize = (int)Math.Round(
                editorControl.AnnotationPanelSize * inverseScaleFactor.Width);
            EditorSettings.Instance.ExplorerSplitterDistance = (int)Math.Round(
                explorerSplitContainer.SplitterDistance * inverseScaleFactor.Width);
            var desktopBounds = WindowState != FormWindowState.Normal ? RestoreBounds : Bounds;
            EditorSettings.Instance.DesktopBounds = ScaleBounds(desktopBounds, inverseScaleFactor);
            if (WindowState == FormWindowState.Minimized)
            {
                EditorSettings.Instance.WindowState = FormWindowState.Normal;
            }
            else EditorSettings.Instance.WindowState = WindowState;
            EditorSettings.Instance.Save();
            formCancellation.Cancel();
        }

        IObservable<Unit> HandleUpdatesAvailableAsync()
        {
            return updatesAvailable
                .ObserveOn(formScheduler)
                .Do(value =>
                {
                    if (value) toolStrip.Items.Add(statusUpdateAvailableLabel);
                    else toolStrip.Items.Remove(statusUpdateAvailableLabel);
                })
                .IgnoreElements()
                .Select(xs => Unit.Default);
        }

        protected override async void OnLoad(EventArgs e)
        {
            RestoreEditorSettings();
            var initialFileName = FileName;
            var validFileName =
                !string.IsNullOrEmpty(initialFileName) &&
                Path.GetExtension(initialFileName) == Project.BonsaiExtension &&
                File.Exists(initialFileName);

            Observable.Merge(
                InitializeSubjectSourcesAsync(),
                InitializeWorkflowFileWatcherAsync(),
                InitializeWorkflowExplorerWatcherAsync(),
                HandleUpdatesAvailableAsync())
                .Subscribe(formCancellation.Token);

            var currentDirectory = Project.GetCurrentBaseDirectory(out bool currentDirectoryRestricted);
            var workflowBaseDirectory = validFileName ? Project.GetWorkflowBaseDirectory(initialFileName) : currentDirectory;
            if (currentDirectoryRestricted)
            {
                currentDirectory = workflowBaseDirectory;
                Environment.CurrentDirectory = currentDirectory;
            }

            directoryToolStripItem.Text = currentDirectory;
            openWorkflowDialog.InitialDirectory = saveWorkflowDialog.InitialDirectory = currentDirectory;
            extensionsPath = new DirectoryInfo(Path.Combine(workflowBaseDirectory, Project.ExtensionsDirectory));
            if (extensionsPath.Exists) OnExtensionsDirectoryChanged(EventArgs.Empty);

            InitializeEditorToolboxTypes();
            initialization = InitializeEditorExtensionsAsync(formCancellation.Token);
            base.OnLoad(e);

            await InitializeWorkflowAsync(validFileName ? initialFileName : default);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            scaleFactor = factor;
            inverseScaleFactor = new SizeF(1f / factor.Width, 1f / factor.Height);

#if NETFRAMEWORK
            var workflowSplitterScale = EditorSettings.IsRunningOnMono ? 0.5f / factor.Width : 1.0f;
            panelSplitContainer.SplitterDistance = (int)(panelSplitContainer.SplitterDistance * factor.Height);
            workflowSplitContainer.SplitterDistance = (int)(workflowSplitContainer.SplitterDistance * workflowSplitterScale * factor.Height);
            propertiesSplitContainer.SplitterDistance = (int)(propertiesSplitContainer.SplitterDistance * factor.Height);
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
#else
            const float PropertiesSplitterScale = 0.4f; // correct for overshoot rescaling in .NET core
            propertiesSplitContainer.SplitterDistance = (int)(propertiesSplitContainer.SplitterDistance * PropertiesSplitterScale * factor.Height);
#endif
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

        IObservable<Unit> InitializeSubjectSourcesAsync()
        {
            var selectedPathChanged = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => selectionModel.SelectionChanged += handler,
                handler => selectionModel.SelectionChanged -= handler)
                .Select(evt => selectionModel.SelectedView)
                .DistinctUntilChanged(view => view?.WorkflowPath);
            var workflowValidated = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => Events.AddHandler(WorkflowValidated, handler),
                handler => Events.RemoveHandler(WorkflowValidated, handler))
                .Select(evt => selectionModel.SelectedView);
            return Observable
                .Merge(selectedPathChanged, workflowValidated)
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

        IObservable<Unit> InitializeWorkflowExplorerWatcherAsync()
        {
            var selectedViewChanged = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => selectionModel.SelectionChanged += handler,
                handler => selectionModel.SelectionChanged -= handler)
                .Select(evt => selectionModel.SelectedView?.WorkflowPath)
                .DistinctUntilChanged()
                .Do(explorerTreeView.SelectNode)
                .IgnoreElements()
                .Select(xs => Unit.Default);

            var workflowValidated = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => Events.AddHandler(WorkflowValidated, handler),
                handler => Events.RemoveHandler(WorkflowValidated, handler))
                .Select(evt => selectionModel.SelectedView)
                .Merge(Observable.Return(selectionModel.SelectedView));
            return Observable.Merge(selectedViewChanged, workflowValidated.Do(view =>
            {
                if (workflowBuilder.Workflow == null)
                    return;

                editorControl.UpdateFindResults();
                explorerTreeView.UpdateWorkflow(
                    GetProjectDisplayName(),
                    workflowBuilder);
            })
            .IgnoreElements()
            .Select(xs => Unit.Default));
        }

        IObservable<Unit> InitializeWorkflowFileWatcherAsync()
        {
            var extensionsDirectoryChanged = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => Events.AddHandler(ExtensionsDirectoryChanged, handler),
                handler => Events.RemoveHandler(ExtensionsDirectoryChanged, handler));
            return extensionsDirectoryChanged
                .Select(evt => Observable.Defer(RefreshWorkflowExtensions))
                .Switch();
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
                    .Concat(Project.EnumerateExtensionWorkflows(extensionsPath.FullName))
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

        Task InitializeTypeVisualizersAsync(CancellationToken cancellationToken)
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
                .ToTask(cancellationToken);
        }

        Task InitializeToolboxAsync(CancellationToken cancellationToken)
        {
            return toolboxElements
                .ObserveOn(formScheduler)
                .Do(package => InitializeToolboxCategory(
                    package.Key,
                    InitializeWorkflowExtensions(package)))
                .SubscribeOn(Scheduler.Default)
                .ToTask(cancellationToken);
        }

        async Task InitializeEditorExtensionsAsync(CancellationToken cancellationToken)
        {
            using var shutdown = ShutdownSequence();
            await Task.WhenAll(
                InitializeToolboxAsync(cancellationToken),
                InitializeTypeVisualizersAsync(cancellationToken));
        }

        async Task InitializeWorkflowAsync(string initialFileName)
        {
            if (!string.IsNullOrEmpty(initialFileName) && await OpenWorkflowAsync(initialFileName, false))
            {
                foreach (var assignment in propertyAssignments)
                {
                    try
                    {
                        workflowBuilder.Workflow.SetWorkflowProperty(assignment.Key, assignment.Value);
                    }
                    catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or FormatException)
                    {
                        var icon = MessageBoxIcon.Error;
                        var caption = Resources.OpenWorkflow_Error_Caption;
                        var innerMessage = ex is FormatException fex && fex.InnerException is not null
                            ? $"{ex.Message} {ex.InnerException.Message}"
                            : ex.Message;
                        var message = string.Format(Resources.PropertyAssignment_Error, assignment.Key, innerMessage);
                        if (MessageBox.Show(this, message, caption, MessageBoxButtons.YesNo, icon, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            continue;
                        else
                            return;
                    }
                }

                var loadAction = LoadAction;
                if (loadAction != LoadAction.None)
                {
                    var debugging = loadAction == LoadAction.Start;
                    StartWorkflow(debugging);
                }
            }
            else ClearWorkflow();
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

        static string GetElementTypeDisplayName(ExpressionBuilder builder)
        {
            var matchTarget = ExpressionBuilder.Unwrap(builder);
            if (matchTarget is SubjectExpressionBuilder ||
                matchTarget is SubscribeSubject ||
                matchTarget is MulticastSubject)
            {
                var subjectName = ((INamedElement)matchTarget).Name;
                return $"{subjectName} ({SubjectCategoryName})";
            }
            else if (matchTarget is IncludeWorkflowBuilder includeBuilder && !string.IsNullOrEmpty(includeBuilder.Path))
            {
                var includeNamespace = Project.DefaultWorkflowNamespace;
                if (TryGetAssemblyResource(includeBuilder.Path, out string _, out string resourceName))
                {
                    var nameSeparator = resourceName.LastIndexOf(ExpressionHelper.MemberSeparator);
                    includeNamespace = nameSeparator >= 0 ? resourceName.Substring(0, nameSeparator) : resourceName;
                }
                else
                {
                    var fullPath = Path.GetFullPath(includeBuilder.Path);
                    var relativePath = PathConvert.GetProjectPath(fullPath);
                    if (relativePath != fullPath)
                        includeNamespace = Project.GetFileNamespace(relativePath);
                }
                return $"{includeBuilder.Name} ({includeNamespace})";
            }
            else
            {
                var elementType = ExpressionBuilder.GetWorkflowElement(matchTarget).GetType();
                return $"{elementType.Name} ({GetPackageDisplayName(elementType.Namespace)})";
            }
        }

        static string GetPackageDisplayName(string packageKey)
        {
            const string BonsaiPackageName = "Bonsai";
            const string BonsaiNamespacePrefix = BonsaiPackageName + ".";
            if (string.IsNullOrEmpty(packageKey))
                return Project.ExtensionsDirectory;
            if (packageKey == BonsaiPackageName || !packageKey.StartsWith(BonsaiNamespacePrefix))
                return packageKey;
            else
                return packageKey.Substring(BonsaiNamespacePrefix.Length);
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

        bool EnsureWorkflowFile()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                var result = MessageBox.Show(
                    this,
                    Resources.EnsureSavedWorkflow_Question,
                    Resources.UnsavedWorkflow_Question_Caption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
                if (result != DialogResult.Yes) return false;
                saveAsToolStripMenuItem_Click(this, EventArgs.Empty);
                return !string.IsNullOrEmpty(FileName);
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
                else if (result == DialogResult.Cancel)
                    return false;
            }

            if (!string.IsNullOrEmpty(FileName))
                SaveWorkflowSettings(FileName);
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
            if (editorControl.AnnotationPanel.Tag is ExpressionBuilder)
                editorControl.ClearAnnotationPanel();
            editorControl.CloseAll();
            editorControl.InitializeEditorLayout();
            editorSite.ValidateWorkflow();
            visualizerSettings.Clear();
            ResetProjectStatus();
            UpdateTitle();
        }

        Task<bool> OpenWorkflowAsync(string fileName)
        {
            return OpenWorkflowAsync(fileName, true);
        }

        async Task<bool> OpenWorkflowAsync(string fileName, bool setWorkingDirectory)
        {
            WorkflowBuilder builderCandidate;
            SemanticVersion workflowVersion;
            try { builderCandidate = ElementStore.LoadWorkflow(fileName, out workflowVersion); }
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

            workflowBuilder = PrepareWorkflow(builderCandidate, workflowVersion, out bool upgraded);
            ClearWorkflowError();
            FileName = fileName;

            editorControl.CloseAll();
            editorSite.ValidateWorkflow();
            var settingsDirectory = Project.GetWorkflowSettingsDirectory(fileName);
            var editorPath = Project.GetEditorSettingsPath(settingsDirectory, fileName);
            editorControl.InitializeEditorLayout(editorPath);

            visualizerSettings.Clear();
            var layoutPath = LayoutHelper.GetCompatibleLayoutPath(settingsDirectory, fileName);
            if (File.Exists(layoutPath))
            {
                try
                {
                    var visualizerLayout = VisualizerLayout.Load(layoutPath);
                    await initialization;
                    visualizerSettings.SetVisualizerLayout(workflowBuilder, visualizerLayout);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    var icon = MessageBoxIcon.Error;
                    var caption = Resources.VisualizerLayout_Caption;
                    var message = string.Format(Resources.VisualizerLayoutCorrupt_Question, ex.Message);
                    if (MessageBox.Show(this, message, caption, MessageBoxButtons.YesNo, icon) == DialogResult.Yes)
                    {
                        File.Delete(layoutPath);
                    }
                }
            }

            ResetProjectStatus();
            if (upgraded)
            {
                MessageBox.Show(
                    this,
                    Resources.UpgradeWorkflow_Warning,
                    Resources.UpgradeWorkflow_Warning_Caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                FileName = null;
                version++;
            }

            UpdateTitle();
            return true;
        }

        bool SaveElement(XmlSerializer serializer, string fileName, object o, string error, XmlSerializerNamespaces namespaces = null)
        {
            try
            {
                ElementStore.SaveElement(serializer, fileName, o, namespaces);
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

            SaveWorkflowSettings(fileName);
            UpdateWorkflowDirectory(fileName);
            UpdateTitle();
            return true;
        }

        bool SaveWorkflowBuilder(string fileName, WorkflowBuilder workflowBuilder)
        {
            return SaveElement(WorkflowBuilder.Serializer, fileName, workflowBuilder, Resources.SaveWorkflow_Error);
        }

        void SaveWorkflowSettings(string fileName)
        {
            var settingsDirectory = Project.GetWorkflowSettingsDirectory(fileName);
            Directory.CreateDirectory(settingsDirectory);

            var editorPath = Project.GetEditorSettingsPath(settingsDirectory, fileName);
            editorControl.SaveEditorLayout(editorPath);

            var visualizerLayout = visualizerSettings.GetVisualizerLayout(workflowBuilder);
            if (visualizerLayout != null)
            {
                var layoutPath = Project.GetLayoutSettingsPath(settingsDirectory, fileName);
                SaveVisualizerLayout(layoutPath, visualizerLayout);
#pragma warning disable CS0612 // Support for deprecated layout config files
                var legacyLayoutPath = new FileInfo(Project.GetLegacyLayoutSettingsPath(fileName));
                if (legacyLayoutPath.Exists)
                {
                    legacyLayoutPath.Delete();
                }
#pragma warning restore CS0612 // Support for deprecated layout config files
            }
        }

        void SaveVisualizerLayout(string fileName, VisualizerLayout layout)
        {
            SaveElement(VisualizerLayout.Serializer, fileName, layout, Resources.SaveLayout_Error, ElementStore.EmptyNamespaces);
        }

        void SaveWorkflowExtension(WorkflowGraphView model, string fileName, GraphNode node)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var groupNode = node;
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

        void OnWorkflowValidated(EventArgs e)
        {
            (Events[WorkflowValidated] as EventHandler)?.Invoke(this, e);
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
                EditorResult = EditorResult.ReloadEditor;
                selectionModel.UpdateSelection(null);
                ResetProjectStatus();
                Close();
                FileName = fileName;
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
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

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

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CloseWorkflow()) return;

            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                await OpenWorkflowAsync(openWorkflowDialog.FileName);
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

            var model = selectionModel.SelectedView;
            if (model is null) return;

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

            SaveWorkflowExtension(model, fileName, groupNode);
        }

        private void exportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.Workflow.Count > 0)
            {
                exportImageDialog.FileName = Path.GetFileNameWithoutExtension(FileName);
                if (exportImageDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportImage(model, exportImageDialog.FileName);
                }
            }
        }

        private void exportPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EnsureWorkflowFile())
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
            return Disposable.Create(() =>
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
                if (visualizerWindows != null)
                {
                    visualizerSettings.Update(visualizerWindows);
                    visualizerWindows = null;
                }
                UpdateTitle();
            });
        }

        void StartWorkflow(bool debug)
        {
            if (running == null)
            {
                debugging = debug;
                ClearWorkflowError();
                building = ShutdownSequence();
                visualizerWindows = visualizerSettings.CreateVisualizerWindows(workflowBuilder);
                LayoutHelper.SetWorkflowNotifications(workflowBuilder.Workflow, debug);
                if (!debug)
                {
                    LayoutHelper.SetLayoutNotifications(workflowBuilder.Workflow, visualizerWindows);
                }

                running = Observable.Using(
                    () =>
                    {
                        var runtimeWorkflow = workflowBuilder.Workflow.BuildObservable();
                        Invoke(() =>
                        {
                            statusTextLabel.Text = Resources.RunningStatus;
                            statusImageLabel.Image = statusRunningImage;
                            visualizerWindows.Show(visualizerSettings, editorSite, this);
                            editorSite.OnWorkflowStarted(EventArgs.Empty);
                            Activate();
                        });
                        return new WorkflowDisposable(runtimeWorkflow, HandleWorkflowDispose);
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
                ClearExceptionBuilderNode(workflowError);
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

        void ClearExceptionBuilderNode(WorkflowException ex)
        {
            var workflowPath = WorkflowEditorPath.GetExceptionPath(workflowBuilder, ex);
            editorControl.ClearGraphNode(workflowPath);

            statusStrip.ContextMenuStrip = null;
            statusTextLabel.Text = Resources.ReadyStatus;
            statusImageLabel.Image = Resources.StatusReadyImage;
            explorerTreeView.SetNodeStatus(ExplorerNodeStatus.Ready);
        }

        void HighlightExceptionBuilderNode(WorkflowException ex, bool showMessageBox)
        {
            var workflowPath = WorkflowEditorPath.GetExceptionPath(workflowBuilder, ex);
            var pathElements = workflowPath.GetPathElements();
            var selectedView = selectionModel.SelectedView;
            editorControl.HighlightGraphNode(selectedView, workflowPath, showMessageBox);

            var buildException = ex is WorkflowBuildException;
            statusTextLabel.Text = ex.Message;
            statusStrip.ContextMenuStrip = statusContextMenuStrip;
            statusImageLabel.Image = buildException ? Resources.StatusBlockedImage : Resources.StatusCriticalImage;
            explorerTreeView.SetNodeStatus(pathElements, ExplorerNodeStatus.Blocked);
            if (showMessageBox)
            {
                var errorCaption = buildException ? Resources.BuildError_Caption : Resources.RuntimeError_Caption;
                editorSite.ShowError(ex.Message, errorCaption);
            }
        }

        void HandleWorkflowDispose()
        {
            if (InvokeRequired)
                BeginInvoke(HandleWorkflowDispose);
            else if (Interlocked.Exchange(ref building, null) is IDisposable shutdown)
            {
                shutdown.Dispose();
            }
        }

        bool HandleSchedulerError(Exception e)
        {
            HandleWorkflowError(e);
            return true;
        }

        void HandleWorkflowError(Exception e)
        {
            if (InvokeRequired)
                BeginInvoke(HandleWorkflowError, e);
            else
            {
                using var shutdown = Interlocked.Exchange(ref building, null);
                if (e is WorkflowException workflowException && workflowException.Builder != null ||
                    exceptionCache.TryGetValue(e, out workflowException))
                {
                    workflowError = workflowException;
                    HighlightExceptionBuilderNode(workflowException, showMessageBox: shutdown != null);
                }
                else editorSite.ShowError(e.Message, Name);
            };
        }

        void HandleWorkflowCompleted()
        {
            Action clearErrors = exceptionCache.Clear;
            if (InvokeRequired) BeginInvoke(clearErrors);
            else clearErrors();
        }

        void NavigateTo(WorkflowEditorPath workflowPath, NavigationPreference navigationPreference)
        {
            switch (navigationPreference)
            {
                case NavigationPreference.Current:
                    var model = selectionModel.SelectedView;
                    if (model is null)
                        goto case NavigationPreference.NewTab;
                    model.WorkflowPath = workflowPath;
                    break;
                case NavigationPreference.NewTab:
                    editorControl.CreateDockContent(workflowPath, WeifenLuo.WinFormsUI.Docking.DockState.Document);
                    break;
                case NavigationPreference.NewWindow:
                    editorControl.CreateDockContent(workflowPath, WeifenLuo.WinFormsUI.Docking.DockState.Float);
                    break;
                default:
                    break;
            }
        }

        void SelectBuilderNode(ExpressionBuilder builder, NavigationPreference navigationPreference = default)
        {
            var builderPath = WorkflowEditorPath.GetBuilderPath(workflowBuilder, builder);
            SelectBuilderNode(builderPath, navigationPreference);
        }

        void SelectBuilderNode(WorkflowEditorPath builderPath, NavigationPreference navigationPreference = default)
        {
            if (builderPath != null)
            {
                NavigateTo(builderPath.Parent, navigationPreference);

                var selectedView = selectionModel.SelectedView;
                var graphNode = selectedView.FindGraphNode(builderPath.Resolve(workflowBuilder));
                if (graphNode == null)
                {
                    throw new InvalidOperationException(Resources.ExceptionNodeNotFound_Error);
                }

                selectedView.SelectGraphNode(graphNode);
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
            title.Append(AboutBox.BuildKindTitleSuffix);
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
            if (model?.GraphView.Focused is true)
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
                editorControl.RefreshSelection(view);
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

        private void editorControl_ActiveContentChanged(object sender, EventArgs e)
        {
            if (editorControl.ActiveContent is not null && (IsActiveControl(propertyGrid) || requireValidation))
            {
                editorSite.ValidateWorkflow();
                requireValidation = false;
                RefreshSelection();
            }
        }

        void editorControl_Enter(object sender, EventArgs e)
        {
            var selectedView = selectionModel.SelectedView;
            if (selectedView != null)
            {
                var container = selectedView.EditorControl;
                if (container != null && container != editorControl && hotKeys.TabState)
                {
                    container.ParentForm.Activate();
                    var forward = ModifierKeys.HasFlag(Keys.Shift);
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

        private string GetProjectDisplayName()
        {
            return !string.IsNullOrEmpty(FileName)
                ? Path.GetFileNameWithoutExtension(FileName)
                : "Workflow";
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
            var hasSelectableObjects = selectedView?.Workflow.Count > 0;
            var hasSelectedObjects = selectedObjects.Length > 0;
            saveAsWorkflowToolStripMenuItem.Enabled = hasSelectedObjects;
            pasteToolStripMenuItem.Enabled = canEdit;
            copyToolStripMenuItem.Enabled = hasSelectedObjects;
            copyAsImageToolStripMenuItem.Enabled = hasSelectableObjects;
            selectAllToolStripMenuItem.Enabled = hasSelectableObjects;
            exportImageToolStripMenuItem.Enabled = hasSelectableObjects;
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
                if (selectedView.WorkflowPath != null)
                {
                    var builder = ExpressionBuilder.Unwrap(selectedView.WorkflowPath.Resolve(workflowBuilder));
                    displayName = ElementHelper.GetElementName(builder);
                    description = ElementHelper.GetElementDescription(builder);
                }
                else
                {
                    description = workflowBuilder.Description ?? Resources.WorkflowPropertiesDescription;
                    displayName = GetProjectDisplayName();
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
                    case Keys.F12:
                        toolboxTreeView_KeyDown(sender, e);
                        break;
                    case Keys.Return:
                        toolboxTreeView_KeyDown(sender, e);
                        if (e.Handled)
                        {
                            searchTextBox.Clear();
                            selectionModel.SelectedView?.Focus();
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

        void CreateGraphNode(WorkflowGraphView model, TreeNode typeNode, Keys modifiers)
        {
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

        void SelectTreeViewSubjectNode(string subjectName)
        {
            var subjectCategory = toolboxCategories[SubjectCategoryName];
            var subjectNode = subjectCategory.Nodes.Find(subjectName, false);
            if (subjectNode.Length > 0)
            {
                toolboxTreeView.SelectedNode = subjectNode[0];
            }
        }

        void FindAllReferences()
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is not true) return;

            var selection = selectionModel.SelectedNodes.ToArray();
            if (selection.Length != 1) return;

            var inspectBuilder = selection[0].Value;
            var matches = workflowBuilder.FindAllReferences(model.Workflow, inspectBuilder);
            var elementTypeDisplayName = GetElementTypeDisplayName(inspectBuilder);
            editorControl.ShowFindResults($"'{elementTypeDisplayName}' references", matches);
        }

        void FindAllReferences(TreeNode typeNode)
        {
            var targetWorkflow = selectionModel.SelectedView?.Workflow;
            var elementCategory = WorkflowGraphView.GetToolboxElementCategory(typeNode);
            var matches = workflowBuilder.FindAllReferences(targetWorkflow, elementCategory, typeNode.Name);
            var displayName = typeNode.Parent is null ? typeNode.Text : $"{typeNode.Text} ({typeNode.Parent.Text})";
            editorControl.ShowFindResults($"'{displayName}' references", matches);
        }

        void FindReference(bool findPrevious)
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is not true) return;

            var selection = selectionModel.SelectedNodes.ToArray();
            if (selection.Length != 1) return;

            var inspectBuilder = selection[0].Value;
            var match = workflowBuilder.FindReference(model.Workflow, inspectBuilder, findPrevious);
            if (match is not null)
                SelectBuilderNode(match.Builder);
        }

        void FindReference(TreeNode typeNode, bool findPrevious)
        {
            var currentNode = selectionModel.SelectedNodes.FirstOrDefault();
            var targetWorkflow = selectionModel.SelectedView?.Workflow;
            var elementCategory = WorkflowGraphView.GetToolboxElementCategory(typeNode);
            var match = workflowBuilder.FindReference(
                targetWorkflow,
                elementCategory,
                typeNode.Name,
                currentNode?.Value,
                findPrevious);
            if (match is not null)
                SelectBuilderNode(match.Builder);
        }

        private void explorerTreeView_Navigate(object sender, WorkflowNavigateEventArgs e)
        {
            NavigateTo(e.WorkflowPath, e.NavigationPreference);
        }

        private void toolboxTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            var selectedNode = toolboxTreeView.SelectedNode;
            if (e.KeyCode == Keys.Return && selectedNode?.Tag != null)
            {
                var model = selectionModel.SelectedView;
                if (model?.CanEdit is not true) return;

                CreateGraphNode(model, selectedNode, e.Modifiers);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (e.KeyData == findAllReferencesToolStripMenuItem.ShortcutKeys && selectedNode?.Tag != null)
            {
                FindAllReferences(selectedNode);
            }

            if (e.KeyCode == findNextToolStripMenuItem.ShortcutKeys && selectedNode?.Tag != null)
            {
                var findPrevious = e.Modifiers == Keys.Shift;
                FindReference(selectedNode, findPrevious);
            }

            var rename = e.KeyData == renameSubjectToolStripMenuItem.ShortcutKeys;
            var goToDefinition = e.KeyData == goToDefinitionToolStripMenuItem.ShortcutKeys;
            if ((rename || goToDefinition) && selectedNode?.Tag != null)
            {
                var elementCategory = WorkflowGraphView.GetToolboxElementCategory(selectedNode);
                if (!selectedNode.IsEditing && elementCategory == ~ElementCategory.Source)
                {
                    var currentName = selectedNode.Name;
                    var targetWorkflow = selectionModel.SelectedView?.Workflow ?? workflowBuilder.Workflow;
                    var definition = workflowBuilder.GetSubjectDefinition(targetWorkflow, currentName);
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
                        SelectBuilderNode(definition.Subject);
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
                            findAllReferencesToolStripMenuItem.Visible = true;
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
                if (model?.GraphView.Focused is not true) return;

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
            else FindReference(findPrevious: false);
        }

        private void findPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolboxTreeView.Focused || searchTextBox.Focused)
            {
                toolboxTreeView_KeyDown(sender, new KeyEventArgs(findPreviousToolStripMenuItem.ShortcutKeys));
            }
            else FindReference(findPrevious: true);
        }

        private void findAllReferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolboxTreeView.Focused || searchTextBox.Focused)
            {
                toolboxTreeView_KeyDown(sender, new KeyEventArgs(findAllReferencesToolStripMenuItem.ShortcutKeys));
            }
            else FindAllReferences();
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
            if (model?.GraphView.Focused is true)
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
                if (model?.GraphView.Focused is true)
                {
                    model.CopyToClipboard();
                }
            }
        }

        private void copyAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.Workflow.Count > 0)
            {
                ExportImage(model);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is true)
            {
                model.PasteFromClipboard();
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is true)
            {
                model.SelectAllGraphNodes();
            }
        }

        private void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is true)
            {
                model.Editor.GroupGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is true)
            {
                model.Editor.UngroupGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is true)
            {
                model.Editor.EnableGraphNodes(selectionModel.SelectedNodes);
            }
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model?.GraphView.Focused is true)
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
                if (separatorIndex >= 0 && !Path.IsPathRooted(path) && path.EndsWith(Project.BonsaiExtension))
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
            if (model?.GraphView.Focused is true)
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

        private async void welcomeToolStripMenuItem_Click(object sender, EventArgs e)
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
                    else await OpenWorkflowAsync(fileName);
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

            public string GetProjectDisplayName()
            {
                return siteForm.GetProjectDisplayName();
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ExpressionBuilderGraph))
                {
                    return siteForm.selectionModel.SelectedView?.Workflow
                        ?? siteForm.workflowBuilder.Workflow;
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

                if (serviceType == typeof(VisualizerLayoutMap))
                {
                    return siteForm.visualizerSettings;
                }

                if (serviceType == typeof(VisualizerWindowMap))
                {
                    return siteForm.visualizerWindows;
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
                    var selectedNode = siteForm.selectionModel.SelectedNodes.FirstOrDefault();
                    if (selectedNode != null && selectedNode.Value is InspectBuilder builder &&
                        siteForm.visualizerWindows.TryGetValue(builder, out VisualizerWindowLauncher visualizerWindow))
                    {
                        var visualizer = visualizerWindow.Visualizer;
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
                HandleMenuItemShortcutKeys(e, siteForm.findAllReferencesToolStripMenuItem, siteForm.findAllReferencesToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.docsToolStripMenuItem, siteForm.docsToolStripMenuItem_Click);
            }

            public void OnKeyPress(KeyPressEventArgs e)
            {
                var selectedView = siteForm.selectionModel.SelectedView;
                if (selectedView != null && selectedView.GraphView.Focused)
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

            public void NavigateTo(WorkflowEditorPath workflowPath, NavigationPreference navigationPreference)
            {
                siteForm.NavigateTo(workflowPath, navigationPreference);
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

            public void SelectBuilderNode(ExpressionBuilder builder, NavigationPreference navigationPreference)
            {
                siteForm.SelectBuilderNode(builder, navigationPreference);
            }

            public void SelectBuilderNode(WorkflowEditorPath builderPath, NavigationPreference navigationPreference)
            {
                siteForm.SelectBuilderNode(builderPath, navigationPreference);
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
                        siteForm.OnWorkflowValidated(EventArgs.Empty);
                    }
                    catch (WorkflowBuildException ex)
                    {
                        siteForm.OnWorkflowValidated(EventArgs.Empty);
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
                   (namedElement is SubscribeSubject || namedElement is MulticastSubject) &&
                   siteForm.selectionModel.SelectedView?.Workflow is ExpressionBuilderGraph workflow)
                {
                    var definition = siteForm.workflowBuilder.GetSubjectDefinition(workflow, namedElement.Name);
                    if (definition != null)
                    {
                        siteForm.SelectBuilderNode(definition.Subject);
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

                    var directory = Directory.CreateDirectory(Path.Combine(siteForm.definitionsPath, Project.DefinitionsDirectory));
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
            requireValidation = true;
        }

        private void propertyGrid_Refreshed(object sender, EventArgs e)
        {
            var selectedObjects = propertyGrid.SelectedObjects;
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                var model = selectionModel.SelectedView;
                if (selectedObjects.Length > 1 || model != null && selectedObjects[0] != model.Workflow)
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
            toolboxTreeView.Renderer = themeRenderer.ToolStripRenderer;
            toolboxDescriptionTextBox.BackColor = panelColor;
            toolboxDescriptionTextBox.ForeColor = ForeColor;
            explorerTreeView.Renderer = themeRenderer.ToolStripRenderer;
            explorerLabel.BackColor = colorTable.SeparatorDark;
            explorerLabel.ForeColor = ForeColor;
            propertiesDescriptionTextBox.BackColor = panelColor;
            propertiesDescriptionTextBox.ForeColor = ForeColor;
            menuStrip.ForeColor = SystemColors.ControlText;
            toolStrip.Renderer = themeRenderer.ToolStripRenderer;
            statusStrip.Renderer = themeRenderer.ToolStripRenderer;

            editorControl.Padding = new Padding(0, toolboxLabel.Top - editorControl.Top, 0, 0);
            propertiesLayoutPanel.RowStyles[0].Height = themeRenderer.LabelHeight;
            toolboxLayoutPanel.RowStyles[0].Height = themeRenderer.LabelHeight;
            explorerLayoutPanel.RowStyles[0].Height = themeRenderer.LabelHeight;
            propertyGrid.Refresh();
            editorControl.InitializeTheme();
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
