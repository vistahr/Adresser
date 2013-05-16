using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Linq;

namespace Adresser
{
    public class Adressbuch
    {

        #region Delegates und Events

        public delegate void OnUpdateAdressbuchDateiDelegate(string s);
        public event OnUpdateAdressbuchDateiDelegate updateAdressbuchDatei;

        public delegate void OnUpdateKontakteDelegate();
        public event OnUpdateKontakteDelegate updateKontakte;

        public delegate void OnUpdateSpeichernDelegate();
        public event OnUpdateSpeichernDelegate updateSpeichern;

        #endregion


        # region Klassenvariablen

        private bool _speichern = false;
        private string _adressenPfad;
        private IList<Adresse> _kontakte;

        #endregion


        # region Getter & Setter

        public bool Speichern
        {
            get { return _speichern; }
            set { _speichern = value; updateSpeichern(); }
        }

        public string AdressenPfad
        {
            get { return _adressenPfad; }
            set
            {
                _adressenPfad = value;
                updateAdressbuchDatei(_adressenPfad);
            }

        }

        public IList<Adresse> Kontakte
        {
            get { return _kontakte; }
            set
            {
                _kontakte = value;
                updateKontakte();
            }
        }

        public IList<Adresse> this [string searchName]
        {
            get 
            {
                var result = (from k in Kontakte
                              where k.Name.ToLower().Contains(searchName.ToLower())
                              orderby (k.Name.Split(' ').Length > 1 ? k.Name.Split(' ')[1] : k.Name)
                              select k).ToList();

                return result;
            }
        }

        #endregion


        #region Methoden

        /// <summary>
        /// Konvertiert die Liste des Adressbuchs zu einem XDocument
        /// </summary>
        /// <returns>XDocument</returns>
        public XDocument ToXML()
        {
            XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

            XElement xroot = new XElement("adresser");
            xdoc.Add(xroot);

            foreach (Adresse data in _kontakte)
            {
                if (data.Name == String.Empty)
                    continue;

                XElement xel = new XElement("contact",
                                   new XAttribute("name", data.Name),
                                   new XElement("street", data.Strasse),
                                   new XElement("postcode", data.Postleitzahl),
                                   new XElement("city", data.Ort)
                               );

                xroot.Add(xel);
            }
            
            return xdoc;
        }

        #endregion

    }
}
