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
        TextBox textBox;
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
            textBox = new TextBox();
            textBox.ReadOnly = true;
            textBox.Multiline = true;
            textBox.WordWrap = false;
            textBox.HideSelection = true;
            textBox.Anchor |= AnchorStyles.Right | AnchorStyles.Bottom;
            textBox.TextChanged += textBox_Changed;
            textBox.ClientSizeChanged += textBox_Changed;
            textBox.ClientSize = new Size(320, 2 * textBox.Font.Height);
            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(textBox);
            }
        }

        void textBox_Changed(object sender, EventArgs e)
        {
            bufferSize = (textBox.ClientSize.Height - 2) / textBox.Font.Height;
            var textSize = TextRenderer.MeasureText(textBox.Text, textBox.Font);
            if (textBox.ClientSize.Width < textSize.Width)
            {
                textBox.ScrollBars = ScrollBars.Horizontal;
            }
            else textBox.ScrollBars = ScrollBars.None;
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
