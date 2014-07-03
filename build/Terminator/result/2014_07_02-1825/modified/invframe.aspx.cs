using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class inventory_invframe : System.Web.UI.Page
{
  protected string url = "invmain.aspx";
  protected string framemain = "70%";
  protected string framecontrolpanel = "30%";

  protected void Page_Load(object sender, EventArgs e)
  {
    string mode = "";
    string itemnum = "";
    if (Request.QueryString["URL"] != null)
    {
      url = Request.QueryString["URL"].ToString();
    }
    if (Request.QueryString["mode"] != null)
    {
      mode = Request.QueryString["mode"].ToString();
    }
    if (Request.QueryString["itemnum"] != null)
    {
      itemnum = Request.QueryString["itemnum"].ToString();
    }
    url = url + "?mode=" + mode + "&itemnum=" + itemnum;

    if (Session["INVFrameMain"] != null)
    {
      framemain = Session["INVFrameMain"].ToString() + "%";
    }
    if (Session["INVFrameControlPanel"] != null)
    {
      framecontrolpanel = Session["INVFrameControlPanel"].ToString() + "%";
    }
  }
}