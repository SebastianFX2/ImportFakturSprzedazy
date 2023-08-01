namespace ZadanieTreningowe
{
    public class KontrahentXml
    {
        public string Kod { get; set; }
        public string NipKraj { get; set; }
        public string Nip { get; set; }
        public string Nazwa { get; set; }
        public string KodPocztowy { get; set; }
        public string Miasto { get; set; }
        public string Ulica { get; set; }
        public string Kraj { get; set; }

        public KontrahentXml(string kod, string nipKraj, string nip, string nazwa, string kodPocztowy, string miasto, string ulica, string kraj)
        {
            Kod = kod;
            NipKraj = nipKraj;
            Nip = nip;
            Nazwa = nazwa;
            KodPocztowy = kodPocztowy;
            Miasto = miasto;
            Ulica = ulica;
            Kraj = kraj;
        }
    }
}