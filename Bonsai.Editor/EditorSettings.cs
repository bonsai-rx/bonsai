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
        const string SettingsExtension = ".settings";
        const string ShowWelcomeDialogElement = "ShowWelcomeDialog";
        const string DesktopBoundsElement = "DesktopBounds";
        const string WindowStateElement = "WindowState";
        const string RectangleXElement = "X";
        const string RectangleYElement = "Y";
        const string RectangleWidthElement = "Width";
        const string RectangleHeightElement = "Height";
        static readonly string SettingsFileName = Assembly.GetEntryAssembly().Location + SettingsExtension;
        static readonly Lazy<EditorSettings> instance = new Lazy<EditorSettings>(Load);

        internal EditorSettings()
        {
            ShowWelcomeDialog = true;
        }

        public static EditorSettings Instance
        {
            get { return instance.Value; }
        }

        public bool ShowWelcomeDialog { get; set; }

        public Rectangle DesktopBounds { get; set; }

        public FormWindowState WindowState { get; set; }

        static EditorSettings Load()
        {
            var settings = new EditorSettings();
            if (File.Exists(SettingsFileName))
            {
                try
                {
                    using (var reader = XmlReader.Create(SettingsFileName))
                    {
                        reader.MoveToContent();
                        while (reader.Read())
                        {
                            if (reader.NodeType != XmlNodeType.Element) continue;
                            if (reader.Name == ShowWelcomeDialogElement)
                            {
                                bool showWelcomeDialog;
                                bool.TryParse(reader.ReadElementContentAsString(), out showWelcomeDialog);
                                settings.ShowWelcomeDialog = showWelcomeDialog;
                            }
                            else if (reader.Name == WindowStateElement)
                            {
                                FormWindowState windowState;
                                Enum.TryParse<FormWindowState>(reader.ReadElementContentAsString(), out windowState);
                                settings.WindowState = windowState;
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
                        }
                    }
                }
                catch (XmlException) { }
            }

            return settings;
        }

        public void Save()
        {
            using (var writer = XmlWriter.Create(SettingsFileName, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartElement(typeof(EditorSettings).Name);
                writer.WriteElementString(ShowWelcomeDialogElement, ShowWelcomeDialog.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(WindowStateElement, WindowState.ToString());
                writer.WriteStartElement(DesktopBoundsElement);
                writer.WriteElementString(RectangleXElement, DesktopBounds.X.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(RectangleYElement, DesktopBounds.Y.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(RectangleWidthElement, DesktopBounds.Width.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString(RectangleHeightElement, DesktopBounds.Height.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
