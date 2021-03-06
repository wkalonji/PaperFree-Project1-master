﻿using System;
using System.Web.UI;
using System.Data.SqlClient;
using BarcodeConversion.App_Code;

namespace BarcodeConversion
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // SHOW 'SETTINGS' BUTTON IF ADMIN. IF NEW, SAVE USER.
            bool isAdmin = false;
            try
            {
                string user = Environment.UserName;
                if (user != null) 
                {
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            // If user exists, get Admin status
                            cmd.CommandText = "SELECT ADMIN FROM OPERATOR WHERE NAME = @user";
                            cmd.Parameters.AddWithValue("@user", user);
                            con.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null)
                                isAdmin = (bool)cmd.ExecuteScalar();
                            else 
                            {
                                // If user doesn't exist, register user and set Admin status to Operator.
                                using (SqlCommand cmd2 = con.CreateCommand()) 
                                {
                                    cmd2.CommandText = "INSERT INTO OPERATOR (NAME, ADMIN) VALUES(@user,@admin)";
                                    cmd2.Parameters.AddWithValue("@user", user);
                                    cmd2.Parameters.AddWithValue("@admin", 0);
                                    try
                                    {
                                        cmd2.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex)
                                    {
                                        string msg = "Issue occured trying to save operator. Please contact system admin. " + Environment.NewLine + ex.Message;
                                        System.Windows.Forms.MessageBox.Show(msg, "Error 94");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to identify active user. Please contact your system admin. " + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 95");
            }
            if (isAdmin) settings.Visible = true;
        }
    }
}