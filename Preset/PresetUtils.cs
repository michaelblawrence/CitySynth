using CitySynth.State;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CitySynth.Preset
{
    public static class PresetUtils
    {
        /// <summary>
        /// Converts the current preset to CitySynth preset format string.
        /// </summary>
        /// <returns>CitySynth preset format string.</returns>
        /// <param name="presetName">Preset name.</param>
        /// <param name="index">Preset index.</param>
        public static string CurrentPresetToString(string presetName, int index, int iden = -1)
        {
            string name = presetName.Replace(':', ' ').Replace('|', ' ');

            string s = index + ":" + name + ":" + iden + "|";
            s += "a:" + R.Attack + ";";
            s += "d:" + R.Decay + ";";
            s += "s:" + R.Sustain + ";";
            s += "r:" + R.Release + ";";
            s += "h:" + R.MaxHarmonic + ";";
            s += "hc:" + R.HarmonicsControl + ";";
            s += "hp:" + R.HarmonicPhase + ";";
            s += "w:" + (int)R.WFunction + ";";
            s += "hw:" + (int)R.HarmonicFunction + ";";
            s += "b:" + Math.Log(R.BaseFrequency / 440, 2) + ";";
            s += "hpf:" + R.HPFCutoff + ";";
            s += "lpf:" + R.LPF + ";";
            s += "lfo:" + R.LPFmodrate + ";";
            s += "prate:" + R.Pitchmod + ";";
            s += "pwidth:" + R.Pitchmodwidth + ";";
            s += "arate:" + R.AmpLFOrate + ";";
            s += "awidth:" + R.AmpLFOwidth + ";";
            s += "lwidth:" + R.LPFwidth + ";";
            s += "la:" + R.LPFattack + ";";
            s += "lr:" + R.LPFrelease + ";";
            s += "lf:" + R.LPFfloor + ";";
            s += "lc:" + R.LPFceiling + ";";
            s += "delay:" + R.DelayTime + ";";
            s += "dwet:" + R.DelayWet + ";";
            s += "rwet:" + R.ReverbWet + ";";
            s += "ffb:" + R.FilterFeedback + ";";
            s += "fd:" + R.FilterDrive + ";";
            s += "filter:" + (R.FFTEnable ? R.FFTMode : 0) + ";";
            s += "lpfenv:" + (R.LPFenvelope ? 1 : 0) + ";";
            if (R.Harmonic2Gain != 0)
                s += "h2:" + R.Harmonic2Gain + ";";
            if (R.SubOscGain != 0)
                s += "sub:" + R.SubOscGain + ";";
            if (!R.GainControlInactive)
                s += "g:" + R.Gain + ";";
            if (!R.HarmonicV1)
                s += "hv1:" + Convert.ToInt16(R.HarmonicV1) + ";";

            return s;
        }

        public static void OpenPresetPackSaveDialog(byte[] bytes, Form owner)
        {
            var save = new SaveFileDialog
            {
                Filter = "CitySynth PresetPack (*.spdx)|*.spdx",
                FileName = "PresetPack_" + Environment.UserName + "_" + DateTime.Now.ToShortDateString().Replace('/', '-')
            };
            var dr = save.ShowDialog(owner);
            if (dr == DialogResult.OK)
            {
                using (FileStream fs = File.Create(save.FileName))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            save.Dispose();
        }

        /// <summary>
        /// Shows the saving preset name dialog.
        /// </summary>
        /// <returns>Result of the preset name dialog.</returns>
        /// <param name="dialogPresetName">Entered preset name.</param>
        public static DialogResult ShowSavePresetDialog(Form owner, Func<(bool placeBelow, Form dialog), Point> imputFormLocation, out string dialogPresetName)
        {
            bool placeBelow = true;
            Form inputForm = new Form
            {
                Text = "Preset Name",
                Size = new Size(217, 60),
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                BackColor = Color.DarkGray
            };
            inputForm.Location = imputFormLocation((placeBelow: placeBelow, dialog: inputForm));

            TextBox tb = new TextBox
            {
                Size = new Size(150, 20),
                BorderStyle = BorderStyle.FixedSingle,
                TabIndex = 0,
                Text = "Untitled " + DateTime.Now.ToShortDateString()
            };
            tb.SelectAll();
            inputForm.Controls.Add(tb);

            Button b = new Button
            {
                FlatStyle = FlatStyle.Popup,
                DialogResult = DialogResult.OK,
                Size = new Size(50, 20),
                Text = "Save",
                TabIndex = 1,
                Location = new Point(150, 0)
            };
            b.Click += (sender, e) =>
            {
                if (tb.Text.Trim() != "")
                    inputForm.Close();
            };

            inputForm.Controls.Add(b);
            inputForm.KeyPreview = true;
            inputForm.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    inputForm.DialogResult = DialogResult.Cancel;
                    inputForm.Close();
                }
            };
            inputForm.AcceptButton = b;

            DialogResult dr = inputForm.ShowDialog(owner);

            dialogPresetName = tb.Text;

            inputForm.Dispose();
            b.Dispose();
            tb.Dispose();

            return dr;
        }


        /// <summary>
        /// Exports the preset pack in packaged form for sharing. Experimental.
        /// </summary>
        public static byte[] ExportPresetPack(string inputPresetsBlob, string factoryPresetsBlob)
        {
            string[] orr = factoryPresetsBlob.Split('\n')
                .ToList().Select(s => s.Trim()).ToArray();
            string[] raws = inputPresetsBlob.Split('\n');
            var expContent = new List<string>();
            foreach (string s in raws) if (!orr.Contains(s)) expContent.Add(s);
            var bytes = new List<object[]>();
            expContent.ForEach(s => bytes.Add(new object[] { Encoding.ASCII.GetBytes(s.Trim()), Encoding.ASCII.GetByteCount(s.Trim()) }));
            int cc = bytes.Sum(o => ((byte[])o[0]).Length);
            var init = (new int[] { 1, 1 }).ToList();
            for (int i = 0; i < 5; i++) init.Add(init[i] + init[i + 1]);
            init.AddRange(init.Reverse<int>().ToList());
            int[] datagaps =
                init.ToArray();

            int datagapindex = 0;
            double sizemul = datagaps.Sum(i => i + 1) / (double)datagaps.Length;
            int c = (int)Math.Ceiling(cc * sizemul);
            byte[] ret = new byte[c];
            int ret_index = 0;
            int byte_counter = 0, bytes_index = 0;
            var r = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < cc; i++)
            {
                if (byte_counter >= ((byte[])(bytes[bytes_index][0])).Length)
                {
                    bytes_index++;
                    byte_counter = 0;
                }
                ret[ret_index] = (byte)(((byte[])(bytes[bytes_index][0]))[byte_counter] + 127);
                ret_index++;
                for (int a = 0; a < datagaps[datagapindex]; a++)
                {
                    ret[ret_index] = (byte)r.Next(255);
                    ret_index++;
                }
                datagapindex++;
                datagapindex %= datagaps.Length;
            }
            return ret;
        }
    }
}