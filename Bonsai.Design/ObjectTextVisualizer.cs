using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Bonsai;
using Bonsai.Design;
using System.Diagnostics;
using System.Drawing;
using System.Reactive;
using System.Text;

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
        int bufferSize = 1;

        /// <inheritdoc/>
        protected override int TargetInterval => 1000 / 30;

        /// <inheritdoc/>
        protected override void ShowBuffer(IList<Timestamped<object>> values)
        {
            if (values.Count == 0)
                return;

            var sb = new StringBuilder();

            // Trim old values if bufferSize was reduced
            while (buffer.Count >= bufferSize)
                buffer.Dequeue();

            // Add new values to the buffer (and only the ones which might appear)
            for (int i = Math.Max(0, values.Count - bufferSize); i < values.Count; i++)
            {
                sb.Clear();
                AppendDisplayText(sb, values[i].Value);
                AppendString(sb.ToString());
            }

            // Update the visual representation of the buffer
            RefreshVisualization(sb);
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            // Updates to this visualizer are expected to go through ShowBuffer
            Debug.Fail($"Likely unintentional call to {nameof(ObjectTextVisualizer)}.{nameof(Show)}");

            var stringBuilder = new StringBuilder();
            AppendDisplayText(stringBuilder, value);
            AppendString(stringBuilder.ToString());
            RefreshVisualization(stringBuilder);
        }

        /// <summary>
        /// Appends the display text for the specified object to the text buffer.
        /// </summary>
        /// <param name="stringBuilder">The string builder which receives the display text.</param>
        /// <param name="value">The object for which to retrieve the display text.</param>
        protected virtual void AppendDisplayText(StringBuilder stringBuilder, object value)
        {
            string rawText = value?.ToString() ?? string.Empty;
            stringBuilder.EnsureCapacity(stringBuilder.Length + rawText.Length);

            foreach (var c in rawText)
            {
                switch (c)
                {
                    // Carriage returns are presumed to be followed by line feeds, skip them entirely
                    case '\r':
                        continue;
                    // Newlines become space so that things like multi-line JSON or matrices are still visible
                    case '\n':
                    case '\x0085': // Next line character (NEL)
                    case '\x2028': // Unicode line separator
                    case '\x2029': // Unicode paragraph separator
                        stringBuilder.Append(' ');
                        break;
                    case '\t':
                        stringBuilder.Append(c);
                        break;
                    // Replace all other control characters with the "�" replacement character
                    case < ' ': // C0 control characters
                    case '\x007F': // Delete
                    case >= '\x0080' and <= '\x009F': // C1 control characters
                        stringBuilder.Append('\xFFFD');
                        break;
                    default:
                        stringBuilder.Append(c);
                        break;
                }
            }
        }

        private void AppendString(string value)
        {
            if (buffer.Count >= bufferSize)
                buffer.Dequeue();

            buffer.Enqueue(value);
        }

        private void RefreshVisualization(StringBuilder stringBuilder)
        {
            Debug.Assert(buffer.Count <= bufferSize);
            stringBuilder.Clear();
            foreach (var line in buffer)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(line);
            }
            textBox.Text = stringBuilder.ToString();
            textPanel.Invalidate();
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
            bufferSize = Math.Max(1, (int)((textBox.ClientSize.Height - 2) / lineHeight));
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
            textPanel.Dispose();
            textBox = null;
            textPanel = null;
            buffer = null;
        }
    }
}
