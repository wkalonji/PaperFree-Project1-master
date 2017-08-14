﻿using System;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using BarcodeConversion.App_Code;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace BarcodeConversion
{
    public partial class Contact : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Make sure only admins can see Settings page
                if (!Page.IsPostBack)
                {
                    jobAbb.Focus();
                    ViewState["switch"] = "on";
                    ViewState["id"] = string.Empty;
                    ViewState["labelRowCount"] = 1;
                    for (int i=1; i<=5; i++)
                    {
                        var sampleList = new List<string> { i.ToString(), "", "", "", "", "on" };
                        var labelValues = new List<string> { i.ToString(), string.Empty, string.Empty, string.Empty };
                        ViewState["labelRow" + i.ToString()] = sampleList;
                        ViewState["labelValues" + i.ToString()] = labelValues;
                    }
                }
                Control c = Helper.GetPostBackControl(this.Page);
                if ( c != null && c.ID.Contains("deleteIcon"))
                {
                    processRequest(c, new EventArgs());
                }
                if ( jobIndexEditingPanel.Visible)
                {
                    int labelRowCount = (int)ViewState["labelRowCount"];
                    for (int i = 1; i <= labelRowCount; i++) getLabelRow(); // Persists all other label rows as well
                }
                    

                if (userStatus() == "True")
                {
                    SettingsPanel.Visible = true;
                }
                else if (userStatus() == "False")
                {
                    SettingsPanel.Visible = false;
                    Response.Redirect("~/");
                    return;
                }
                else if (userStatus() == "Failed")
                {
                    return;
                }
                else if (userStatus() == "Not Found")
                {
                    string msg = "Operator not Found.";
                    System.Windows.Forms.MessageBox.Show(msg);
                    return;
                }
              
                setDropdownColor();
                success.Visible = false;
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to load page. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 50");
                return;
            }
        }



        // CHECK WHETHER USER IS ADMIN
        private string userStatus() 
        {
            string user = Environment.UserName;
            using (SqlConnection con = Helper.ConnectionObj) 
            {
                using (SqlCommand cmd = con.CreateCommand()) 
                {
                    cmd.CommandText = "SELECT ADMIN FROM OPERATOR WHERE NAME=@userName";
                    cmd.Parameters.AddWithValue("@userName", user);
                    try 
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            return result.ToString();
                        else return "Not Found";
                    }
                    catch (Exception ex)
                    {
                        string msg = "Issue occured while attempting to identify operator status. Contact system admin." + Environment.NewLine + ex.Message ;
                        System.Windows.Forms.MessageBox.Show(msg, "Error 51");
                        return "Failed";
                    }
                }
            }
        }



        // 'CHOOSE YOUR ACTION' DROPDOWN: CHOOSE TO CREATE/EDIT JOB
        protected void actionChange(object sender, EventArgs e)
        {   
            try
            {
                if (selectAction.SelectedValue == "edit")
                {
                    // Fill dropdown list with jobs.
                    selectJobList.Items.Clear();
                    selectJobList.Items.Add("Select");
                    getDropdownJobItems();
                    jobAbb.Visible = false;
                    createJobBtn.Visible = false;
                    deleteJobBtn.Visible = false;
                    selectJobList.Visible = true;
                    editJobBtn.Visible = true;
                    jobNameLabel.Visible = true;
                    jobName.Visible = true;
                    jobName.Attributes["placeholder"] = " Optional";
                    jobActiveLabel.Visible = true;
                    jobActiveBtn.Visible = true;
                }
                else if (selectAction.SelectedValue == "create")
                {
                    jobSectionDefault();
                }
                else
                {
                    editJobBtn.Visible = false;
                    jobAbb.Visible = false;
                    createJobBtn.Visible = false;
                    jobNameLabel.Visible = false;
                    jobName.Visible = false;
                    jobActiveLabel.Visible = false;
                    jobActiveBtn.Visible = false;
                    jobAssignedToLabel.Visible = false;
                    jobAssignedTo.Visible = false;
                    selectJobList.Visible = true;
                    deleteJobBtn.Visible = true;
                    getDropdownJobItems();
                }
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to choose action. Contact system admin. " + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 52");    
            }
        }



        // 'CREATE' CLICKED: CREATE NEW JOB. FUNCTION
        protected void createJob_Click(object sender, EventArgs e)
        {   
            try
            {
                if (!Page.IsValid) return;
                if (this.jobAbb.Text == string.Empty)
                {
                    string msg = "Job abbreviation is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    jobAbb.Text = string.Empty;
                    jobAbb.Focus();
                    return;
                }
                else if (this.jobName.Text == string.Empty)
                {
                    string warning2 = "Job name is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + warning2 + "');", true);
                    jobName.Text = string.Empty;
                    jobName.Focus();
                    return;
                }
                else
                {
                    try
                    {
                        using(SqlConnection con = Helper.ConnectionObj) 
                        {
                            // Save created job
                            using (SqlCommand cmd = con.CreateCommand()) 
                            {
                                cmd.CommandText = "INSERT INTO JOB (ABBREVIATION, NAME, ACTIVE) VALUES(@abbr, @name, @active)";
                                cmd.Parameters.AddWithValue("@abbr", this.jobAbb.Text);
                                cmd.Parameters.AddWithValue("@name", this.jobName.Text);
                                cmd.Parameters.AddWithValue("@active", this.jobActiveBtn.SelectedValue);
                                con.Open();
                                if (cmd.ExecuteNonQuery() == 1)
                                {
                                    // Assign job to assignee
                                    if (jobAssignedTo.Text != string.Empty)
                                    {
                                        string assignee = jobAssignedTo.Text;
                                        string abbr = jobAbb.Text;
                                        bool answer = AssignJob(assignee, abbr); // calling assignJob function
                                        if (answer == true)
                                        {
                                            string msg = "New job successfully saved & assigned!";
                                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                            jobFormClear();
                                        }
                                        else
                                        {
                                            string msg = "Error 53: New job successfully saved, but not assigned! Contact system admin.";
                                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                            jobFormClear();
                                        }
                                    }
                                    else
                                    {
                                        success.Text = "New Job Created!";
                                        success.Visible = true;
                                        ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                        jobFormClear();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        jobFormClear();
                        if (ex.Message.Contains("Violation of UNIQUE KEY"))
                        {
                            string msg = "The Job Abbreviation entered already exists.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }
                        else
                        {
                            string msg = "Issue occured while attempting to save the created job. Contact system admin." + Environment.NewLine + ex.Message;
                            System.Windows.Forms.MessageBox.Show(msg, "Error 54");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to save the created job. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 55");
            }
        }

        
        // 'EDIT' CLICKED: EDIT JOB NAME OR ACTIVE STATUS. FUNCTION
        protected void editJob_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            try
            {
                // Edit job ACTIVE status
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (this.selectJobList.SelectedValue == "Select")
                        {
                            string msg = "Please select a Job.";
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            jobFormClear();
                            return;
                        }
                        if (this.jobName.Text == string.Empty)
                            cmd.CommandText = "UPDATE JOB SET ACTIVE = @active WHERE ABBREVIATION = @abbr";
                        else
                        {
                            cmd.CommandText = "UPDATE JOB SET NAME=@job, ACTIVE=@active WHERE ABBREVIATION=@abbr";
                            cmd.Parameters.AddWithValue("@job", this.jobName.Text);
                        }
                        cmd.Parameters.AddWithValue("@active", this.jobActiveBtn.SelectedValue);
                        cmd.Parameters.AddWithValue("@abbr", this.selectJobList.SelectedValue);
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            // Assign job to assignee
                            if (jobAssignedTo.Visible = true && jobAssignedTo.Text != string.Empty)
                            {
                                string assignee = jobAssignedTo.Text;
                                string abbr = this.selectJobList.SelectedValue;
                                bool answer = AssignJob(assignee, abbr); // calling assignJob function
                                if (answer == true)
                                {
                                    success.Text = "Job successfully updated & assigned!";
                                    success.Visible = true;
                                    ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                    jobFormClear();
                                }
                                else
                                {
                                    string msg = "Error 55a: Job updated successfully, but not assigned! Contact System Admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    jobFormClear();
                                }
                            }
                            else
                            {
                                success.Text = "Job updated successfully!";
                                success.Visible = true;
                                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                jobFormClear();
                            }
                            getDropdownJobItems();
                            getActiveJobs();
                            getUnassignedJobs(null);
                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to update the job ACTIVE status. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 56");
            }
        }
        


        // 'DELETE' CLICKED: DELETE JOB. FUNCTION
        protected void deleteJob_Click(object sender, EventArgs e)
        {
            int jobID = 0;
            SqlConnection con = Helper.ConnectionObj;
            con.Open();
            if (selectJobList.SelectedValue != "Select")
            {
                // First, get ID of specified job.
                SqlCommand cmd = new SqlCommand("SELECT ID FROM JOB WHERE ABBREVIATION = @abb", con);               
                cmd.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                try
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            jobID = (int)reader.GetValue(0);
                        }
                        reader.Close();
                    }
                    else
                    {
                        string msg = "Specified job does not exist, thus cannot be deleted!";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        jobFormClear();
                        jobAssignedToLabel.Visible = false;
                        jobAssignedTo.Visible = false;
                        con.Close();
                        return;
                    }
                }
                catch(Exception ex)
                {
                    string msg = "Error: Something went wrong while attempting to identify this job. Contact your system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    Console.WriteLine(ex.Message);
                }
                

                if(jobID > 0)
                {
                    try 
                    {
                        // TBD: Got to delete the config 1st
                        // Then, delete job record in OPERATOR_ACCESS
                        SqlCommand cmd2 = new SqlCommand("DELETE FROM OPERATOR_ACCESS WHERE JOB_ID = @jobID", con);
                        cmd2.Parameters.AddWithValue("@jobID", jobID);
                        if (cmd2.ExecuteNonQuery() == 1)
                        {
                            // Finally, delete job record in JOB table
                            SqlCommand cmd3 = new SqlCommand("DELETE FROM JOB WHERE ABBREVIATION = @abb", con);
                            cmd3.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                            if (cmd3.ExecuteNonQuery() == 1)
                            {
                                success.Text = "Job successfully deleted!";
                                success.Visible = true;
                                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                            else
                            {
                                string msg = "Error: There was an error in deleting this job in JOB.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                        }
                        else
                        {
                            // If no record in OPERATOR_ACCESS, just delete it in JOB
                            SqlCommand cmd3 = new SqlCommand("DELETE FROM JOB WHERE ABBREVIATION = @abb", con);
                            cmd3.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                            if (cmd3.ExecuteNonQuery() == 1)
                            {
                                success.Text = "Job successfully deleted!";
                                success.Visible = true;
                                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                            else
                            {
                                string msg = "Error: There was an error in deleting this job in JOB.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                        }
                    } catch (Exception ex) {
                        string msg = "Error: Something went wrong while attempting to delete this job. Contact your system admin.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        Console.WriteLine(ex.Message);
                    }
                   
                }
            }
            con.Close();
            getDropdownJobItems();
            jobAssignedToLabel.Visible = false;
            jobAssignedTo.Visible = false;
        }

    

        // SET OPERATOR ADMIN PERMISSIONS. FUNCTION
        protected void setPermissions_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            try
            {
                if (user.Text != null)
                {
                    string msg;
                    // If user exists, set Admin status
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "UPDATE OPERATOR SET ADMIN = @admin WHERE NAME = @user";
                            cmd.Parameters.AddWithValue("@admin", this.permissions.SelectedValue);
                            cmd.Parameters.AddWithValue("@user", this.user.Text);
                            con.Open();
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                msg = "Operator permissions set!";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                permissionsFormClear();
                                return;
                            }

                            // If user doesn't exist, register user and set Admin status.
                            cmd.CommandText = "INSERT INTO OPERATOR (NAME, ADMIN) VALUES(@user,@admin)";
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                msg = "Operator permissions set!";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                permissionsFormClear();
                            }
                        }
                    }
                }
                else
                {
                    string msg = "Operator field is required!";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "')", true);
                    permissionsFormClear();
                }
            }
            catch (Exception ex)
            {   
                string msg  = "Issue occured while attempting to set permissions. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 58");
            }
        }



        // CHECKBOX THAT SETS ALL THE OTHERS. HELPER FUNCTION
        protected void selectAll_changed(object sender, EventArgs e)
        {   
            try
            {
                CheckBox ChkBoxHeader = (CheckBox)jobAccessGridView.HeaderRow.FindControl("selectAll");
                foreach (GridViewRow row in jobAccessGridView.Rows)
                {
                    CheckBox ChkBoxRows = (CheckBox)row.FindControl("cbSelect");
                    if (ChkBoxHeader.Checked == true)
                        ChkBoxRows.Checked = true;
                    else ChkBoxRows.Checked = false;
                }
            }
            catch (Exception ex)
            {   
                string msg  = "Issue occured while processing master CheckBox. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 59");
            }
        }



        // CLEAR JOB FORM. HELPER FUNCTION
        private void jobFormClear()
        {   
            try
            {
                jobAbb.Text = string.Empty;
                selectJobList.SelectedValue = "Select";
                jobName.Text = string.Empty;
                jobActiveBtn.SelectedValue = "1";
                jobAssignedTo.Text = string.Empty;
                jobAssignedToLabel.Visible = true;
                jobAssignedTo.Visible = true;
                jobAbb.Focus();
            }
            catch (Exception ex)
            {   
                string msg  = "Issue occured while attempting to clear fields. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 60");
            }
        }



        // CLEAR PERMISSIONS FORM. HELPER FUNCTION
        private void permissionsFormClear()
        {
            user.Text = string.Empty;
            permissions.SelectedValue = "0";
            user.Focus();
        }



        // HIDE/COLLAPSE JOB SECTION. FUNCTION
        protected void newJobShow_Click(object sender, EventArgs e)
        {
           try
           {
                if (jobSection.Visible == false)
                {
                    jobSection.Visible = true;
                    line.Visible = true;
                    jobSectionDefault();
                }
                else
                {
                    jobSection.Visible = false;
                    if (newUserSection.Visible == false) line.Visible = false;
                }
            }
           catch (Exception ex)
           {
                string msg  = "Issue occured while attempting to hide or collapse panel. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 61");
           }
        }



        // 'PERMISSION SECTION' CLICKED: HIDE/SHOW USER PERMISSION SECTION
        protected void permissionsShow_Click(object sender, EventArgs e)
        {
            if(newUserSection.Visible == false)
            {
                newUserSection.Visible = true;
                line.Visible = true;
            }
            else
            {
                newUserSection.Visible = false;
                if (jobSection.Visible == false) line.Visible = false;
            }          
        }



        // 'JOB ACCESS SECTION' CLICKED: HIDE/SHOW JOB-ACCESS SECTION & PULL InnaccessibleED JOBS
        protected void assignShow_Click(object sender, EventArgs e)
        {   
            try
            {
                if (assignPanel.Visible == false)
                {
                    Page.Validate();
                    assignPanel.Visible = true;
                    assignee.Text = string.Empty;
                    assignee.Focus();
                    // Get all unassigned jobs
                    getUnassignedJobs(null);
                }
                else
                {
                    assignPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to hide or show panel. Contac system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 62");
            }
        }


        
        // 'ACCESSIBLE' CLICKED: GET OP'S ACCESSIBLE JOBS. FUNCTION
        protected void assignedJob_Click(object sender, EventArgs e)
        {
            try
            {
                if (assignee.Text == string.Empty)
                {
                    string msg = "Operator field is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    return;
                }
                // If operator field not empty, get operator ID, then jobs
                int opID = 0;
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @name";
                        cmd.Parameters.AddWithValue("@name", assignee.Text);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) opID = (int)result;
                        else
                        {
                            string msg = "Specified Operator could not be found!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignee.Focus();
                            return;
                        }
                        jobsLabel.Text = "Operator's Currently Accessible Jobs";

                        // Get operator's assigned jobs
                        jobAccessBtn.Visible = false;
                        cmd.Parameters.Clear();
                        cmd.CommandText =   "SELECT ABBREVIATION " +
                                            "FROM JOB " +
                                            "INNER JOIN OPERATOR_ACCESS ON JOB.ID = OPERATOR_ACCESS.JOB_ID " +
                                            "WHERE ACTIVE = 1 AND OPERATOR_ACCESS.OPERATOR_ID = @ID";
                        cmd.Parameters.AddWithValue("@ID", opID);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                jobAccessGridView.DataSource = ds.Tables[0];
                                jobAccessGridView.DataBind();
                                jobAccessGridView.Visible = true;
                                assignee.Focus();
                            }

                            // Handling of whether any JOB was returned from DB
                            if (jobAccessGridView.Rows.Count == 0)
                            {
                                jobAccessGridView.Visible = false;
                                jobAccessBtn.Visible = false;
                                deleteAssignedBtn.Visible = false;
                                jobsLabel.Text = "Operator's Accessible Jobs Not Found";
                                assignee.Focus();
                            }
                            else
                            {
                                jobAccessBtn.Visible = false;
                                deleteAssignedBtn.Visible = true;
                                assignee.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to get operator's accessible jobs. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 65");
            }
        }
        


        // 'ACTIVE' CLICKED: GET ALL ACTIVE JOBS. FUNCTION
        protected void unassignedJob_Click(object sender, EventArgs e)
        {   
            try
            {
                getUnassignedJobs(sender);
                assignee.Focus();
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to retrieve active jobs. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 66");
            }
        }



        // 'DENY' CLICKED: REMOVE OPERATOR ACCESS TO JOBS. FUNCTION
        protected void deleteAssigned_Click(object sender, EventArgs e)
        {   
            try
            {
                int opID = 0, jobID = 0, count = 0;
                if (assignee.Text == string.Empty)
                {
                    string msg = "Operator field is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    return;
                }
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // If operator field not empty, get Operator ID
                        cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @name";
                        cmd.Parameters.AddWithValue("@name", assignee.Text);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) opID = (int)result;
                        else
                        {
                            string msg = "Specified Operator could not be found!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignee.Focus();
                            assignedJob_Click(new object(), new EventArgs());
                            return;
                        }

                        // For each checked job, remove access.
                        foreach (GridViewRow row in jobAccessGridView.Rows)
                        {
                            CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                            if (chxBox.Checked)
                            {
                                // First, get ID of selectd job
                                cmd.Parameters.Clear();
                                cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @abbr";
                                cmd.Parameters.AddWithValue("@abbr", row.Cells[2].Text);
                                object result2 = cmd.ExecuteScalar();
                                if (result2 != null) jobID = (int)result2;
                                else
                                {
                                    string msg = "Error 68: Selected job " + row.Cells[2].Text + " could not be found. Contact system admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    assignee.Focus();
                                    assignedJob_Click(new object(), new EventArgs());
                                    return;
                                }

                                // Then, remove job from OPERATOR_ACCESS
                                cmd.Parameters.Clear();
                                cmd.CommandText = "DELETE FROM OPERATOR_ACCESS WHERE OPERATOR_ACCESS.JOB_ID = @job AND OPERATOR_ACCESS.OPERATOR_ID = @op";
                                cmd.Parameters.AddWithValue("@job", jobID);
                                cmd.Parameters.AddWithValue("@op", opID);
                                if (cmd.ExecuteNonQuery() == 1) count++;
                                else
                                {
                                    string msg = "Error 70: Something went wrong while removing job: " + row.Cells[2].Text + ". Contact system admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    assignedJob_Click(new object(), new EventArgs());
                                    assignee.Focus();
                                }
                            }
                        }

                        // Handling whether or not any job access was denied
                        if (count == 0)
                        {
                            string msg = "Please select at least one job.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignedJob_Click(new object(), new EventArgs());
                        }
                        else
                        {
                            string msg = count + " Job(s) access denied!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignee.Focus();
                            assignedJob_Click(new object(), new EventArgs());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to deny operator's job accesses. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 71");
            }
        }



        // 'GRANT' CLICKED: ASSIGN OPERATORS JOB-ACCESSES. FUNCTION
        protected void jobAccess_Click(object sender, EventArgs e)
        {   
            try
            {
                string assigneeName = assignee.Text;
                int countGranted = 0, countChecked = 0, countError = 0;
                if (assigneeName == string.Empty)
                {
                    string msg = "Operator field is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    return;
                }

                if (jobAccessGridView.Rows.Count > 0)
                {
                    // For each checked job, assign job to assignee
                    foreach (GridViewRow row in jobAccessGridView.Rows)
                    {
                        CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                        if (chxBox.Checked)
                        {
                            countChecked++;
                            string abbr = row.Cells[2].Text;
                            bool answer = AssignJob(assigneeName, abbr); // calling assignJob function
                            if (answer == true) countGranted++;
                            else
                            {
                                chxBox.Checked = false;
                                countError++;
                            }
                        }
                    }

                    // Handling whether any job access was granted.
                    if (countChecked == 0)
                    {
                        string msg = "Please make sure that at least 1 job is selected.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        return;
                    }
                    else
                    {   
                        if (countGranted > 0 && countGranted == countChecked)
                        {
                            string msg = countGranted + " job(s) granted.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            getUnassignedJobs(sender);
                            assignee.Focus();
                            return;
                        }
                        else 
                        {
                            string msg = countGranted + " Job(s) access granted. Operator already has access to the other " +countError+ " Job(s)";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            getUnassignedJobs(sender);
                            assignee.Focus();
                            return;
                        }
                    }
                }
                else
                {
                    string msg = "There are no jobs to be granted.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to grant jobs accesses to operator. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 72");
            }
        }



        // 'JOB INDEX CONFIGURATION SECTION' CLICKED: SHOW & INDEX FORM CONTROLS. FUNCTION
        protected void jobIndexEditingShow_Click(object sender, EventArgs e)
        {   
            try
            {
                // Fill Job Abb. dropdown list with jobs.
                selectJob.Items.Clear();
                selectJob.Items.Add("Select");
                if (jobIndexEditingPanel.Visible == false)
                {
                    jobIndexEditingPanel.Visible = true;
                    getActiveJobs();
                    getLabelRow();
                    //labelDropdown_Click(new object(), new EventArgs());
                }
                else
                {
                    jobIndexEditingPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to show or hide section. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 73");
            }
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS. FUNCTION
        protected void onJobSelect(object sender, EventArgs e)
        {
            //setDropdownColor();
        }



        // 'ACTIVE' SELECTED: SET 'ASSIGN TO' VISIBLE OR NOT
        protected void onActiveSelect(object sender, EventArgs e)
        {
            if(jobActiveBtn.Visible && jobActiveBtn.SelectedValue == "1")
            {
                jobAssignedToLabel.Visible = true;
                jobAssignedTo.Visible = true;
            }
            else
            {
                jobAssignedToLabel.Visible = false;
                jobAssignedTo.Visible = false;
            }
        }



        // 'ADD LABEL' CLICKED.
        protected void addLabel_Click(object sender, EventArgs e)
        {
            int labelRowCount = (int)ViewState["labelRowCount"];
            int rowCount = labelsTable.Rows.Count;
            // Hide 'add label' button if 5 labels are up.
            if (labelRowCount == 4) addLabel.Visible = false;
            if (labelRowCount == 6)
            {
                ViewState["labelRowCount"] = rowCount;
                return;
            }


            // Before adding a label row, check how many we have so far
            if (rowCount < 5 && ViewState["action"] == null) getLabelRow();
            //bool foundDeleted = false;
            //bool stop = false;
            //int i = 2;
            //while (foundDeleted == false && stop == false)
            //{
            //    var sampleLabel = (List<string>)ViewState["labelRow" + i.ToString()];
            //    if (sampleLabel[5] == "off")
            //    {
            //        sampleLabel[5] = "on";
            //        foundDeleted = true;
            //        ViewState["labelRow" + i.ToString()] = sampleLabel;
            //    }
            //    i++;
            //    if (i == 6) stop = true;
            //}

            // Check whether it's an 'edit' or 'delete' icon action
           
        }


        // PROCESS LABEL ROW IDS
        private void getLabelRow()
        {
            TableRow row = new TableRow();
            row.ID = (labelsTable.Rows.Count + 1).ToString();
            ViewState["labelRowCount"] = Convert.ToInt32(row.ID);

            Label label = new Label();
            label.Height = 25;
            label.Text = "LABEL: ";

            TextBox textBox = new TextBox();
            textBox.ReadOnly = true;
            textBox.Attributes["placeholder"] = " Required only for Set";
            textBox.Width = 195;
            //var currentLabelValues = (List<string>)ViewState["labelValues" + row.ID];
            //var labelName = currentLabelValues[1];
            //if (labelName != string.Empty) textBox.Text = labelName;
            //else textBox.Text = " Row ID: " + row.ID;
            textBox.Style.Add("margin-left", "40px");

            ImageButton editIcon = new ImageButton();
            editIcon.ImageUrl = "Content/edit.png";
            editIcon.Height = 16;
            editIcon.Width = 16;
            editIcon.Style.Add("margin-top", "5px");
            editIcon.Click += new ImageClickEventHandler(processRequest);

            ImageButton deleteIcon = new ImageButton();
            deleteIcon.ImageUrl = "Content/delete.png";
            deleteIcon.Height = 18;
            deleteIcon.Width = 18;
            deleteIcon.Style.Add("margin-top", "5px");
            deleteIcon.Click += new ImageClickEventHandler(processRequest);

            label.ID = "label" + row.ID;
            textBox.ID = "textBox" + row.ID;
            editIcon.ID = "editIcon" + row.ID;
            deleteIcon.ID = "deleteIcon" + row.ID;

            List<Control> controlList = new List<Control>();
            controlList.Add(label);
            controlList.Add(textBox);
            controlList.Add(editIcon);
            controlList.Add(deleteIcon);

            var labelRow = new List<string> { label.ID, textBox.ID, editIcon.ID, deleteIcon.ID, "on" };
            ViewState["labelRow" + row.ID] = labelRow;
            
            foreach (Control c in controlList)
            {
                TableCell cell = new TableCell();
                cell.Height = 35;
                cell.Style.Add("padding-right", "2px");
                cell.VerticalAlign = VerticalAlign.Middle;
                cell.Controls.Add(c);
                row.Cells.Add(cell);
            }
            labelsTable.Rows.AddAt(Convert.ToInt32(row.ID) - 1, row);
            labelsTable.Visible = true;
        }

        // 'SUBMIT' ICON CLICKED: ADD LABEL VALUES
        protected void labelValues_Click(object sender, EventArgs e)
        {
            try
            {
                // Make sure label field is not empty  
                if (labelTextBox.Text == string.Empty)
                {
                    string msg = "LABEL field is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    labelTextBox.Text = string.Empty;
                    labelTextBox.Attributes["placeholder"] = " Required for Set";
                    labelTextBox.Focus();
                    return;
                }

                // Make sure that regex & message fields are both either filled or blank
                if ((regexTextBox.Text == string.Empty && msgTextBox.Text != string.Empty) || (regexTextBox.Text != string.Empty && msgTextBox.Text == string.Empty))
                {
                    string msg = "Both REGEX and MESSAGE fields must be either filled or empty.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    labelTextBox.Text = string.Empty;
                    labelTextBox.Attributes["placeholder"] = " Required for Set";
                    if (regexTextBox.Text == string.Empty) regexTextBox.Focus();
                    else if (msgTextBox.Text == string.Empty) msgTextBox.Focus();
                    return;
                }

                // Store entered values in a viewState
                var id = (string)ViewState["id"];
                var labelValues = new List<string> {id, labelTextBox.Text, regexTextBox.Text, msgTextBox.Text };
                ViewState["labelValues" + id] = labelValues;
                getLabelRow();

                // Clear fields
                labelTextBox.Text = string.Empty;
                regexTextBox.Text = string.Empty;
                msgTextBox.Text = string.Empty;
                labelControlsTable.Visible = false;
            }
            catch (Exception ex)
            {
                string msg = "Error 74: Issue occured while attempting to add label controls. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + ex.Message + "');", true);
            }
        }



        // 'EDIT or DELETE' ICON CLICKED: EDIT or DELETE LABEL CONTROLS
        protected void processRequest(object sender, EventArgs e)
        {
            Control button = (Control)sender;
            string lastChar = button.ID.Substring(button.ID.Length - 1);
            int index = Convert.ToInt32(lastChar);

            // If edit icon clicked
            if (button.ID.Contains("edit"))
            {
                // Show label values table and its current content
                ViewState["action"] = "edit";
                labelControlsTable.Visible = true;
                var labelValues = (List<string>)ViewState["labelValues" + lastChar];
                ViewState["id"] = lastChar;
                labelTextBox.Text = labelValues[1];
                regexTextBox.Text = labelValues[2];
                msgTextBox.Text = labelValues[3];
                labelsTable.Rows.RemoveAt(index - 1);
            }
            // If delete icon clicked
            else
            {
                ViewState["labelRow" + lastChar] = null;
                ViewState["action"] = null;
                addLabel.Visible = true;
                for (int i=0; i<labelsTable.Rows.Count; i++)
                {
                    if (labelsTable.Rows[i].ID == lastChar) labelsTable.Rows.Remove(labelsTable.Rows[i]);
                }
                ViewState["labelRowCount"] = labelsTable.Rows.Count;
                for (int i = 1; i <= labelsTable.Rows.Count; i++) getLabelRow();
                return;
            }
        }



        // 'SET' CLICKED: SET INDEX FORM RULES. FUNCTION
        protected void setRules_Click(object sender, EventArgs e)
        {
            try
            {
                int jobID = 0;

                // Make sure a job is selected & LABEL1 is filled.
                if (this.selectJob.SelectedValue == "Select")
                {
                    string msg = "Please select a specific job to Set!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    jobAbb.Text = string.Empty;
                    jobAbb.Focus();
                    return;
                }
                else if (this.l1.Text == string.Empty)
                {
                    string msg = "LABEL1 is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    jobAbb.Text = string.Empty;
                    jobAbb.Focus();
                    return;
                }

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand()) 
                    {
                        // First, get job ID of selected job
                        cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @jobAbb";
                        cmd.Parameters.AddWithValue("@jobAbb", selectJob.SelectedValue);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) jobID = (int)result;
                        else
                        {
                            string msg = "Job selected could not be found.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            selectJob.SelectedValue = "Select";
                            return;
                        }

                        // Then, use that job ID to set job rules into JOB_CONFIG_INDEX
                        cmd.Parameters.Clear();
                        cmd.CommandText =   "INSERT INTO JOB_CONFIG_INDEX" +
                                            "(JOB_ID, LABEL1, REGEX1, LABEL2, REGEX2, LABEL3, REGEX3, LABEL4, REGEX4, LABEL5, REGEX5) " +
                                            "VALUES(@jobID, @label1, @regex1, @label2, @regex2, @label3, @regex3, @label4, @regex4, @label5, @regex5)";
                        cmd.Parameters.AddWithValue("@jobID", jobID);
                        cmd.Parameters.AddWithValue("@label1", l1.Text);
                        if (regex1.Text == string.Empty) cmd.Parameters.AddWithValue("@regex1", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@regex1", regex1.Text);
                        if (l2.Text == string.Empty) cmd.Parameters.AddWithValue("@label2", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@label2", l2.Text);
                        if (regex2.Text == string.Empty) cmd.Parameters.AddWithValue("@regex2", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@regex2", regex2.Text);
                        if (l3.Text == string.Empty) cmd.Parameters.AddWithValue("@label3", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@label3", l3.Text);
                        if (regex3.Text == string.Empty) cmd.Parameters.AddWithValue("@regex3", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@regex3", regex3.Text);
                        if (l4.Text == string.Empty) cmd.Parameters.AddWithValue("@label4", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@label4", l4.Text);
                        if (regex4.Text == string.Empty) cmd.Parameters.AddWithValue("@regex4", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@regex4", regex4.Text);
                        if (l5.Text == string.Empty) cmd.Parameters.AddWithValue("@label5", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@label5", l5.Text);
                        if (regex5.Text == string.Empty) cmd.Parameters.AddWithValue("@regex5", DBNull.Value);
                        else cmd.Parameters.AddWithValue("@regex5", regex5.Text);

                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            string msg = selectJob.SelectedValue + " Job config successfully set.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            setDropdownColor();
                            clearRules();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clearRules();
                string msg;
                if (ex.Message.Contains("Violation of PRIMARY KEY"))
                {
                    msg = "The job selected has already been configured. If you want to reconfigure, please Unset then Set again!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }  
                else
                {
                    msg = "Issue occured while attempting to configure selected job" + Environment.NewLine + ex.Message + ex.InnerException;
                    System.Windows.Forms.MessageBox.Show(msg, "Error 75");
                }
                   
            }
        }



        // 'UNSET' CLICKED: UNSET INPUT-CONTROLS RULES. FUNCTION.
        protected void unsetRules_Click(object sender, EventArgs e)
        {
            try
            {
                int jobID = 0;

                // Make sure a job is selected
                if (this.selectJob.SelectedValue == "Select")
                {
                    string msg = "Please select a specific job to Unset!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    jobAbb.Text = string.Empty;
                    jobAbb.Focus();
                    return;
                }
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // First, get job ID of selected job
                        cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @jobAbb";
                        cmd.Parameters.AddWithValue("@jobAbb", selectJob.SelectedValue);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) jobID = (int)result;
                        else
                        {
                            string msg = "Job selected could not be found.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            selectJob.SelectedValue = "Select";
                            return;
                        }

                        // Then, use that job ID to unset job rules into JOB_CONFIG_INDEX
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM JOB_CONFIG_INDEX WHERE JOB_ID=@jobID";
                        cmd.Parameters.AddWithValue("@jobID", jobID);
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            string msg = selectJob.SelectedValue + " Job config successfully unset.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            setDropdownColor();
                            clearRules();
                            return;
                        }
                        else
                        {
                            string msg = "Error 77: Something went wrong while attempting to unset. Please contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            clearRules();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clearRules();
                string msg = "Configuration rules for the selected job has already been unset. If you want to reconfigure, Make sure it is selected, then Set!" + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 78");
            }
        }


        // COLLAPSE ALL SECTIONS. 
        protected void collapseAll_Click(object sender, EventArgs e)
        {   
            try
            {
                if (collapseIcon.ImageUrl == "Content/collapse_all.png")
                {
                    collapseIcon.ImageUrl = "Content/hide_all.png";
                    jobSection.Visible = true;
                    newUserSection.Visible = true;
                    getUnassignedJobs(null);
                    assignPanel.Visible = true;
                    jobIndexEditingPanel.Visible = true;
                    getActiveJobs();
                    jobSectionDefault();
                    line.Visible = true;
                    //labelDropdown_Click(new object(), new EventArgs());
                }
                else
                {
                    collapseIcon.ImageUrl = "Content/collapse_all.png";
                    jobSection.Visible = false;
                    newUserSection.Visible = false;
                    assignPanel.Visible = false;
                    jobIndexEditingPanel.Visible = false;
                    line.Visible = false;
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to hile or collapse all sections. Contacy system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 79");
            }
        }



        // GET ALL ACTIVE JOBS OR OPERATOR'S INACCESSIBLE JOBS. HELPER FUNCTION
        private void getUnassignedJobs(Object sender)
        {
            try
            {
                string assigneeName = string.Empty;
                int opID = 0;
                SqlCommand cmd = null;
                Button button = (Button)sender;
                string buttonId = string.Empty;
                if (button != null) buttonId = button.ID;

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (cmd = con.CreateCommand())
                    {
                        // If 'INACCESSIBLE' clicked
                        if (button != null && (buttonId == "inaccessibleBtn" || buttonId == "jobAccessBtn"))
                        {
                            // Make sure Operator is entered
                            if (assignee.Text != string.Empty) assigneeName = assignee.Text;
                            else
                            {
                                string msg = "Operator field required.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }

                            // Then check if specified operator exists. If so, get ID.
                            cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @assignedTo";
                            cmd.Parameters.AddWithValue("@assignedTo", assigneeName);
                            con.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null) opID = (int)result;
                            else
                            {
                                string msg = "Operator entered could not be found.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }

                            // Then, set cmd to retrieve operator's inaccessible jobs
                            cmd.Parameters.Clear();
                            cmd.CommandText = "SELECT ABBREVIATION " +
                                                "FROM JOB " +
                                                "WHERE ACTIVE = 1 AND ID NOT IN (SELECT JOB_ID FROM OPERATOR_ACCESS WHERE OPERATOR_ID=@opId)";
                            cmd.Parameters.AddWithValue("@opId", opID);
                        }
                        else
                        {
                            // If 'INACCESSIBLE' not clicked, set cmd to retrieve all Active jobs.
                            cmd.Parameters.Clear();
                            cmd.CommandText = "SELECT ABBREVIATION " +
                                                "FROM JOB WHERE ACTIVE=1";
                                                // "LEFT JOIN OPERATOR_ACCESS ON JOB.ID = OPERATOR_ACCESS.JOB_ID " +   //In case we want jobs inaccessible by everyone
                                                // "WHERE JOB.ACTIVE = 1 AND OPERATOR_ACCESS.JOB_ID IS NULL", con);"
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                jobAccessGridView.DataSource = ds.Tables[0];
                                jobAccessGridView.DataBind();
                                jobAccessGridView.Visible = true;
                            }

                            // Handling of whether any JOB was returned from DB
                            if (jobAccessGridView.Rows.Count == 0)
                            {
                                jobAccessGridView.Visible = false;
                                jobAccessBtn.Visible = false;
                                deleteAssignedBtn.Visible = false;
                                if (buttonId == "inaccessibleBtn" || buttonId == "jobAccessBtn") jobsLabel.Text = "Operator's Inaccessible Jobs Not Found";
                                else jobsLabel.Text = "No Active Jobs Found";
                                jobsLabel.Visible = true;
                            }
                            else
                            {
                                if (buttonId == "inaccessibleBtn" || buttonId == "jobAccessBtn") jobsLabel.Text = "Operator's Currently Inaccessible Jobs";
                                else jobsLabel.Text = "Active Jobs";
                                jobsLabel.Visible = true;
                                jobAccessBtn.Visible = true;
                                deleteAssignedBtn.Visible = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to retrieve jobs. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 81");
            }
        }



        // Get dropdown list job items
        private void getDropdownJobItems()
        {
            try 
            {
                selectJobList.Items.Clear();
                selectJobList.Items.Add("Select");

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ABBREVIATION, ACTIVE FROM JOB";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string jobAbb = (string)reader.GetValue(0);
                                    bool active = (bool)reader.GetValue(1);
                                    selectJobList.Items.Add(jobAbb);
                                    if (active)
                                    {
                                        // Red config job items from 'JOB' section
                                        foreach (ListItem item in selectJobList.Items)
                                        {
                                            if (item.Value == jobAbb)
                                            {
                                                item.Attributes.Add("style", "color:Red;");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string msg = "No Jobs could be found.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                string msg = "Issue occured while attempting to retrieve jobs. Contact system admin. " + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 84");
            }
        }



        // Get dropdown list ACTIVE job items
        private void getActiveJobs()
        {
            try 
            {
                selectJob.Items.Clear();
                selectJob.Items.Add("Select");
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {   
                        // Get all active jobs
                        cmd.CommandText = "SELECT ABBREVIATION FROM JOB WHERE ACTIVE = 1";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string jobAbb = (string)reader.GetValue(0);
                                    selectJob.Items.Add(jobAbb);
                                }
                            }
                            else
                            {
                                string msg = "No Active jobs could be found";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }
                        }
                    }
                }
                setDropdownColor();
            }
            catch(Exception ex) 
            {
                string msg = "Issue occured while attempting to retrieve active jobs. Contact system admin. " + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 86");
            }
        }



        // ASSIGN JOB TO OPERATOR. HELPER FUNCTION
        private bool AssignJob(string assignee, string abbr)
        {
            try
            {
                int opID = 0, jobID = 0;

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // First check if specified operator exists. If so, get operator ID.
                        cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @assignedTo";
                        cmd.Parameters.AddWithValue("@assignedTo", assignee);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) opID = (int)result;
                        else
                        {
                            string msg = "Specified operator could not be found!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return false;
                        }

                        // Then, get ID of job to be assigned.
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @abbr";
                        cmd.Parameters.AddWithValue("@abbr", abbr);
                        try {
                            object result2 = cmd.ExecuteScalar();
                            if (result != null) jobID = (int)result2;
                        }
                        catch (SqlException ex) {
                            string msg = "Issue occured while attempting to retrieve job ID. Contact system admin. " + Environment.NewLine + ex.Message;
                            System.Windows.Forms.MessageBox.Show(msg, "Error 88");
                        }

                        // Now, Save operator ID & new job ID in OPERATOR_ACCESS
                        if (opID > 0 && jobID > 0)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO OPERATOR_ACCESS (OPERATOR_ID, JOB_ID) VALUES(@opId, @jobID)";
                            cmd.Parameters.AddWithValue("@opID", opID);
                            cmd.Parameters.AddWithValue("@jobID", jobID);
                            if (cmd.ExecuteNonQuery() == 1) return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg;
                // Skip jobs that have already been made accessible to specified operator.
                if (!ex.Message.Contains("Violation of PRIMARY KEY"))
                {
                    msg = "Issue occured while attempting to grant job accessiblity to specified operator. Contact system admin. " + Environment.NewLine + ex.Message;
                    System.Windows.Forms.MessageBox.Show(msg, "Error 89");
                }
                return false;
            }
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS. HELPER FUNCTION
        private void setDropdownColor()
        {   
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText =   "SELECT ABBREVIATION " +
                                            "FROM JOB " +
                                            "INNER JOIN JOB_CONFIG_INDEX ON JOB.ID = JOB_CONFIG_INDEX.JOB_ID " +
                                            "WHERE JOB.ACTIVE = 1";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    // Red config job items from 'JOB INDEX CONFIG' section
                                    foreach (ListItem item in selectJob.Items)
                                    {
                                        if (item.Value == (string)reader.GetValue(0))
                                        {
                                            item.Attributes.Add("style", "color:Red;");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to color appropriate jobs. Contact system admin. " + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 90");
            }
        }



        // HANDLE NEXT PAGE CLICK. FUNCTION
        protected void pageChange_Click(object sender, GridViewPageEventArgs e)
        {   
            try
            {
                jobAccessGridView.PageIndex = e.NewPageIndex;
                if (jobsLabel.Text == "Operator's Currently Accessible Jobs")
                {
                    assignedJob_Click(new object(), new EventArgs());
                }
                else
                {
                    getUnassignedJobs(null);
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to operator's ID. Contact system admin. " + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 91");
            }
        }



        // CLEAR RULES. HELPER FUNCTION
        private void clearRules()
        {
            selectJob.SelectedValue = "Select";
            l1.Text = string.Empty;
            l1.Focus();
            regex1.Text = string.Empty;
            l2.Text = string.Empty;
            regex2.Text = string.Empty;
            l3.Text = string.Empty;
            regex3.Text = string.Empty;
            l4.Text = string.Empty;
            regex4.Text = string.Empty;
            l5.Text = string.Empty;
            regex5.Text = string.Empty;
        }


        // JOB SECTION DEFAULTS. HELPER FUNCTION
        private void jobSectionDefault()
        {
            selectAction.SelectedValue = "create";
            selectJobList.Visible = false;
            deleteJobBtn.Visible = false;
            editJobBtn.Visible = false;

            jobAbb.Visible = true;
            createJobBtn.Visible = true;
            jobNameLabel.Visible = true;
            jobName.Visible = true;
            jobName.Attributes["placeholder"] = " Required";
            jobActiveLabel.Visible = true;
            jobActiveBtn.Visible = true;
            jobActiveBtn.SelectedValue = "1";
            jobAssignedToLabel.Visible = true;
            jobAssignedTo.Visible = true;
        }
    }
}