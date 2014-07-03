using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class FailurecodeMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected int screenwidth;
    protected string m_location;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;
    protected Location objloc;
    protected NameValueCollection m_msg = new NameValueCollection();
    protected string m_address = "";

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');top.document.location.href='../login.aspx';</script></html>");
            Response.End();
        }
        

        if (Request.QueryString["location"] != null)
        {
          m_location = Request.QueryString["location"].ToString();
          objloc = new Location(Session["Login"].ToString(), "Location", "Location", m_location);
          m_address = objloc.ModuleData["Address1"] + "," + objloc.ModuleData["Address2"];
        }
        else
        {
          objloc = new Location(Session["Login"].ToString(), "Location", "Location");
        }
        

        screen = new AzzierScreen("codes/map.aspx", "MainForm", MainControlsPanel.Controls);

        Session.LCID = Convert.ToInt32(Session["LCID"]);
        screen.LCID = Session.LCID;
        screen.LoadScreen();
        screen.SetValidationControls();
        
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
           screen.PopulateScreen("location", objloc.ModuleData);
        }
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/failurecodemain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}