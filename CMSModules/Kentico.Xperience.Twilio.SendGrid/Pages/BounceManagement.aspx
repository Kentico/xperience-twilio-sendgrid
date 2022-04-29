<%@ Page Language="C#" AutoEventWireup="true" Theme="Default" CodeBehind="BounceManagement.aspx.cs" Inherits="Kentico.Xperience.Twilio.SendGrid.Pages.BounceManagement" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" %>

<%@ Register Namespace="CMS.UIControls.UniGridConfig" TagPrefix="ug" Assembly="CMS.UIControls" %>
<%@ Register Tagprefix="cms" Tagname="UniGrid" Src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" %>

<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <div class="cms-bootstrap" style="margin-top:30px">
        <cms:UniGrid ID="gridReport" runat="server" ObjectType="om.contact" ShowExportMenu="true" EnableViewState="false" IsLiveSite="false" WhereCondition="ContactEmail IS NOT NULL">
            <GridActions>
                <ug:Action Caption="Purge Xperience bounces" Name="purge-kx" CommandArgument="ContactID" FontIconClass="icon-kentico" />
                <ug:Action Caption="Purge SendGrid bounce" Name="purge-sg" CommandArgument="ContactEmail" FontIconClass="icon-earth" />
            </GridActions>
            <GridColumns>
                <ug:Column runat="server" Source="ContactEmail" Caption="Email" Wrap="false">
                    <Filter Type="Text" />
                </ug:Column>
                <ug:Column runat="server" Source="ContactID" Caption="Name" ExternalSourceName="#transform: om.contact: {%ContactDescriptiveName%}" Wrap="false" />
                <ug:Column runat="server" Source="ContactEmail" Caption="Bounced in SendGrid" ExternalSourceName="sg-bounced" Wrap="false" AllowSorting="false" />
                <ug:Column runat="server" Source="##ALL##" Caption="Bounced in Xperience" ExternalSourceName="kx-bounced" Wrap="false" AllowSorting="false" />
                <ug:Column runat="server" Source="ContactBounces" Caption="Xperience bounces" Wrap="false" />
                <ug:Column runat="server" CssClass="filling-column" />
            </GridColumns>
            <GridOptions DisplayFilter="true" />
        </cms:UniGrid>
    </div>
</asp:Content>