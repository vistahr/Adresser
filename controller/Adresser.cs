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
        private Addressbook _adrBuch;
        private Address _adr;

        #endregion

        #region Konstruktor

        public Adresser() 
        {
            _adrBuch = new Addressbook();
            _adr = new Address();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _view = new AdresserForm(_adrBuch, _adr);

            // delegate registrieren
            _adrBuch.updateAdressbuchDatei += new Addressbook.OnUpdateAdressbuchDateiDelegate(ReadAdressbookXMLFile);
            _adrBuch.updateSpeichern += new Addressbook.OnUpdateSpeichernDelegate(WriteAdressbookXMLFile);


            Application.Run(_view);
        }

        #endregion

        #region Methoden

        /// <summary>
        /// Schreibe Adressbuch in XML Datei.
        /// </summary>
        private void WriteAdressbookXMLFile()
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
        private void ReadAdressbookXMLFile(string kontaktDatei)
        {
            if (!System.IO.File.Exists(kontaktDatei))
                throw new FileNotFoundException("Adressdatei nicht gefunden.");
            
            XDocument xdoc = XDocument.Load(_adrBuch.AdressenPfad);

            var query = from kontakt in xdoc.Descendants("contact")
                        select kontakt;

            IList<Address> tempKontakte = new List<Address>();
            Address tempAdr;

            foreach (XElement element in query)
            {
                tempAdr = new Address();
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
