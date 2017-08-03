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
                ConnectionStringSettings conString = ConfigurationManager.ConnectionStrings["myConnection"];
                string connectionString = conString.ConnectionString;
                SqlConnection con = new SqlConnection(connectionString);
                return con;
            }
        }


        // GET USER ID VIA USERNAME. HELPER FUNCTION
        public static int getUserId(string user)
        {
            SqlConnection con = ConnectionObj;
            con.Open();
            int opID = 0;
            SqlCommand cmd = new SqlCommand("SELECT ID FROM OPERATOR WHERE NAME = @username", con);
            cmd.Parameters.AddWithValue("@username", user);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    opID = (int)reader.GetValue(0);
                }
                reader.Close();
                con.Close();
                return opID;
            }
            else
            {
                con.Close();
                return opID;
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