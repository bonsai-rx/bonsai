using Bonsai.Editor.Themes;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Bonsai.Editor
{
    sealed class EditorSettings
    {
        const int MaxRecentFiles = 25;
        const string SettingsFileName = "Bonsai.exe.settings";
        public static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
        static readonly Lazy<EditorSettings> instance = new Lazy<EditorSettings>(Load);
        readonly RecentlyUsedFileCollection recentlyUsedFiles = new RecentlyUsedFileCollection(MaxRecentFiles);
        readonly string settingsPath;

        internal EditorSettings(string path)
        {
            AnnotationPanelSize = 400;
            ExplorerSplitterDistance = 300;
            settingsPath = path;
        }

        public static EditorSettings Instance
        {
            get { return instance.Value; }
        }

        public Rectangle DesktopBounds { get; set; }

        public FormWindowState WindowState { get; set; }

        public ColorTheme EditorTheme { get; set; }

        public int AnnotationPanelSize { get; set; }

        public int ExplorerSplitterDistance { get; set; }

        public RecentlyUsedFileCollection RecentlyUsedFiles
        {
            get { return recentlyUsedFiles; }
        }

        static EditorSettings Load()
        {
            var assemblyLocation = Assembly.GetEntryAssembly()?.Location;
            var settingsPath = !string.IsNullOrEmpty(assemblyLocation)
                ? Path.Combine(Path.GetDirectoryName(assemblyLocation), SettingsFileName)
                : string.Empty;
            var settings = new EditorSettings(settingsPath);
            if (File.Exists(settingsPath))
            {
                try
                {
                    using (var reader = XmlReader.Create(settingsPath))
                    {
                        reader.MoveToContent();
                        while (reader.Read())
                        {
                            if (reader.NodeType != XmlNodeType.Element) continue;
                            if (reader.Name == nameof(WindowState))
                            {
                                Enum.TryParse(reader.ReadElementContentAsString(), out FormWindowState windowState);
                                settings.WindowState = windowState;
                            }
                            else if (reader.Name == nameof(EditorTheme))
                            {
                                Enum.TryParse(reader.ReadElementContentAsString(), out ColorTheme editorTheme);
                                settings.EditorTheme = editorTheme;
                            }
                            else if (reader.Name == nameof(AnnotationPanelSize))
                            {
                                int.TryParse(reader.ReadElementContentAsString(), out int annotationPanelSize);
                                settings.AnnotationPanelSize = annotationPanelSize;
                            }
                            else if (reader.Name == nameof(ExplorerSplitterDistance))
                            {
                                int.TryParse(reader.ReadElementContentAsString(), out int explorerSplitterDistance);
                                settings.ExplorerSplitterDistance = explorerSplitterDistance;
                            }
                            else if (reader.Name == nameof(DesktopBounds))
                            {
                                reader.ReadToFollowing(nameof(Rectangle.X));
                                int.TryParse(reader.ReadElementContentAsString(), out int x);
                                reader.ReadToFollowing(nameof(Rectangle.Y));
                                int.TryParse(reader.ReadElementContentAsString(), out int y);
                                reader.ReadToFollowing(nameof(Rectangle.Width));
                                int.TryParse(reader.ReadElementContentAsString(), out int width);
                                reader.ReadToFollowing(nameof(Rectangle.Height));
                                int.TryParse(reader.ReadElementContentAsString(), out int height);
                                settings.DesktopBounds = new Rectangle(x, y, width, height);
                            }
                            else if (reader.Name == nameof(RecentlyUsedFiles))
                            {
                                var fileReader = reader.ReadSubtree();
                                while (fileReader.ReadToFollowing(nameof(RecentlyUsedFile)))
                                {
                                    if (fileReader.Name == nameof(RecentlyUsedFile))
                                    {
                                        string fileName;
                                        fileReader.ReadToFollowing(nameof(RecentlyUsedFile.Timestamp));
                                        DateTimeOffset.TryParse(fileReader.ReadElementContentAsString(), out DateTimeOffset timestamp);
                                        fileReader.ReadToFollowing(nameof(RecentlyUsedFile.Name));
                                        fileName = fileReader.ReadElementContentAsString();
                                        settings.recentlyUsedFiles.Add(timestamp, fileName);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (XmlException) { }
            }

            return settings;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(settingsPath)) return;
            using (var writer = XmlWriter.Create(settingsPath, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartElement(typeof(EditorSettings).Name);
                writer.WriteElementString(nameof(WindowState), WindowState.ToString());
                writer.WriteElementString(nameof(EditorTheme), EditorTheme.ToString());
                writer.WriteElementString(nameof(AnnotationPanelSize), AnnotationPanelSize.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(nameof(ExplorerSplitterDistance), ExplorerSplitterDistance.ToString(CultureInfo.InvariantCulture));

                writer.WriteStartElement(nameof(DesktopBounds));
                writer.WriteElementString(nameof(Rectangle.X), DesktopBounds.X.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(nameof(Rectangle.Y), DesktopBounds.Y.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(nameof(Rectangle.Width), DesktopBounds.Width.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(nameof(Rectangle.Height), DesktopBounds.Height.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();

                if (recentlyUsedFiles.Count > 0)
                {
                    writer.WriteStartElement(nameof(RecentlyUsedFiles));
                    foreach (var file in recentlyUsedFiles)
                    {
                        writer.WriteStartElement(nameof(RecentlyUsedFile));
                        writer.WriteElementString(nameof(RecentlyUsedFile.Timestamp), file.Timestamp.ToString("o"));
                        writer.WriteElementString(nameof(RecentlyUsedFile.Name), file.FileName);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }
    }
}
