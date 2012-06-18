﻿using System;
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
using System.Windows.Forms.Design;

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
                directoryToolStripTextBox.Text = Environment.CurrentDirectory;
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
            saveWorkflowDialog.FileName = fileName;
            workflowBuilder = LoadWorkflow(fileName);
            ResetProjectStatus();

            var layoutPath = GetLayoutPath(fileName);
            if (File.Exists(layoutPath))
            {
                using (var reader = XmlReader.Create(layoutPath))
                {
                    workflowViewModel.VisualizerLayout = (VisualizerLayout)layoutSerializer.Deserialize(reader);
                }
            }
            else workflowViewModel.VisualizerLayout = null;

            workflowViewModel.Workflow = workflowBuilder.Workflow;
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
            workflowViewModel.VisualizerLayout = null;
            workflowViewModel.Workflow = workflowBuilder.Workflow;
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
                using (var writer = XmlWriter.Create(saveWorkflowDialog.FileName, new XmlWriterSettings { Indent = true }))
                {
                    var serializerWorkflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.FromInspectableGraph());
                    serializer.Serialize(writer, serializerWorkflowBuilder);
                    saveVersion = version;
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

        void StartWorkflow()
        {
            if (running == null)
            {
                try
                {
                    var runningWorkflow = workflowBuilder.Workflow.Build();
                    var subscribeExpression = runningWorkflow.BuildSubscribe(HandleWorkflowError, WorkflowCompleted);
                    loaded = runningWorkflow.Load();

                    var subscriber = subscribeExpression.Compile();
                    running = subscriber();

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
                editorSite.OnWorkflowStopped(EventArgs.Empty);

                loaded.Dispose();
                loaded = null;
                running = null;
                workflowViewModel.UpdateVisualizerLayout();
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

        void WorkflowCompleted()
        {
            running.Dispose();
            BeginInvoke((Action)StopWorkflow);
        }

        #endregion

        #region Workflow Controller

        private void DeleteSelectedNode()
        {
            var model = selectionModel.SelectedModel;
            if (model != null && model.WorkflowGraphView.Focused)
            {
                var node = selectionModel.SelectedNode;
                model.DeleteGraphNode(node);
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
            DeleteSelectedNode();
        }

        private void toolboxTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && toolboxTreeView.SelectedNode != null && toolboxTreeView.SelectedNode.GetNodeCount(false) == 0)
            {
                var typeNode = toolboxTreeView.SelectedNode;
                var model = selectionModel.SelectedModel;
                if (model != null)
                {
                    var branch = e.Modifiers.HasFlag(Keys.Control);
                    var predecessor = e.Modifiers.HasFlag(Keys.Shift) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                    model.CreateGraphNode(typeNode, selectionModel.SelectedNode, predecessor, branch);
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
                    var branch = Control.ModifierKeys.HasFlag(Keys.Control);
                    var predecessor = Control.ModifierKeys.HasFlag(Keys.Shift) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                    model.CreateGraphNode(typeNode, selectionModel.SelectedNode, predecessor, branch);
                }
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedNode();
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
                MessageBox.Show(ex.Message, siteForm.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
