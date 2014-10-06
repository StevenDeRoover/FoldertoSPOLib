using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using System.Windows.Forms;

namespace CNCryptography
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnencrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string encrypted = Cryptography.Encrypt<RijndaelManaged>(txtSource.Text, txtPassword.Text);
                this.txtResult.Text = encrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not enrypt the message. Reason: " + ex.Message);
            }

        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string decrypted = Cryptography.Decrypt<RijndaelManaged>(txtSource.Text, txtPassword.Text);
                this.txtResult.Text = decrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not decrypt the message. Reason: " + ex.Message);
            }
        }
    }
}
