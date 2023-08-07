using Soneta.Business;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZadanieTreningowe
{
    public class ReadXml
    {
        public static ListXml ReadFile(NamedStream XMLFileName)
        {
            StreamReader objReader = new StreamReader(XMLFileName.FileName);

            string linia = "";
            string kawalki = "";
            int iloscVat = 0;
            int pom = 0;

            //odczytanie danych z xml (każda wartość do nowej lini)
            while (linia != null)
            {
                linia = objReader.ReadLine();

                if (linia != null && linia != "" && pom != 0)
                {
                    iloscVat = Regex.Matches(linia, "LINIA_VAT").Count;
                    kawalki = Regex.Replace(linia, "<.*?>", "\n");
                }
                pom++;
            }
            objReader.Close();

            iloscVat /= 2;

            //Wrzucenie wartości do listy
            List<string> daneXml = kawalki.Split('\n').ToList();

            if (daneXml[37] == "")
            {
                daneXml[37] = null;
                daneXml[60] = null;
            }

            //usunięcie pustych elementów listy
            int i = 0;
            while (i < daneXml.Count)
            {
                if (daneXml[i] == "")
                {
                    daneXml.Remove(daneXml[i]);
                }
                else
                    i++;
            }

            //wyodrębnianie danych z listy

            //kontrahent
            KontrahentXml kontrahent = new KontrahentXml(daneXml[15], daneXml[16], daneXml[17], daneXml[18],
                                                                 daneXml[20], daneXml[21], daneXml[22], daneXml[23]);

            //nr Dokumentu
            string numerDokumentu = daneXml[5];

            //vat
            List<ListVatXml> stawkiVat = new List<ListVatXml>();

            int flagStawka = daneXml.Count - 9;
            int flagBrutto = daneXml.Count - 4;

            //pętla korzysta z zmiennej iloscVat która mówi ile Vat-ów ma dokument
            //(zmienna została naliczona przy pętli do odczytania danych z XML)
            for (int j = 0; j < iloscVat; j++)
            {
                stawkiVat.Add(new ListVatXml(daneXml[flagStawka], daneXml[flagBrutto]));

                flagStawka -= 9;
                flagBrutto -= 9;
            }

            //kwoty
            ListaKwoty kwoty = new ListaKwoty(daneXml[50], daneXml[51]);

            //Obiekt klasy przechowującej kontrahenta, numer dokumentu i stawki VAT
            return new ListXml(kontrahent, numerDokumentu, stawkiVat, kwoty);
        }
    }
}