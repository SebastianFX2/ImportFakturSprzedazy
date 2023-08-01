using Soneta.Tools;
using System.Collections.Generic;

namespace ZadanieTreningowe
{
    public class ListXml
    {
        public KontrahentXml Kontrahent { get; set; }
        public string NumerPelny { get; set; }
        public List<ListVatXml> Vat { get; set; }
        public ListaKwoty Kwoty { get; set; }

        public ListXml(KontrahentXml kontrahent, string numerPelny, List<ListVatXml> vat, ListaKwoty kwoty)
        {
            Kontrahent = kontrahent;
            NumerPelny = numerPelny;
            Vat = vat;
            Kwoty = kwoty;
        }
    }
}