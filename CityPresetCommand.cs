namespace CitySynth
{
    public class CityPresetCommand
    {
        public string CommandType { get; set; }
        public string RawCommandValue { get; set; }
        public float? CommandValue => float.TryParse(RawCommandValue, out var i) ? (float?)i : null;
    }
}