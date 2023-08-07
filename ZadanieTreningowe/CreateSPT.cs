using Soneta.Core;
using Soneta.CRM;
using Soneta.EwidencjaVat;

namespace ZadanieTreningowe
{
    public class CreateSPT
    {
        public static SprzedazEwidencja Create(SprzedazEwidencja nowySPT, DefinicjaDokumentu def, ListXml dane, Kontrahent kontrahent)
        {
            nowySPT.Definicja = def;
            nowySPT.Numer.NumerPelny = dane.NumerPelny.Substring(3, dane.NumerPelny.Length - 8);
            nowySPT.NumerDokumentu = dane.NumerPelny;
            nowySPT.Opis = "Dokument Sprzedaży SPT";
            nowySPT.Stan = StanEwidencji.Bufor;
            nowySPT.Podmiot = kontrahent;

            return nowySPT;
        }
    }
}