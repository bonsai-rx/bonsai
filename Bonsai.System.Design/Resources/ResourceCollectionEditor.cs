﻿using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Bonsai.Resources.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for editing a collection
    /// of resources, supporting drag and drop of compatible file extensions.
    /// </summary>
    public class ResourceCollectionEditor : CollectionEditor
    {
        string[] supportedExtensions;
        Dictionary<string, Func<string, object>> resourceConstructors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCollectionEditor"/> class
        /// using the specified collection type.
        /// </summary>
        /// <inheritdoc/>
        public ResourceCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// Gets the file extensions that support drag and drop into the collection editor.
        /// </summary>
        protected string[] SupportedExtensions
        {
            get
            {
                if (supportedExtensions == null)
                {
                    supportedExtensions = CreateSupportedExtensions();
                }

                return supportedExtensions;
            }
        }

        static IEnumerable<string> ParseFilterExtensions(string filter)
        {
            var entries = filter.Split(new[] { '|' }, StringSplitOptions.None);
            for (int i = 1; i < entries.Length; i += 2)
            {
                var extensions = entries[i].Split(new[] { ';' }, StringSplitOptions.None);
                for (int j = 0; j < extensions.Length; j++)
                {
                    yield return extensions[j].TrimStart('*');
                }
            }
        }

        /// <summary>
        /// Gets the file extensions that support drag and drop into the collection editor.
        /// </summary>
        /// <returns>
        /// The array of file extensions that support drag and drop into the collection
        /// editor.
        /// </returns>
        protected virtual string[] CreateSupportedExtensions()
        {
            var newItemTypes = NewItemTypes;
            if (newItemTypes == null || newItemTypes.Length == 0) return new string[0];

            var extensions = from type in newItemTypes
                             from property in type.GetProperties()
                             where property.PropertyType == typeof(string)
                             let filterAttribute = property.GetCustomAttribute<FileNameFilterAttribute>()
                             where filterAttribute != null
                             from extension in ParseFilterExtensions(filterAttribute.Filter).Distinct()
                             select new { extension, type, property };

            resourceConstructors = new Dictionary<string, Func<string, object>>();
            foreach (var item in extensions)
            {
                var itemType = item.type;
                var itemProperty = item.property;
                if (item.extension.Contains('*') || resourceConstructors.ContainsKey(item.extension)) continue;
                var nameProperty = itemType.GetProperty("Name");
                resourceConstructors[item.extension] = fileName =>
                {
                    var instance = Activator.CreateInstance(itemType);
                    itemProperty.SetValue(instance, PathConvert.GetProjectPath(fileName));
                    if (nameProperty != null)
                    {
                        nameProperty.SetValue(instance, Path.GetFileNameWithoutExtension(fileName));
                    }
                    return instance;
                };
            }

            return extensions.Select(item => item.extension).ToArray();
        }

        private bool IsResourceSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return Array.Exists(SupportedExtensions, extension.Equals);
        }

        /// <summary>
        /// Creates a new resource configuration object from a specified file name.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file from which the resource configuration object should
        /// be created.
        /// </param>
        /// <returns>
        /// A new resource configuration object to be added to the collection.
        /// </returns>
        protected virtual object CreateResourceConfiguration(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (resourceConstructors != null && resourceConstructors.TryGetValue(extension, out Func<string, object> constructor))
            {
                return constructor(fileName);
            }

            return null;
        }

        /// <inheritdoc/>
        protected override CollectionEditorDialog CreateEditorDialog()
        {
            var editorDialog = base.CreateEditorDialog();
            var editorControl = editorDialog.EditorControl;
            editorControl.AllowDrop = true;
            editorControl.DragEnter += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    e.Effect = Array.Exists(fileNames, IsResourceSupported) ? DragDropEffects.Copy : DragDropEffects.None;
                }
            };

            editorControl.DragDrop += (sender, e) =>
            {
                if (e.Effect == DragDropEffects.Copy && e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        var path = fileNames[i];
                        if (IsResourceSupported(path))
                        {
                            var item = CreateResourceConfiguration(path);
                            if (item != null)
                            {
                                editorControl.AddItem(item);
                            }
                        }
                    }
                }
            };
            return editorDialog;
        }
    }
}
