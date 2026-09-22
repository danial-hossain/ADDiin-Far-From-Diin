namespace AdDiin.Models
{
    public class Hadith
    {
        public int HadithNumber { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Collection { get; set; } = string.Empty;
    }
}
