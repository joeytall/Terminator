using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;


public partial class inventory_SetPrice : System.Web.UI.Page
{
    protected NameValueCollection m_msg = new NameValueCollection();
    protected NameValueCollection nvc;
    protected AzzierScreen screen;
    protected string m_counter = "";
    protected ModuleoObject objInventory;

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');
        Session.LCID = Convert.ToInt32(Session["LCID"]);

        
        if (Request.QueryString["counter"] != null)
        {
            m_counter = Request.QueryString["counter"].ToString();

            objInventory = new ModuleoObject(Session["login"].ToString(), "InvMain", "Counter", m_counter);
            nvc = objInventory.ModuleData;
            if (nvc["lastprice"] == "")
                nvc["lastprice"] = "0";
            if (nvc["avgprice"] == "")
                nvc["avgprice"] = "0";
        }

        screen = new AzzierScreen("inventory/setprice.aspx", "MainForm", MainControlsPanel.Controls, "edit", 1);
        screen.LCID = Session.LCID;
        screen.LoadScreen();
        screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        TextBox t;
        if (!Page.IsPostBack)
        {
            //screen.PopulateScreen("InvMain", nvc);
            t = MainControlsPanel.FindControl("txtoldlastprice") as TextBox;
            t.Text = nvc["lastprice"];
            t = MainControlsPanel.FindControl("txtoldavgprice") as TextBox;
            t.Text = nvc["avgprice"];
        }
    }
    

    private void RetrieveMessage()
    {
        // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);

    }
}