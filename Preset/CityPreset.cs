using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CitySynth.Preset
{
    public class CityPreset
    {
        public string Name { get; set; }

        public CityPresetCommand[] Commands { get => _commands.ToArray(); }

        private List<CityPresetCommand> _commands;

        public CityPreset()
        {
            _commands = new List<CityPresetCommand>();
        }

        public CityPreset(string presetName, string presetStoreLine) : this()
        {
            Name = presetName;
            var parsed = presetStoreLine.Split(';')
                .Select(commandValuePair => buildCommand(commandValuePair.Split(':')));
            _commands.AddRange(parsed);
        }

        public static (string presetName, string presetLine) GetNthLine(string presetsText, int index)
        {
            var lines = presetsText
                .Replace("\r", "")
                .Trim()
                .Split('\n')
                .Where(s => !s.StartsWith("//") && s.Trim() != "")
                .ToArray();
            var line = lines[index]
                .Trim()
                .TrimEnd(';')
                .Trim();
            var whole = line.Trim().Split('|');
            string name = whole[0];
            string data = whole[1];
            return (presetName: name, presetLine: data);
        }

        private static CityPresetCommand buildCommand(string[] splitPair)
        {
            return new CityPresetCommand { CommandType = splitPair[0], RawCommandValue = splitPair[1] };
        }
    }
}