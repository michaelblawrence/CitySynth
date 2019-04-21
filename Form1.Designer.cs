namespace CitySynth.UI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.osc1WavePreviewBox = new System.Windows.Forms.PictureBox();
            this.touchPad = new System.Windows.Forms.PictureBox();
            this.hLine_filterVtouchpad = new System.Windows.Forms.PictureBox();
            this.hLine_lfoVdelay = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.recButton = new CitySynth.UI_Elements.SquareButton();
            this.closeWindowBtn = new CitySynth.UI_Elements.ToggleIcon();
            this.presetSelector = new CitySynth.UI_Elements.PresetSelector();
            this.lfoHpfToggle = new CitySynth.UI_Elements.ToggleIcon();
            this.ampLfoHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.lfoLpfToggle = new CitySynth.UI_Elements.ToggleIcon();
            this.lfoSendHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.touchPadEnableToggle = new CitySynth.UI_Elements.ToggleIcon();
            this.osc1WaveSelDial = new CitySynth.UI_Elements.Dial();
            this.envActivateButton = new CitySynth.UI_Elements.ToggleIcon();
            this.filterCutoffDial = new CitySynth.UI_Elements.Dial();
            this.fftFilterToggle = new CitySynth.UI_Elements.ToggleIcon();
            this.osc1PhaseDial = new CitySynth.UI_Elements.Dial();
            this.osc1GainDial = new CitySynth.UI_Elements.Dial();
            this.ampReleaseDial = new CitySynth.UI_Elements.Dial();
            this.ampLfoWidthDial = new CitySynth.UI_Elements.Dial();
            this.polyMonoSwitch = new CitySynth.UI_Elements.Switch();
            this.ampLfoRateDial = new CitySynth.UI_Elements.Dial();
            this.midiIndicator = new CitySynth.UI_Elements.Indicator();
            this.ampSustainDial = new CitySynth.UI_Elements.Dial();
            this.osc1HeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.envCeilingDial = new CitySynth.UI_Elements.Dial();
            this.touchPadHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.envReleaseDial = new CitySynth.UI_Elements.Dial();
            this.filterHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.envFloorDial = new CitySynth.UI_Elements.Dial();
            this.lfoHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.ampDecayDial = new CitySynth.UI_Elements.Dial();
            this.delayHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.envAttackDial = new CitySynth.UI_Elements.Dial();
            this.headerLabel1 = new CitySynth.UI_Elements.HeaderLabel();
            this.ampAttackDial = new CitySynth.UI_Elements.Dial();
            this.pitchHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.delayWetDial = new CitySynth.UI_Elements.Dial();
            this.ampHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.delayLengthDial = new CitySynth.UI_Elements.Dial();
            this.envHeaderLabel = new CitySynth.UI_Elements.HeaderLabel();
            this.pitchWidthDial = new CitySynth.UI_Elements.Dial();
            this.masterMeter = new CitySynth.UI_Elements.Meter();
            this.pitchRateDial = new CitySynth.UI_Elements.Dial();
            this.lfoWidthDial = new CitySynth.UI_Elements.Dial();
            this.masterLevelDial = new CitySynth.UI_Elements.Dial();
            this.lfoRateDial = new CitySynth.UI_Elements.Dial();
            ((System.ComponentModel.ISupportInitialize)(this.osc1WavePreviewBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.touchPad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hLine_filterVtouchpad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hLine_lfoVdelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // osc1WavePreviewBox
            // 
            this.osc1WavePreviewBox.BackColor = System.Drawing.Color.Transparent;
            this.osc1WavePreviewBox.BackgroundImage = global::CitySynth.Properties.Resources.ctpreviewboxbg;
            this.osc1WavePreviewBox.Image = global::CitySynth.Properties.Resources.ctpreview_sawtooth;
            this.osc1WavePreviewBox.Location = new System.Drawing.Point(131, 120);
            this.osc1WavePreviewBox.Name = "osc1WavePreviewBox";
            this.osc1WavePreviewBox.Size = new System.Drawing.Size(178, 51);
            this.osc1WavePreviewBox.TabIndex = 2;
            this.osc1WavePreviewBox.TabStop = false;
            // 
            // touchPad
            // 
            this.touchPad.BackColor = System.Drawing.Color.Transparent;
            this.touchPad.BackgroundImage = global::CitySynth.Properties.Resources.cttouchpadbg;
            this.touchPad.Location = new System.Drawing.Point(683, 323);
            this.touchPad.Name = "touchPad";
            this.touchPad.Size = new System.Drawing.Size(184, 147);
            this.touchPad.TabIndex = 0;
            this.touchPad.TabStop = false;
            this.touchPad.Click += new System.EventHandler(this.touchPad_Click);
            this.touchPad.DoubleClick += new System.EventHandler(this.touchPad_DoubleClick);
            this.touchPad.MouseEnter += new System.EventHandler(this.touchPad_MouseEnter);
            this.touchPad.MouseLeave += new System.EventHandler(this.touchPad_MouseLeave);
            this.touchPad.MouseMove += new System.Windows.Forms.MouseEventHandler(this.touchPad_MouseMove);
            // 
            // hLine_filterVtouchpad
            // 
            this.hLine_filterVtouchpad.BackColor = System.Drawing.Color.Transparent;
            this.hLine_filterVtouchpad.Image = global::CitySynth.Properties.Resources.ctlinehorizontal;
            this.hLine_filterVtouchpad.Location = new System.Drawing.Point(670, 315);
            this.hLine_filterVtouchpad.Name = "hLine_filterVtouchpad";
            this.hLine_filterVtouchpad.Size = new System.Drawing.Size(1, 155);
            this.hLine_filterVtouchpad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.hLine_filterVtouchpad.TabIndex = 7;
            this.hLine_filterVtouchpad.TabStop = false;
            // 
            // hLine_lfoVdelay
            // 
            this.hLine_lfoVdelay.BackColor = System.Drawing.Color.Transparent;
            this.hLine_lfoVdelay.Image = global::CitySynth.Properties.Resources.ctlinehorizontal;
            this.hLine_lfoVdelay.Location = new System.Drawing.Point(791, 110);
            this.hLine_lfoVdelay.Name = "hLine_lfoVdelay";
            this.hLine_lfoVdelay.Size = new System.Drawing.Size(1, 155);
            this.hLine_lfoVdelay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.hLine_lfoVdelay.TabIndex = 7;
            this.hLine_lfoVdelay.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::CitySynth.Properties.Resources.ctlinehorizontal;
            this.pictureBox1.Location = new System.Drawing.Point(331, 109);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1, 155);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.hLine_lfoVdelay);
            this.panel1.Controls.Add(this.recButton);
            this.panel1.Controls.Add(this.closeWindowBtn);
            this.panel1.Controls.Add(this.presetSelector);
            this.panel1.Controls.Add(this.lfoHpfToggle);
            this.panel1.Controls.Add(this.ampLfoHeaderLabel);
            this.panel1.Controls.Add(this.lfoLpfToggle);
            this.panel1.Controls.Add(this.lfoSendHeaderLabel);
            this.panel1.Controls.Add(this.touchPadEnableToggle);
            this.panel1.Controls.Add(this.osc1WaveSelDial);
            this.panel1.Controls.Add(this.envActivateButton);
            this.panel1.Controls.Add(this.filterCutoffDial);
            this.panel1.Controls.Add(this.fftFilterToggle);
            this.panel1.Controls.Add(this.osc1PhaseDial);
            this.panel1.Controls.Add(this.osc1GainDial);
            this.panel1.Controls.Add(this.ampReleaseDial);
            this.panel1.Controls.Add(this.osc1WavePreviewBox);
            this.panel1.Controls.Add(this.ampLfoWidthDial);
            this.panel1.Controls.Add(this.polyMonoSwitch);
            this.panel1.Controls.Add(this.ampLfoRateDial);
            this.panel1.Controls.Add(this.midiIndicator);
            this.panel1.Controls.Add(this.ampSustainDial);
            this.panel1.Controls.Add(this.osc1HeaderLabel);
            this.panel1.Controls.Add(this.envCeilingDial);
            this.panel1.Controls.Add(this.touchPadHeaderLabel);
            this.panel1.Controls.Add(this.envReleaseDial);
            this.panel1.Controls.Add(this.filterHeaderLabel);
            this.panel1.Controls.Add(this.envFloorDial);
            this.panel1.Controls.Add(this.lfoHeaderLabel);
            this.panel1.Controls.Add(this.ampDecayDial);
            this.panel1.Controls.Add(this.delayHeaderLabel);
            this.panel1.Controls.Add(this.envAttackDial);
            this.panel1.Controls.Add(this.headerLabel1);
            this.panel1.Controls.Add(this.ampAttackDial);
            this.panel1.Controls.Add(this.pitchHeaderLabel);
            this.panel1.Controls.Add(this.delayWetDial);
            this.panel1.Controls.Add(this.ampHeaderLabel);
            this.panel1.Controls.Add(this.delayLengthDial);
            this.panel1.Controls.Add(this.envHeaderLabel);
            this.panel1.Controls.Add(this.pitchWidthDial);
            this.panel1.Controls.Add(this.masterMeter);
            this.panel1.Controls.Add(this.pitchRateDial);
            this.panel1.Controls.Add(this.touchPad);
            this.panel1.Controls.Add(this.lfoWidthDial);
            this.panel1.Controls.Add(this.hLine_filterVtouchpad);
            this.panel1.Controls.Add(this.masterLevelDial);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.lfoRateDial);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(925, 528);
            this.panel1.TabIndex = 14;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // 
            // recButton
            // 
            this.recButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.recButton.InactiveText = "STOP";
            this.recButton.Location = new System.Drawing.Point(805, 208);
            this.recButton.Name = "recButton";
            this.recButton.Size = new System.Drawing.Size(66, 29);
            this.recButton.TabIndex = 13;
            this.recButton.Text = "REC";
            this.recButton.ToggleChanged += new CitySynth.UI_Elements.OnToggleChanged(this.recButton_ToggleChanged);
            this.recButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.recButton_MouseClick);
            // 
            // closeWindowBtn
            // 
            this.closeWindowBtn.BackColor = System.Drawing.Color.Transparent;
            this.closeWindowBtn.Location = new System.Drawing.Point(8, 6);
            this.closeWindowBtn.Name = "closeWindowBtn";
            this.closeWindowBtn.Size = new System.Drawing.Size(20, 19);
            this.closeWindowBtn.TabIndex = 12;
            this.closeWindowBtn.Text = null;
            this.closeWindowBtn.CheckedChanged += new CitySynth.UI_Elements.OnCheckedChanged(this.closeWindowBtn_CheckChanged);
            // 
            // presetSelector
            // 
            this.presetSelector.BackColor = System.Drawing.Color.Transparent;
            this.presetSelector.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.presetSelector.Items = ((System.Collections.Generic.List<string>)(resources.GetObject("presetSelector.Items")));
            this.presetSelector.Location = new System.Drawing.Point(367, 33);
            this.presetSelector.MinimumSize = new System.Drawing.Size(329, 27);
            this.presetSelector.Name = "presetSelector";
            this.presetSelector.ScrollLoop = true;
            this.presetSelector.SelectedIndex = 0;
            this.presetSelector.Size = new System.Drawing.Size(329, 27);
            this.presetSelector.TabIndex = 8;
            this.presetSelector.SelectedIndexChanged += new CitySynth.UI_Elements.OnSelectedIndexChanged(this.presetSelector_SelectedIndexChanged);
            this.presetSelector.PresetButtonClicked += new CitySynth.UI_Elements.OnPresetButtonClicked(this.presetSelector_PresetButtonClicked);
            // 
            // lfoHpfToggle
            // 
            this.lfoHpfToggle.BackColor = System.Drawing.Color.Transparent;
            this.lfoHpfToggle.Draggable = true;
            this.lfoHpfToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lfoHpfToggle.Location = new System.Drawing.Point(719, 205);
            this.lfoHpfToggle.Name = "lfoHpfToggle";
            this.lfoHpfToggle.Size = new System.Drawing.Size(81, 34);
            this.lfoHpfToggle.TabIndex = 11;
            this.lfoHpfToggle.Text = "HPF...";
            this.lfoHpfToggle.CheckedChanged += new CitySynth.UI_Elements.OnCheckedChanged(this.lfoHpfToggle_CheckChanged);
            this.lfoHpfToggle.DoubleClick += new System.EventHandler(this.lfoHpfToggle_DoubleClick);
            this.lfoHpfToggle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lfoHpfToggle_MouseMove);
            this.lfoHpfToggle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lfoHpfToggle_MouseUp);
            // 
            // ampLfoHeaderLabel
            // 
            this.ampLfoHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.ampLfoHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.ampLfoHeaderLabel.Location = new System.Drawing.Point(349, 465);
            this.ampLfoHeaderLabel.Name = "ampLfoHeaderLabel";
            this.ampLfoHeaderLabel.Size = new System.Drawing.Size(38, 17);
            this.ampLfoHeaderLabel.TabIndex = 5;
            this.ampLfoHeaderLabel.Text = "LFO";
            // 
            // lfoLpfToggle
            // 
            this.lfoLpfToggle.BackColor = System.Drawing.Color.Transparent;
            this.lfoLpfToggle.Checked = true;
            this.lfoLpfToggle.DefaultChecked = true;
            this.lfoLpfToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lfoLpfToggle.Location = new System.Drawing.Point(731, 170);
            this.lfoLpfToggle.Name = "lfoLpfToggle";
            this.lfoLpfToggle.Size = new System.Drawing.Size(57, 34);
            this.lfoLpfToggle.TabIndex = 11;
            this.lfoLpfToggle.Text = "LPF";
            this.lfoLpfToggle.CheckedChanged += new CitySynth.UI_Elements.OnCheckedChanged(this.lfoLpfToggle_CheckChanged);
            // 
            // lfoSendHeaderLabel
            // 
            this.lfoSendHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.lfoSendHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.lfoSendHeaderLabel.Location = new System.Drawing.Point(726, 153);
            this.lfoSendHeaderLabel.Name = "lfoSendHeaderLabel";
            this.lfoSendHeaderLabel.Size = new System.Drawing.Size(67, 17);
            this.lfoSendHeaderLabel.TabIndex = 5;
            this.lfoSendHeaderLabel.Text = "SEND TO";
            // 
            // touchPadEnableToggle
            // 
            this.touchPadEnableToggle.BackColor = System.Drawing.Color.Transparent;
            this.touchPadEnableToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.touchPadEnableToggle.Location = new System.Drawing.Point(844, 294);
            this.touchPadEnableToggle.Name = "touchPadEnableToggle";
            this.touchPadEnableToggle.Size = new System.Drawing.Size(28, 24);
            this.touchPadEnableToggle.TabIndex = 11;
            this.touchPadEnableToggle.Text = null;
            this.touchPadEnableToggle.CheckedChanged += new CitySynth.UI_Elements.OnCheckedChanged(this.touchPadEnableToggle_CheckedChanged);
            // 
            // osc1WaveSelDial
            // 
            this.osc1WaveSelDial.AutoSize = false;
            this.osc1WaveSelDial.BackColor = System.Drawing.Color.Transparent;
            this.osc1WaveSelDial.DefaultValue = 0F;
            this.osc1WaveSelDial.Location = new System.Drawing.Point(127, 185);
            this.osc1WaveSelDial.Maximum = 0F;
            this.osc1WaveSelDial.Minimum = 0F;
            this.osc1WaveSelDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.osc1WaveSelDial.Name = "osc1WaveSelDial";
            this.osc1WaveSelDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.osc1WaveSelDial.Size = new System.Drawing.Size(57, 83);
            this.osc1WaveSelDial.SnapSegments = 5;
            this.osc1WaveSelDial.SnapToMarkings = true;
            this.osc1WaveSelDial.TabIndex = 0;
            this.osc1WaveSelDial.Text = "Wave Selector";
            this.osc1WaveSelDial.Value = 0F;
            this.osc1WaveSelDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.osc1WaveSelDial_ValueChanged);
            // 
            // envActivateButton
            // 
            this.envActivateButton.BackColor = System.Drawing.Color.Transparent;
            this.envActivateButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.envActivateButton.Location = new System.Drawing.Point(619, 90);
            this.envActivateButton.Name = "envActivateButton";
            this.envActivateButton.Size = new System.Drawing.Size(28, 24);
            this.envActivateButton.TabIndex = 11;
            this.envActivateButton.Text = null;
            this.envActivateButton.CheckedChanged += new CitySynth.UI_Elements.OnCheckedChanged(this.envActivateButton_CheckedChanged);
            // 
            // filterCutoffDial
            // 
            this.filterCutoffDial.AutoSize = false;
            this.filterCutoffDial.BackColor = System.Drawing.Color.Transparent;
            this.filterCutoffDial.DefaultValue = 0.5F;
            this.filterCutoffDial.Location = new System.Drawing.Point(584, 327);
            this.filterCutoffDial.Maximum = 0F;
            this.filterCutoffDial.Minimum = 0F;
            this.filterCutoffDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.filterCutoffDial.Name = "filterCutoffDial";
            this.filterCutoffDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.filterCutoffDial.Size = new System.Drawing.Size(57, 68);
            this.filterCutoffDial.TabIndex = 0;
            this.filterCutoffDial.Text = "Cutoff";
            this.filterCutoffDial.Value = 0.5F;
            this.filterCutoffDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.filterCutoffDial_ValueChanged);
            // 
            // fftFilterToggle
            // 
            this.fftFilterToggle.BackColor = System.Drawing.Color.Transparent;
            this.fftFilterToggle.Checked = true;
            this.fftFilterToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.fftFilterToggle.Location = new System.Drawing.Point(584, 421);
            this.fftFilterToggle.Name = "fftFilterToggle";
            this.fftFilterToggle.Size = new System.Drawing.Size(57, 34);
            this.fftFilterToggle.TabIndex = 11;
            this.fftFilterToggle.Text = "FFT Filter";
            this.fftFilterToggle.CheckedChanged += new CitySynth.UI_Elements.OnCheckedChanged(this.fftFilterToggle_CheckedChanged);
            // 
            // osc1PhaseDial
            // 
            this.osc1PhaseDial.AutoSize = false;
            this.osc1PhaseDial.BackColor = System.Drawing.Color.Transparent;
            this.osc1PhaseDial.DefaultValue = 0F;
            this.osc1PhaseDial.Location = new System.Drawing.Point(193, 196);
            this.osc1PhaseDial.Maximum = 0F;
            this.osc1PhaseDial.Minimum = 0F;
            this.osc1PhaseDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.osc1PhaseDial.Name = "osc1PhaseDial";
            this.osc1PhaseDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.osc1PhaseDial.Size = new System.Drawing.Size(57, 69);
            this.osc1PhaseDial.TabIndex = 0;
            this.osc1PhaseDial.Text = "Phase";
            this.osc1PhaseDial.Value = 0F;
            this.osc1PhaseDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.osc1PhaseDial_ValueChanged);
            // 
            // osc1GainDial
            // 
            this.osc1GainDial.AutoSize = false;
            this.osc1GainDial.BackColor = System.Drawing.Color.Transparent;
            this.osc1GainDial.DefaultValue = 0.75F;
            this.osc1GainDial.Location = new System.Drawing.Point(259, 199);
            this.osc1GainDial.Maximum = 0F;
            this.osc1GainDial.Minimum = 0F;
            this.osc1GainDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.osc1GainDial.Name = "osc1GainDial";
            this.osc1GainDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.osc1GainDial.Size = new System.Drawing.Size(57, 69);
            this.osc1GainDial.TabIndex = 0;
            this.osc1GainDial.Text = "Gain";
            this.osc1GainDial.Value = 0.75F;
            this.osc1GainDial.InactiveChanged += new CitySynth.UI_Elements.OnInactiveChanged(this.osc1GainDial_InactiveChanged);
            this.osc1GainDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.osc1GainDial_ValueChanged);
            // 
            // ampReleaseDial
            // 
            this.ampReleaseDial.AutoSize = false;
            this.ampReleaseDial.BackColor = System.Drawing.Color.Transparent;
            this.ampReleaseDial.DefaultValue = 0F;
            this.ampReleaseDial.Location = new System.Drawing.Point(465, 323);
            this.ampReleaseDial.Maximum = 0F;
            this.ampReleaseDial.Minimum = 0F;
            this.ampReleaseDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.ampReleaseDial.Name = "ampReleaseDial";
            this.ampReleaseDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.ampReleaseDial.Size = new System.Drawing.Size(57, 70);
            this.ampReleaseDial.TabIndex = 10;
            this.ampReleaseDial.Text = "Release";
            this.ampReleaseDial.Value = 0F;
            this.ampReleaseDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.ampReleaseDial_ValueChanged);
            // 
            // ampLfoWidthDial
            // 
            this.ampLfoWidthDial.AutoSize = false;
            this.ampLfoWidthDial.BackColor = System.Drawing.Color.Transparent;
            this.ampLfoWidthDial.DefaultValue = 0F;
            this.ampLfoWidthDial.Location = new System.Drawing.Point(392, 405);
            this.ampLfoWidthDial.Maximum = 0F;
            this.ampLfoWidthDial.Minimum = 0F;
            this.ampLfoWidthDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.ampLfoWidthDial.Name = "ampLfoWidthDial";
            this.ampLfoWidthDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.ampLfoWidthDial.Size = new System.Drawing.Size(57, 68);
            this.ampLfoWidthDial.TabIndex = 10;
            this.ampLfoWidthDial.Text = "Width";
            this.ampLfoWidthDial.Value = 0F;
            this.ampLfoWidthDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.ampLfoWidthDial_ValueChanged);
            // 
            // polyMonoSwitch
            // 
            this.polyMonoSwitch.BackColor = System.Drawing.Color.Transparent;
            this.polyMonoSwitch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.polyMonoSwitch.Location = new System.Drawing.Point(753, 26);
            this.polyMonoSwitch.Name = "polyMonoSwitch";
            this.polyMonoSwitch.Size = new System.Drawing.Size(74, 39);
            this.polyMonoSwitch.TabIndex = 3;
            this.polyMonoSwitch.Text = "Poly/MONO";
            this.polyMonoSwitch.CheckedChanged += new CitySynth.UI_Elements.OnCheckedChanged(this.polyMonoSwitch_CheckedChanged);
            // 
            // ampLfoRateDial
            // 
            this.ampLfoRateDial.AutoSize = false;
            this.ampLfoRateDial.BackColor = System.Drawing.Color.Transparent;
            this.ampLfoRateDial.DefaultValue = 0F;
            this.ampLfoRateDial.Location = new System.Drawing.Point(289, 405);
            this.ampLfoRateDial.Maximum = 0F;
            this.ampLfoRateDial.Minimum = 0F;
            this.ampLfoRateDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.ampLfoRateDial.Name = "ampLfoRateDial";
            this.ampLfoRateDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.ampLfoRateDial.Size = new System.Drawing.Size(57, 68);
            this.ampLfoRateDial.TabIndex = 10;
            this.ampLfoRateDial.Text = "RATE";
            this.ampLfoRateDial.Value = 0F;
            this.ampLfoRateDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.ampLfoRateDial_ValueChanged);
            // 
            // midiIndicator
            // 
            this.midiIndicator.BackColor = System.Drawing.Color.Transparent;
            this.midiIndicator.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.midiIndicator.Location = new System.Drawing.Point(714, 29);
            this.midiIndicator.Name = "midiIndicator";
            this.midiIndicator.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.midiIndicator.Size = new System.Drawing.Size(30, 36);
            this.midiIndicator.TabIndex = 4;
            this.midiIndicator.Text = "MIDI";
            // 
            // ampSustainDial
            // 
            this.ampSustainDial.AutoSize = false;
            this.ampSustainDial.BackColor = System.Drawing.Color.Transparent;
            this.ampSustainDial.DefaultValue = 1F;
            this.ampSustainDial.Location = new System.Drawing.Point(382, 323);
            this.ampSustainDial.Maximum = 0F;
            this.ampSustainDial.Minimum = 0F;
            this.ampSustainDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.ampSustainDial.Name = "ampSustainDial";
            this.ampSustainDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.ampSustainDial.Size = new System.Drawing.Size(57, 70);
            this.ampSustainDial.TabIndex = 10;
            this.ampSustainDial.Text = "Sustain";
            this.ampSustainDial.Value = 1F;
            this.ampSustainDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.ampSustainDial_ValueChanged);
            // 
            // osc1HeaderLabel
            // 
            this.osc1HeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.osc1HeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.osc1HeaderLabel.Location = new System.Drawing.Point(131, 97);
            this.osc1HeaderLabel.Name = "osc1HeaderLabel";
            this.osc1HeaderLabel.Size = new System.Drawing.Size(38, 17);
            this.osc1HeaderLabel.TabIndex = 5;
            this.osc1HeaderLabel.Text = "OSC 1";
            // 
            // envCeilingDial
            // 
            this.envCeilingDial.AutoSize = false;
            this.envCeilingDial.BackColor = System.Drawing.Color.Transparent;
            this.envCeilingDial.DefaultValue = 1F;
            this.envCeilingDial.Location = new System.Drawing.Point(570, 200);
            this.envCeilingDial.Maximum = 0F;
            this.envCeilingDial.Minimum = 0F;
            this.envCeilingDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.envCeilingDial.Name = "envCeilingDial";
            this.envCeilingDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.envCeilingDial.Size = new System.Drawing.Size(57, 70);
            this.envCeilingDial.TabIndex = 10;
            this.envCeilingDial.Text = "Ceiling";
            this.envCeilingDial.Value = 1F;
            this.envCeilingDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.envCeilingDial_ValueChanged);
            // 
            // touchPadHeaderLabel
            // 
            this.touchPadHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.touchPadHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.touchPadHeaderLabel.Location = new System.Drawing.Point(683, 300);
            this.touchPadHeaderLabel.Name = "touchPadHeaderLabel";
            this.touchPadHeaderLabel.Size = new System.Drawing.Size(62, 17);
            this.touchPadHeaderLabel.TabIndex = 5;
            this.touchPadHeaderLabel.Text = "TOUCHPAD";
            // 
            // envReleaseDial
            // 
            this.envReleaseDial.AutoSize = false;
            this.envReleaseDial.BackColor = System.Drawing.Color.Transparent;
            this.envReleaseDial.DefaultValue = 0F;
            this.envReleaseDial.Location = new System.Drawing.Point(570, 120);
            this.envReleaseDial.Maximum = 0F;
            this.envReleaseDial.Minimum = 0F;
            this.envReleaseDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.envReleaseDial.Name = "envReleaseDial";
            this.envReleaseDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.envReleaseDial.Size = new System.Drawing.Size(57, 70);
            this.envReleaseDial.TabIndex = 10;
            this.envReleaseDial.Text = "Release";
            this.envReleaseDial.Value = 0.1F;
            this.envReleaseDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.envReleaseDial_ValueChanged);
            // 
            // filterHeaderLabel
            // 
            this.filterHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.filterHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.filterHeaderLabel.Location = new System.Drawing.Point(572, 300);
            this.filterHeaderLabel.Name = "filterHeaderLabel";
            this.filterHeaderLabel.Size = new System.Drawing.Size(39, 17);
            this.filterHeaderLabel.TabIndex = 5;
            this.filterHeaderLabel.Text = "FILTER";
            // 
            // envFloorDial
            // 
            this.envFloorDial.AutoSize = false;
            this.envFloorDial.BackColor = System.Drawing.Color.Transparent;
            this.envFloorDial.DefaultValue = 0F;
            this.envFloorDial.Location = new System.Drawing.Point(471, 200);
            this.envFloorDial.Maximum = 0F;
            this.envFloorDial.Minimum = 0F;
            this.envFloorDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.envFloorDial.Name = "envFloorDial";
            this.envFloorDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.envFloorDial.Size = new System.Drawing.Size(57, 70);
            this.envFloorDial.TabIndex = 10;
            this.envFloorDial.Text = "Floor";
            this.envFloorDial.Value = 0.05F;
            this.envFloorDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.envFloorDial_ValueChanged);
            // 
            // lfoHeaderLabel
            // 
            this.lfoHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.lfoHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.lfoHeaderLabel.Location = new System.Drawing.Point(670, 97);
            this.lfoHeaderLabel.Name = "lfoHeaderLabel";
            this.lfoHeaderLabel.Size = new System.Drawing.Size(39, 17);
            this.lfoHeaderLabel.TabIndex = 5;
            this.lfoHeaderLabel.Text = "LFO";
            // 
            // ampDecayDial
            // 
            this.ampDecayDial.AutoSize = false;
            this.ampDecayDial.BackColor = System.Drawing.Color.Transparent;
            this.ampDecayDial.DefaultValue = 0F;
            this.ampDecayDial.Location = new System.Drawing.Point(299, 323);
            this.ampDecayDial.Maximum = 0F;
            this.ampDecayDial.Minimum = 0F;
            this.ampDecayDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.ampDecayDial.Name = "ampDecayDial";
            this.ampDecayDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.ampDecayDial.Size = new System.Drawing.Size(57, 70);
            this.ampDecayDial.TabIndex = 10;
            this.ampDecayDial.Text = "Decay";
            this.ampDecayDial.Value = 0F;
            this.ampDecayDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.ampDecayDial_ValueChanged);
            // 
            // delayHeaderLabel
            // 
            this.delayHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.delayHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.delayHeaderLabel.Location = new System.Drawing.Point(355, 97);
            this.delayHeaderLabel.Name = "delayHeaderLabel";
            this.delayHeaderLabel.Size = new System.Drawing.Size(57, 17);
            this.delayHeaderLabel.TabIndex = 5;
            this.delayHeaderLabel.Text = "DELAY";
            // 
            // envAttackDial
            // 
            this.envAttackDial.AutoSize = false;
            this.envAttackDial.BackColor = System.Drawing.Color.Transparent;
            this.envAttackDial.DefaultValue = 0F;
            this.envAttackDial.Location = new System.Drawing.Point(471, 120);
            this.envAttackDial.Maximum = 0F;
            this.envAttackDial.Minimum = 0F;
            this.envAttackDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.envAttackDial.Name = "envAttackDial";
            this.envAttackDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.envAttackDial.Size = new System.Drawing.Size(57, 70);
            this.envAttackDial.TabIndex = 10;
            this.envAttackDial.Text = "Attack";
            this.envAttackDial.Value = 0.2F;
            this.envAttackDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.envAttackDial_ValueChanged);
            // 
            // headerLabel1
            // 
            this.headerLabel1.BackColor = System.Drawing.Color.Transparent;
            this.headerLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.headerLabel1.Location = new System.Drawing.Point(814, 97);
            this.headerLabel1.Name = "headerLabel1";
            this.headerLabel1.Size = new System.Drawing.Size(48, 17);
            this.headerLabel1.TabIndex = 5;
            this.headerLabel1.Text = "MASTER";
            // 
            // ampAttackDial
            // 
            this.ampAttackDial.AutoSize = false;
            this.ampAttackDial.BackColor = System.Drawing.Color.Transparent;
            this.ampAttackDial.DefaultValue = 0F;
            this.ampAttackDial.Location = new System.Drawing.Point(216, 323);
            this.ampAttackDial.Maximum = 0F;
            this.ampAttackDial.Minimum = 0F;
            this.ampAttackDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.ampAttackDial.Name = "ampAttackDial";
            this.ampAttackDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.ampAttackDial.Size = new System.Drawing.Size(57, 70);
            this.ampAttackDial.TabIndex = 10;
            this.ampAttackDial.Text = "Attack";
            this.ampAttackDial.Value = 0.1F;
            this.ampAttackDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.ampAttackDial_ValueChanged);
            // 
            // pitchHeaderLabel
            // 
            this.pitchHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.pitchHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.pitchHeaderLabel.Location = new System.Drawing.Point(131, 300);
            this.pitchHeaderLabel.Name = "pitchHeaderLabel";
            this.pitchHeaderLabel.Size = new System.Drawing.Size(39, 17);
            this.pitchHeaderLabel.TabIndex = 5;
            this.pitchHeaderLabel.Text = "PITCH";
            // 
            // delayWetDial
            // 
            this.delayWetDial.AutoSize = false;
            this.delayWetDial.BackColor = System.Drawing.Color.Transparent;
            this.delayWetDial.DefaultValue = 0.5F;
            this.delayWetDial.Location = new System.Drawing.Point(355, 200);
            this.delayWetDial.Maximum = 0F;
            this.delayWetDial.Minimum = 0F;
            this.delayWetDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.delayWetDial.Name = "delayWetDial";
            this.delayWetDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.delayWetDial.Size = new System.Drawing.Size(57, 68);
            this.delayWetDial.TabIndex = 9;
            this.delayWetDial.Text = "Dry/wet";
            this.delayWetDial.Value = 0.5F;
            this.delayWetDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.delayWetDial_ValueChanged);
            // 
            // ampHeaderLabel
            // 
            this.ampHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.ampHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.ampHeaderLabel.Location = new System.Drawing.Point(219, 300);
            this.ampHeaderLabel.Name = "ampHeaderLabel";
            this.ampHeaderLabel.Size = new System.Drawing.Size(62, 17);
            this.ampHeaderLabel.TabIndex = 5;
            this.ampHeaderLabel.Text = "AMPLIFIER";
            // 
            // delayLengthDial
            // 
            this.delayLengthDial.AutoSize = false;
            this.delayLengthDial.BackColor = System.Drawing.Color.Transparent;
            this.delayLengthDial.DefaultValue = 0F;
            this.delayLengthDial.Location = new System.Drawing.Point(355, 123);
            this.delayLengthDial.Maximum = 0F;
            this.delayLengthDial.Minimum = 0F;
            this.delayLengthDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.delayLengthDial.Name = "delayLengthDial";
            this.delayLengthDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.delayLengthDial.Size = new System.Drawing.Size(57, 68);
            this.delayLengthDial.TabIndex = 9;
            this.delayLengthDial.Text = "Length";
            this.delayLengthDial.Value = 0.1F;
            this.delayLengthDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.delayLengthDial_ValueChanged);
            // 
            // envHeaderLabel
            // 
            this.envHeaderLabel.BackColor = System.Drawing.Color.Transparent;
            this.envHeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(129)))), ((int)(((byte)(12)))));
            this.envHeaderLabel.Location = new System.Drawing.Point(460, 97);
            this.envHeaderLabel.Name = "envHeaderLabel";
            this.envHeaderLabel.Size = new System.Drawing.Size(62, 17);
            this.envHeaderLabel.TabIndex = 5;
            this.envHeaderLabel.Text = "ENVELOPE";
            // 
            // pitchWidthDial
            // 
            this.pitchWidthDial.AutoSize = false;
            this.pitchWidthDial.BackColor = System.Drawing.Color.Transparent;
            this.pitchWidthDial.DefaultValue = 0F;
            this.pitchWidthDial.Location = new System.Drawing.Point(123, 400);
            this.pitchWidthDial.Maximum = 0F;
            this.pitchWidthDial.Minimum = 0F;
            this.pitchWidthDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.pitchWidthDial.Name = "pitchWidthDial";
            this.pitchWidthDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.pitchWidthDial.Size = new System.Drawing.Size(57, 68);
            this.pitchWidthDial.TabIndex = 9;
            this.pitchWidthDial.Text = "Width";
            this.pitchWidthDial.Value = 0F;
            this.pitchWidthDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.pitchWidthDial_ValueChanged);
            // 
            // masterMeter
            // 
            this.masterMeter.BackColor = System.Drawing.Color.Transparent;
            this.masterMeter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.masterMeter.LeftProgress = 0.8F;
            this.masterMeter.Location = new System.Drawing.Point(841, 12);
            this.masterMeter.Name = "masterMeter";
            this.masterMeter.RightProgress = 0.8F;
            this.masterMeter.Size = new System.Drawing.Size(46, 60);
            this.masterMeter.TabIndex = 6;
            this.masterMeter.Text = "Master";
            // 
            // pitchRateDial
            // 
            this.pitchRateDial.AutoSize = false;
            this.pitchRateDial.BackColor = System.Drawing.Color.Transparent;
            this.pitchRateDial.DefaultValue = 0F;
            this.pitchRateDial.Location = new System.Drawing.Point(123, 323);
            this.pitchRateDial.Maximum = 0F;
            this.pitchRateDial.Minimum = 0F;
            this.pitchRateDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.pitchRateDial.Name = "pitchRateDial";
            this.pitchRateDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.pitchRateDial.Size = new System.Drawing.Size(57, 68);
            this.pitchRateDial.TabIndex = 9;
            this.pitchRateDial.Text = "Rate";
            this.pitchRateDial.Value = 0F;
            this.pitchRateDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.pitchRateDial_ValueChanged);
            // 
            // lfoWidthDial
            // 
            this.lfoWidthDial.AutoSize = false;
            this.lfoWidthDial.BackColor = System.Drawing.Color.Transparent;
            this.lfoWidthDial.DefaultValue = 0.5F;
            this.lfoWidthDial.Location = new System.Drawing.Point(673, 202);
            this.lfoWidthDial.Maximum = 0F;
            this.lfoWidthDial.Minimum = 0F;
            this.lfoWidthDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.lfoWidthDial.Name = "lfoWidthDial";
            this.lfoWidthDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.lfoWidthDial.Size = new System.Drawing.Size(57, 68);
            this.lfoWidthDial.TabIndex = 9;
            this.lfoWidthDial.Text = "Width";
            this.lfoWidthDial.Value = 0.5F;
            this.lfoWidthDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.lfoWidthDial_ValueChanged);
            // 
            // masterLevelDial
            // 
            this.masterLevelDial.AutoSize = false;
            this.masterLevelDial.BackColor = System.Drawing.Color.Transparent;
            this.masterLevelDial.DefaultMarkings = false;
            this.masterLevelDial.DefaultValue = 0.82F;
            this.masterLevelDial.EnableStandbyMode = false;
            this.masterLevelDial.Location = new System.Drawing.Point(810, 124);
            this.masterLevelDial.Maximum = 6F;
            this.masterLevelDial.Minimum = -48F;
            this.masterLevelDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.masterLevelDial.Name = "masterLevelDial";
            this.masterLevelDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.masterLevelDial.Size = new System.Drawing.Size(57, 65);
            this.masterLevelDial.TabIndex = 9;
            this.masterLevelDial.Text = "Level";
            this.masterLevelDial.Value = 0.82F;
            this.masterLevelDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.masterLevelDial_ValueChanged);
            // 
            // lfoRateDial
            // 
            this.lfoRateDial.AutoSize = false;
            this.lfoRateDial.BackColor = System.Drawing.Color.Transparent;
            this.lfoRateDial.DefaultValue = 0F;
            this.lfoRateDial.Location = new System.Drawing.Point(673, 123);
            this.lfoRateDial.Maximum = 0F;
            this.lfoRateDial.Minimum = 0F;
            this.lfoRateDial.MinimumSize = new System.Drawing.Size(57, 65);
            this.lfoRateDial.Name = "lfoRateDial";
            this.lfoRateDial.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.lfoRateDial.Size = new System.Drawing.Size(57, 68);
            this.lfoRateDial.TabIndex = 9;
            this.lfoRateDial.Text = "Rate";
            this.lfoRateDial.Value = 0F;
            this.lfoRateDial.ValueChanged += new CitySynth.UI_Elements.OnValueChanged(this.lfoRateDial_ValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(925, 528);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(925, 528);
            this.MinimumSize = new System.Drawing.Size(925, 528);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "City Synth";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.osc1WavePreviewBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.touchPad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hLine_filterVtouchpad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hLine_lfoVdelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox osc1WavePreviewBox;
        private UI_Elements.Dial osc1GainDial;
        private UI_Elements.Dial osc1WaveSelDial;
        private UI_Elements.Switch polyMonoSwitch;
        private UI_Elements.Indicator midiIndicator;
        private UI_Elements.HeaderLabel osc1HeaderLabel;
        private UI_Elements.Meter masterMeter;
        private UI_Elements.HeaderLabel touchPadHeaderLabel;
        private System.Windows.Forms.PictureBox touchPad;
        private UI_Elements.HeaderLabel filterHeaderLabel;
        private UI_Elements.Dial filterCutoffDial;
        private System.Windows.Forms.PictureBox hLine_filterVtouchpad;
        private UI_Elements.PresetSelector presetSelector;
        private UI_Elements.Dial lfoRateDial;
        private UI_Elements.HeaderLabel lfoHeaderLabel;
        private UI_Elements.HeaderLabel ampHeaderLabel;
        private UI_Elements.Dial ampAttackDial;
        private UI_Elements.Dial ampReleaseDial;
        private UI_Elements.Dial ampDecayDial;
        private UI_Elements.Dial ampSustainDial;
        private UI_Elements.Dial ampLfoRateDial;
        private UI_Elements.Dial ampLfoWidthDial;
        private UI_Elements.HeaderLabel ampLfoHeaderLabel;
        private UI_Elements.ToggleIcon fftFilterToggle;
        private UI_Elements.HeaderLabel lfoSendHeaderLabel;
        private UI_Elements.HeaderLabel envHeaderLabel;
        private UI_Elements.Dial envAttackDial;
        private UI_Elements.Dial envReleaseDial;
        private UI_Elements.Dial envFloorDial;
        private UI_Elements.Dial envCeilingDial;
        private UI_Elements.HeaderLabel pitchHeaderLabel;
        private UI_Elements.Dial pitchRateDial;
        private UI_Elements.Dial lfoWidthDial;
        private UI_Elements.Dial pitchWidthDial;
        private UI_Elements.ToggleIcon envActivateButton;
        private UI_Elements.ToggleIcon closeWindowBtn;
        private UI_Elements.ToggleIcon touchPadEnableToggle;
        private UI_Elements.ToggleIcon lfoLpfToggle;
        private UI_Elements.ToggleIcon lfoHpfToggle;
        private System.Windows.Forms.PictureBox hLine_lfoVdelay;
        private UI_Elements.HeaderLabel delayHeaderLabel;
        private UI_Elements.Dial delayLengthDial;
        private UI_Elements.Dial delayWetDial;
        private System.Windows.Forms.PictureBox pictureBox1;
        private UI_Elements.HeaderLabel headerLabel1;
        private UI_Elements.Dial masterLevelDial;
        private UI_Elements.SquareButton recButton;
        private System.Windows.Forms.Panel panel1;
        private UI_Elements.Dial osc1PhaseDial;
    }
}

