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
using static Soneta.Towary.CenyRabatyProgowe;
using static Soneta.CRM.CRMModule;
using Soneta.BI;

[assembly: Worker(typeof(ZadanieTreningowe.Zadanie), typeof(DokEwidencja))]

namespace ZadanieTreningowe
{
    public class Zadanie
    {
        [Context, Required]
        public NamedStream XMLFileName { get; set; }

        [Context]
        public Context context { get; set; }

        [Action(
            "Import faktur",
            Priority = 1000,
            Icon = ActionIcon.Open,
            Mode = ActionMode.SingleSession,
            Target = ActionTarget.ToolbarWithText)]

        public MessageBoxInformation Fun()
        {
            List<ListXml> lxml = new List<ListXml>();
            ListXml dane = ReadXml.ReadFile(XMLFileName);

            //Otwieramy tranzakcję bazodawnową
            using (Session session = context.Login.CreateSession(false, true))
            {
                CoreModule coreModule = CoreModule.GetInstance(session);
                CRMModule crmModule = CRMModule.GetInstance(session);
                HandelModule handel = HandelModule.GetInstance(session);
                EwidencjaVatModule ewidencjaVatModule = EwidencjaVatModule.GetInstance(session);

                
                bool isKontrahentExist = false;
                bool isDokumentExist = false;

                Kontrahenci kontrahenci = crmModule.Kontrahenci;
                //sprawdzanie czy istnieje kontrahent
                var checkKontahent = kontrahenci.WgKodu[dane.Kontrahent.Kod];


                if (checkKontahent != null)
                    isKontrahentExist = true;

                //Otwieramy transkację biznesową do edycji
                using (ITransaction tran = session.Logout(true))
                {
                    //Tworzymy pustego kontrahenta
                    Kontrahent kontrahent = new Kontrahent();
                    if (!isKontrahentExist)
                    {
                        //Dodajemy kontrahenta do bazy
                        crmModule.Kontrahenci.AddRow(kontrahent);
                        kontrahent = CreateKontrahent.Create(kontrahent, dane);
                    }
                    else
                    {
                        //Znajdujemy istniejącego kontrahenta w bazie
                        kontrahent = crmModule.Kontrahenci.WgKodu[dane.Kontrahent.Kod];
                    }

                    DefinicjaDokumentu def = coreModule.DefDokumentow.WgSymbolu["SPT"];
                    SprzedazEwidencja nowySPT = new SprzedazEwidencja();
                    coreModule.DokEwidencja.AddRow(nowySPT);

                    // Ustawienie numeru dokumentu, podmiotu i opisu
                    nowySPT = CreateSPT.Create(nowySPT, def, dane, kontrahent);



                    // Dodanie elementów VAT
                    AddElementVat.Add(nowySPT, dane, coreModule, ewidencjaVatModule);
                    


                    tran.Commit();
                }

                session.Save();
            }

            return new MessageBoxInformation("Import")
            {
                Type = MessageBoxInformationType.Information,
                Text = "Zakończono proces importowania dokumentu" + Environment.NewLine + "Odśwież listę lub naciśnij klawisz F5. ",
                OKHandler = () => null
            };
        }


       
    }
}

