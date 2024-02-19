using System;
using System.Windows.Forms;
using System.Net;

namespace ProjektAmpel
{
    public partial class BrokerConnectForm : Form
    {
        public string BrokerAddress { get; private set; }

        public BrokerConnectForm()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (IsValidIpAddress(ipAddressTextBox.Text))
            {
                BrokerAddress = ipAddressTextBox.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid IP address.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsValidIpAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }
    }
}
