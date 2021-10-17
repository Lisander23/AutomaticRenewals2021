using AutomaticRenewals2021.Models;
using COTDP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomaticRenewals2021
{
    public partial class Form1 : Form
    {
        DataUtility objDUT = new DataUtility();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public ServiceResponse AutomaticRenewalProcess()
        {
            ServiceResponse response = new ServiceResponse();
            try
            {
                objDUT.EnviarCorreoLP(0, "", "Automatic Process Began", "Automatic Process Began", "lisander23@gmail.com", null, "", "");
                objDUT.WriteLog("START PROCESS: " + DateTime.Now, 1000);

                RunAutomaticRenewals();

                RunOneTimePaymentRequest();

                RunRecurrentPaymentRequests();

            }
            catch (Exception ex)
            {

                objDUT.WriteLog("Error in Load Method: " + ex.Message);
            }

            return response;
        }
    }
}
