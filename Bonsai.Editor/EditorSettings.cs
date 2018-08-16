using Bonsai.Editor.Themes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Editor
{
    sealed class EditorSettings
    {
        const int MaxRecentFiles = 25;
        const string RecentlyUsedFilesElement = "RecentlyUsedFiles";
        const string DesktopBoundsElement = "DesktopBounds";
        const string WindowStateElement = "WindowState";
        const string RecentlyUsedFileElement = "RecentlyUsedFile";
        const string FileTimestampElement = "Timestamp";
        const string FileNameElement = "Name";
        const string RectangleXElement = "X";
        const string RectangleYElement = "Y";
        const string RectangleWidthElement = "Width";
        const string RectangleHeightElement = "Height";
        const string EditorThemeElement = "EditorTheme";
        const string SettingsFileName = "Bonsai.exe.settings";
        static readonly string SettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SettingsFileName);
        static readonly Lazy<EditorSettings> instance = new Lazy<EditorSettings>(Load);
        readonly RecentlyUsedFileCollection recentlyUsedFiles = new RecentlyUsedFileCollection(MaxRecentFiles);

        internal EditorSettings()
        {
        }

        public static EditorSettings Instance
        {
            get { return instance.Value; }
        }

        public Rectangle DesktopBounds { get; set; }

        public FormWindowState WindowState { get; set; }

        public ColorTheme EditorTheme { get; set; }

        public RecentlyUsedFileCollection RecentlyUsedFiles
        {
            get { return recentlyUsedFiles; }
        }

        static EditorSettings Load()
        {
            var settings = new EditorSettings();
            if (File.Exists(SettingsPath))
            {
                try
                {
                    using (var reader = XmlReader.Create(SettingsPath))
                    {
                        reader.MoveToContent();
                        while (reader.Read())
                        {
                            if (reader.NodeType != XmlNodeType.Element) continue;
                            if (reader.Name == WindowStateElement)
                            {
                                FormWindowState windowState;
                                Enum.TryParse<FormWindowState>(reader.ReadElementContentAsString(), out windowState);
                                settings.WindowState = windowState;
                            }
                            else if (reader.Name == EditorThemeElement)
                            {
                                ColorTheme editorTheme;
                                Enum.TryParse<ColorTheme>(reader.ReadElementContentAsString(), out editorTheme);
                                settings.EditorTheme = editorTheme;
                            }
                            else if (reader.Name == DesktopBoundsElement)
                            {
                                int x, y, width, height;
                                reader.ReadToFollowing(RectangleXElement);
                                int.TryParse(reader.ReadElementContentAsString(), out x);
                                reader.ReadToFollowing(RectangleYElement);
                                int.TryParse(reader.ReadElementContentAsString(), out y);
                                reader.ReadToFollowing(RectangleWidthElement);
                                int.TryParse(reader.ReadElementContentAsString(), out width);
                                reader.ReadToFollowing(RectangleHeightElement);
                                int.TryParse(reader.ReadElementContentAsString(), out height);
                                settings.DesktopBounds = new Rectangle(x, y, width, height);
                            }
                            else if (reader.Name == RecentlyUsedFilesElement)
                            {
                                var fileReader = reader.ReadSubtree();
                                while (fileReader.ReadToFollowing(RecentlyUsedFileElement))
                                {
                                    if (fileReader.Name == RecentlyUsedFileElement)
                                    {
                                        string fileName;
                                        DateTimeOffset timestamp;
                                        fileReader.ReadToFollowing(FileTimestampElement);
                                        DateTimeOffset.TryParse(fileReader.ReadElementContentAsString(), out timestamp);
                                        fileReader.ReadToFollowing(FileNameElement);
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
            using (var writer = XmlWriter.Create(SettingsPath, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartElement(typeof(EditorSettings).Name);
                writer.WriteElementString(WindowStateElement, WindowState.ToString());
                writer.WriteElementString(EditorThemeElement, EditorTheme.ToString());

                writer.WriteStartElement(DesktopBoundsElement);
                writer.WriteElementString(RectangleXElement, DesktopBounds.X.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(RectangleYElement, DesktopBounds.Y.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(RectangleWidthElement, DesktopBounds.Width.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(RectangleHeightElement, DesktopBounds.Height.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();

                if (recentlyUsedFiles.Count > 0)
                {
                    writer.WriteStartElement(RecentlyUsedFilesElement);
                    foreach (var file in recentlyUsedFiles)
                    {
                        writer.WriteStartElement(RecentlyUsedFileElement);
                        writer.WriteElementString(FileTimestampElement, file.Timestamp.ToString("o"));
                        writer.WriteElementString(FileNameElement, file.FileName);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }
    }
}
