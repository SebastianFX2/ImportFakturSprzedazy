using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Core;
using Soneta.CRM;
using Soneta.EwidencjaVat;
using Soneta.Kadry;
using Soneta.Kalend;
using Soneta.Tools;
using Soneta.Types;
using System;
using System.Text;

using static Soneta.Place.WypElementNadgodziny;

[assembly: Worker(typeof(ZadanieTreningowe.Zadanie), typeof(DokEwidencja))]

namespace ZadanieTreningowe
{
    public class Zadanie
    {
        [Context, Required]
        public NamedStream[] XMLFileName { get; set; }

        [Context]
        public Context Context { get; set; }

        [Action(
            "Import faktur",
            Priority = 1000,
            Icon = ActionIcon.Open,
            Mode = ActionMode.Progress,
            Target = ActionTarget.Menu | ActionTarget.ToolbarWithText)]
        public Object Fun()
        {
            int i = 0;
            StringBuilder endMessage = new StringBuilder();
            foreach (var xmlFName in XMLFileName)
            {
                Percent percentProgress = new Percent((decimal)i / XMLFileName.Length);
                TraceInfo.SetProgressBar(percentProgress);
                ListXml dane = ReadXml.ReadFile(xmlFName);
                //TraceInfo.WriteProgress("Pobranie danych z pliku");
                //Otwieramy tranzakcję bazodawnową
                using (Session session = Context.Login.CreateSession(false, true))
                {
                    CoreModule coreModule = CoreModule.GetInstance(session);
                    CRMModule crmModule = CRMModule.GetInstance(session);
                    EwidencjaVatModule ewidencjaVatModule = EwidencjaVatModule.GetInstance(session);

                    bool isKontrahentExist = false;

                    Kontrahenci kontrahenci = crmModule.Kontrahenci;
                    //sprawdzanie czy istnieje kontrahent
                    var checkKontahent = kontrahenci.WgKodu[dane.Kontrahent.Kod];


                    isKontrahentExist = checkKontahent != null;
                /*
                    if (checkKontahent != null)
                        isKontrahentExist = true;
                */
                    //Otwieramy transkację biznesową do edycji
                    using (ITransaction tran = session.Logout(true))
                    {
                        //Tworzymy pustego kontrahenta
                        //TraceInfo.WriteProgress("Dodanie kontrahenta");
                        Kontrahent kontrahent = new Kontrahent();
                        if (!isKontrahentExist)
                        {
                            try
                            {
                                //Dodajemy kontrahenta do bazy
                                crmModule.Kontrahenci.AddRow(kontrahent);
                                kontrahent = CreateKontrahent.Create(kontrahent, dane);
                            }
                            catch (Exception)
                            {
                                endMessage.Append(xmlFName.FileName + ": Blad w danych kontrahenta\n");
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
                        //TraceInfo.WriteProgress("Dodanie dokumentu");
                        try
                        {
                            nowySPT = CreateSPT.Create(nowySPT, def, dane, kontrahent);
                        }
                        catch (DuplicatedRowException) { continue; }

                        // Dodanie elementów VAT
                        //TraceInfo.WriteProgress("Dodanie elementów VAT");
                        try
                        {
                            AddElementVat.Add(nowySPT, dane, coreModule, ewidencjaVatModule);
                        }
                        catch (Exception)
                        {
                            endMessage.Append(xmlFName.FileName + ": Blad w danych ewidencji VAT\n");
                            continue;
                        }

                        tran.Commit();
                    }

                    session.Save();
                    //TraceInfo.WriteProgress("Zapis do bazy");
                }
                i++;
            }

            if (endMessage.Length == 0)
                endMessage.Append("Zakończono proces importowania dokumentu pomyślnie");
            else
                endMessage.Append("Prosze spróbować ponownie albo upewnić się że dokumenty mają poprawyn format");

            return new MessageBoxInformation("Import")
            {
                Type = MessageBoxInformationType.Information,
                Text = endMessage.ToString(),
                OKHandler = () => null
            };
        }
    }
}