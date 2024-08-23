namespace ClientApp.Models
{
    public class HavaDurumu
    {
        public DateOnly Tarih { get; set; }

        public int SicaklikC { get; set; }

        public int SicaklikF => 32 + (int)(SicaklikC / 0.5556);

        public string? Ozet { get; set; }
    }
}
