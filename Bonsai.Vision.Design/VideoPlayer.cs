using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCV.Net;
using System.Globalization;
using OpenTK;

namespace Bonsai.Vision.Design
{
    public partial class VideoPlayer : UserControl
    {
        bool playing;
        int frameCount;
        double playbackRate;
        volatile bool allowUpdate;
        ToolStripButton loopButton;
        ToolStripStatusLabel statusLabel;
        ToolStripStatusLabel frameNumberHeaderLabel;
        ToolStripStatusLabel frameNumberLabel;
        ToolStripTextBox frameNumberTextBox;

        public VideoPlayer()
        {
            InitializeComponent();
            allowUpdate = true;
            var playButton = new ToolStripButton(">");
            var pauseButton = new ToolStripButton("| |");
            var slowerButton = new ToolStripButton("<<");
            var fasterButton = new ToolStripButton(">>");
            loopButton = new ToolStripButton("loop");
            loopButton.CheckOnClick = true;
            statusLabel = new ToolStripStatusLabel();
            frameNumberHeaderLabel = new ToolStripStatusLabel("Frame:");
            frameNumberLabel = new ToolStripStatusLabel();
            frameNumberLabel.DoubleClickEnabled = true;
            frameNumberLabel.DoubleClick += new EventHandler(frameNumberIndicator_DoubleClick);
            frameNumberTextBox = new ToolStripTextBox();
            frameNumberTextBox.GotFocus += (sender, e) => frameNumberTextBox.SelectAll();
            frameNumberTextBox.LostFocus += new EventHandler(frameNumberTextBox_LostFocus);
            frameNumberTextBox.KeyDown += new KeyEventHandler(frameNumberTextBox_KeyDown);
            statusStrip.Items.Add(playButton);
            statusStrip.Items.Add(pauseButton);
            statusStrip.Items.Add(slowerButton);
            statusStrip.Items.Add(fasterButton);
            statusStrip.Items.Add(loopButton);
            statusStrip.Items.Add(frameNumberHeaderLabel);
            statusStrip.Items.Add(frameNumberLabel);
            statusStrip.Items.Add(statusLabel);

            imageControl.Canvas.MouseClick += new MouseEventHandler(imageControl_MouseClick);
            playButton.Click += (sender, e) => Playing = true;
            pauseButton.Click += (sender, e) => Playing = false;
            slowerButton.Click += (sender, e) => DecreasePlaybackRate();
            fasterButton.Click += (sender, e) => IncreasePlaybackRate();
            imageControl.Canvas.MouseMove += (sender, e) =>
            {
                var image = imageControl.Image;
                if (image != null)
                {
                    var cursorPosition = imageControl.Canvas.PointToClient(Form.MousePosition);
                    if (imageControl.ClientRectangle.Contains(cursorPosition))
                    {
                        var imageX = (int)(cursorPosition.X * ((float)image.Width / imageControl.Width));
                        var imageY = (int)(cursorPosition.Y * ((float)image.Height / imageControl.Height));
                        var cursorColor = image[imageY, imageX];
                        statusLabel.Text = string.Format("Cursor: ({0},{1}) Value: ({2},{3},{4})", imageX, imageY, cursorColor.Val0, cursorColor.Val1, cursorColor.Val2);
                    }
                }
            };

            imageControl.Canvas.DoubleClick += (sender, e) =>
            {
                var image = imageControl.Image;
                if (image != null)
                {
                    Parent.ClientSize = new System.Drawing.Size(image.Width, image.Height);
                }
            };

            seekBar.Scroll += (sender, e) =>
            {
                if (e.Type != ScrollEventType.EndScroll)
                {
                    OnSeek(new SeekEventArgs(e.NewValue));
                }
            };
        }

        void frameNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                statusStrip.Select();
            }
        }

        void frameNumberTextBox_LostFocus(object sender, EventArgs e)
        {
            int frameNumber;
            statusStrip.SuspendLayout();
            statusStrip.Items.Remove(frameNumberTextBox);
            statusStrip.Items.Insert(statusStrip.Items.Count - 1, frameNumberLabel);
            if (frameNumberTextBox.Text != frameNumberLabel.Text &&
                int.TryParse(frameNumberTextBox.Text, out frameNumber))
            {
                OnSeek(new SeekEventArgs(frameNumber));
            }
            statusStrip.ResumeLayout();
        }

        void frameNumberIndicator_DoubleClick(object sender, EventArgs e)
        {
            statusStrip.SuspendLayout();
            statusStrip.Items.Remove(frameNumberLabel);
            statusStrip.Items.Insert(statusStrip.Items.Count - 1, frameNumberTextBox);
            frameNumberTextBox.Size = frameNumberLabel.Size;
            frameNumberTextBox.Text = frameNumberLabel.Text;
            statusStrip.ResumeLayout();
            frameNumberTextBox.Focus();
        }

        public int FrameCount
        {
            get { return frameCount; }
            set
            {
                frameCount = value;
                seekBar.Maximum = frameCount + seekBar.LargeChange - 2;
            }
        }

        public double PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                playbackRate = value;
                OnPlaybackRateChanged(EventArgs.Empty);
            }
        }

        public bool Loop
        {
            get { return loopButton.CheckState == CheckState.Checked; }
            set { loopButton.CheckState = value ? CheckState.Checked : CheckState.Unchecked; }
        }

        public bool Playing
        {
            get { return playing; }
            set
            {
                playing = value;
                OnPlayingChanged(EventArgs.Empty);
            }
        }

        public event EventHandler<SeekEventArgs> Seek;

        public event EventHandler PlaybackRateChanged;

        public event EventHandler PlayingChanged;

        public event EventHandler LoopChanged
        {
            add { loopButton.CheckStateChanged += value; }
            remove { loopButton.CheckStateChanged -= value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) return;
            var refreshRate = DisplayDevice.Default.RefreshRate;
            updateTimer.Interval = Math.Max(1, (int)(500 / refreshRate));
            updateTimer.Start();
            base.OnLoad(e);
        }

        protected virtual void OnSeek(SeekEventArgs e)
        {
            var handler = Seek;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPlayingChanged(EventArgs e)
        {
            var handler = PlayingChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPlaybackRateChanged(EventArgs e)
        {
            var handler = PlaybackRateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Update(IplImage frame, int frameNumber)
        {
            if (!allowUpdate) return;
            else
            {
                allowUpdate = false;
                BeginInvoke((Action)(() =>
                {
                    imageControl.Image = frame;
                    seekBar.Value = frameNumber;
                    if (frame == null) statusLabel.Text = string.Empty;
                    frameNumberLabel.Text = frameNumber.ToString(CultureInfo.CurrentCulture);
                }));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!frameNumberTextBox.Focused)
            {
                if (keyData == Keys.Space)
                {
                    Playing = !Playing;
                }

                if (keyData == Keys.Add) IncreasePlaybackRate();
                if (keyData == Keys.Subtract) DecreasePlaybackRate();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            allowUpdate = true;
        }

        void imageControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                statusStrip.Visible = !statusStrip.Visible;
            }
        }

        void IncreasePlaybackRate()
        {
            PlaybackRate *= 2;
        }

        void DecreasePlaybackRate()
        {
            PlaybackRate /= 2;
        }
    }
}
