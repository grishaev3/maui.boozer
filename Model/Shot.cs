using System.Text.Json.Serialization;

namespace maui.boozer.Model
{
    public class Shot
    {
        public DateTime Date { get; set; }

        public int ThirdLitterCount { get; set; }
        public int HalfLitterCount { get; set; }
        public int OneLitterCount { get; set; }
        public int OneAndHalfLitterCount { get; set; }

        [JsonIgnore]
        public int ID => Date.GetHashCode();

        [JsonIgnore]
        public string ThirdLitter => ThirdLitterCount == default ? _e : $"0.3{_coeff(ThirdLitterCount)}";
        [JsonIgnore]
        public string HalfLitter => HalfLitterCount == default ? _e : $"0.5{_coeff(HalfLitterCount)}";
        [JsonIgnore]
        public string OneLitter => OneLitterCount == default ? _e : $"1.0{_coeff(OneLitterCount)}";
        [JsonIgnore]
        public string OneAndHalfLitter => OneAndHalfLitterCount == default ? _e : $"1.5{_coeff(OneAndHalfLitterCount)}";

        private const string _e = "-";

        private string _coeff(int value) => (value == 1) ? "" : $"*{value}";
    };
}
