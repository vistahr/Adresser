using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;

namespace Adresser
{
    public partial class AdresserForm : Form
    {

        enum Anzeigemodus { Anzeigen, Bearbeiten }

        private Adressbuch _adressBuch;
        private Adresse _adresse;

        private Anzeigemodus _aktuellerModus;

        public AdresserForm(Adressbuch adressBuch, Adresse adresse)
        {
            _adressBuch = adressBuch;
            _adresse = adresse;

            InitializeComponent();

            // delegates registrieren
            _adressBuch.updateKontakte += new Adressbuch.OnUpdateKontakteDelegate(ladeKontakte);
            _adresse.updateAdressEigenschaft += new Adresse.OnUpdateAdressEigenschaft(ladeAdressdetails);
        }


        /// <summary>
        /// Filtert die Namen nach dem Suchstring
        /// </summary>
        /// <param name="name">Suchstring</param>
        private void kontakteFiltern(string name)
        {
            lstAdressen.Items.Clear();
            lstAdressen.Items.AddRange(_adressBuch[txtKontakteSuchen.Text].ToArray());
        }


        /// <summary>
        /// Lädt alle Dateien der Listbox ein. Neu und Update.
        /// </summary>
        private void ladeKontakte()
        {
            // Nach Nachnamen sortieren. Wenn kein Nachname vorhanden, dann Vorname.
            lstAdressen.Items.Clear();
            lstAdressen.Items.AddRange(_adressBuch[String.Empty].ToArray());

            stalblAdressbuchdatei.Text = "Adressbuchdatei: " + _adressBuch.AdressenPfad;

            setzeAnzeigeModus(Anzeigemodus.Anzeigen);
        }


        /// <summary>
        /// Lädt eine Adresse in die aktuelle Anzeige
        /// </summary>
        /// <param name="adr">Adresssatz</param>
        private void ladeAdressdetails(Adresse adr)
        {
            txtName.Text = adr.Name;
            txtStrasse.Text = adr.Strasse;
            txtPostleitzahl.Text = adr.Postleitzahl;
            txtOrt.Text = adr.Ort;

            setzeAnzeigeModus(Anzeigemodus.Anzeigen);
        }



        private void autorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("by vistahr - 2012");
        }

        private void beendenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void öffnenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileLoadDialog = new OpenFileDialog();
            fileLoadDialog.Filter = "Adressbuchdatei|*.adx";
            fileLoadDialog.Title = "Adresser Adressbuchdatei laden";

            if (fileLoadDialog.ShowDialog() == DialogResult.OK)
            {
                _adressBuch.AdressenPfad = fileLoadDialog.FileName;
                exportToolStripMenuItem1.Enabled = true;
            }
        }


        private void lstAdressen_Click(object sender, EventArgs e)
        {
            ListBox lstAdressbuch = sender as ListBox;

            if (_aktuellerModus == Anzeigemodus.Bearbeiten)
            {
                var result = MessageBox.Show("Bearbeiten abbrechen und Änderungen verwerfen ?", "Abbbruch",
                                   MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    setzeAnzeigeModus(Anzeigemodus.Anzeigen);
                }
                else
                {
                    return;
                }
            }

            try
            {
                _adresse.Name = ((Adresse)lstAdressbuch.SelectedItem).Name;
                _adresse.Strasse = ((Adresse)lstAdressbuch.SelectedItem).Strasse;
                _adresse.Postleitzahl = ((Adresse)lstAdressbuch.SelectedItem).Postleitzahl;
                _adresse.Ort = ((Adresse)lstAdressbuch.SelectedItem).Ort;
            }
            catch (NullReferenceException)
            {
                // keine Adressbuchdatei geladen, order keinen gültigen Namen angeklickt
                return;
            }

        }

        private void btnAendern_Click(object sender, EventArgs e)
        {
            setzeAnzeigeModus(Anzeigemodus.Bearbeiten);
        }

        /// <summary>
        /// Speichern-Klick aktualisiert den Adressbuchsatz und schreibt die XML-Adressdatei.
        /// </summary>
        private void btnSpeichern_Click(object sender, EventArgs e)
        {
            bool error = false;
            errorProvider.Clear();

            // Validierung PLZ
            if (!_adresse.IsValidPostleitzahl(txtPostleitzahl.Text))
            {
                errorProvider.SetError(txtPostleitzahl, "Keine gültige Postleitzahl! PLZ besteht auf 5 Ziffern.");
                error = true;
            }
            // Validierung Name
            if (!_adresse.IsValidName(txtName.Text))
            {
                errorProvider.SetError(txtName, "Keine gültiger Name! Name muss aus Vor- und Nachnamen bestehen.");
                error = true;
            }
            // Validierung Ort
            if(!_adresse.IsValidOrt(txtOrt.Text))
            {
                errorProvider.SetError(txtOrt, "Keine gültiger Ort! Dieser darf nicht leer sein.");
                error = true;
            }

            if (error) return;

            try
            {
                var kontakt = (from k in _adressBuch.Kontakte
                               where k.Name.Equals(_adresse.Name)
                               select k).First();

                kontakt.Name = txtName.Text;
                kontakt.Strasse = txtStrasse.Text;
                kontakt.Postleitzahl = txtPostleitzahl.Text;
                kontakt.Ort = txtOrt.Text;

                ladeKontakte(); // refreshed die Listbox und lädt alle Kontakte neu

                _adressBuch.Speichern = true; // saveflag speichert im Controller die Datei

            }
            catch (NullReferenceException)
            {
                // Datensatz wurde nicht gefunden
            }

            setzeAnzeigeModus(Anzeigemodus.Anzeigen);
        }


        private void setzeAnzeigeModus(Anzeigemodus amod)
        {
            _aktuellerModus = amod;

            if (amod == Anzeigemodus.Anzeigen)
            {
                // * entfernen
                this.Text = this.Text.Replace("*", "");

                btnSpeichern.Enabled = false;
                btnAendern.Enabled = true;

                txtName.ReadOnly = true;
                txtStrasse.ReadOnly = true;
                txtPostleitzahl.ReadOnly = true;
                txtOrt.ReadOnly = true;

                itemNeu.Enabled = true;
                splitContainer1.Enabled = true;

                grpAdressHauptdaten.Enabled = true;
            }
            else if (amod == Anzeigemodus.Bearbeiten)
            {
                // Im Titel signalisiert ein * den Bearbeitungsmodus
                this.Text += "*";

                btnSpeichern.Enabled = true;
                btnAendern.Enabled = false;

                txtName.ReadOnly = false;
                txtStrasse.ReadOnly = false;
                txtPostleitzahl.ReadOnly = false;
                txtOrt.ReadOnly = false;

                itemNeu.Enabled = false;
            }
        }

        private void lstAdressen_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                cmenuKontakte.Show(lstAdressen, e.Location);

                if (lstAdressen.SelectedIndex == -1)
                {
                    itemEntfernen.Enabled = false;
                }
                else
                {
                    itemEntfernen.Enabled = true;
                };
            }
        }


        private void itemEntfernen_Click(object sender, EventArgs e)
        {
            _adressBuch.Kontakte.RemoveAt(lstAdressen.SelectedIndex);
            ladeKontakte(); // refreshed die Listbox und lädt alle Kontakte neu
            _adressBuch.Speichern = true; // saveflag speichert im Controller die Datei
        }

        private void itemNeu_Click(object sender, EventArgs e)
        {
            try
            {
                Adresse newAdr = new Adresse();
                _adressBuch.Kontakte.Insert(0, newAdr);

                ladeAdressdetails(newAdr);

                setzeAnzeigeModus(Anzeigemodus.Bearbeiten);

            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Kein Adressbuch geladen.");
            }
        }

        private void adressbuchNeu_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.CreatePrompt = false;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "Adressbuchdatei|*.adx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = File.Create(saveFileDialog.FileName);

                XmlWriter xw = XmlWriter.Create(fs);
                xw.WriteStartDocument();
                xw.WriteStartElement("adresser");
                xw.WriteEndElement();
                xw.Flush();
                fs.Close();

                _adressBuch.AdressenPfad = saveFileDialog.FileName;
            }
        }

        private void xMLToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.CreatePrompt = false;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "Extensible Markup Language|*.xml";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _adressBuch.ToXML().Save(saveFileDialog.FileName);
            }
        }




        private void txtKontakteSuchen_Enter(object sender, EventArgs e)
        {
            txtKontakteSuchen.Text = "";
        }

        private void txtKontakteSuchen_Leave(object sender, EventArgs e)
        {
            if (txtKontakteSuchen.Text.Equals(String.Empty))
            {
                kontakteFiltern(String.Empty);
                txtKontakteSuchen.Text = "Nach Name suchen";
            }
        }

        private void txtKontakteSuchen_KeyUp(object sender, KeyEventArgs e)
        {
            kontakteFiltern(txtKontakteSuchen.Text);
        }







    }
}
