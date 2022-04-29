<%@ Page Language="C#" AutoEventWireup="true" Theme="Default" CodeBehind="WhitelistReport.aspx.cs" Inherits="Kentico.Xperience.SendGrid.Pages.WhitelistReport"  MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" %>
<%@ Register Namespace="CMS.UIControls.UniGridConfig" TagPrefix="ug" Assembly="CMS.UIControls" %>
<%@ Register Tagprefix="cms" Tagname="UniGrid" Src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" %>

<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <asp:Button ID="btnEnableWhitelist" runat="server" CssClass="btn btn-primary" Text="Enable whitelist" OnClick="btnEnableWhitelist_Click" Visible="false" />
    <asp:Button ID="btnDisableWhitelist" runat="server" CssClass="btn btn-primary" Text="Disable whitelist" OnClick="btnDisableWhitelist_Click" Visible="false" />
    <div class="cms-bootstrap" style="margin-top:30px">
        <cms:UniGrid ID="gridReport" runat="server" ObjectType="om.contact" ShowExportMenu="true" EnableViewState="false" IsLiveSite="false" WhereCondition="ContactEmail IS NOT NULL">
            <GridActions>
                <ug:Action Caption="Add email to whitelist" Name="addemail" CommandArgument="ContactEmail" ExternalSourceName="addemail" FontIconClass="icon-doc-user" />
                <ug:Action Caption="Add domain to whitelist" Name="adddomain" CommandArgument="ContactEmail" ExternalSourceName="adddomain" FontIconClass="icon-earth" />
                <ug:Action Caption="Remove email from whitelist" Name="removeemail" CommandArgument="ContactEmail" ExternalSourceName="removeemail" FontIconClass="icon-doc-user" FontIconStyle="Critical" />
                <ug:Action Caption="Remove domain from whitelist" Name="removedomain" CommandArgument="ContactEmail" ExternalSourceName="removedomain" FontIconClass="icon-earth" FontIconStyle="Critical" />
            </GridActions>
            <GridColumns>
                <ug:Column runat="server" Source="ContactEmail" Caption="Whitelisted" ExternalSourceName="whitelisted" Wrap="false" />
                <ug:Column runat="server" Source="ContactEmail" Caption="Email" Wrap="false">
                    <Filter Type="Text" />
                </ug:Column>
                <ug:Column runat="server" Source="ContactID" Caption="Name" ExternalSourceName="#transform: om.contact: {%ContactDescriptiveName%}" Wrap="false" />
                <ug:Column runat="server" CssClass="filling-column" />
            </GridColumns>
            <GridOptions DisplayFilter="true" />
        </cms:UniGrid>
    </div>
</asp:Content>