using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Adresser
{
    public class Adresser
    {

        # region Klassenvariablen

        private AdresserForm _view;
        private Adressbuch _adrBuch;
        private Adresse _adr;

        #endregion

        #region Konstruktor

        public Adresser() 
        {
            _adrBuch = new Adressbuch();
            _adr = new Adresse();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _view = new AdresserForm(_adrBuch, _adr);

            // delegate registrieren
            _adrBuch.updateAdressbuchDatei += new Adressbuch.OnUpdateAdressbuchDateiDelegate(leseAdressbuchAusXMLDatei);
            _adrBuch.updateSpeichern += new Adressbuch.OnUpdateSpeichernDelegate(schreibeAdressbuchXMLDatei);


            Application.Run(_view);
        }

        #endregion

        #region Methoden

        /// <summary>
        /// Schreibe Adressbuch in XML Datei.
        /// </summary>
        private void schreibeAdressbuchXMLDatei()
        {
            if (!System.IO.File.Exists(_adrBuch.AdressenPfad))
                throw new FileNotFoundException("Adressdatei nicht gefunden.");

            XDocument xdoc = new XDocument(_adrBuch.ToXML());
            xdoc.Save(_adrBuch.AdressenPfad);
        }

        /// <summary>
        /// Liest eine XML Datei aus und speicher diese im Adressbuch als Liste.
        /// </summary>
        /// <param name="kontaktDatei">Absoluter Pfad mit Datei und Endung</param>
        private void leseAdressbuchAusXMLDatei(string kontaktDatei)
        {
            if (!System.IO.File.Exists(kontaktDatei))
                throw new FileNotFoundException("Adressdatei nicht gefunden.");
            
            XDocument xdoc = XDocument.Load(_adrBuch.AdressenPfad);

            var query = from kontakt in xdoc.Descendants("contact")
                        select kontakt;

            IList<Adresse> tempKontakte = new List<Adresse>();
            Adresse tempAdr;

            foreach (XElement element in query)
            {
                tempAdr = new Adresse();
                try
                {
                    tempAdr.Name = element.Attribute("name").Value;
                    tempAdr.Strasse = element.Element("street").Value;
                    tempAdr.Postleitzahl = element.Element("postcode").Value;
                    tempAdr.Ort = element.Element("city").Value;
                }
                catch (NullReferenceException) {  }
                tempKontakte.Add(tempAdr);
            }

            _adrBuch.Kontakte = tempKontakte;
            
        }

        #endregion

    }
}
