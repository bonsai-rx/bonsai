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

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        const string BonsaiExtension = ".bonsai";
        const string LayoutExtension = ".layout";
        const string BonsaiPackageName = "Bonsai";
        const string CombinatorCategoryName = "Combinator";
        const string ExamplesDirectory = "Examples";
        const int CycleNextHotKey = 0;
        const int CyclePreviousHotKey = 1;

        int version;
        int saveVersion;
        Font regularFont;
        Font selectionFont;
        EditorSite editorSite;
        HotKeyManager hotKeys;
        WorkflowBuilder workflowBuilder;
        WorkflowGraphView workflowGraphView;
        WorkflowSelectionModel selectionModel;
        TypeDescriptionProvider selectionTypeDescriptor;
        Dictionary<string, string> propertyAssignments;
        List<TreeNode> treeCache;
        WorkflowException workflowError;
        Label statusTextLabel;

        XmlSerializer serializer;
        XmlSerializer layoutSerializer;
        TypeVisualizerMap typeVisualizers;
        List<WorkflowElementDescriptor> workflowElements;
        IDisposable running;
        bool building;

        IObservable<IGrouping<string, WorkflowElementDescriptor>> toolboxElements;
        IObservable<TypeVisualizerDescriptor> visualizerElements;

        public MainForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider)
        {
            InitializeComponent();
            statusTextLabel = new Label();
            statusTextLabel.AutoSize = true;
            statusTextLabel.Text = Resources.ReadyStatus;
            searchTextBox.CueBanner = Resources.SearchModuleCueBanner;
            statusStrip.Items.Add(new ToolStripControlHost(statusTextLabel));
            statusStrip.SizeChanged += new EventHandler(statusStrip_SizeChanged);
            UpdateStatusLabelSize();

            hotKeys = new HotKeyManager(this);
            treeCache = new List<TreeNode>();
            editorSite = new EditorSite(this);
            workflowBuilder = new WorkflowBuilder();
            serializer = new XmlSerializer(typeof(WorkflowBuilder));
            layoutSerializer = new XmlSerializer(typeof(VisualizerLayout));
            regularFont = new Font(toolboxDescriptionTextBox.Font, FontStyle.Regular);
            selectionFont = new Font(toolboxDescriptionTextBox.Font, FontStyle.Bold);
            typeVisualizers = new TypeVisualizerMap();
            workflowElements = new List<WorkflowElementDescriptor>();
            selectionModel = new WorkflowSelectionModel();
            propertyAssignments = new Dictionary<string, string>();
            workflowGraphView = new WorkflowGraphView(editorSite);
            workflowGraphView.Workflow = workflowBuilder.Workflow;
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGroupBox.Controls.Add(workflowGraphView);
            propertyGrid.Site = editorSite;

            selectionModel.UpdateSelection(workflowGraphView);
            selectionModel.SelectionChanged += new EventHandler(selectionModel_SelectionChanged);

            toolboxElements = elementProvider;
            visualizerElements = visualizerProvider;
            if (!IsRunningOnMono())
            {
                hotKeys.RegisterHotKey(CycleNextHotKey, Keys.Control | Keys.Tab);
                hotKeys.RegisterHotKey(CyclePreviousHotKey, Keys.Control | Keys.Shift | Keys.Tab);
            }
        }

        #region Loading

        public bool LaunchPackageManager { get; set; }

        public string InitialFileName { get; set; }

        public bool StartOnLoad { get; set; }

        public IDictionary<string, string> PropertyAssignments
        {
            get { return propertyAssignments; }
        }

        static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        void ShowWelcomeDialog()
        {
            using (var welcome = new WelcomeDialog())
            {
                if (welcome.ShowDialog(this) == DialogResult.OK)
                {
                    if (welcome.ShowWelcomeDialog != EditorSettings.Instance.ShowWelcomeDialog)
                    {
                        EditorSettings.Instance.ShowWelcomeDialog = welcome.ShowWelcomeDialog;
                        EditorSettings.Instance.Save();
                    }
                }
            }
        }

        void RestoreEditorBounds()
        {
            var desktopBounds = EditorSettings.Instance.DesktopBounds;
            if (desktopBounds.Width > 0)
            {
                DesktopBounds = EditorSettings.Instance.DesktopBounds;
            }

            WindowState = EditorSettings.Instance.WindowState;
        }

        protected override void OnLoad(EventArgs e)
        {
            RestoreEditorBounds();
            var initialFileName = InitialFileName;
            var validFileName =
                !string.IsNullOrEmpty(initialFileName) &&
                Path.GetExtension(initialFileName) == BonsaiExtension &&
                File.Exists(initialFileName);

            var currentDirectory = Path.GetFullPath(Environment.CurrentDirectory).TrimEnd('\\');
            var appDomainBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
            var systemPath = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.System)).TrimEnd('\\');
            var systemX86Path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)).TrimEnd('\\');
            var currentDirectoryRestricted = currentDirectory == appDomainBaseDirectory || currentDirectory == systemPath || currentDirectory == systemX86Path;
            directoryToolStripTextBox.Text = !currentDirectoryRestricted ? currentDirectory : (validFileName ? Path.GetDirectoryName(initialFileName) : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            var initialization = InitializeToolbox().Merge(InitializeTypeVisualizers()).TakeLast(1).ObserveOn(Scheduler.Default);
            var examplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExamplesDirectory);
            examplesToolStripMenuItem.Enabled = InitializeExampleDirectory(examplesPath, examplesToolStripMenuItem);
            DeleteTempDirectory();

            if (validFileName)
            {
                OpenWorkflow(initialFileName, false);
                foreach (var assignment in propertyAssignments)
                {
                    workflowBuilder.Workflow.SetWorkflowProperty(assignment.Key, assignment.Value);
                }

                if (StartOnLoad) initialization = initialization.Do(xs => BeginInvoke((Action)(() => StartWorkflow())));
            }

            initialization.Subscribe();
            base.OnLoad(e);
        }

        protected override void OnShown(EventArgs e)
        {
            if (EditorSettings.Instance.ShowWelcomeDialog)
            {
                ShowWelcomeDialog();
            }

            base.OnShown(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!IsRunningOnMono())
            {
                hotKeys.UnregisterHotKey(CyclePreviousHotKey);
                hotKeys.UnregisterHotKey(CycleNextHotKey);
            }

            var desktopBounds = EditorSettings.Instance.DesktopBounds;
            if (WindowState != FormWindowState.Normal)
            {
                desktopBounds.Size = RestoreBounds.Size;
            }
            else desktopBounds = DesktopBounds;
            EditorSettings.Instance.DesktopBounds = desktopBounds;
            if (WindowState == FormWindowState.Minimized)
            {
                EditorSettings.Instance.WindowState = FormWindowState.Normal;
            }
            else EditorSettings.Instance.WindowState = WindowState;
            EditorSettings.Instance.Save();
            base.OnClosed(e);
        }

        #endregion

        #region Toolbox

        IObservable<Unit> InitializeTypeVisualizers()
        {
            var visualizerMapping = from typeVisualizer in visualizerElements
                                    let targetType = Type.GetType(typeVisualizer.TargetTypeName)
                                    let visualizerType = Type.GetType(typeVisualizer.VisualizerTypeName)
                                    where targetType != null && visualizerType != null
                                    select Tuple.Create(targetType, visualizerType);

            return visualizerMapping
                .ObserveOn(this)
                .Do(typeMapping => typeVisualizers.Add(typeMapping.Item1, typeMapping.Item2))
                .SubscribeOn(Scheduler.Default)
                .TakeLast(1)
                .Select(xs => Unit.Default);
        }

        IObservable<Unit> InitializeToolbox()
        {
            return toolboxElements
                .ObserveOn(this)
                .Do(package => InitializeToolboxCategory(package.Key, package))
                .SubscribeOn(Scheduler.Default)
                .TakeLast(1)
                .Select(xs => Unit.Default);
        }

        static string GetPackageDisplayName(string packageKey)
        {
            if (packageKey == BonsaiPackageName) return packageKey;
            return packageKey.Replace(BonsaiPackageName + ".", string.Empty);
        }

        static int GetElementTypeIndex(string typeName)
        {
            return
                typeName == ElementCategory.Source.ToString() ? 0 :
                typeName == ElementCategory.Condition.ToString() ? 1 :
                typeName == ElementCategory.Transform.ToString() ? 2 :
                typeName == ElementCategory.Sink.ToString() ? 3 : 4;
        }

        static int CompareLoadableElementType(string left, string right)
        {
            return GetElementTypeIndex(left).CompareTo(GetElementTypeIndex(right));
        }

        void InitializeToolboxCategory(string categoryName, IEnumerable<WorkflowElementDescriptor> types)
        {
            foreach (var type in types.OrderBy(type => type.Name))
            {
                workflowElements.Add(type);
                foreach (var elementType in type.ElementTypes)
                {
                    var typeCategory = elementType;
                    if (typeCategory == ElementCategory.Nested || typeCategory == ElementCategory.Condition)
                    {
                        typeCategory = ElementCategory.Combinator;
                    }
                    else if (typeCategory == ElementCategory.Property)
                    {
                        typeCategory = ElementCategory.Source;
                    }

                    var typeCategoryName = typeCategory.ToString();
                    var elementTypeNode = treeCache.Count > 0
                        ? treeCache.Single(ns => ns.Name == typeCategoryName)
                        : toolboxTreeView.Nodes[typeCategoryName];
                    var category = elementTypeNode.Nodes[categoryName];
                    if (category == null)
                    {
                        category = elementTypeNode.Nodes.Add(categoryName, GetPackageDisplayName(categoryName));
                    }

                    var node = category.Nodes.Add(type.AssemblyQualifiedName, type.Name);
                    node.Tag = elementType;
                    node.ToolTipText = type.Description;
                }
            }
        }

        #endregion

        #region Workflow Examples

        static void DeleteTempDirectory()
        {
            var tempPath = GetTempPath();
            try { Directory.Delete(tempPath, true); }
            catch { } //best effort
        }

        static string GetTempPath()
        {
            return Path.Combine(Path.GetTempPath(), BonsaiPackageName);
        }

        static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.Equals(target.FullName, StringComparison.OrdinalIgnoreCase)) return;
            if (!target.Exists) Directory.CreateDirectory(target.FullName);

            foreach (var file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            foreach (var directory in source.GetDirectories())
            {
                var targetDirectory = target.CreateSubdirectory(directory.Name);
                CopyDirectory(directory, targetDirectory);
            }
        }

        void OpenExample(string examplePath)
        {
            if (CheckUnsavedChanges())
            {
                var tempPath = GetTempPath();
                var targetDirectory = Directory.CreateDirectory(Path.Combine(tempPath, Path.GetRandomFileName()));
                var sourceDirectory = new DirectoryInfo(Path.GetDirectoryName(examplePath));
                CopyDirectory(sourceDirectory, targetDirectory);

                examplePath = Path.Combine(targetDirectory.FullName, Path.GetFileName(examplePath));
                OpenWorkflow(examplePath);
                saveWorkflowDialog.FileName = null;
            }
        }

        bool InitializeExampleDirectory(string path, ToolStripMenuItem exampleMenuItem)
        {
            if (!Directory.Exists(path)) return false;

            var examplePath = Path.Combine(path, Path.ChangeExtension(exampleMenuItem.Text, BonsaiExtension));
            if (File.Exists(examplePath))
            {
                exampleMenuItem.Tag = examplePath;
                exampleMenuItem.Click += (sender, e) => OpenExample(examplePath);
                return true;
            }
            else
            {
                var containsExamples = false;
                foreach (var directory in Directory.GetDirectories(path))
                {
                    var menuItem = new ToolStripMenuItem(Path.GetFileNameWithoutExtension(directory));
                    var examplesFound = InitializeExampleDirectory(directory, menuItem);
                    if (examplesFound)
                    {
                        exampleMenuItem.DropDownItems.Add(menuItem);
                        containsExamples = true;
                    }
                }

                return containsExamples;
            }
        }

        #endregion

        #region File Menu

        void ResetProjectStatus()
        {
            commandExecutor.Clear();
            version = 0;
            saveVersion = 0;
        }

        bool CheckUnsavedChanges()
        {
            if (editorSite.WorkflowRunning)
            {
                var result = MessageBox.Show("Do you want to stop the workflow?", "Workflow Running", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    StopWorkflow();
                }
                else return false;
            }

            if (saveVersion != version)
            {
                var result = MessageBox.Show("Workflow has unsaved changes. Save project file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
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

        WorkflowBuilder LoadWorkflow(string fileName)
        {
            using (var reader = XmlReader.Create(fileName))
            {
                var workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);
                workflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.ToInspectableGraph());
                return workflowBuilder;
            }
        }

        void OpenWorkflow(string fileName)
        {
            OpenWorkflow(fileName, true);
        }

        void OpenWorkflow(string fileName, bool setWorkingDirectory)
        {
            try { workflowBuilder = LoadWorkflow(fileName); }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(string.Format("There was an error opening the Bonsai workflow:\n{0}", ex.InnerException.Message), "Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            saveWorkflowDialog.FileName = fileName;
            ResetProjectStatus();
            UpdateTitle();

            var layoutPath = GetLayoutPath(fileName);
            workflowGraphView.VisualizerLayout = null;
            workflowGraphView.Workflow = workflowBuilder.Workflow;
            if (File.Exists(layoutPath))
            {
                using (var reader = XmlReader.Create(layoutPath))
                {
                    try { workflowGraphView.VisualizerLayout = (VisualizerLayout)layoutSerializer.Deserialize(reader); }
                    catch (InvalidOperationException) { }
                }
            }

            editorSite.ValidateWorkflow();
            ResetProjectStatus();

            if (setWorkingDirectory || string.IsNullOrEmpty(directoryToolStripTextBox.Text))
            {
                directoryToolStripTextBox.Text = Path.GetDirectoryName(fileName);
            }
        }

        void SaveWorkflow(string fileName, WorkflowBuilder workflowBuilder)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, workflowBuilder);
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fileStream);
                }
            }
        }

        void UpdateCurrentDirectory()
        {
            if (Directory.Exists(directoryToolStripTextBox.Text))
            {
                Environment.CurrentDirectory = directoryToolStripTextBox.Text;
            }
            else directoryToolStripTextBox.Text = Environment.CurrentDirectory;
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
            if (!CheckUnsavedChanges()) return;

            saveWorkflowDialog.FileName = null;
            workflowBuilder.Workflow.Clear();
            workflowGraphView.VisualizerLayout = null;
            workflowGraphView.Workflow = workflowBuilder.Workflow;
            ResetProjectStatus();
            UpdateTitle();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckUnsavedChanges()) return;

            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                OpenWorkflow(openWorkflowDialog.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(saveWorkflowDialog.FileName)) saveAsToolStripMenuItem_Click(this, e);
            else
            {
                var serializerWorkflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.FromInspectableGraph());
                SaveWorkflow(saveWorkflowDialog.FileName, serializerWorkflowBuilder);
                saveVersion = version;

                workflowGraphView.UpdateVisualizerLayout();
                if (workflowGraphView.VisualizerLayout != null)
                {
                    var layoutPath = GetLayoutPath(saveWorkflowDialog.FileName);
                    using (var writer = XmlWriter.Create(layoutPath, new XmlWriterSettings { Indent = true }))
                    {
                        layoutSerializer.Serialize(writer, workflowGraphView.VisualizerLayout);
                    }
                }

                if (string.IsNullOrEmpty(directoryToolStripTextBox.Text))
                {
                    directoryToolStripTextBox.Text = Path.GetDirectoryName(saveWorkflowDialog.FileName);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                saveToolStripMenuItem_Click(this, e);
                var workflowDirectory = Path.GetDirectoryName(saveWorkflowDialog.FileName);
                directoryToolStripTextBox.Text = workflowDirectory;
                UpdateTitle();
            }
        }

        private void saveSelectionAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentFileName = saveWorkflowDialog.FileName;
            try
            {
                if (saveWorkflowDialog.ShowDialog() == DialogResult.OK)
                {
                    var serializerWorkflowBuilder = selectionModel.SelectedNodes.ToWorkflowBuilder();
                    SaveWorkflow(saveWorkflowDialog.FileName, serializerWorkflowBuilder);
                }
            }
            finally { saveWorkflowDialog.FileName = currentFileName; }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CheckUnsavedChanges()) e.Cancel = true;
            base.OnClosing(e);
        }

        #endregion

        #region Workflow Model

        IDisposable ShutdownSequence()
        {
            return new ScheduledDisposable(new ControlScheduler(this), Disposable.Create(() =>
            {
                editorSite.OnWorkflowStopped(EventArgs.Empty);
                workflowGraphView.UpdateVisualizerLayout();
                undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
                redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
                deleteToolStripMenuItem.Enabled = true;
                groupToolStripMenuItem.Enabled = true;
                cutToolStripMenuItem.Enabled = true;
                pasteToolStripMenuItem.Enabled = true;
                startToolStripButton.Visible = startToolStripMenuItem.Visible = true;
                stopToolStripButton.Visible = stopToolStripMenuItem.Visible = false;
                restartToolStripButton.Visible = restartToolStripMenuItem.Visible = false;
                if (workflowError == null)
                {
                    statusTextLabel.Text = Resources.ReadyStatus;
                    statusImageLabel.Image = Resources.StatusReadyImage;
                }

                running = null;
                building = false;
                UpdateTitle();
            }));
        }

        void StartWorkflow()
        {
            if (running == null)
            {
                building = true;
                ClearWorkflowError();
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
                                statusImageLabel.Image = Resources.StatusReadyImage;
                                editorSite.OnWorkflowStarted(EventArgs.Empty);
                            }));
                        }
                        catch
                        {
                            shutdown.Dispose();
                            throw;
                        }

                        return new WorkflowDisposable(runtimeWorkflow, shutdown);
                    },
                    resource => resource.Workflow.TakeUntil(workflowBuilder.Workflow.InspectErrors()))
                    .SubscribeOn(NewThreadScheduler.Default.Catch<Exception>(HandleSchedulerError))
                    .Subscribe(unit => { }, HandleWorkflowError, () => { });
            }

            UpdateTitle();
            undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = false;
            redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = false;
            deleteToolStripMenuItem.Enabled = false;
            groupToolStripMenuItem.Enabled = false;
            cutToolStripMenuItem.Enabled = false;
            pasteToolStripMenuItem.Enabled = false;
            startToolStripButton.Visible = startToolStripMenuItem.Visible = false;
            stopToolStripButton.Visible = stopToolStripMenuItem.Visible = true;
            restartToolStripButton.Visible = restartToolStripMenuItem.Visible = true;
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

        void ClearWorkflowError()
        {
            if (workflowError != null)
            {
                ClearExceptionBuilderNode(workflowGraphView, workflowError);
            }

            workflowError = null;
        }

        void HighlightWorkflowError()
        {
            if (workflowError != null)
            {
                HighlightExceptionBuilderNode(workflowGraphView, workflowError, false);
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
                    nestedEditor = editorLauncher != null ? editorLauncher.WorkflowGraphView : null;
                }

                ClearExceptionBuilderNode(nestedEditor, nestedException);
            }
            else
            {
                statusTextLabel.Text = Resources.ReadyStatus;
                statusImageLabel.Image = Resources.StatusReadyImage;
            }
        }

        void HighlightExceptionBuilderNode(WorkflowGraphView workflowView, WorkflowException e, bool showMessageBox)
        {
            GraphNode graphNode = null;
            if (workflowView != null)
            {
                graphNode = workflowView.FindGraphNode(e.Builder);
                if (graphNode == null)
                {
                    throw new InvalidOperationException("Exception builder node not found in active workflow editor.");
                }

                workflowView.GraphView.Invalidate(graphNode);
                workflowView.GraphView.SelectedNode = graphNode;
                graphNode.Highlight = true;
            }

            var nestedException = e.InnerException as WorkflowException;
            if (nestedException != null)
            {
                WorkflowGraphView nestedEditor = null;
                if (workflowView != null)
                {
                    if (building)
                    {
                        workflowView.LaunchWorkflowView(graphNode);
                    }

                    var editorLauncher = workflowView.GetWorkflowEditorLauncher(graphNode);
                    nestedEditor = editorLauncher != null ? editorLauncher.WorkflowGraphView : null;
                }

                HighlightExceptionBuilderNode(nestedEditor, nestedException, showMessageBox);
            }
            else
            {
                if (workflowView != null)
                {
                    workflowView.GraphView.Select();
                }

                var buildException = e is WorkflowBuildException;
                var errorCaption = buildException ? "Build Error" : "Runtime Error";
                statusTextLabel.Text = e.Message;
                statusImageLabel.Image = buildException ? Resources.StatusBlockedImage : Resources.StatusCriticalImage;
                if (showMessageBox)
                {
                    MessageBox.Show(e.Message, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var workflowException = e as WorkflowException;
            if (workflowException != null)
            {
                Action selectExceptionNode = () =>
                {
                    workflowError = workflowException;
                    HighlightExceptionBuilderNode(workflowGraphView, workflowException, building);
                };

                if (InvokeRequired) BeginInvoke(selectExceptionNode);
                else selectExceptionNode();

                var shutdown = ShutdownSequence();
                shutdown.Dispose();
            }
            else MessageBox.Show(e.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region Workflow Controller

        void UpdateTitle()
        {
            var workflowRunning = running != null;
            var fileName = Path.GetFileName(saveWorkflowDialog.FileName);
            var emptyFileName = string.IsNullOrEmpty(fileName);
            var title = emptyFileName ? Resources.BonsaiTitle : fileName;
            if (workflowRunning) title += string.Format(" ({0})", Resources.RunningStatus);
            if (!emptyFileName) title += string.Format(" - {0}", Resources.BonsaiTitle);
            Text = title;
        }

        IEnumerable<TreeNode> GetTreeViewLeafNodes(System.Collections.IEnumerable nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Nodes.Count == 0) yield return node;
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
            if (node.Tag == null) return -1;
            var matchIndex = node.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0 && node.Parent != null)
            {
                matchIndex = node.Parent.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase);
            }

            return matchIndex;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (workflowGraphView.Focused)
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

        private void selectionModel_SelectionChanged(object sender, EventArgs e)
        {
            if (selectionTypeDescriptor != null)
            {
                TypeDescriptor.RemoveProvider(selectionTypeDescriptor, propertyGrid.SelectedObject);
                selectionTypeDescriptor = null;
            }

            var selectedObjects = selectionModel.SelectedNodes.Select(node =>
            {
                var builder = ExpressionBuilder.Unwrap((ExpressionBuilder)node.Value);
                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                return workflowElement ?? builder;
            }).ToArray();

            saveSelectionAsToolStripMenuItem.Enabled = selectedObjects.Length > 0;
            if (selectedObjects.Length == 1)
            {
                propertyGrid.PropertyTabs.AddTabType(typeof(MappingTab), PropertyTabScope.Document);
                propertyGrid.SelectedObject = selectedObjects[0];
            }
            else
            {
                propertyGrid.RefreshTabs(PropertyTabScope.Document);
                propertyGrid.SelectedObjects = selectedObjects;
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                if (Control.ModifierKeys.HasFlag(Keys.Control)) RestartWorkflow();
                else StopWorkflow();
            }
            StartWorkflow();
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
                var searchFilter = searchTextBox.Text.Trim();
                foreach (var entry in from node in GetTreeViewLeafNodes(treeCache)
                                      let matchIndex = IndexOfMatch(node, searchFilter)
                                      where matchIndex >= 0
                                      orderby node.Text ascending
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
                        break;
                }
            }
        }

        private void toolboxTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return &&
                !editorSite.WorkflowRunning &&
                toolboxTreeView.SelectedNode != null &&
                toolboxTreeView.SelectedNode.Tag != null)
            {
                var typeNode = toolboxTreeView.SelectedNode;
                var model = selectionModel.SelectedView;
                var branch = e.Modifiers.HasFlag(WorkflowGraphView.BranchModifier);
                var predecessor = e.Modifiers.HasFlag(WorkflowGraphView.PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                model.CreateGraphNode(typeNode, selectionModel.SelectedNodes.FirstOrDefault(), predecessor, branch);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void toolboxTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Node.Tag != null)
            {
                var typeNode = e.Node;
                var model = selectionModel.SelectedView;
                var branch = Control.ModifierKeys.HasFlag(WorkflowGraphView.BranchModifier);
                var predecessor = Control.ModifierKeys.HasFlag(WorkflowGraphView.PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                model.CreateGraphNode(typeNode, selectionModel.SelectedNodes.FirstOrDefault(), predecessor, branch);
            }
        }

        private void toolboxTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNode = e.Node;
            if (selectedNode != null && selectedNode.GetNodeCount(false) == 0)
            {
                var description = selectedNode.ToolTipText;
                toolboxDescriptionTextBox.SuspendLayout();
                toolboxDescriptionTextBox.Lines = new[]
                {
                    selectedNode.Text,
                    description
                };

                toolboxDescriptionTextBox.SelectionStart = 0;
                toolboxDescriptionTextBox.SelectionLength = selectedNode.Text.Length;
                toolboxDescriptionTextBox.SelectionFont = selectionFont;
                toolboxDescriptionTextBox.SelectionStart = selectedNode.Text.Length;
                toolboxDescriptionTextBox.SelectionLength = description.Length;
                toolboxDescriptionTextBox.SelectionFont = regularFont;
                toolboxDescriptionTextBox.ResumeLayout();
            }
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
                        if (selectedNode.Tag.Equals(ElementCategory.Source))
                        {
                            toolboxSourceContextMenuStrip.Show(toolboxTreeView, e.X, e.Y);
                        }
                        else toolboxContextMenuStrip.Show(toolboxTreeView, e.X, e.Y);
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

        private void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = selectionModel.SelectedView;
            if (model.GraphView.Focused)
            {
                model.GroupGraphNodes(selectionModel.SelectedNodes);
            }
        }

        #endregion

        #region Undo/Redo

        private void commandExecutor_StatusChanged(object sender, EventArgs e)
        {
            undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
            redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
            version++;
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

        #region Cycle Active Forms

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                if ((ushort)m.WParam == CycleNextHotKey) CycleActiveForm(1);
                if ((ushort)m.WParam == CyclePreviousHotKey) CycleActiveForm(-1);
            }
        }

        static void CycleActiveForm(int step)
        {
            if (Application.OpenForms.Count > 1 && !Form.ActiveForm.Modal)
            {
                for (int i = 0; i < Application.OpenForms.Count; i++)
                {
                    var form = Application.OpenForms[i];
                    if (form == Form.ActiveForm)
                    {
                        i = (i + Application.OpenForms.Count + step) % Application.OpenForms.Count;
                        var activeForm = Application.OpenForms[i];
                        activeForm.Activate();
                        break;
                    }
                }
            }
        }

        #endregion

        #region Package Manager

        private void packageManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
            LaunchPackageManager = true;
        }

        #endregion

        #region Help Menu

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var about = new AboutBox())
            {
                about.ShowDialog();
            }
        }

        private void welcomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWelcomeDialog();
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

                if (serviceType == typeof(IWorkflowEditorService) ||
                    serviceType == typeof(IWorkflowEditorState) ||
                    serviceType == typeof(IWorkflowToolboxService) ||
                    serviceType == typeof(IUIService))
                {
                    return this;
                }

                return null;
            }

            void HandleMenuItemShortcutKeys(KeyEventArgs e, ToolStripMenuItem menuItem, EventHandler onShortcut)
            {
                if (menuItem.Enabled && menuItem.ShortcutKeys == e.KeyData)
                {
                    onShortcut(this, e);
                }
            }

            public void OnKeyDown(KeyEventArgs e)
            {
                if (e.Control && e.KeyCode == Keys.E)
                {
                    siteForm.searchTextBox.Focus();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }

                HandleMenuItemShortcutKeys(e, siteForm.newToolStripMenuItem, siteForm.newToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.openToolStripMenuItem, siteForm.openToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.saveToolStripMenuItem, siteForm.saveToolStripMenuItem_Click);
                HandleMenuItemShortcutKeys(e, siteForm.saveSelectionAsToolStripMenuItem, siteForm.saveSelectionAsToolStripMenuItem_Click);
            }

            public void OnKeyPress(KeyPressEventArgs e)
            {
                var model = siteForm.selectionModel.SelectedView ?? siteForm.workflowGraphView;
                if (!WorkflowRunning && model.GraphView.Focused)
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

            public WorkflowBuilder LoadWorkflow(string fileName)
            {
                return siteForm.LoadWorkflow(fileName);
            }

            public void OpenWorkflow(string fileName)
            {
                siteForm.OpenWorkflow(fileName);
            }

            public void StoreWorkflowElements(WorkflowBuilder builder)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException("builder");
                }

                if (builder.Workflow.Count > 0)
                {
                    var stringBuilder = new StringBuilder();
                    using (var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings { Indent = true }))
                    {
                        siteForm.serializer.Serialize(writer, builder);
                    }

                    Clipboard.SetText(stringBuilder.ToString());
                }
            }

            public WorkflowBuilder RetrieveWorkflowElements()
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    var stringReader = new StringReader(text);
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        try
                        {
                            if (siteForm.serializer.CanDeserialize(reader))
                            {
                                return (WorkflowBuilder)siteForm.serializer.Deserialize(reader);
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

            public void StartWorkflow()
            {
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
                        siteForm.ClearWorkflowError();
                        siteForm.workflowBuilder.Workflow.Build();
                    }
                    catch (WorkflowBuildException ex)
                    {
                        siteForm.HandleWorkflowError(ex);
                        siteForm.workflowError = ex;
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
                siteForm.version -= 2;
                siteForm.commandExecutor.Undo();
            }

            public void Redo()
            {
                siteForm.commandExecutor.Redo();
            }

            public bool CanShowComponentEditor(object component)
            {
                var editor = TypeDescriptor.GetEditor(component, typeof(ComponentEditor));
                return editor != null;
            }

            public IWin32Window GetDialogOwnerWindow()
            {
                return siteForm;
            }

            public void SetUIDirty()
            {
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

                return false;
            }

            public DialogResult ShowDialog(Form form)
            {
                return form.ShowDialog(siteForm);
            }

            public void ShowError(Exception ex, string message)
            {
                MessageBox.Show(message, siteForm.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public void ShowError(Exception ex)
            {
                siteForm.HandleWorkflowError(ex);
            }

            public void ShowError(string message)
            {
                MessageBox.Show(message, siteForm.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons)
            {
                return MessageBox.Show(message, caption, buttons);
            }

            public void ShowMessage(string message, string caption)
            {
                MessageBox.Show(message, caption);
            }

            public void ShowMessage(string message)
            {
                MessageBox.Show(message);
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
            resetToolStripMenuItem.Enabled = item != null && item.PropertyDescriptor.CanResetValue(item.Parent.Value);
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
        }

        private void propertyGrid_DragEnter(object sender, DragEventArgs e)
        {
            var gridItem = propertyGrid.SelectedGridItem;
            if (gridItem != null && !gridItem.PropertyDescriptor.IsReadOnly &&
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
            if (gridItem != null)
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
    }
}
