using Soneta.Business;
using Soneta.Business.App;
using Soneta.Business.UI;
using Soneta.Core;
using Soneta.CRM;
using Soneta.Ksiega.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Soneta.Ksiega;
using Soneta.Handel;
using Soneta.Kasa;
using Soneta.EwidencjaVat;
using Mono.Cecil.Cil;
using Soneta.Types;

[assembly: Worker(typeof(ZadanieTreningowe.Zadanie), typeof(DokEwidencja))]

namespace ZadanieTreningowe
{
    public class Zadanie
    {
        [Context, Required]
        public NamedStream XMLFileName { get; set; }

        [Context]
        public Session Session { get; set; }

        [Action(
            "Import faktur",
            Priority = 1000,
            Icon = ActionIcon.Open,
            Mode = ActionMode.SingleSession,
            Target = ActionTarget.ToolbarWithText)]

        public void Fun(Context context)
        {
            CoreModule coreModule = CoreModule.GetInstance(Session);
            ListXml dane = ReadFile(XMLFileName);
            CRMModule crmModule = CRMModule.GetInstance(Session);
            KsiegaModule ksiegaModule = KsiegaModule.GetInstance(Session);
            EwidencjaVatModule ewidencjaVatModule = EwidencjaVatModule.GetInstance(Session);

            //Otwieramy tranzakcję bazodawnową
            using (Session session = context.Login.CreateSession(true, false))
            {
                bool isKontrahentExist = false;
                
                Kontrahenci kontrahenci = crmModule.Kontrahenci;
                //sprawdzanie czy istnieje kontrahent
                var checkKontahent = kontrahenci.WgKodu[dane.Kontrahent.Kod];
                if (checkKontahent != null)
                    isKontrahentExist = true;


                //Otwieramy transkację biznesową do edycji
                using (ITransaction tran = Session.Logout(true))
                {
                    if (!isKontrahentExist)
                    {
                        //Tworzymy pustego kontrahenta
                        Kontrahent kontrahent = new Kontrahent()
                        {
                            Kod = dane.Kontrahent.Kod,
                            Nazwa = dane.Kontrahent.Nazwa,
                            NIP = dane.Kontrahent.Nip,
                        };
                        //Dodajemy pusty obiekt do tablie kontrahentów
                        crmModule.Kontrahenci.AddRow(kontrahent);
                        kontrahent.Adres.Ulica = dane.Kontrahent.Ulica;
                        kontrahent.Adres.Miejscowosc = dane.Kontrahent.Miasto;
                        kontrahent.Adres.Kraj = dane.Kontrahent.Kraj;
                    }

                    SprzedazEwidencja ewidencja = new SprzedazEwidencja();
                    coreModule.DokEwidencja.AddRow(ewidencja);

                    for (int i = 0; i < dane.Vat.Count; i++)
                    {
                        int stawka = int.Parse(dane.Vat[i].Stawka);
                        ewidencjaVatModule.EleEwidencjiVATT.AddRow(new ElemEwidencjiVATSprzedaz(ewidencja)
                        {  
                            DefinicjaStawki = coreModule.DefStawekVat[StatusStawkiVat.Opodatkowana, new Percent(stawka), false],
                            Brutto = Int32.Parse(dane.Vat[i].Brutto)
                        });
                    }

                    ElementOpisuEwidencji elemOpisu = new ElementOpisuEwidencji(ewidencja)
                    {
                        Kwota = Int32.Parse(dane.Kwoty.brutto),
                        Symbol = dane.NumerPelny
                    };
                    ksiegaModule.OpisAnalityczny.AddRow(elemOpisu);

                    tran.Commit();
                }

                session.Save();
            }


        }


        public MessageBoxInformation Funkcja(Kontrahenci kontrahenci)
        {

            return new MessageBoxInformation("Import")
            {
                Type = MessageBoxInformationType.Information,
                Text = "Zakończono proces importowania dokumentu" + Environment.NewLine + "Odśwież listę lub naciśnij klawisz F5. ",
                OKHandler = () => null
            };
        }

        public ListXml ReadFile(NamedStream XMLFileName)
        {
            StreamReader objReader = new StreamReader(XMLFileName.FileName);

            string linia = "";
            string kawalki = "";
            int iloscVat = 0;

            //odczytanie danych z xml (każda wartość do nowej lini)
            while (linia != null)
            {
                linia = objReader.ReadLine();

                if (linia != null && linia != "")
                {
                    iloscVat = Regex.Matches(linia, "LINIA_VAT").Count;
                    kawalki = Regex.Replace(linia, "<.*?>", "\n");
                }
            }
            objReader.Close();

            iloscVat /= 2;

            //Wrzucenie wartości do listy
            List<string> daneXml = kawalki.Split('\n').ToList();

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
            KontrahentXml kontrahent = new KontrahentXml(daneXml[24], daneXml[25], daneXml[26],
                                                         daneXml[27], daneXml[28], daneXml[29],
                                                         daneXml[30], daneXml[31]);

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

