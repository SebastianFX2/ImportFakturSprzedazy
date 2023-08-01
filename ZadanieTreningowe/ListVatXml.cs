namespace ZadanieTreningowe
{
    public class ListVatXml
    {
        public string Stawka { get; set; }
        public string Brutto { get; set; }

        public ListVatXml(string stawka, string brutto)
        {
            Stawka = stawka;
            Brutto = brutto;
        }
    }
}