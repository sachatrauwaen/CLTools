using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System.ServiceModel;
using System.ServiceModel.Channels;
using DotNetNuke.Common;

namespace Satrabel.DNN.CL
{


    public partial class CLTools : PortalModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CLControl1.Visible = false;
            if ((Request.IsAuthenticated))
            {
                bool isAdmin = UserInfo.IsInRole(PortalSettings.AdministratorRoleName);
                if (isAdmin )
                {
                    CLControl1.Visible = true;
                }
            }
            
            var JSPath = HttpContext.Current.IsDebuggingEnabled
                       ? "~/DesktopModules/CLTools/js/CLTools.js"
                       : "~/DesktopModules/CLTools/js/CLTools.js";
            ClientResourceManager.RegisterScript(Page, JSPath);
            //ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/CLTools/module.css", FileOrder.Css.ModuleCss);
            //pAll.CssClass = "";
            if (!Page.IsPostBack)
            {
                TabController tc = new TabController();
                string CultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
                var Pages = TabController.GetTabsBySortOrder(PortalSettings.PortalId, CultureCode, true).Where(t => !t.IsDeleted && t.ParentId != PortalSettings.AdminTabId && t.TabID != PortalSettings.AdminTabId );
                ddlPages.DataSource = Pages;
                ddlPages.DataBind();
                if (ddlPages.Items.Count > 0)
                {
                    //ddlPages_SelectedIndexChanged(null, null);
                    if (Request.QueryString["SelectedTabId"] != null) { 
                        string SelectedTabId = Request.QueryString["SelectedTabId"];
                        if (ddlPages.Items.FindByValue(SelectedTabId) != null)
                        {
                            ddlPages.SelectedValue = SelectedTabId;

                        }
                    }
                    CLControl1.BindAll(int.Parse(ddlPages.SelectedValue));
                }
                else
                {
                    CLControl1.Visible = false;
                }
            }
            
        }
        protected void ddlPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CLControl1.BindAll(int.Parse(ddlPages.SelectedValue));
            Response.Redirect(Globals.NavigateURL(TabId, "", new string[] { "SelectedTabId=" + ddlPages.SelectedValue }), true);
        }

    }

}
