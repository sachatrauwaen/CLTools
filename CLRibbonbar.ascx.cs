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
using DotNetNuke.UI.Skins;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;


namespace Satrabel.DNN.CL
{

    public partial class CLRibbonbar : SkinObjectBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            pAll.Visible = false;
            if ((Request.IsAuthenticated))
            {
                UserInfo user = UserController.GetCurrentUserInfo();
                if (((user != null)))
                {
                    bool isAdmin = user.IsInRole(PortalSettings.AdministratorRoleName);
                    if (isAdmin &&
                        !PortalSettings.ActiveTab.IsSuperTab &&
                        PortalSettings.ActiveTab.TabID != PortalSettings.AdminTabId &&
                        PortalSettings.ActiveTab.ParentId != PortalSettings.AdminTabId)
                    {
                        pAll.Visible = true;
                    }
                }
            }
            //if (IsPageAdmin())

            if (pAll.Visible)
            {

                var JSPath = HttpContext.Current.IsDebuggingEnabled
                           ? "~/DesktopModules/CLTools/js/CLTools.js"
                           : "~/DesktopModules/CLTools/js/CLTools.js";

                ClientResourceManager.RegisterScript(Page, JSPath);

                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/CLTools/module.css", FileOrder.Css.ModuleCss);
                if (!Page.IsPostBack)
                {

                }
                CLControl1.BindAll(PortalSettings.ActiveTab.TabID);
            }
        }
    }
}