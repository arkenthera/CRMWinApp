﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRMWinApp.Forms;
using System.IO;
using CRMWinApp.Models;

namespace CRMWinApp.UserControls
{
    public partial class RegisterCrime : UserControl, IUserPermissionDisable
    {
        private CRMDataModel context = new CRMDataModel();
        private Criminal selectedCriminal;

        private User _user;

        public delegate void CrimeRegisteredDelegate();
        public CrimeRegisteredDelegate EventCrimeRegistered;

        public delegate void ShowCrimesDelegate(Criminal c);
        public ShowCrimesDelegate EventShowCrimes;
        public User User
        {
            get { return _user; }
            set
            {
                User user = value;

                User ux = context.Users.Where(u => u.Id == user.Id).FirstOrDefault();

                _user = ux;

            }
        }

        public RegisterCrime()
        {
            InitializeComponent();
        }

        private void findPrisoner_Click(object sender, EventArgs e)
        {
            SearchCriminal sc = new SearchCriminal();
            sc.PassCriminal = new SearchCriminal.PassCriminalDel(SetCriminalInfo);
            sc.Show();

        }
        void SetCriminalInfo(Models.Criminal c)
        {
            if (c != null)
            {
                nameTB.Text = c.Name;
                surnameTB.Text = c.Surname;
                criminalPictureBox.Image = ByteToImage(c.PictureFront);
                selectedCriminal = c;
                goCrimesButton.Enabled = true;
            }
        }

        public Image ByteToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        private void RegisterCrime_Load(object sender, EventArgs e)
        {
            var types = context.CrimeTypes.ToList();

            for (int i = 0; i < types.Count; ++i)
            {
                crimeTypeCB.Items.Add(types[i].Name);
            }

            var agencies = context.Agencies.ToList();

            for (int i = 0; i < agencies.Count; ++i)
            {
                agencyCB.Items.Add(agencies[i].Name);
            }

            var attorneys = context.Attorneys.ToList();


            attorneyCB.DataSource = attorneys;

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void addTypeButton_Click(object sender, EventArgs e)
        {
            AddNewCrimeType anct = new AddNewCrimeType();
            anct.TypeAddedEvent = new AddNewCrimeType.TypeAddedDelegate(EventTypeAdded);
            anct.Show();

        }
        private void EventTypeAdded()
        {
            crimeTypeCB.Items.Clear();
            var types = context.CrimeTypes.ToList();

            for (int i = 0; i < types.Count; ++i)
            {
                crimeTypeCB.Items.Add(types[i].Name);
            }
            crimeTypeCB.SelectedText = types[types.Count - 1].Name;
        }
        private void submitButton_Click(object sender, EventArgs e)
        {
            if (selectedCriminal == null)
            {
                MessageBox.Show("You have to select a criminal first.");
            }
            else
            {
                if (startPunishDate.Value >= endPunishDate.Value)
                {
                    MessageBox.Show("Huehuehueheu");
                    return;
                }
                if (string.IsNullOrEmpty(locationTB.Text) ||
                    string.IsNullOrEmpty(noteTB.Text)
                    )
                {
                    MessageBox.Show("All fields should not be empty.");
                    return;

                }
                Arrest newArrest = new Arrest();
                Criminal cr = context.Criminals.Where(x => x.Id == selectedCriminal.Id).FirstOrDefault();

                var crimeType = context.CrimeTypes.Where(x => x.Name == (string)crimeTypeCB.SelectedItem).FirstOrDefault();

                newArrest.Type = crimeType;

                var agency = context.Agencies.Where(x => x.Name == (string)agencyCB.SelectedItem).FirstOrDefault();

                newArrest.Agency = agency;

                newArrest.Location = locationTB.Text;

                newArrest.Date = crimeDate.Value;

                var att = context.Attorneys.Where(x => x.Name == ((Attorney)attorneyCB.SelectedItem).Name).FirstOrDefault();

                newArrest.Criminal = cr;

                newArrest.Description = noteTB.Text;

                Cite newCite = new Cite();
                newCite.Note = noteTB.Text;
                newCite.Officer = _user;

                CiteType citeType = new CiteType();
                citeType.Name = "Arrest of " + selectedCriminal.Name;

                newCite.Type = citeType;

                var exc = context.CiteTypes.Where(x => x.Name == citeType.Name).FirstOrDefault();

                if (exc != null)
                    newCite.Type = exc;
                var exPunishment = context.Punishments.Where(x => x.Criminal.Id == cr.Id).ToList();

                if (exPunishment.Count > 0)
                {
                    for (int i = 0; i < exPunishment.Count; ++i)
                    {
                        //Compare the dates

                        if (!(startPunishDate.Value > exPunishment[i].Start && startPunishDate.Value > exPunishment[i].End))
                        {
                            MessageBox.Show("There is an ongoing crime record for this range of dates.");
                            return;
                        }
                    }
                }
                Punishment p = new Punishment();
                p.Criminal = cr;
                p.Start = startPunishDate.Value;
                p.End = endPunishDate.Value;



                Charge newCharge = new Charge();
                newCharge.Attorney = att;
                newCharge.Cite = newCite;
                newCharge.Date = crimeDate.Value;
                newCharge.Against = cr;

                try
                {


                    context.Punishments.Add(p);
                    context.Arrests.Add(newArrest);
                    context.Charges.Add(newCharge);
                    context.SaveChanges();

                    EventCrimeRegistered();

                    MessageBox.Show("Added succcesfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void goCrimesButton_Click(object sender, EventArgs e)
        {
            EventShowCrimes(selectedCriminal);
        }

        public void Disable(List<Permission> permissions)
        {
            foreach (Permission p in permissions)
            {
                if (p.Name == "CAN_REGISTER_CRIME_CRIMINAL")
                {
                    groupBox1.Enabled = true;
                }

            }
        }
    }
}
