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
using RabbitMQ.Client.Framing.Impl;

[assembly: Worker(typeof(ZadanieTreningowe.Zadanie), typeof(DokEwidencja))]

namespace ZadanieTreningowe
{
    public class Zadanie
    {
        [Context, Required]
        public NamedStream[] XMLFileName { get; set; }

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


            string endMessage = "";

            foreach (var xmlFName in XMLFileName) 
            {
                
                ListXml dane = ReadXml.ReadFile(xmlFName);
                //Otwieramy tranzakcję bazodawnową
                using (Session session = context.Login.CreateSession(false, true))
            {
                CoreModule coreModule = CoreModule.GetInstance(session);
                CRMModule crmModule = CRMModule.GetInstance(session);
                HandelModule handel = HandelModule.GetInstance(session);
                EwidencjaVatModule ewidencjaVatModule = EwidencjaVatModule.GetInstance(session);

                
                bool isKontrahentExist = false;

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
                            try
                            {
                                //Dodajemy kontrahenta do bazy
                                crmModule.Kontrahenci.AddRow(kontrahent);
                                kontrahent = CreateKontrahent.Create(kontrahent, dane);
                            }catch(Exception e)
                            {   endMessage += xmlFName.FileName + ": Blad w danych kontrahenta\n";
                                continue;
                            }
                                 

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
                    try
                    { 
                    nowySPT = CreateSPT.Create(nowySPT, def, dane, kontrahent);
                    }catch (Soneta.Business.DuplicatedRowException e) { continue; }


                        // Dodanie elementów VAT
                        try 
                        { 
                         AddElementVat.Add(nowySPT, dane, coreModule, ewidencjaVatModule);
                        }catch(Exception e) 
                        { endMessage += xmlFName.FileName + ": Blad w danych ewidencji VAT\n";
                            continue;
                        }


                        tran.Commit();
                }

                session.Save();
            }
            }

            if (endMessage == "")
                endMessage = "Zakończono proces importowania dokumentu pomyślnie";
            else
                endMessage += "Prosze spróbować ponownie albo upewnić się że dokumenty mają poprawyn format";

            return new MessageBoxInformation("Import")
            {
                Type = MessageBoxInformationType.Information,
                Text = endMessage,
                OKHandler = () => null
            };
        }


       
    }
}

