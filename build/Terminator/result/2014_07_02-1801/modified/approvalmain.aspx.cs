using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.OleDb;

using Telerik.Web.UI;

public partial class ApprovalMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected string counters;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;
    protected string tablename = "approve";
    protected string referer = "";
    protected RadComboBox cbbapprovecode;

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');

        if (Request.QueryString["referer"] != null)
            referer = Request.QueryString["referer"].ToString();

        if (Request.QueryString["counter"] != null)
            counters = Request.QueryString["counter"];
        else
            counters = "";

        if (counters == "")
        {
            btnDelete.Visible = false;
            mode = "new";
        }
        else
        {
            btnDelete.Visible = true;
            mode = "edit";
        }

        btnSave.Visible = true;
        screen = new AzzierScreen("codes/approvalmain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

        Session.LCID = Convert.ToInt32(Session["LCID"]);
        screen.LCID = Session.LCID;
        screen.LoadScreen();
        screen.SetValidationControls();
        NameValueCollection nvc = new NameValueCollection();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            cbbapprovecode = (RadComboBox)MainControlsPanel.FindControl("cbbapprovecode");

            NameValueCollection nvc = new NameValueCollection();

            if (counters != "")
            {
                ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "approve", "counter", counters);
                nvc = obj.ModuleData;

                GetPendingCode(cbbapprovecode, nvc["approvecode"]);
            }
            else
            {
                GetPendingCode(cbbapprovecode, "");
            }

            if (mode == "edit")
            {
                cbbapprovecode.Enabled = false;
            }
            screen.PopulateScreen("approve", nvc);
        }
    }

    protected void Save(object sender, EventArgs e)
    {
        NameValueCollection nvc;
        Panel CntlPanel = Page.FindControl("MainControlsPanel") as Panel;
        TextBox tbx = null;
        string dirtylog = "0";
        if (CntlPanel != null)
        {
            nvc = screen.CollectFormValues("approve", false);

            tbx = CntlPanel.FindControl("txtdirtylog") as TextBox;
            if (nvc["dirtylog"] == null)
            {
                if (tbx != null)
                {
                    dirtylog = tbx.Text;
                    nvc.Add("dirtylog", dirtylog);
                }
            }
            else
                dirtylog = nvc["dirtylog"];
        }
        else
            nvc = null;

        cbbapprovecode = (RadComboBox)MainControlsPanel.FindControl("cbbapprovecode");
        string appcode = cbbapprovecode.SelectedItem.Text;
        if (!string.IsNullOrEmpty(appcode))
        {
            nvc.Remove("approvecode");
            nvc.Add("approvecode", appcode);
        }

        ModuleoObject obj;

        bool success = false;
        if (counters == "")
        {
            if (referer == "WO")
            {
                nvc["module"] = "WORKORDER";
            }
            else if (referer == "PO")
            {
                nvc["module"] = "PURCHASE";
            }
            else if (referer == "PROJ")
            {
                nvc["module"] = "PROJECT";
            }

            nvc["onepersonapprove"] = "0";

            obj = new ModuleoObject(Session["Login"].ToString(), "approve", "counter");
            success = obj.Create(nvc);
        }
        else
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "approve", "counter", counters);
            success = obj.Update(nvc);
        }

        if (success)
        {
            litScript1.Text = "setTimeout(\"CloseAndRebind()\",100)";
        }
        else
        {
            litScript1.Text = "alert('" + m_msg["T2"] + "')";
        }
    }

    protected void Delete(object sender, EventArgs e)
    {
        bool success = false;
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "approve", "counter", counters);
        success = obj.Delete();
        if (success)
        {
            litScript1.Text = "setTimeout(\"CloseAndRebind()\",100)";
        }
        else
        {
            litScript1.Text = "alert('" + m_msg["T3"] + "')";
        }
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/chgstatusmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }

    protected void GetPendingCode(RadComboBox combox,string pendingStr)
    {
        RadComboBoxItem item;

        OleDbConnection conn = new OleDbConnection();
        string connectionStr = Application["ConnString"].ToString();
        conn.ConnectionString = connectionStr;
        if (conn.State != ConnectionState.Open)
            conn.Open();
        string sql = "select tcode,tdesc from codes where tfield='wostatus' and tcode1=100";
        OleDbCommand cmd = new OleDbCommand(sql, conn);
        OleDbDataReader dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            //if (dr[1].ToString() != "")
            if (dr[0].ToString() != "")
            {
                item = new RadComboBoxItem();
                item.Text = dr.GetString(0).ToString();
                item.Value = dr.GetString(0).ToString();
                if (pendingStr == dr.GetString(0))
                {
                    item.Selected = true;
                }
                combox.Items.Add(item);
            }
        }
        dr.Close();
        conn.Close();
    }
}