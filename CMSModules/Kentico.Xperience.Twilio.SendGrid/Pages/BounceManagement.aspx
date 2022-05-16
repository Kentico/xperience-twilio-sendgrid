<%@ Page Language="C#" AutoEventWireup="true" Theme="Default" CodeBehind="BounceManagement.aspx.cs" Inherits="Kentico.Xperience.Twilio.SendGrid.Pages.BounceManagement" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" %>

<%@ Register Namespace="CMS.UIControls.UniGridConfig" TagPrefix="ug" Assembly="CMS.UIControls" %>
<%@ Register Tagprefix="cms" Tagname="UniGrid" Src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" %>
<%@ Register TagPrefix="cms" TagName="MassActions" Src="~/CMSAdminControls/UI/UniGrid/Controls/MassActions.ascx" %>

<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <div class="cms-bootstrap" style="margin-top:30px">
        <cms:UniGrid ID="gridReport" runat="server" ShowExportMenu="true" EnableViewState="false" IsLiveSite="false">
            <GridColumns>
                <ug:Column runat="server" Source="ContactEmail" Caption="Email" Wrap="false">
                    <Filter Type="Text" />
                </ug:Column>
                <ug:Column runat="server" Source="ContactID" Caption="Name" ExternalSourceName="#transform: om.contact: {%ContactDescriptiveName%}" Wrap="false" />
                <ug:Column runat="server" Source="ContactEmail" Caption="Bounced in SendGrid" ExternalSourceName="sg-bounced" Wrap="false" AllowSorting="false" />
                <ug:Column runat="server" Source="##ALL##" Caption="Bounced in Xperience" ExternalSourceName="kx-bounced" Wrap="false" AllowSorting="false" />
                <ug:Column runat="server" CssClass="filling-column" />
            </GridColumns>
            <GridOptions ShowSelection="true" DisplayFilter="true" />
        </cms:UniGrid>
        <cms:MassActions ID="ctrlMassActions" runat="server" />
    </div>
</asp:Content>