using System;
using System.Collections;
using System.Collections.Generic;
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Modules;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.Html;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Messaging.Data;
using DotNetNuke.Services.Messaging;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;

namespace Satrabel.DNN.CL
{

    public partial class CLControl : UserControl
    {
      
        
        protected DnnPages Data { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            lbTranslate.Attributes.Add("onclick", "return confirm('Do you want translate all texts (overwrite existing texts) ?');");
            hlConfigTranslate.NavigateUrl = "http://dnncltools.codeplex.com/wikipage?title=Enable%20Auto-Translate";

            if (!Page.IsPostBack){

                LocaleController lc = new LocaleController();
                // get all portal languages
                string[] Locales = lc.GetLocales(PortalSettings.PortalId).Keys.OrderByDescending(l => lc.IsDefaultLanguage(l)).ToArray();
                
                string DefaultCultureCode = Locales[0];
                cblLanguages.DataSource = Locales.Where(l => l != DefaultCultureCode);
                cblLanguages.DataBind();

            }
        }

        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.GetCurrentPortalSettings();
            }
        }



        public void BindAll(int tabID)
        {
            hlAutoTranslate.Visible = TranslatorUtils.TranslateConfigured();
            lbTranslate.Visible = TranslatorUtils.TranslateConfigured();
            hlConfigTranslate.Visible = !lbTranslate.Visible;
            pWarning.Visible = lbTranslate.Visible;

            hfTabId.Value = tabID.ToString();

            TabController tc = new TabController();
            var mc = new ModuleController();

            TabInfo CurrentPage = tc.GetTab(tabID, PortalSettings.PortalId, false);
            
            //Unique id of default language page
            Guid UnId;
            if (CurrentPage.DefaultLanguageGuid != Null.NullGuid)
                UnId = CurrentPage.DefaultLanguageGuid;
            else
                UnId = CurrentPage.UniqueId;
            // get all non admin pages and not deleted
            var AllPages = tc.GetTabsByPortal(PortalSettings.PortalId).Values.Where(t => t.TabID != PortalSettings.AdminTabId && (t.ParentId == null || t.ParentId != PortalSettings.AdminTabId));
            AllPages = AllPages.Where(t => t.IsDeleted == false);
            // get all localized pages of current page
            var Pages = AllPages.Where(t => t.DefaultLanguageGuid == UnId || t.UniqueId == UnId);

            LocaleController lc = new LocaleController();

            // get all portal languages
            string[] Locales = lc.GetLocales(PortalSettings.PortalId).Keys.OrderByDescending(l => lc.IsDefaultLanguage(l)).ToArray();
            // locale neutral page
            if (Pages.Count() == 1 && Pages.First().CultureCode == "")
                Locales = new string[] { "" };

            
            Pages = Pages.Where(p => Locales.Contains(p.CultureCode));

            Pages = Pages.OrderBy(t => t.DefaultLanguageGuid);

            string DefaultPosition = "?";
            bool Error = false;

            Data = new DnnPages(Locales);

            foreach (TabInfo tab in Pages)
            {
                DnnPage p = Data.Page(tab.CultureCode);
                p.TabID = tab.TabID;
                p.TabName = tab.TabName;
                p.Title = tab.Title;
                p.Description = tab.Description;
                p.Path = tab.TabPath.Substring(0, tab.TabPath.LastIndexOf("//")).Replace("//", "");
                // calculate position in the form of 1.3.2...
                var SublingTabs = AllPages.Where(t => t.ParentId == tab.ParentId && t.CultureCode == tab.CultureCode || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                p.Position = (SublingTabs.IndexOf(tab) + 1).ToString();
                int ParentTabId = tab.ParentId;
                while (ParentTabId > 0)
                {
                    TabInfo ParentTab = AllPages.Single(t => t.TabID == ParentTabId);  // tc.GetTab(ParentTabId, PortalSettings.PortalId, false);
                    SublingTabs = AllPages.Where(t => t.ParentId == ParentTabId && t.CultureCode == tab.CultureCode || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                    p.Position = (SublingTabs.IndexOf(tab) + 1).ToString() + "." + p.Position;
                    ParentTabId = ParentTab.ParentId;
                }

                /*
                if (tab.DefaultLanguageGuid == Null.NullGuid)
                    DefaultPosition = p.Position;
                else
                    p.Position = p.Position + " -> " + DefaultPosition;
                */

                p.DefaultLanguageGuid = tab.DefaultLanguageGuid;
                p.IsTranslated = tab.IsTranslated;
                p.IsPublished = tab.TabPermissions.Where(tp => tp.PermissionKey == "VIEW" && tp.RoleID != PortalSettings.AdministratorRoleId).Any();
                // generate modules information
                
                foreach (ModuleInfo mi in mc.GetTabModules(tab.TabID).Values.Where(m => !m.IsDeleted))
                {
                    Guid UniqueId;
                    if (mi.DefaultLanguageGuid == Null.NullGuid)
                        UniqueId = mi.UniqueId;
                    else
                        UniqueId = mi.DefaultLanguageGuid;

                    DnnModules ms = Data.Module(UniqueId); // modules of each language
                    DnnModule m = ms.Module(tab.CultureCode);
                    // detect bug : 2 modules with same uniqueId on the same page
                    if (m.TabModuleID > 0)
                    {
                        m.Error3 = true;
                        Error = true;
                        continue;
                    }

                    m.ModuleTitle = mi.ModuleTitle;
                    m.DefaultLanguageGuid = mi.DefaultLanguageGuid;
                    m.TabId = tab.TabID;
                    m.TabModuleID = mi.TabModuleID;
                    m.ModuleID = mi.ModuleID;

                    if (mi.DefaultLanguageGuid != null)
                    {
                        ModuleInfo dtm = mc.GetModuleByUniqueID(mi.DefaultLanguageGuid);
                        if (dtm != null)
                        {
                            m.DefaultModuleID = dtm.ModuleID;
                            if (dtm.ParentTab.UniqueId != mi.ParentTab.DefaultLanguageGuid)
                                m.DefaultTabName = dtm.ParentTab.TabName;
                        }
                    }
                    m.IsTranslated = mi.IsTranslated;
                    m.IsLocalized = mi.IsLocalized;

                    m.IsShared = tc.GetTabsByModuleID(mi.ModuleID).Values.Where(t => t.CultureCode == mi.CultureCode).Count() > 1;
                    // detect bug : the default language module is on an other page
                    m.Error1 = mi.DefaultLanguageGuid != Null.NullGuid && mi.DefaultLanguageModule == null;
                    // detect bug : # culture on tab and module
                    m.Error2 = mi.CultureCode != tab.CultureCode;
                    Error = Error || m.Error1 || m.Error2 ;
                }

                

            }

            foreach (DnnModules mods in Data.Modules){
                foreach (DnnModule mod in mods.Modules) {
                    DnnPage page = Data.Page(mod.CultureCode);
                    mod.TabId = page.TabID;
                }
                
            }



            rDnnModules.DataSource = Data.Modules;
            rDnnModules.DataBind();
            lbCorrect.Visible = Error;

            lbReadyToTranslate.Visible = false;
            foreach (DnnModules mods in Data.Modules) {
                foreach (DnnModule mod in mods.Modules)
                {
                    if (mod.NotExist) {
                        lbReadyToTranslate.Visible = true;
                        break;
                    }
                }
                if (lbReadyToTranslate.Visible)
                    break;
            }


        }



        protected void rDnnModules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem item = e.Item;
            if (e.Item.ItemType == ListItemType.Header)
            {
                Repeater rDnnPage = (Repeater)item.FindControl("rDnnPage");
                rDnnPage.DataSource = Data.Pages;
                rDnnPage.DataBind();

                Repeater rHeader = (Repeater)item.FindControl("rHeader");
                rHeader.DataSource = Data.Pages;
                rHeader.DataBind();
            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                Repeater rFooter = (Repeater)item.FindControl("rFooter");
                rFooter.DataSource = Data.Pages;
                rFooter.DataBind();
            }
            else if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
            {
                Repeater rDnnModule = (Repeater)item.FindControl("rDnnModule");
                DnnModules ms = (DnnModules)item.DataItem;
                rDnnModule.DataSource = ms.Modules;
                rDnnModule.DataBind();
            }
        }

        protected void rDnnModule_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
            {
                DnnModule m = (DnnModule)item.DataItem;

                if (m.Exist)
                {
                    CheckBoxList cblPages = (CheckBoxList)item.FindControl("cblPages");
                    TabController tc = new TabController();
                    var mc = new ModuleController();
                    // its the same for each module -> to cache
                    var til = TabController.GetTabsBySortOrder(PortalSettings.PortalId, m.CultureCode, true).Where(t => /*!t.IsDeleted &&*/ t.ParentId != PortalSettings.AdminTabId && t.TabID != PortalSettings.AdminTabId && t.CultureCode == m.CultureCode);
                    //var tid = tc.GetTabsByModuleID(m.ModuleID);
                    var tmiLst = mc.GetModuleTabs(m.ModuleID).Cast<ModuleInfo>();

                    foreach (TabInfo ti in til)
                    {
                        {
                            ListItem li = new ListItem(ti.TabName, ti.TabID.ToString());
                            li.Text = ti.IndentedTabName;
                            li.Enabled = ti.TabID != m.TabId;
                            cblPages.Items.Add(li);

                            ModuleInfo tmi = tmiLst.SingleOrDefault(t => t.TabID == ti.TabID);

                            //if (tid.Keys.Contains(ti.TabID))
                            if (tmi != null)
                            {
                                if (tmi.IsDeleted)
                                {
                                    //li.Enabled = false;
                                    li.Text = "<i>" +li.Text + "</i>";
                                }
                                else
                                {
                                    li.Selected = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /*
        protected void ddlActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            if (ddl.SelectedIndex > 0 && ddl.SelectedValue.Length > 2) {
                string Action = ddl.SelectedValue.Substring(0, 2);
                int TabModuleId = int.Parse(ddl.SelectedValue.Substring(2, ddl.SelectedValue.Length-2));
                if (Action == "TR") {
                    ModuleController mc = new ModuleController();
                    ModuleInfo mi = mc.GetTabModule(TabModuleId);
                    mc.UpdateTranslationStatus(mi, true);
                }
                else if (Action == "NT")
                {
                    ModuleController mc = new ModuleController();
                    ModuleInfo mi = mc.GetTabModule(TabModuleId);
                    mc.UpdateTranslationStatus(mi, false);
                }
                else if (Action == "BI")
                {
                    ModuleController mc = new ModuleController();
                    ModuleInfo mi = mc.GetTabModule(TabModuleId);
                    mc.DeLocalizeModule(mi);
                }
                else if (Action == "UB")
                {
                    ModuleController mc = new ModuleController();
                    ModuleInfo mi = mc.GetTabModule(TabModuleId);
                    Locale locale = LocaleController.Instance.GetLocale(mi.CultureCode);
                    mc.LocalizeModule(mi, locale);
                }
                else if (Action == "DU")
                {
                    ModuleController mc = new ModuleController();
                    ModuleInfo mi = mc.GetTabModule(TabModuleId);
                    mc.DeleteTabModule(PortalSettings.ActiveTab.TabID, mi.ModuleID, true);
                

                }
                //BindAll();
                //Redirect to the same page to pick up changes
                Response.Redirect(Request.RawUrl, true);
            }


        }
        */

        protected void lbUpdate_Click(object sender, EventArgs e)
        {

            bool CurentTabUpdated = UpdateAll();
            if (CurentTabUpdated)
                Response.Redirect(Globals.NavigateURL(PortalSettings.Current.ActiveTab.TabID), true);
            else
                Response.Redirect(Request.RawUrl, true);
            
        }

        private bool UpdateAll()
        {
            bool CurentTabUpdated = false;
            // pages actions
            TabController tc = new TabController();
            Repeater rDnnPage = (Repeater)rDnnModules.Controls[0].Controls[0].FindControl("rDnnPage");
            foreach (RepeaterItem ri in rDnnPage.Items)
            {

                CheckBox cbTranslated = (CheckBox)ri.FindControl("cbTranslated");
                bool tabTranslated = cbTranslated.Checked;
                TextBox tbTabName = (TextBox)ri.FindControl("tbTabName");
                string tabName = tbTabName.Text;
                TextBox tbTabTitle = (TextBox)ri.FindControl("tbTitle");
                string tabTitle = tbTabTitle.Text;
                TextBox tbTabDescription = (TextBox)ri.FindControl("tbDescription");
                string tabDescription = tbTabDescription.Text;
                CheckBox cbAddPage = (CheckBox)ri.FindControl("cbAddPage");

                HiddenField hfTabID = (HiddenField)ri.FindControl("hfTabID");
                
                if (!string.IsNullOrEmpty(hfTabID.Value))
                {

                    int tabID = int.Parse(hfTabID.Value);
                    TabInfo ti = tc.GetTab(tabID, PortalSettings.PortalId, true);
                    bool UpdateTab = false;
                    if (ti.IsTranslated != tabTranslated)
                    {
                        tc.UpdateTranslationStatus(ti, tabTranslated);
                    }
                    if (ti.TabName != tabName)
                    {
                        ti.TabName = tabName;
                        UpdateTab = true;
                        if (ti.TabID == PortalSettings.Current.ActiveTab.TabID)
                            CurentTabUpdated = true;

                    }
                    if (ti.Title != tabTitle)
                    {
                        ti.Title = tabTitle;
                        UpdateTab = true;
                    }
                    if (ti.Description != tabDescription)
                    {
                        ti.Description = tabDescription;
                        UpdateTab = true;
                    }
                    if (UpdateTab)
                    {
                        tc.UpdateTab(ti);
                        tc.UpdateTabOrder(ti);
                        //tbLog.Text = tbLog.Text + tabName + "/" + tabID + "\n";
                    }
                }
                else if (cbAddPage.Checked) // copy page
                { 
                    HiddenField hfDefaultTabID = (HiddenField)rDnnPage.Items[0].FindControl("hfTabID");
                    int DefaultTabID = int.Parse(hfDefaultTabID.Value);
                    HiddenField hfCultureCode = (HiddenField)ri.FindControl("hfCultureCode");
                    TabInfo DafaultTabInfo = tc.GetTab(DefaultTabID, PortalSettings.PortalId, true);

                    // correct bug of empty culturecode
                    var moduleCtrl = new ModuleController();
                    foreach (KeyValuePair<int, ModuleInfo> kvp in moduleCtrl.GetTabModules(DafaultTabInfo.TabID))
                    {
                        ModuleInfo defaultModule = kvp.Value;
                        //Make sure module has the correct culture code
                        if (string.IsNullOrEmpty(defaultModule.CultureCode))
                        {
                            defaultModule.CultureCode = DafaultTabInfo.CultureCode;
                            moduleCtrl.UpdateModule(defaultModule);
                        }
                    }


                    tc.CreateLocalizedCopy(DafaultTabInfo, LocaleController.Instance.GetLocale(hfCultureCode.Value));
                
                
                }

            }
            // modules actions
            var mc = new ModuleController();
            foreach (RepeaterItem riDnnModules in rDnnModules.Items)
            {
                Repeater rDnnModule = (Repeater)riDnnModules.FindControl("rDnnModule");
                foreach (RepeaterItem riDnnModule in rDnnModule.Items)
                {
                    HiddenField hfTabModuleID = (HiddenField)riDnnModule.FindControl("hfTabModuleID");
                    int tabModuleID = int.Parse(hfTabModuleID.Value);
                    if (tabModuleID > 0)
                    {
                        TextBox tbModuleTitle = (TextBox)riDnnModule.FindControl("tbModuleTitle");
                        string moduleTitle = tbModuleTitle.Text;
                        CheckBox cbLocalized = (CheckBox)riDnnModule.FindControl("cbLocalized");
                        bool moduleLocalized = cbLocalized.Checked;
                        CheckBox cbTranslated = (CheckBox)riDnnModule.FindControl("cbTranslated");
                        bool moduleTranslated = cbTranslated.Checked;
                        CheckBoxList cblPages = (CheckBoxList)riDnnModule.FindControl("cblPages");

                        ModuleInfo mi = mc.GetTabModule(tabModuleID);
                        if (mi.ModuleTitle != moduleTitle)
                        {
                            mi.ModuleTitle = moduleTitle;
                            mc.UpdateModule(mi);

                            var tabModules = mc.GetModuleTabs(mi.ModuleID).Cast<ModuleInfo>().Where(t => t.IsDeleted == false);
                            foreach (var tm in tabModules) {
                                if (tm.CultureCode == mi.CultureCode) {
                                    tm.ModuleTitle = moduleTitle;
                                    mc.UpdateModule(tm);                                
                                }
                            }
                        }
                        bool moduleLocalizedChange = false;
                        if (mi.DefaultLanguageGuid != Null.NullGuid && mi.IsLocalized != moduleLocalized)
                        {
                            Locale loc = LocaleController.Instance.GetLocale(mi.CultureCode);
                            if (moduleLocalized)
                                mc.LocalizeModule(mi, loc);
                            else
                                mc.DeLocalizeModule(mi);

                            moduleLocalizedChange = true;
                        }
                        if (mi.DefaultLanguageGuid != Null.NullGuid && mi.IsTranslated != moduleTranslated)
                        {
                            mc.UpdateTranslationStatus(mi, moduleTranslated);
                        }

                        if (!moduleLocalizedChange) // because if Localized change ModuleId change
                        {
                            var tabModules = mc.GetModuleTabs(mi.ModuleID).Cast<ModuleInfo>().Where(t=> t.IsDeleted == false);
                            foreach (ListItem li in cblPages.Items)
                            {
                                if (li.Enabled)
                                {
                                    bool Add = li.Selected;
                                    int TabId = int.Parse(li.Value);

                                    if (Add && tabModules.Any(m => m.TabID == TabId && m.IsDeleted == true))
                                    {
                                        

                                    }
                                    if (Add && !tabModules.Any(m => m.TabID == TabId))
                                    {
                                        ModuleInfo sourceModule = mc.GetModule(mi.ModuleID, mi.TabID);
                                        TabInfo destinationTab = tc.GetTab(TabId, PortalSettings.PortalId, false);

                                        ModuleInfo tmpModule = mc.GetModule(mi.ModuleID, TabId);
                                        if (tmpModule != null && tmpModule.IsDeleted)
                                        {
                                            mc.RestoreModule(tmpModule);
                                        }
                                        else
                                        {
                                            mc.CopyModule(sourceModule, destinationTab, Null.NullString, true);
                                        }
                                        if (sourceModule.DefaultLanguageModule != null && destinationTab.DefaultLanguageTab != null) // not default language
                                        {
                                            ModuleInfo defaultLanguageModule = mc.GetModule(sourceModule.DefaultLanguageModule.ModuleID, destinationTab.DefaultLanguageTab.TabID);
                                            if (defaultLanguageModule != null)
                                            {
                                                ModuleInfo destinationModule = destinationModule = mc.GetModule(sourceModule.ModuleID, destinationTab.TabID);
                                                destinationModule.DefaultLanguageGuid = defaultLanguageModule.UniqueId;
                                                mc.UpdateModule(destinationModule);
                                            }
                                        }
                                        
                                    }
                                    else if (!Add && tabModules.Any(m => m.TabID == TabId))
                                    {
                                        mc.DeleteTabModule(TabId, mi.ModuleID, true);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Copy module
                        CheckBox cbAddModule = (CheckBox)riDnnModule.FindControl("cbAddModule");
                        if (cbAddModule.Checked)
                        {
                            // find the first existing module on the line
                            foreach (RepeaterItem riCopy in rDnnModule.Items)
                            {
                                HiddenField hfTabModuleIDCopy = (HiddenField)riCopy.FindControl("hfTabModuleID");
                                int tabModuleIDCopy = int.Parse(hfTabModuleIDCopy.Value);
                                if (tabModuleIDCopy > 0)
                                {
                                    ModuleInfo miCopy = mc.GetTabModule(tabModuleIDCopy);

                                    TabInfo tiCopy = tc.GetTab(miCopy.TabID);

                                    //Make sure module has the correct culture code
                                    if (string.IsNullOrEmpty(miCopy.CultureCode))
                                    {
                                        miCopy.CultureCode = tiCopy.CultureCode;
                                        mc.UpdateModule(miCopy);
                                    }


                                    
                                    if (miCopy.DefaultLanguageGuid == Null.NullGuid)
                                    { // default 
                                        //int tabid2 = Data.Pages[riDnnModule.ItemIndex].TabID.Value;
                                        HiddenField hfTabID = (HiddenField)rDnnPage.Items[riDnnModule.ItemIndex].FindControl("hfTabID");
                                        int tabID = int.Parse(hfTabID.Value);

                                        //mc.GetModuleTabs(miCopy.ModuleID);
                                        ModuleInfo tmpModule = mc.GetModule(miCopy.ModuleID, tabID);
                                        if (tmpModule != null && tmpModule.IsDeleted)                                        
                                            mc.RestoreModule(tmpModule);                                        
                                        else                                        
                                            mc.CopyModule(miCopy.ModuleID, miCopy.TabID, tabID, Null.NullString, true);

                                        //Fetch new module
                                        ModuleInfo localizedModule = mc.GetModule(miCopy.ModuleID, tabID);

                                        mc.LocalizeModule(localizedModule, LocaleController.Instance.GetLocale(localizedModule.CultureCode));
                                        
                                    }
                                    else
                                    {
                                        ModuleInfo miCopyDefault = mc.GetModuleByUniqueID(miCopy.DefaultLanguageGuid);
                                        
                                        //int tabid2 = Data.Pages[riDnnModule.ItemIndex].TabID.Value;
                                        HiddenField hfTabID = (HiddenField)rDnnPage.Items[riDnnModule.ItemIndex].FindControl("hfTabID");
                                        int tabID = int.Parse(hfTabID.Value);
                                        if (miCopyDefault.TabID == tabID && miCopyDefault.IsDeleted)
                                            //mc.RestoreModule(miCopyDefault);
                                            mc.CopyModule(miCopy.ModuleID, miCopy.TabID, tabID, Null.NullString, true);
                                        else
                                            mc.CopyModule(miCopyDefault.ModuleID, miCopyDefault.TabID, tabID, Null.NullString, true);
                                    }

                                    if (riDnnModule.ItemIndex == 0)
                                    { // default language
                                        ModuleInfo miDefault = null;
                                        foreach (RepeaterItem ri in rDnnPage.Items)
                                        {

                                            HiddenField hfTabID = (HiddenField)ri.FindControl("hfTabID");
                                            int tabID = int.Parse(hfTabID.Value);
                                            if (ri.ItemIndex == 0)
                                            {
                                                miDefault = mc.GetModule(miCopy.ModuleID, tabID);

                                            }
                                            else
                                            {
                                                ModuleInfo mi = mc.GetModule(miCopy.ModuleID, tabID);
                                                if (mi != null)
                                                {
                                                    mi.DefaultLanguageGuid = miDefault.UniqueId;
                                                    mc.UpdateModule(mi);
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }

                            }

                        }
                    }
                    //tbLog.Text = tbLog.Text + moduleTitle + "/" + tabModuleID + "\n";
                }
            }
            return CurentTabUpdated;
        }

        protected void lbCorrect_Click(object sender, EventArgs e)
        {
            BindAll(int.Parse(hfTabId.Value));

            foreach (DnnModules mods in Data.Modules)
            {
                foreach (DnnModule mod in mods.Modules)
                {
                    if (mod.Error1) //the default language module is on an other page
                    {

                    }
                    else if (mod.Error2)
                    { // # culture tab and module
                        var mc = new ModuleController();
                        ModuleInfo mi = mc.GetTabModule(mod.TabModuleID);
                        mi.CultureCode = mod.CultureCode;
                        mc.UpdateModule(mi);
                    }
                    else if (mod.Error3) // duplicate
                    {
                        var mc = new ModuleController();
                        //ModuleInfo mi = mc.GetTabModule(mod.TabModuleID);
                        if (mod.TabId.HasValue)
                            mc.DeleteTabModule(mod.TabId.Value, mod.ModuleID, true);

                    }
                }
            }

            Response.Redirect(Request.RawUrl, true);
        }

        // public IModuleControl ModuleControl { get; set; }

        #region "classes for DataModel"


        protected class DnnPages
        {
            public DnnPages(string[] Locales)
            {
                this.Locales = Locales;
                Pages = new List<DnnPage>(); // one of each language
                Modules = new List<DnnModules>(); // one for each module on the page
                foreach (string locale in Locales)
                {
                    Pages.Add(new DnnPage() { CultureCode = locale });
                }
            }
            public string[] Locales { get; private set; }
            public List<DnnPage> Pages { get; private set; }
            public List<DnnModules> Modules { get; private set; }

            public DnnPage Page(string Locale)
            {
                return Pages.Single(pa => pa.CultureCode == Locale);
            }

            public DnnModules Module(Guid UniqueId)
            {
                DnnModules m = Modules.SingleOrDefault(dm => dm.UniqueId == UniqueId);
                if (m == null)
                {
                    m = new DnnModules(Locales) { UniqueId = UniqueId };
                    Modules.Add(m);
                }
                return m;

            }

            public bool Error1(int ModuleId, Guid UniqueId, string CultureCode)
            {
                return Modules.Any(dm => dm.Modules.Any(mm => mm.ModuleID == ModuleId && mm.CultureCode != CultureCode) && dm.UniqueId != UniqueId);
            }

        }

        protected class DnnPage
        {
            public int? TabID { get; set; }
            public String TabName { get; set; }
            public String Title { get; set; }
            public String Description { get; set; }
            public String CultureCode { get; set; }
            public Guid DefaultLanguageGuid { get; set; }
            public bool IsTranslated { get; set; }
            public bool IsPublished { get; set; }
            public String Position { get; set; }
            public String Path { get; set; }

            public bool TranslatedVisible
            {
                get
                {
                    return !Default && TabName != null;
                }
            }

            public string TranslatedTooltip
            {
                get
                {
                    if (DefaultLanguageGuid == Null.NullGuid)
                        return "";
                    else if (IsTranslated)
                        return "Translated";
                    else
                        return "Not translated";
                }

            }
            public string ImageUrl
            {
                get
                {
                    if (DefaultLanguageGuid == Null.NullGuid)
                        return "";
                    else if (IsTranslated)
                        return "~/images/translated.gif";
                    else
                        return ""; // "~/images/synchronize.gif";        
                }
            }
            public bool Default
            {
                get
                {
                    return DefaultLanguageGuid == Null.NullGuid;
                }
            }

            public bool NotDefault
            {
                get
                {
                    return !Default;
                }
            }
        }

        protected class DnnModules
        {
            public DnnModules(string[] Locales)
            {
                Modules = new List<DnnModule>(); // one module for each language
                foreach (string locale in Locales)
                {
                    Modules.Add(new DnnModule() { CultureCode = locale });
                }
            }
            public Guid UniqueId { get; set; }
            public List<DnnModule> Modules { get; private set; }
            public DnnModule Module(string Locale)
            {
                return Modules.Single(mo => mo.CultureCode == Locale);
            }
        }

        protected class DnnModule
        {
            public String ModuleTitle { get; set; }
            public String CultureCode { get; set; }
            public Guid DefaultLanguageGuid { get; set; }
            public int? TabId { get; set; }
            public int TabModuleID { get; set; }
            public int ModuleID { get; set; }
            public int DefaultModuleID { get; set; }
            public string DefaultTabName { get; set; }
            public bool IsTranslated { get; set; }
            public bool IsLocalized { get; set; }
            public bool IsShared { get; set; }
            public bool Error3 { get; set; }
            public bool Error1 { get; set; }
            public bool Error2 { get; set; }

            public bool Default
            {
                get
                {
                    return DefaultLanguageGuid == Null.NullGuid;
                }
            }

            public bool NotDefault
            {
                get
                {
                    return !Default;
                }
            }

            public bool TranslatedVisible
            {
                get
                {
                    if (ErrorVisible)
                        return false;

                    if (CultureCode == null /*|| ti.CultureCode == null*/ )
                        return false;
                    else if (DefaultLanguageGuid == Null.NullGuid)
                        return false;
                    else
                        return ModuleID != DefaultModuleID;
                }
            }

            public bool LocalizedVisible
            {
                get
                {
                    if (ErrorVisible)
                        return false;

                    if (CultureCode == null /*|| ti.CultureCode == null*/ )
                        return false;
                    else if (DefaultLanguageGuid == Null.NullGuid)
                        return false;
                    else
                        return true;
                }
            }

            public bool Exist
            {
                get
                {
                    return TabModuleID > 0;
                }
            }

            public bool NotExist
            {
                get
                {
                    return !Exist;
                }
            }

            public bool NotExistAndTabExist
            {
                get
                {
                    return !Exist && TabId.HasValue;
                }
            }

            public string TranslatedTooltip
            {
                get
                {
                    if (CultureCode == null /*|| ti.CultureCode == null*/ )
                        return "";
                    else if (DefaultLanguageGuid == Null.NullGuid)
                    {
                        return "";
                    }
                    else
                    {
                        string PageName = "";
                        if (DefaultTabName != null)
                            PageName = " / " + DefaultTabName;

                        if (ModuleID == DefaultModuleID)
                            return "Reference " + PageName;
                        else
                        {
                            if (IsTranslated)
                                return "Translated " + PageName;
                            else
                                return "Not Translated " + PageName;
                        }
                    }
                }
            }

            public string LocalizedTooltip
            {
                get
                {
                    if (CultureCode == null /*|| ti.CultureCode == null*/ )
                        return "";
                    else if (DefaultLanguageGuid == Null.NullGuid)
                    {
                        return "";
                    }
                    else
                    {
                        string PageName = "";
                        if (DefaultTabName != null)
                            PageName = " / " + DefaultTabName;

                        if (ModuleID == DefaultModuleID)
                            return "Reference to default language" + PageName;
                        else
                        {
                            return "Detached " + PageName;
                        }
                    }
                }
            }

            public string ImageUrl
            {
                get
                {
                    if (CultureCode == null /*|| ti.CultureCode == null*/ )
                        return "";
                    else if (DefaultLanguageGuid == Null.NullGuid)
                    {
                        return "";
                    }
                    else
                    {
                        if (Error3)
                        {
                            return "~/images/error-icn.png";
                        }
                        else if (ModuleID == DefaultModuleID)
                            return "~/images/modulebind.gif";
                        else
                        {
                            if (IsTranslated)
                                return "~/images/translated.gif";
                            else
                                return "~/images/moduleunbind.gif"; ;
                        }
                    }
                }
            }
            public string TranslateImageUrl
            {
                get
                {
                    if (CultureCode == null /*|| ti.CultureCode == null*/ )
                        return "";
                    else if (DefaultLanguageGuid == Null.NullGuid)
                    {
                        return "";
                    }
                    else
                    {
                        if (Error3)
                        {
                            return "";
                        }
                        else if (ModuleID == DefaultModuleID)
                            return "";
                        else
                        {
                            if (IsTranslated)
                                return "~/DesktopModules/CLTools/images/translated.png";
                            else
                                return "~/DesktopModules/CLTools/images/untranslated.png"; ;
                        }
                    }
                }
            }

            public bool ErrorVisible
            {
                get
                {
                    return Error1 || Error2 || Error3;
                }
            }

            public string ErrorToolTip
            {
                get
                {
                    if (Error1)
                    {
                        return "Default module on other tab";
                    }
                    else if (Error2)
                    {
                        return "culture of module # culture of tab";
                    }
                    else if (Error3)
                    {
                        return "Duplicate module";
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        #endregion

        static int MaxChars = 10240;
        protected void lbTranslate_Click(object sender, EventArgs e)
        {
            
            bool CurentTabUpdated = false;
            UpdateAll();

            BindAll(int.Parse(hfTabId.Value));

            TabController tc = new TabController();
            var mc = new ModuleController();
            string DefaultCultureCode = Data.Locales[0];
            foreach (ListItem li in cblLanguages.Items)
            {
                if (!li.Selected)
                    continue;

                string CultureCode = li.Value;
                    
                if (CultureCode == DefaultCultureCode)
                    continue;

                List<string> strTabLst = new List<string>();
                List<string> strModuleLst = new List<string>();
                List<string> strHtmlLst = new List<string>();


                bool PageTranslated = false;

                DnnPage pageDefault = Data.Page(DefaultCultureCode);
                DnnPage page = Data.Page(CultureCode);

                TabInfo ti = tc.GetTab(page.TabID.Value);
                if (pageDefault.TabID.HasValue && page.TabID.HasValue)
                {
                    if (!ti.IsTranslated)
                    {
                        strTabLst.Add(pageDefault.TabName);
                        strTabLst.Add(pageDefault.Title);
                        strTabLst.Add(pageDefault.Description);
                        PageTranslated = true;
                    }
                }

                List<ModuleInfo> miLst = new List<ModuleInfo>();
                List<HtmlTextInfo> htiLst = new List<HtmlTextInfo>();
                
                foreach (DnnModules mods in Data.Modules)
                {
                    DnnModule modDefault = mods.Module(DefaultCultureCode);
                    DnnModule mod = mods.Module(CultureCode);
                    if (mod.TabModuleID > 0 && modDefault.TabModuleID > 0)
                    {
                        ModuleInfo mi = mc.GetTabModule(mod.TabModuleID);
                        if (!ti.IsTranslated)
                        {
                            miLst.Add(mi);
                            strModuleLst.Add(modDefault.ModuleTitle);
                        }

                        if (mi.IsLocalized && !mi.IsTranslated && mi.ModuleDefinition.FriendlyName == "Text/HTML")
                        {
                            HtmlTextController htc = new HtmlTextController();
                            int workflowID = htc.GetWorkflow(mod.ModuleID, ti.TabID, ti.PortalID).Value;
                            HtmlTextInfo htiDefault = htc.GetTopHtmlText(modDefault.ModuleID, false, workflowID);
                            if (htiDefault != null)
                            {
                                HtmlTextInfo hti = htc.GetTopHtmlText(mod.ModuleID, false, workflowID);
                                if (hti == null)
                                {
                                    var wsc = new WorkflowStateController();
                                    hti = new HtmlTextInfo();
                                    hti.ItemID = -1;
                                    hti.StateID = wsc.GetFirstWorkflowStateID(workflowID);
                                    hti.WorkflowID = workflowID;
                                    hti.ModuleID = mi.ModuleID;
                                }

                                string txtHtml = HttpUtility.HtmlDecode(htiDefault.Content);
                                htiLst.Add(hti);
                                strHtmlLst.Add(txtHtml);
                            }
                        }

                    }                    
                }

                List<string> strLst = new List<string>();
                strLst.AddRange(strTabLst);
                strLst.AddRange(strModuleLst);
                strLst.AddRange(strHtmlLst);
                
                if (strLst.Count() > 0)
                {
                    int IdxStrLst = 0;
                    string[] res = TranslateArray(DefaultCultureCode, CultureCode, strLst);
                    if (PageTranslated) {
                        ti.TabName = res[0];
                        ti.Title = res[1];
                        ti.Description = res[2];
                        IdxStrLst = 3;
                        if (ti.TabID == PortalSettings.Current.ActiveTab.TabID)
                            CurentTabUpdated = true;                        
                    }
                    for (int i = 0; i < miLst.Count(); i++)
                    {
                        ModuleInfo mi = miLst[i];
                        mi.ModuleTitle = res[IdxStrLst];
                        mc.UpdateModule(mi);

                        IdxStrLst++;                        
                    }
                    for (int i = 0; i < htiLst.Count(); i++)
                    {
                            HtmlTextController htc = new HtmlTextController();
                            //var wsc = new WorkflowStateController();
                            //int workflowID = htc.GetWorkflow(htiLst[i].ModuleID, ti.TabID, ti.PortalID).Value;
                            //HtmlTextInfo hti = htc.GetTopHtmlText(htiLst[i].ModuleID, false, workflowID);
                            /*
                            if (hti == null)
                            {
                                hti = new HtmlTextInfo();
                                hti.ItemID = -1;
                                hti.StateID = wsc.GetFirstWorkflowStateID(workflowID);
                                hti.WorkflowID = workflowID;
                                hti.ModuleID = mi.ModuleID;
                            }
                            */
                            htiLst[i].Content = HttpUtility.HtmlEncode(res[IdxStrLst]);
                            htc.UpdateHtmlText(htiLst[i], htc.GetMaximumVersionHistory(ti.PortalID));
                            IdxStrLst++;
                            ModuleInfo mi = mc.GetModule(htiLst[i].ModuleID, ti.TabID);
                            mc.UpdateTranslationStatus(mi, true);

                    }
                    tc.UpdateTab(ti);
                    tc.UpdateTabOrder(ti);
                    tc.UpdateTranslationStatus(ti, true);
                }
            }

            if (CurentTabUpdated)
                Response.Redirect(Globals.NavigateURL(PortalSettings.Current.ActiveTab.TabID), true);
            else
                Response.Redirect(Request.RawUrl, true);
            
        }

        private static string[] TranslateArray(string DefaultCultureCode, string CultureCode, List<string> strLst)
        {
            int NbrChars = 0;
            List<string> resLst = new List<string>();
            List<string> tmpLst = new List<string>();
            foreach (var str in strLst)
            {
                if (str.Length > MaxChars)
                {
                    if (tmpLst.Count > 0)
                    {
                        string[] res = TranslatorUtils.TranslateArray(tmpLst.ToArray(), DefaultCultureCode.Substring(0, 2), CultureCode.Substring(0, 2));
                        resLst.AddRange(res);
                        NbrChars = 0;
                        tmpLst.Clear();
                    }
                    //don't translate
                    resLst.Add(str);
                }
                else
                {
                    NbrChars += str.Length;
                    if (NbrChars > MaxChars)
                    {
                        string[] res = TranslatorUtils.TranslateArray(tmpLst.ToArray(), DefaultCultureCode.Substring(0, 2), CultureCode.Substring(0, 2));
                        resLst.AddRange(res);
                        NbrChars = str.Length;
                        tmpLst.Clear();
                        tmpLst.Add(str);
                    }
                    else
                    {
                        tmpLst.Add(str);
                    }
                }
            }
            if (tmpLst.Count > 0) {
                string[] res = TranslatorUtils.TranslateArray(tmpLst.ToArray(), DefaultCultureCode.Substring(0, 2), CultureCode.Substring(0, 2));
                resLst.AddRange(res);                
            }
            
            //string[] res = TranslatorUtils.TranslateArray(strLst.ToArray(), DefaultCultureCode.Substring(0, 2), CultureCode.Substring(0, 2));
            return resLst.ToArray();
        }

        protected void lbSynchonizePermmissions_Click(object sender, EventArgs e) { 
            var modCtrl = new ModuleController();
            var tabCtrl = new TabController();

            int TabId = int.Parse(hfTabId.Value);
            var Tab = tabCtrl.GetTab(TabId);

            if (Tab.DefaultLanguageTab != null)
                Tab = Tab.DefaultLanguageTab;


            foreach (TabInfo localizedTab in Tab.LocalizedTabs.Values)
            {
                var moduleCtrl = new ModuleController();
                foreach (KeyValuePair<int, ModuleInfo> kvp in moduleCtrl.GetTabModules(Tab.TabID))
                {
                    ModuleInfo sourceModule = kvp.Value;
                    ModuleInfo localizedModule = null;
                               
                    if (sourceModule.LocalizedModules.TryGetValue(localizedTab.CultureCode, out localizedModule))
                    {


                        if (!sourceModule.IsDeleted)
                        {
                            var sourcePerms = sourceModule.ModulePermissions.Where(p => p.PermissionKey == "EDIT");
                            foreach (var sourcePerm in sourcePerms)
                            {

                                var perm = localizedModule.ModulePermissions.Where(tp => tp.RoleName == sourcePerm.RoleName && tp.PermissionKey == "EDIT").SingleOrDefault();
                                if (perm == null)
                                {
                                    var modulePermission = new ModulePermissionInfo(sourcePerm);
                    
                                    modulePermission.RoleID = sourcePerm.RoleID;
                                    modulePermission.AllowAccess = true;

                                    localizedModule.ModulePermissions.Add(modulePermission);
                                    ModulePermissionController.SaveModulePermissions(localizedModule);
                                }

                            }

                        }
                    }
                }
           }
        
        }
        

        protected void lbReadyToTranslate_Click(object sender, EventArgs e)
        {
            var modCtrl = new ModuleController();
            var tabCtrl = new TabController();

            int TabId = int.Parse(hfTabId.Value);
            var Tab = tabCtrl.GetTab(TabId);


            foreach (TabInfo localizedTab in Tab.LocalizedTabs.Values)
            {
                //Make Deep copies of all modules
                var moduleCtrl = new ModuleController();
                foreach (KeyValuePair<int, ModuleInfo> kvp in moduleCtrl.GetTabModules(Tab.TabID))
                {
                    ModuleInfo sourceModule = kvp.Value;
                    ModuleInfo localizedModule = null;

                    //Make sure module has the correct culture code
                    if (string.IsNullOrEmpty(sourceModule.CultureCode))
                    {
                        sourceModule.CultureCode = Tab.CultureCode;
                        moduleCtrl.UpdateModule(sourceModule);
                    }

                    if (!sourceModule.LocalizedModules.TryGetValue(localizedTab.CultureCode, out localizedModule))
                    {
                        if (!sourceModule.IsDeleted)
                        {
                            //Shallow (Reference Copy)

                            {
                                if (sourceModule.AllTabs)
                                {
                                    foreach (ModuleInfo m in moduleCtrl.GetModuleTabs(sourceModule.ModuleID))
                                    {
                                        //Get the tab
                                        TabInfo allTabsTab = tabCtrl.GetTab(m.TabID, m.PortalID, false);
                                        TabInfo localizedAllTabsTab = null;
                                        if (allTabsTab.LocalizedTabs.TryGetValue(localizedTab.CultureCode, out localizedAllTabsTab))
                                        {
                                            moduleCtrl.CopyModule(m, localizedAllTabsTab, Null.NullString, true);
                                        }
                                    }
                                }
                                else
                                {
                                    moduleCtrl.CopyModule(sourceModule, localizedTab, Null.NullString, true);
                                }
                            }

                            //Fetch new module
                            localizedModule = moduleCtrl.GetModule(sourceModule.ModuleID, localizedTab.TabID);

                            //Convert to deep copy
                            moduleCtrl.LocalizeModule(localizedModule, LocaleController.Instance.GetLocale(localizedTab.CultureCode));
                        }
                    }
                }

                var users = new Dictionary<int, UserInfo>();

                //Give default translators for this language and administrators permissions
                tabCtrl.GiveTranslatorRoleEditRights(localizedTab, users);

                //Send Messages to all the translators of new content
                /*
                foreach (UserInfo translator in users.Values)
                {
                    if (translator.UserID != PortalSettings.AdministratorId)
                    {
                        var message = new Message();
                        message.FromUserID = PortalSettings.AdministratorId;
                        message.ToUserID = translator.UserID;
                        message.Subject = Localization.GetString("NewContentMessage.Subject", LocalResourceFile);
                        message.Status = MessageStatusType.Unread;
                        message.Body = string.Format(Localization.GetString("NewContentMessage.Body", LocalResourceFile),
                                                     localizedTab.TabName,
                                                     Globals.NavigateURL(localizedTab.TabID, false, PortalSettings, Null.NullString, localizedTab.CultureCode, new string[] { }),
                                                     txtTranslationComment.Text);

                        var messageCtrl = new MessagingController();
                        messageCtrl.SaveMessage(message);
                    }
                }
                 */
            }

            //Redirect to refresh page (and skinobjects)
            Response.Redirect(Request.RawUrl, true);
        }

        protected object Iif(object condition, object trueResult, object falseResult)
        {

            return (bool)condition ? trueResult : falseResult;

        }

       


    }
}