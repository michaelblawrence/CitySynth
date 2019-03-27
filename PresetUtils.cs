using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CitySynth.Presets
{
    public static class PresetUtils
    {
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