using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bonsai;
using Bonsai.Design;
using System.Drawing;

[assembly: TypeVisualizer(typeof(ObjectTextVisualizer), Target = typeof(object))]

namespace Bonsai.Design
{
    public class ObjectTextVisualizer : DialogTypeVisualizer
    {
        const int AutoScaleHeight = 13;
        const float DefaultDpi = 96f;

        TextBox textBox;
        UserControl textPanel;
        Queue<string> buffer;
        int bufferSize;

        public override void Show(object value)
        {
            value = value ?? string.Empty;
            buffer.Enqueue(value.ToString());
            while (buffer.Count > bufferSize)
            {
                buffer.Dequeue();
            }
            textBox.Text = string.Join(Environment.NewLine, buffer);
        }

        public override void Load(IServiceProvider provider)
        {
            buffer = new Queue<string>();
            textBox = new TextBox { Dock = DockStyle.Fill };
            textBox.ReadOnly = true;
            textBox.Multiline = true;
            textBox.WordWrap = false;
            textBox.TextChanged += (sender, e) => textPanel.Invalidate();

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
            if (textBox.ScrollBars == ScrollBars.None && textBox.ClientSize.Width < textSize.Width)
            {
                textBox.ScrollBars = ScrollBars.Horizontal;
                var offset = 2 * lineHeight + SystemInformation.HorizontalScrollBarHeight - textPanel.Height;
                if (offset > 0)
                {
                    textPanel.Parent.Height += (int)offset;
                }
            }
        }

        public override void Unload()
        {
            bufferSize = 0;
            textBox.Dispose();
            textBox = null;
            buffer = null;
        }
    }
}
