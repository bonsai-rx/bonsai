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
using System.Reactive.Disposables;
using System.Linq.Expressions;
using Bonsai.Editor.Properties;
using System.IO;

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        const string BonsaiExtension = ".bonsai";
        const string LayoutExtension = ".layout";
        const string BonsaiPackageName = "Bonsai";
        const string CombinatorCategoryName = "Combinator";
        const string ExpressionBuilderSuffix = "Builder";

        int version;
        int saveVersion;
        EditorSite editorSite;
        WorkflowBuilder workflowBuilder;
        WorkflowViewModel workflowViewModel;
        WorkflowSelectionModel selectionModel;

        XmlSerializer serializer;
        XmlSerializer layoutSerializer;
        Dictionary<Type, Type> typeVisualizers;

        IDisposable loaded;
        IDisposable running;

        public MainForm()
        {
            InitializeComponent();
            InitializeToolbox();

            editorSite = new EditorSite(this);
            workflowBuilder = new WorkflowBuilder();
            serializer = new XmlSerializer(typeof(WorkflowBuilder));
            layoutSerializer = new XmlSerializer(typeof(VisualizerLayout));
            typeVisualizers = TypeVisualizerLoader.GetTypeVisualizerDictionary();
            selectionModel = new WorkflowSelectionModel();
            workflowViewModel = new WorkflowViewModel(workflowGraphView, editorSite);
            workflowViewModel.Workflow = workflowBuilder.Workflow;
            propertyGrid.Site = editorSite;

            selectionModel.SetSelectedNode(workflowViewModel, null);
            selectionModel.SelectionChanged += new EventHandler(selectionModel_SelectionChanged);
        }

        #region Loading

        public string InitialFileName { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            if (!string.IsNullOrEmpty(InitialFileName) &&
                Path.GetExtension(InitialFileName) == BonsaiExtension &&
                File.Exists(InitialFileName))
            {
                OpenWorkflow(InitialFileName);
            }
            base.OnLoad(e);
        }

        #endregion

        #region Toolbox

        void InitializeToolbox()
        {
            var packages = WorkflowElementLoader.GetWorkflowElementTypes();
            var bonsaiPackage = packages.Single(package => package.Key == BonsaiPackageName);
            foreach (var package in packages)
            {
                if (package == bonsaiPackage) continue;
                InitializeToolboxCategory(package.Key, package);
            }

            InitializeToolboxCategory(CombinatorCategoryName, bonsaiPackage);
        }

        string GetPackageDisplayName(string packageKey)
        {
            return packageKey.Replace(BonsaiPackageName + ".", string.Empty);
        }

        string GetElementDisplayName(Type type)
        {
            var displayNameAttribute = (DisplayNameAttribute)TypeDescriptor.GetAttributes(type)[typeof(DisplayNameAttribute)];
            if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }
            else return type.IsSubclassOf(typeof(ExpressionBuilder)) ? RemoveSuffix(type.Name, ExpressionBuilderSuffix) : type.Name;
        }

        int GetElementTypeIndex(string typeName)
        {
            return
                typeName == WorkflowElementType.Source.ToString() ? 0 :
                typeName == WorkflowElementType.Filter.ToString() ? 1 :
                typeName == WorkflowElementType.Projection.ToString() ? 2 :
                typeName == WorkflowElementType.Sink.ToString() ? 3 : 4;
        }

        int CompareLoadableElementType(string left, string right)
        {
            return GetElementTypeIndex(left).CompareTo(GetElementTypeIndex(right));
        }

        //TODO: Remove duplicate method from ExpressionBuilderTypeConverter.cs
        string RemoveSuffix(string source, string suffix)
        {
            var suffixStart = source.LastIndexOf(suffix);
            return suffixStart >= 0 ? source.Remove(suffixStart) : source;
        }

        void InitializeToolboxCategory(string categoryName, IEnumerable<Type> types)
        {
            TreeNode category = null;

            foreach (var type in types.OrderBy(type => type.Name))
            {
                foreach (var elementType in WorkflowElementType.FromType(type))
                {
                    var name = GetElementDisplayName(type);
                    if (category == null)
                    {
                        category = toolboxTreeView.Nodes.Add(categoryName, GetPackageDisplayName(categoryName));
                    }

                    var elementTypeNode = categoryName == CombinatorCategoryName ? category : category.Nodes[elementType.ToString()];
                    if (elementTypeNode == null)
                    {
                        int index;
                        for (index = 0; index < category.Nodes.Count; index++)
                        {
                            if (CompareLoadableElementType(elementType.ToString(), category.Nodes[index].Name) <= 0)
                            {
                                break;
                            }
                        }

                        elementTypeNode = category.Nodes.Insert(index, elementType.ToString(), elementType.ToString());
                    }

                    var node = elementTypeNode.Nodes.Add(type.AssemblyQualifiedName, name);
                    node.Tag = elementType;
                }
            }
        }

        #endregion

        #region File Menu

        void ResetProjectStatus(string currentDirectory)
        {
            commandExecutor.Clear();
            version = 0;
            saveVersion = 0;
            Environment.CurrentDirectory = currentDirectory;
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

        void OpenWorkflow(string fileName)
        {
            saveWorkflowDialog.FileName = fileName;
            using (var reader = XmlReader.Create(fileName))
            {
                workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);
                workflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.ToInspectableGraph());
                ResetProjectStatus(Path.GetDirectoryName(fileName));
            }

            var layoutPath = GetLayoutPath(fileName);
            if (File.Exists(layoutPath))
            {
                using (var reader = XmlReader.Create(layoutPath))
                {
                    workflowViewModel.VisualizerLayout = (VisualizerLayout)layoutSerializer.Deserialize(reader);
                }
            }

            workflowViewModel.Workflow = workflowBuilder.Workflow;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckUnsavedChanges()) return;

            saveWorkflowDialog.FileName = null;
            workflowBuilder.Workflow.Clear();
            workflowViewModel.Workflow = workflowBuilder.Workflow;
            ResetProjectStatus(Path.GetDirectoryName(Application.ExecutablePath));
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
                using (var writer = XmlWriter.Create(saveWorkflowDialog.FileName, new XmlWriterSettings { Indent = true }))
                {
                    var serializerWorkflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.FromInspectableGraph());
                    serializer.Serialize(writer, serializerWorkflowBuilder);
                    saveVersion = version;
                    Environment.CurrentDirectory = Path.GetDirectoryName(saveWorkflowDialog.FileName);
                }

                if (workflowViewModel.VisualizerLayout != null)
                {
                    var layoutPath = GetLayoutPath(saveWorkflowDialog.FileName);
                    using (var writer = XmlWriter.Create(layoutPath, new XmlWriterSettings { Indent = true }))
                    {
                        layoutSerializer.Serialize(writer, workflowViewModel.VisualizerLayout);
                    }
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

        IEnumerable<Node<ExpressionBuilder, ExpressionBuilderParameter>> FlattenHierarchy(ExpressionBuilderGraph workflow)
        {
            foreach (var node in workflow)
            {
                yield return node;

                var workflowExpressionBuilder = node.Value as WorkflowExpressionBuilder;
                if (workflowExpressionBuilder != null)
                {
                    foreach (var childNode in FlattenHierarchy(workflowExpressionBuilder.Workflow))
                    {
                        yield return childNode;
                    }
                }
            }
        }

        void StartWorkflow()
        {
            if (running == null)
            {
                try
                {
                    var runningWorkflow = workflowBuilder.Workflow.Build();
                    var subscribeExpression = runningWorkflow.BuildSubscribe(HandleWorkflowError);
                    loaded = runningWorkflow.Load();

                    var subscriber = subscribeExpression.Compile();
                    var sourceConnections = workflowBuilder.GetSources().Select(source => source.Connect());
                    running = new CompositeDisposable(Enumerable.Repeat(subscriber(), 1).Concat(sourceConnections));
                    editorSite.OnWorkflowStarted(EventArgs.Empty);
                }
                catch (InvalidOperationException ex) { HandleWorkflowError(ex); return; }
                catch (ArgumentException ex) { HandleWorkflowError(ex); return; }
            }

            runningStatusLabel.Text = Resources.RunningStatus;
        }

        void StopWorkflow()
        {
            if (running != null)
            {
                running.Dispose();
                loaded.Dispose();
                loaded = null;
                running = null;
                workflowViewModel.UpdateVisualizerLayout();
                editorSite.OnWorkflowStopped(EventArgs.Empty);
            }

            runningStatusLabel.Text = Resources.StoppedStatus;
        }

        void HandleWorkflowError(Exception e)
        {
            MessageBox.Show(e.Message, "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (running != null)
            {
                BeginInvoke((Action)StopWorkflow);
            }
        }

        #endregion

        #region Workflow Controller

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
            var node = selectionModel.SelectedNode;
            if (node != null && node.Value != null)
            {
                var loadableElement = node.Value.GetType().GetProperties().FirstOrDefault(property =>
                {
                    var browsable = typeof(LoadableElement).IsAssignableFrom(property.PropertyType);
                    if (browsable)
                    {
                        var browsableAttributes = property.GetCustomAttributes(typeof(BrowsableAttribute), true);
                        if (browsableAttributes != null && browsableAttributes.Length > 0)
                        {
                            var browsableAttribute = (BrowsableAttribute)browsableAttributes[0];
                            browsable = browsableAttribute.Browsable;
                        }
                    }

                    return browsable;
                });

                if (loadableElement != null)
                {
                    propertyGrid.SelectedObject = loadableElement.GetValue(node.Value, null);
                }
                else propertyGrid.SelectedObject = node.Value;
            }
            else propertyGrid.SelectedObject = null;
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
            var model = selectionModel.SelectedModel;
            if (model != null)
            {
                var node = selectionModel.SelectedNode;
                model.DeleteGraphNode(node);
            }
        }

        private void toolboxTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && toolboxTreeView.SelectedNode != null && toolboxTreeView.SelectedNode.GetNodeCount(false) == 0)
            {
                var typeNode = toolboxTreeView.SelectedNode;
                var model = selectionModel.SelectedModel;
                if (model != null)
                {
                    model.CreateGraphNode(typeNode, selectionModel.SelectedNode, e.Modifiers == Keys.Control);
                }
            }
        }

        private void toolboxTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Node.GetNodeCount(false) == 0)
            {
                var typeNode = e.Node;
                var model = selectionModel.SelectedModel;
                if (model != null)
                {
                    model.CreateGraphNode(typeNode, selectionModel.SelectedNode, Control.ModifierKeys == Keys.Control);
                }
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
            version -= 2;
            commandExecutor.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            commandExecutor.Redo();
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

        #endregion

        #region EditorSite Class

        class EditorSite : ISite, IWorkflowEditorService
        {
            MainForm siteForm;

            public EditorSite(MainForm form)
            {
                siteForm = form;
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
                    var selectedModel = siteForm.selectionModel.SelectedModel;
                    return selectedModel != null ? selectedModel.Workflow : null;
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

                return null;
            }

            public Type GetTypeVisualizer(Type targetType)
            {
                Type visualizerType;
                siteForm.typeVisualizers.TryGetValue(targetType, out visualizerType);
                return visualizerType;
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
        }

        #endregion
    }
}
