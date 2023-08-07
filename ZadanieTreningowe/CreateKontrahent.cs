using Soneta.CRM;
using System;
using System.Text.RegularExpressions;

namespace ZadanieTreningowe
{
    public class CreateKontrahent
    {
        public static Kontrahent Create(Kontrahent kontrahent, ListXml dane)
        {
            string kodPocztowyString = dane.Kontrahent.KodPocztowy.Replace("-", "");
            int kodPocztowy = Int32.Parse(kodPocztowyString);

            kontrahent.Kod = dane.Kontrahent.Kod;
            kontrahent.Nazwa = dane.Kontrahent.Nazwa;
            kontrahent.NIP = dane.Kontrahent.Nip;
            kontrahent.Adres.Ulica = new Regex(@"\d+$").Replace(dane.Kontrahent.Ulica, "");
            kontrahent.Adres.NrDomu = Regex.Match(dane.Kontrahent.Ulica, @"\d+$", RegexOptions.RightToLeft).Value;
            kontrahent.Adres.Miejscowosc = dane.Kontrahent.Miasto;
            kontrahent.Adres.Kraj = dane.Kontrahent.Kraj;
            kontrahent.Adres.KodPocztowy = kodPocztowy;

            return kontrahent;
        }
    }
}