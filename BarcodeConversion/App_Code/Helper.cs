using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace BarcodeConversion.App_Code
{
    public class Helper
    {
        // GET CONNECTION OBJECT. FUNCTION
        public static SqlConnection ConnectionObj
        {
            get
            {   
                try
                {
                    ConnectionStringSettings conString = ConfigurationManager.ConnectionStrings["myConnection"];
                    string connectionString = conString.ConnectionString;
                    SqlConnection con = new SqlConnection(connectionString);
                    return con;
                }
                catch (Exception ex)
                {
                    string msg = "Issue occured while attempting to operator's ID. Contact system admin. " + Environment.NewLine + ex.Message;
                    System.Windows.Forms.MessageBox.Show(msg, "Error 92");
                    return null;
                }
            }
        }


        // GET USER ID VIA USERNAME. HELPER FUNCTION
        public static int getUserId(string user)
        {
            try
            {
                int opID = 0;
                using (SqlConnection con = ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @username";
                        cmd.Parameters.AddWithValue("@username", user);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            opID = (int)result;
                            return opID;
                        } 
                        else return opID;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to operator's ID. Contact system admin. " + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 93");
                return 0;
            }
        }



        // GET CONTROL THAT FIRED POSTBACK. HELPER FUNCTION.
        public static Control GetPostBackControl(Page page)
        {
            Control control = null;

            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if (ctrlname != null && ctrlname != string.Empty)
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }
    }
}