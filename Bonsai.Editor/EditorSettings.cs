using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Editor
{
    sealed class EditorSettings
    {
        const string SettingsExtension = ".settings";
        const string ShowWelcomeDialogElement = "ShowWelcomeDialog";
        static readonly string SettingsFileName = typeof(EditorSettings).Assembly.Location + SettingsExtension;
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

        static EditorSettings Load()
        {
            var settings = new EditorSettings();
            if (File.Exists(SettingsFileName))
            {
                try
                {
                    using (var reader = XmlReader.Create(SettingsFileName))
                    {
                        bool showWelcomeDialog;
                        reader.ReadToFollowing(ShowWelcomeDialogElement);
                        bool.TryParse(reader.ReadElementContentAsString(), out showWelcomeDialog);
                        settings.ShowWelcomeDialog = showWelcomeDialog;
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
                writer.WriteEndElement();
            }
        }
    }
}
