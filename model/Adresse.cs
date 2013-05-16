using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adresser
{

    public class Adresse
    {

        # region Delegates & Events

        public delegate void OnUpdateAdressEigenschaft(Adresse adr);
        public event OnUpdateAdressEigenschaft updateAdressEigenschaft;

        #endregion


        #region Klassenvariablen

        private string _name;
        private string _strasse;
        private string _postleitzahl;
        private string _ort;

        #endregion


        #region Getter & Setter


        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                notifyAdressEigenschaft();
            }
        }
        public string Strasse
        {
            get { return _strasse; }
            set
            {
                _strasse = value;
                notifyAdressEigenschaft();
            }
        }
        public string Postleitzahl
        {
            get { return _postleitzahl; }
            set
            {
                _postleitzahl = value;
                notifyAdressEigenschaft();
            }
        }
        public string Ort
        {
            get { return _ort; }
            set
            {
                _ort = value;
                notifyAdressEigenschaft();
            }
        }

        #endregion


        #region Validierungsmethoden

        /// <summary>
        /// PLZ muss 5stellig sein und darf nur Ziffern enthalten
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Ob die Validierung erfolgreich war</returns>
        public bool IsValidPostleitzahl(string input)
        {
            bool error = false;
            int plz = 0;
            try
            {
                plz = Convert.ToInt32(input);
            }
            catch (Exception)
            {
                error = true;
            }

            // auf 5 stellige plz prüfen
            if ((Math.Floor(Math.Log10(plz)) + 1) != 5)
                error = true;

            if (error == true)
                return false;

            return true;
        }

        /// <summary>
        /// Name besteht aus 2 Wörtern und darf nicht leer sein
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Ob die Validierung erfolgreich war</returns>
        public bool IsValidName(string input)
        {
            if (input != String.Empty && input != null && input.Trim().Contains(" "))
                return true;

            return false;
        }

        /// <summary>
        /// Ort darf nicht leer und nicht null sein
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Ob die Validierung erfolgreich war</returns>
        public bool IsValidOrt(string input)
        {
            if (input != String.Empty && input != null)
                return true;

            return false;
        }

        #endregion

        #region Methoden

        /// <summary>
        /// Gibt den Namen zurück.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return _name;
        }

        /// <summary>
        /// Ruft den Delegate OnUpdateAdressEigenschaft auf.
        /// </summary>
        public void notifyAdressEigenschaft()
        {
            if (this.updateAdressEigenschaft != null)
            {
                updateAdressEigenschaft(this);
            }
        }

        #endregion


        #region Konstruktor

        public Adresse(string name)
        {
            this.Name = name;
        }

        public Adresse()
        {
            this.Name = String.Empty; // name darf nicht null sein
        }

        #endregion

    }

}
