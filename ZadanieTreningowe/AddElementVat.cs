using Soneta.Core;
using Soneta.EwidencjaVat;
using Soneta.Types;
using System;

namespace ZadanieTreningowe
{
    public class AddElementVat
    {
        public static void Add(SprzedazEwidencja nowySPT, ListXml dane, CoreModule coreModule, EwidencjaVatModule ewidencjaVatModule)
        {
            for (int i = 0; i < dane.Vat.Count; i++)
            {
                ElemEwidencjiVATSprzedaz elemEwidencjiVATSprzedaz = new ElemEwidencjiVATSprzedaz(nowySPT);
                ewidencjaVatModule.EleEwidencjiVATT.AddRow(elemEwidencjiVATSprzedaz);
                elemEwidencjiVATSprzedaz.DefinicjaStawki = coreModule.DefStawekVat.WgKodu[dane.Vat[i].Stawka.Substring(0, dane.Vat[i].Stawka.Length - 3) + "%"];
                elemEwidencjiVATSprzedaz.Brutto = new Currency(Double.Parse(dane.Vat[i].Brutto, System.Globalization.CultureInfo.InvariantCulture));
            }
        }
    }
}