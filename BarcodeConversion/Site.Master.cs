using System;
using System.Web.UI;
using System.Data.SqlClient;
using BarcodeConversion.App_Code;

namespace BarcodeConversion
{
    public partial class SiteMaster : MasterPage
    {
        SqlConnection con = Helper.ConnectionObj;

        protected void Page_Load(object sender, EventArgs e)
        {
            // SHOW 'SETTINGS' BUTTON IF ADMIN. IF NEW, SAVE USER.
            bool isAdmin = false;
            try
            {
                string user = Environment.UserName;
                if (user != null)
                {   
                    // If user exists, get Admin status
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT ADMIN FROM OPERATOR WHERE NAME = @user", con);
                    cmd.Parameters.AddWithValue("@user", user);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            isAdmin = (bool)reader.GetValue(0);
                        }
                        reader.Close();
                        con.Close();
                    }
                    else
                    {
                        reader.Close();

                        // If user doesn't exist, register user and set Admin status to operator.
                        string msg;
                        SqlCommand cmd2 = new SqlCommand("INSERT INTO OPERATOR (NAME, ADMIN) VALUES(@user,@admin)", con);
                        cmd2.Parameters.AddWithValue("@user", user);
                        cmd2.Parameters.AddWithValue("@admin", 0);

                        if (cmd2.ExecuteNonQuery() != 1)
                        {
                            //TBD
                            msg = "Error: Failed trying to identify this computer. Please contact system admin!";
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "')", true);
                        }
                    }                 
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                //string msg = "You've not been found into our system. Contact the system admin.";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Error: Something went wrong while attempting to identify this computer. Please contact your system admin.');", true);
            }
            if (isAdmin) settings.Visible = true;

        }

    }
}