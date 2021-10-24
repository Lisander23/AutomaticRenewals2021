using AutomaticRenewals2021.Models;
using COTDP.Models;
using COTDP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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
            objDUT.WriteLog("TEST");
            ServiceResponse response = new ServiceResponse();
            RenewalService _renewalService = new RenewalService();
            try
            {

                response = _renewalService.AutomaticRenewalProcess();
                if (response.Success == true)
                {
                    objDUT.WriteLog("ALL RENEWAL PROCESS FINISHED SUCCESSFULLY");
                }
                else
                {
                    objDUT.WriteLog("RENEWAL PROCESS FINISHED WITH ERRORS" + response.Message);
                }
            }
            catch (Exception)
            {
                objDUT.WriteLog("RENEWAL PROCESS DIDN'T FINISH DUE TO ERRORS" + response.Message);
            }
            Application.Exit();
        }

    }
}
