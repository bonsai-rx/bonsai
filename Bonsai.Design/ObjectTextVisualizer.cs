using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Bonsai;
using Bonsai.Design;
using System.Drawing;
using System.Reactive;
using System.Text.RegularExpressions;

[assembly: TypeVisualizer(typeof(ObjectTextVisualizer), Target = typeof(object))]

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a type visualizer for displaying any object type as text.
    /// </summary>
    public class ObjectTextVisualizer : BufferedVisualizer
    {
        const int AutoScaleHeight = 13;
        const float DefaultDpi = 96f;

        RichTextBox textBox;
        UserControl textPanel;
        Queue<string> buffer;
        int bufferSize;

        /// <inheritdoc/>
        protected override int TargetInterval => 1000 / 30;

        /// <inheritdoc/>
        protected override void ShowBuffer(IList<Timestamped<object>> values)
        {
            if (values.Count > 0)
            {
                base.ShowBuffer(values);
                textBox.Text = string.Join(Environment.NewLine, buffer);
                textPanel.Invalidate();
            }
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            value ??= string.Empty;
            var text = value.ToString();
            text = Regex.Replace(text, @"\r|\n", string.Empty);
            buffer.Enqueue(text);
            while (buffer.Count > bufferSize)
            {
                buffer.Dequeue();
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            buffer = new Queue<string>();
            textBox = new RichTextLabel { Dock = DockStyle.Fill };
            textBox.Multiline = true;
            textBox.WordWrap = false;
            textBox.ScrollBars = RichTextBoxScrollBars.Horizontal;
            textBox.MouseDoubleClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    buffer.Clear();
                    textBox.Text = string.Empty;
                    textPanel.Invalidate();
                }
            };

            textPanel = new UserControl();
            textPanel.SuspendLayout();
            textPanel.Dock = DockStyle.Fill;
            textPanel.MinimumSize = textPanel.Size = new Size(320, 2 * AutoScaleHeight);
            textPanel.AutoScaleDimensions = new SizeF(6F, AutoScaleHeight);
            textPanel.AutoScaleMode = AutoScaleMode.Font;
            textPanel.Paint += textPanel_Paint;
            textPanel.Controls.Add(textBox);
            textPanel.ResumeLayout(false);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(textPanel);
            }
        }

        void textPanel_Paint(object sender, PaintEventArgs e)
        {
            var lineHeight = AutoScaleHeight * e.Graphics.DpiY / DefaultDpi;
            bufferSize = (int)((textBox.ClientSize.Height - 2) / lineHeight);
            var textSize = TextRenderer.MeasureText(textBox.Text, textBox.Font);
            if (textBox.ClientSize.Width < textSize.Width)
            {
                var offset = 2 * lineHeight + SystemInformation.HorizontalScrollBarHeight - textPanel.Height;
                if (offset > 0)
                {
                    textPanel.Parent.Height += (int)offset;
                }
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            bufferSize = 0;
            textBox.Dispose();
            textBox = null;
            buffer = null;
        }
    }
}
