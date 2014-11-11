<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CLControl.ascx.cs" Inherits="Satrabel.DNN.CL.CLControl" Debug="true" %>

<asp:HiddenField ID="hfTabId" runat="server" />

<div id="DnnPages">
<div class="container">
<asp:Repeater ID="rDnnModules" runat="server" 
    onitemdatabound="rDnnModules_ItemDataBound">
<HeaderTemplate>
    <div style="border: 1px solid #CCCCCC;" class="table">
    
        <div class="tr">
            <asp:Repeater ID="rHeader" runat="server">
                <ItemTemplate>
                    <div class="td">
                        <div class="colText">
                            <asp:Image ID="Image2" runat="server" ImageUrl='<%# "~/images/Flags/"+(Eval("CultureCode") == "" ? "none" : Eval("CultureCode") )+".gif" %>' ToolTip='<%# (Eval("CultureCode") == "" ? "Neutral Culture" : Eval("CultureCode") ) %>' ImageAlign="Middle" />
                            <asp:Label ID="lCultureCode" runat="server" Text='<%# (Eval("CultureCode") == "" ? "Neutral Culture" : Eval("CultureCode") ) %>' ></asp:Label>
                        </div>
                        <div style="float:left;padding-left:5px">   
                            <div style="width:20px;height:20px;float:left;" class="colDetached<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                 <asp:Image ID="Image4" runat="server" ImageUrl='<%# "~/images/moduleunbind.gif" %>' ToolTip='Detached ?' Visible='<%# Eval("NotDefault") %>' />                            
                            </div>
                            <div style="width:20px;height:20px;float:left;" class="colTranslated<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                 <asp:Image ID="Image3" runat="server" ImageUrl='<%# "~/images/translated.gif" %>' ToolTip='Translated ?' Visible='<%# Eval("NotDefault") %>' />                            
                            </div>
                            <div style="width:20px;height:20px;float:left" class="colShared">
                                 <asp:Image ID="Image5" runat="server" ImageUrl='<%# "~/images/shared.gif" %>' ToolTip='Added to pages ?' />                            
                            </div>
                        </div>
                    </div>
                    
                </ItemTemplate>
            </asp:Repeater>
            
            
        </div>
    
    
        <div class="tr">
            <asp:Repeater ID="rDnnPage" runat="server">
                <ItemTemplate>
                    <div style="background-color:#eee;" class="td">

                        <div class="PageInfo" style="overflow:hidden;height:22px">
                        <div class="colText">
                        <asp:HiddenField ID="hfTabID" runat="server" Value='<%# Eval("TabID") %>' />                        
                        <asp:HiddenField ID="hfCultureCode" runat="server" Value='<%# Eval("CultureCode") %>' />                        
                        <asp:TextBox ID="tbTabName" runat="server" Text='<%# Eval("TabName") %>' ToolTip="Page name" Visible='<%# Eval("TabID") != null%>' CssClass="PageName"></asp:TextBox>
                        <asp:CheckBox ID="cbAddPage" runat="server" Visible='<%# Eval("TabID") == null %>' Text="Copy page" />
                        </div>
                        
                        <div style="float:left;padding-left:5px">                            
                                <div style="width:20px;height:20px;float:left;" class="colDetached<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                  
                                </div>
                                <div style="width:20px;height:20px;float:left;" class="colTranslated<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                    <asp:CheckBox ID="cbTranslated" runat="server" Checked='<%# Eval("IsTranslated")  %>' Visible='<%# Eval("TranslatedVisible")  %>' ToolTip='<%# Eval("TranslatedTooltip")  %>'  CssClass="PageTranslated" />                        
                                </div>
                                <div style="width:20px;height:20px;float:left" class="colShared">
                                </div>                            
                            </div>
                        
                        <div class="dnnClear"></div>
                        <div style="padding-top:5px;">
                            <asp:TextBox ID="tbTitle" runat="server" Text='<%# Eval("Title") %>' ToolTip="Page title" Visible='<%# Eval("TabID") != null%>' CssClass="PageTitle"></asp:TextBox>
                        </div>
                        
                        <div style="padding-top:5px;">
                            <asp:TextBox ID="tbDescription" runat="server" Text='<%# Eval("Description") %>' ToolTip="Page description" Visible='<%# Eval("TabID") != null%>' TextMode="MultiLine" Rows="3" CssClass="PageDescription" ></asp:TextBox>
                        </div>
                        
                        <div style="padding-top:5px;">
                            <asp:Label ID="lPosition" runat="server" Text='<%# "Menu position : " + Eval("Position") %>' Visible='<%# Eval("TabID") != null%>' ></asp:Label>
                        </div>                    
                        </div>
                    </div>
                    
                </ItemTemplate>
            </asp:Repeater>
            
            
        </div>
        
</HeaderTemplate>


<ItemTemplate>
        <div class="tr">
            <asp:Repeater ID="rDnnModule" runat="server" onitemdatabound="rDnnModule_ItemDataBound">
                <ItemTemplate>
                    <div style="vertical-align:top;" class="td">

                        <asp:Panel ID="pAddModule" runat="server" Visible='<%# Eval("NotExist") %>' style="overflow:hidden;height:22px;">
                            <div style="float:left;" class="colText">
                                <asp:CheckBox ID="cbAddModule" runat="server" Visible='<%# Eval("NotExistAndTabExist") %>' Text="Copy module" />
                            </div>
                            <div style="float:left;padding-left:5px">
                            
                                <div style="width:20px;height:20px;float:left;" class="colDetached<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                
                                </div>
                                <div style="width:20px;height:20px;float:left;" class="colTranslated<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                </div>
                                <div style="width:20px;height:20px;float:left" class="colShared">
                                </div>
                            
                            </div>
                            <div style="clear:both;margin:10px"></div>
                            <div class="AddToPages">
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pDnnModule" runat="server" Visible='<%# Eval("Exist") %>' style="overflow:hidden;height:22px;">
                        
                            <div style="float:left;" class="colText">
                            <asp:HiddenField ID="hfTabModuleID" runat="server" Value='<%# Eval("TabModuleID") %>'  />
                            <asp:Label ID="Label3" runat="server" Text='<%# Eval("ModuleTitle") %>' Visible="false"></asp:Label>
                            <asp:TextBox ID="tbModuleTitle" runat="server" Text='<%# Eval("ModuleTitle") %>' Visible='<%# Eval("Exist") %>' ToolTip="Module title" CssClass="PageName" ></asp:TextBox>
                            
                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("TranslatedTooltip")  %>' Visible="false"></asp:Label>
                            
                            </div>
                            <div style="float:left;padding-left:5px">
                            
                                <div style="width:20px;height:20px;float:left;" class="colDetached<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                    <asp:Image ID="iError" runat="server" ImageUrl="~/images/error-icn.png" Visible='<%# Eval("ErrorVisible") %>' ToolTip='<%# Eval("ErrorToolTip") %>'  ImageAlign="Middle" />
                            
                                    <asp:CheckBox ID="cbLocalized" runat="server" Checked='<%# Eval("IsLocalized")  %>'  Visible='<%# Eval("LocalizedVisible") %>' ToolTip='<%# Eval("LocalizedTooltip")  %>'  />                        
                                </div>
                                <div style="width:20px;height:20px;float:left;" class="colTranslated<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                                    <asp:CheckBox ID="cbTranslated" runat="server" Checked='<%# Eval("IsTranslated")  %>' Visible='<%# Eval("TranslatedVisible")  %>' ToolTip='<%# Eval("TranslatedTooltip")  %>' CssClass="ModuleTranslated" />                        
                                </div>
                                <div style="width:20px;height:20px;float:left" class="colShared">
                                    <asp:CheckBox ID="cbShared" runat="server" Checked='<%# Eval("IsShared")  %>' Visible='<%# Eval("Exist")  %>' ToolTip='Added to pages ?' CssClass="shared"  />                        
                                </div>
                            
                            </div>
                            <div style="clear:both;margin:10px"></div>
                            <div class="AddToPages">
                            <asp:Label ID="Label8" runat="server" Text="Added to pages :"></asp:Label>
                            <br />
                            <asp:CheckBoxList ID="cblPages" runat="server" CssClass="TreeView" RepeatLayout="Flow">
                            </asp:CheckBoxList>
                            </div>
                     
                     </asp:Panel>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            
            
        </div>
</ItemTemplate>
<SeparatorTemplate>
</SeparatorTemplate>
<FooterTemplate>
        <div class="tr">
            <asp:Repeater ID="rFooter" runat="server">
                <ItemTemplate>
                
                 <div class="td">
                        <div class="colText">
                            <asp:CheckBox ID="cbPublish" runat="server" Text="Published" Visible='<%# Eval("TranslatedVisible")  %>' Checked='<%# Eval("IsPublished") %>'/>                                                                        
                        </div>
                        <div style="float:left;padding-left:5px">   
                            <div style="width:20px;height:20px;float:left;" class="colDetached<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                            </div>
                            <div style="width:20px;height:20px;float:left;" class="colTranslated<%# Iif(Container.ItemIndex == 0, "Default" , "") %>">
                            </div>
                            <div style="width:20px;height:20px;float:left" class="colShared">
                            </div>
                        </div>
                    </div>
                
                </ItemTemplate>
            </asp:Repeater>
        </div>


    </div>
</FooterTemplate>
</asp:Repeater>

</div>

    <asp:Label ID="Label5" runat="server" Text="Show colums : "></asp:Label>
    &nbsp;&nbsp;&nbsp;&nbsp;
    <asp:Image ID="Image4" runat="server" ImageUrl="~/images/moduleunbind.gif" ToolTip='Detached ?'  />                            
    <input id="cbDetached" type="checkbox" class="cbDetached" checked="checked"  />
    <asp:Label ID="Label9" runat="server" Text="Detached "></asp:Label>
    &nbsp;&nbsp;&nbsp;&nbsp;
    
    <asp:Image ID="Image3" runat="server" ImageUrl="~/images/translated.gif" ToolTip='Translated ?' />                            
    <input id="cbTranslated" type="checkbox" class="cbTranslated" checked="checked"  />
    <asp:Label ID="Label10" runat="server" Text="Translated"></asp:Label>
    &nbsp;&nbsp;&nbsp;&nbsp;
    
    <asp:Image ID="Image5" runat="server" ImageUrl="~/images/shared.gif" ToolTip='Added to pages ?' />                            
    <input id="cbShared" type="checkbox" class="cbShared" checked="checked"  />
    <asp:Label ID="Label6" runat="server" Text="Add to pages"></asp:Label>
    
            
    
<div class="dnnForm" id="form-demo">

    <ul class="dnnActions dnnClear" >
        <li>
            <asp:LinkButton ID="lbUpdate" runat="server" CssClass="dnnPrimaryAction" 
                onclick="lbUpdate_Click">Update</asp:LinkButton>
        </li>
        <li>
            <a id="CloseLocalization" class="dnnSecondaryAction" href="#">Cancel</a>
        </li>        
        <li>
            <asp:Label ID="Label11" runat="server" Text=" " Width="50px" Height="1px"></asp:Label>
        </li>        
        <li>
            <asp:LinkButton ID="lbReadyToTranslate" runat="server" CssClass="dnnSecondaryAction" onclick="lbReadyToTranslate_Click" >Ready to translate</asp:LinkButton>
        </li>        

        <li>
            <asp:LinkButton ID="lbCorrect" runat="server" CssClass="dnnSecondaryAction" onclick="lbCorrect_Click" 
                >Correct errors (without update)</asp:LinkButton>
        </li>

        <li>
            <asp:HyperLink ID="hlAutoTranslate" runat="server" CssClass="dnnSecondaryAction AutoTranslate"  NavigateUrl="javascript::return false">Auto-Translate...</asp:HyperLink>
            <asp:HyperLink ID="hlConfigTranslate" runat="server" CssClass="dnnSecondaryAction"  Target="_blank">Configue Auto-Translate</asp:HyperLink>
        </li>     
        <li>
                <a class="dnnSecondaryAction AllPagesTranslated" href="#">Mark all pages Translated</a>

        </li>
        <li>
                <a class="dnnSecondaryAction AllModulesTranslated" href="#">Mark all modules Translated</a>

        </li>
        <li>
            <asp:LinkButton ID="lSynchonizePermmissions" runat="server" CssClass="dnnSecondaryAction" onclick="lbSynchonizePermmissions_Click" >Synchonize Permmissions</asp:LinkButton>
        </li>        
           

    </ul>
</div>

<div class="dnnForm" id="form-auto-translate" style="display:none">
    <fieldset>
            <asp:Panel ID="pWarning" runat="server">            
                <div class="dnnFormMessage dnnFormWarning">
                <asp:Label ID="Label1"  runat="server" Text="* Translate page names, page tiltes, page descriptions & modules titles of <b>all pages not marked as translated</b>."></asp:Label>
                <br />
                <asp:Label ID="Label2"  runat="server" Text=" Also translate text/html module content of <b>all modules not marked as translated</b>."></asp:Label>
                </div>
            </asp:Panel>    

        <div class="dnnFormItem">
            <asp:Label ID="Label7" runat="server" Text="Select languages to auto-translate"></asp:Label>
            <asp:CheckBoxList ID="cblLanguages" runat="server" RepeatColumns="10" RepeatLayout="Flow">
            </asp:CheckBoxList>
        </div>    
    </fieldset>
    <ul class="dnnActions dnnClear" >
        <li class="dnnClear">
            <asp:LinkButton ID="lbTranslate" runat="server" CssClass="dnnPrimaryAction" onclick="lbTranslate_Click" Visible="false" >Auto-Translate *</asp:LinkButton>
        </li>
        <li class="dnnClear">
        </li>
        <li >
                
        </li>

    </ul>
</div>


</div>
