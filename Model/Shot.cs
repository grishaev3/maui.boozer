using System.Text.Json.Serialization;

namespace maui.boozer.Model
{
    public class Shot
    {
        public DateTime Date { get; set; }

        public int ThirdLiterCount { get; set; }
        public int HalfLiterCount { get; set; }
        public int OneLiterCount { get; set; }
        public int OneAndHalfLiterCount { get; set; }

        [JsonIgnore]
        public int ID
        {
            get
            {
                int hash = 23;
                hash = hash * 31 + Date.GetHashCode();
                hash = hash * 31 + ThirdLiterCount.GetHashCode();
                hash = hash * 31 + HalfLiterCount.GetHashCode();
                hash = hash * 31 + OneLiterCount.GetHashCode();
                hash = hash * 31 + OneAndHalfLiterCount.GetHashCode();
                return hash;
            }
        }

        [JsonIgnore]
        public string ThirdLitter => ThirdLiterCount == default ? _e : $"0.3{_coeff(ThirdLiterCount)}";
        [JsonIgnore]
        public string HalfLitter => HalfLiterCount == default ? _e : $"0.5{_coeff(HalfLiterCount)}";
        [JsonIgnore]
        public string OneLitter => OneLiterCount == default ? _e : $"1.0{_coeff(OneLiterCount)}";
        [JsonIgnore]
        public string OneAndHalfLitter => OneAndHalfLiterCount == default ? _e : $"1.5{_coeff(OneAndHalfLiterCount)}";

        private const string _e = "-";

        private string _coeff(int value) => (value == 1) ? "" : $"*{value}";
    };
}
