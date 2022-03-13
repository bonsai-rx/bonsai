using System;
using System.Windows.Forms;
using OpenCV.Net;
using System.Globalization;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Represents a graphics accelerated video player control.
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        bool playing;
        int frameCount;
        double playbackRate;
        readonly ToolStripButton loopButton;
        readonly ToolStripStatusLabel statusLabel;
        readonly ToolStripStatusLabel frameNumberHeaderLabel;
        readonly ToolStripStatusLabel frameNumberLabel;
        readonly ToolStripTextBox frameNumberTextBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPlayer"/> class.
        /// </summary>
        public VideoPlayer()
        {
            InitializeComponent();
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

        /// <summary>
        /// Gets the graphics canvas used to render video frames.
        /// </summary>
        public VisualizerCanvas Canvas
        {
            get { return imageControl; }
        }

        /// <summary>
        /// Gets or sets the number of frames in the video.
        /// </summary>
        public int FrameCount
        {
            get { return frameCount; }
            set
            {
                frameCount = value;
                seekBar.Maximum = frameCount + seekBar.LargeChange - 2;
            }
        }

        /// <summary>
        /// Gets or sets the speed, in frames per second, at which to play
        /// images from the video.
        /// </summary>
        public double PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                playbackRate = value;
                OnPlaybackRateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the video should loop
        /// when the end of the file is reached.
        /// </summary>
        public bool Loop
        {
            get { return loopButton.CheckState == CheckState.Checked; }
            set { loopButton.CheckState = value ? CheckState.Checked : CheckState.Unchecked; }
        }

        /// <summary>
        /// Gets or sets a value specifying whether the video is playing.
        /// </summary>
        public bool Playing
        {
            get { return playing; }
            set
            {
                playing = value;
                OnPlayingChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the user moves the video seek bar.
        /// </summary>
        public event EventHandler<SeekEventArgs> Seek;

        /// <summary>
        /// Occurs when the <see cref="PlaybackRate"/> property value changes.
        /// </summary>
        public event EventHandler PlaybackRateChanged;

        /// <summary>
        /// Occurs when the <see cref="Playing"/> property value changes.
        /// </summary>
        public event EventHandler PlayingChanged;

        /// <summary>
        /// Occurs when the <see cref="Loop"/> property value changes.
        /// </summary>
        public event EventHandler LoopChanged
        {
            add { loopButton.CheckStateChanged += value; }
            remove { loopButton.CheckStateChanged -= value; }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) return;
            base.OnLoad(e);
        }

        /// <summary>
        /// Raises the <see cref="Seek"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="SeekEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnSeek(SeekEventArgs e)
        {
            Seek?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PlayingChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPlayingChanged(EventArgs e)
        {
            PlayingChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PlaybackRateChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPlaybackRateChanged(EventArgs e)
        {
            PlaybackRateChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Updates the video player control with the specified frame.
        /// </summary>
        /// <param name="frame">
        /// An <see cref="IplImage"/> object containing the pixel data of the
        /// current video frame.
        /// </param>
        /// <param name="frameNumber">
        /// The zero-based index of the current video frame.
        /// </param>
        public void Update(IplImage frame, int frameNumber)
        {
            imageControl.Image = frame;
            seekBar.Value = frameNumber;
            if (frame == null) statusLabel.Text = string.Empty;
            frameNumberLabel.Text = frameNumber.ToString(CultureInfo.CurrentCulture);
        }

        /// <inheritdoc/>
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
