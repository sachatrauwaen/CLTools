<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CLTools.ascx.cs" Inherits="Satrabel.DNN.CL.CLTools" %>
<%@ Register src="CLControl.ascx" tagname="CLControl" tagprefix="dnn" %>

<div style="border-bottom: 1px solid #EEEEEE;margin-bottom:10px;padding-bottom:5px;">
<asp:Label ID="Label1" runat="server" Text="Select page"></asp:Label>
&nbsp;<asp:DropDownList ID="ddlPages" runat="server" DataTextField="IndentedTabName" 
    DataValueField="TabID" AutoPostBack="True" 
    onselectedindexchanged="ddlPages_SelectedIndexChanged">
</asp:DropDownList>
</div>
<dnn:CLControl ID="CLControl1" runat="server" />

<style type="text/css">#CloseLocalization{display:none}</style>
