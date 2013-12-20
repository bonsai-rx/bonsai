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
using Bonsai.Configuration;

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        const string BonsaiExtension = ".bonsai";
        const string LayoutExtension = ".layout";
        const string BonsaiPackageName = "Bonsai";
        const string CombinatorCategoryName = "Combinator";

        int version;
        int saveVersion;
        Font regularFont;
        Font selectionFont;
        EditorSite editorSite;
        WorkflowBuilder workflowBuilder;
        WorkflowGraphView workflowGraphView;
        WorkflowSelectionModel selectionModel;
        TypeDescriptionProvider selectionTypeDescriptor;
        Dictionary<string, string> propertyAssignments;
        List<TreeNode> treeCache;
        WorkflowException workflowError;

        XmlSerializer serializer;
        XmlSerializer layoutSerializer;
        TypeVisualizerMap typeVisualizers;
        IDisposable running;
        bool building;

        public MainForm()
        {
            InitializeComponent();
            searchTextBox.CueBanner = Resources.SearchModuleCueBanner;
            propertyAssignments = new Dictionary<string, string>();

            treeCache = new List<TreeNode>();
            editorSite = new EditorSite(this);
            workflowBuilder = new WorkflowBuilder();
            serializer = new XmlSerializer(typeof(WorkflowBuilder));
            layoutSerializer = new XmlSerializer(typeof(VisualizerLayout));
            regularFont = new Font(toolboxDescriptionTextBox.Font, FontStyle.Regular);
            selectionFont = new Font(toolboxDescriptionTextBox.Font, FontStyle.Bold);
            typeVisualizers = new TypeVisualizerMap();
            selectionModel = new WorkflowSelectionModel();
            workflowGraphView = new WorkflowGraphView(editorSite);
            workflowGraphView.Workflow = workflowBuilder.Workflow;
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGroupBox.Controls.Add(workflowGraphView);
            propertyGrid.Site = editorSite;

            selectionModel.UpdateSelection(workflowGraphView);
            selectionModel.SelectionChanged += new EventHandler(selectionModel_SelectionChanged);
        }

        #region Loading

        internal bool LaunchPackageManager { get; set; }

        public PackageConfiguration PackageConfiguration { get; set; }

        public string InitialFileName { get; set; }

        public bool StartOnLoad { get; set; }

        public IDictionary<string, string> PropertyAssignments
        {
            get { return propertyAssignments; }
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

        protected override void OnLoad(EventArgs e)
        {
            var initialFileName = InitialFileName;
            var validFileName =
                !string.IsNullOrEmpty(initialFileName) &&
                Path.GetExtension(initialFileName) == BonsaiExtension &&
                File.Exists(initialFileName);

            var configuration = PackageConfiguration ?? ConfigurationHelper.Load();
            var currentDirectory = Path.GetFullPath(Environment.CurrentDirectory).TrimEnd('\\');
            var appDomainBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
            var systemPath = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.System)).TrimEnd('\\');
            var systemX86Path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)).TrimEnd('\\');
            var currentDirectoryRestricted = currentDirectory == appDomainBaseDirectory || currentDirectory == systemPath || currentDirectory == systemX86Path;
            directoryToolStripTextBox.Text = !currentDirectoryRestricted ? currentDirectory : (validFileName ? Path.GetDirectoryName(initialFileName) : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            var initialization = InitializeToolbox(configuration).Merge(InitializeTypeVisualizers(configuration)).TakeLast(1).ObserveOn(Scheduler.Default);
            InitializeExampleDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Examples"), examplesToolStripMenuItem);

            if (validFileName)
            {
                OpenWorkflow(initialFileName);
                foreach (var assignment in propertyAssignments)
                {
                    workflowGraphView.SetWorkflowProperty(assignment.Key, assignment.Value);
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

        #endregion

        #region Toolbox

        IObservable<Unit> InitializeTypeVisualizers(PackageConfiguration configuration)
        {
            var visualizerMapping = TypeVisualizerLoader.GetTypeVisualizerDictionary(configuration);
            return visualizerMapping
                .ObserveOn(this)
                .Do(typeMapping => typeVisualizers.Add(typeMapping.Item1, typeMapping.Item2))
                .SubscribeOn(Scheduler.Default)
                .TakeLast(1)
                .Select(xs => Unit.Default);
        }

        IObservable<Unit> InitializeToolbox(PackageConfiguration configuration)
        {
            var packages = WorkflowElementLoader.GetWorkflowElementTypes(configuration);
            return packages
                .ObserveOn(this)
                .Do(package => InitializeToolboxCategory(package.Key, package))
                .SubscribeOn(Scheduler.Default)
                .TakeLast(1)
                .Select(xs => Unit.Default);
        }

        string GetPackageDisplayName(string packageKey)
        {
            if (packageKey == BonsaiPackageName) return packageKey;
            return packageKey.Replace(BonsaiPackageName + ".", string.Empty);
        }

        int GetElementTypeIndex(string typeName)
        {
            return
                typeName == ElementCategory.Source.ToString() ? 0 :
                typeName == ElementCategory.Condition.ToString() ? 1 :
                typeName == ElementCategory.Transform.ToString() ? 2 :
                typeName == ElementCategory.Sink.ToString() ? 3 : 4;
        }

        int CompareLoadableElementType(string left, string right)
        {
            return GetElementTypeIndex(left).CompareTo(GetElementTypeIndex(right));
        }

        void InitializeToolboxCategory(string categoryName, IEnumerable<WorkflowElementDescriptor> types)
        {
            foreach (var type in types.OrderBy(type => type.Name))
            {
                foreach (var elementType in type.ElementTypes)
                {
                    var typeCategory = elementType == ElementCategory.Nested ? ElementCategory.Combinator : elementType;
                    var elementTypeNode = toolboxTreeView.Nodes[typeCategory.ToString()];
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

        bool InitializeExampleDirectory(string path, ToolStripMenuItem exampleMenuItem)
        {
            if (!Directory.Exists(path)) return false;

            var examplePath = Path.Combine(path, Path.ChangeExtension(exampleMenuItem.Text, BonsaiExtension));
            if (File.Exists(examplePath))
            {
                exampleMenuItem.Tag = examplePath;
                exampleMenuItem.Click += (sender, e) =>
                {
                    if (CheckUnsavedChanges())
                    {
                        OpenWorkflow(examplePath);
                        saveWorkflowDialog.FileName = null;
                    }
                };
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
            try { workflowBuilder = LoadWorkflow(fileName); }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(string.Format("There was an error opening the Bonsai workflow:\n{0}", ex.InnerException.Message), "Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            saveWorkflowDialog.FileName = fileName;
            ResetProjectStatus();

            var layoutPath = GetLayoutPath(fileName);
            workflowGraphView.VisualizerLayout = null;
            if (File.Exists(layoutPath))
            {
                using (var reader = XmlReader.Create(layoutPath))
                {
                    try { workflowGraphView.VisualizerLayout = (VisualizerLayout)layoutSerializer.Deserialize(reader); }
                    catch (InvalidOperationException) { }
                }
            }

            workflowGraphView.Workflow = workflowBuilder.Workflow;
            editorSite.ValidateWorkflow();

            if (string.IsNullOrEmpty(directoryToolStripTextBox.Text))
            {
                directoryToolStripTextBox.Text = Path.GetDirectoryName(fileName);
            }
        }

        void UpdateCurrentDirectory()
        {
            Environment.CurrentDirectory = directoryToolStripTextBox.Text;
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
            folderBrowserDialog.SelectedPath = directoryToolStripTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                directoryToolStripTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckUnsavedChanges()) return;

            saveWorkflowDialog.FileName = null;
            workflowBuilder.Workflow.Clear();
            workflowGraphView.VisualizerLayout = null;
            workflowGraphView.Workflow = workflowBuilder.Workflow;
            ResetProjectStatus();
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
                using (var memoryStream = new MemoryStream())
                using (var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings { Indent = true }))
                {
                    var serializerWorkflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.FromInspectableGraph());
                    serializer.Serialize(writer, serializerWorkflowBuilder);
                    using (var fileStream = new FileStream(saveWorkflowDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        memoryStream.WriteTo(fileStream);
                    }

                    saveVersion = version;
                }

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
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CheckUnsavedChanges()) e.Cancel = true;
            else StopWorkflow();
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
                runningStatusLabel.Text = Resources.StoppedStatus;
                runningStatusLabel.Image = Resources.StoppedStatusImage;
                undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
                redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
                deleteToolStripMenuItem.Enabled = true;
                groupToolStripMenuItem.Enabled = true;
                cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled = true;
                pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled = true;
                running = null;
                building = false;
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
                        runningStatusLabel.Text = Resources.RunningStatus;
                        runningStatusLabel.Image = Resources.RunningStatusImage;
                        editorSite.OnWorkflowStarted(EventArgs.Empty);

                        var shutdown = ShutdownSequence();
                        return new WorkflowDisposable(runtimeWorkflow, shutdown);
                    },
                    resource => resource.Workflow
                        .TakeUntil(workflowBuilder.Workflow.InspectErrors())
                        .SubscribeOn(NewThreadScheduler.Default))
                    .Subscribe(unit => { }, HandleWorkflowError, () => { });
            }

            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
            deleteToolStripMenuItem.Enabled = false;
            groupToolStripMenuItem.Enabled = false;
            cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled = false;
            pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled = false;
        }

        void StopWorkflow()
        {
            if (running != null)
            {
                running.Dispose();
            }
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
                HighlightExceptionBuilderNode(workflowGraphView, workflowError);
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
                errorStatusLabel.Text = string.Empty;
                errorStatusLabel.BorderSides = ToolStripStatusLabelBorderSides.None;
            }
        }

        void HighlightExceptionBuilderNode(WorkflowGraphView workflowView, WorkflowException e)
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

                HighlightExceptionBuilderNode(nestedEditor, nestedException);
            }
            else
            {
                if (workflowView != null)
                {
                    workflowView.GraphView.Select();
                }

                var errorCaption = e is WorkflowBuildException ? "Build Error" : "Runtime Error";
                errorStatusLabel.Text = e.Message;
                errorStatusLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
                if (building)
                {
                    MessageBox.Show(e.Message, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void HandleWorkflowError(Exception e)
        {
            var workflowException = e as WorkflowException;
            if (workflowException != null)
            {
                Action selectExceptionNode = () =>
                {
                    HighlightExceptionBuilderNode(workflowGraphView, workflowException);
                    workflowError = workflowException;
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
                var builder = node.Value as ExpressionBuilder;
                var workflowElement = builder != null ? ExpressionBuilder.GetWorkflowElement(builder) : null;

                if (workflowElement != null)
                {
                    var conditionBuilder = node.Value as ConditionBuilder;
                    if (conditionBuilder != null)
                    {
                        var builderProperties = TypeDescriptor.GetProperties(conditionBuilder);
                        var provider = new DynamicTypeDescriptionProvider();

                        var selectorProperty = builderProperties["Selector"];
                        var attributes = new Attribute[selectorProperty.Attributes.Count];
                        selectorProperty.Attributes.CopyTo(attributes, 0);
                        var dynamicProperty = new DynamicPropertyDescriptor(
                            selectorProperty.Name, selectorProperty.PropertyType,
                            xs => selectorProperty.GetValue(conditionBuilder),
                            (xs, value) => selectorProperty.SetValue(conditionBuilder, value),
                            attributes);
                        provider.Properties.Add(dynamicProperty);

                        TypeDescriptor.AddProvider(provider, workflowElement);
                        selectionTypeDescriptor = provider;
                    }

                    return workflowElement;
                }
                return node.Value;
            }).ToArray();

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
            StartWorkflow();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopWorkflow();
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

                toolboxTreeView.Nodes.Clear();
                var searchFilter = searchTextBox.Text.Trim();
                foreach (var entry in from node in GetTreeViewLeafNodes(treeCache)
                                      where node.Tag != null && node.Text.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0
                                      orderby node.Text ascending
                                      select new { category = node.Parent.Text, node = (TreeNode)node.Clone() })
                {
                    entry.node.Text += string.Format(" ({0})", entry.category);
                    toolboxTreeView.Nodes.Add(entry.node);
                }

                if (toolboxTreeView.Nodes.Count > 0)
                {
                    toolboxTreeView.SelectedNode = toolboxTreeView.Nodes[0];
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
            if (e.Button == MouseButtons.Left && e.Node.GetNodeCount(false) == 0)
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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.E && e.Control)
            {
                searchTextBox.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
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
            undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
            redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
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

        class EditorSite : ISite, IWorkflowEditorService, IUIService
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

                if (serviceType == typeof(IWorkflowEditorService))
                {
                    return this;
                }

                if (serviceType == typeof(IUIService))
                {
                    return this;
                }

                return null;
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

            public void StartWorkflow()
            {
                siteForm.StartWorkflow();
            }

            public void StopWorkflow()
            {
                siteForm.StopWorkflow();
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

        #endregion
    }
}
