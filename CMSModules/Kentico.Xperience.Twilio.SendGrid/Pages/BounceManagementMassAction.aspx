<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BounceManagementMassAction.aspx.cs" Inherits="Kentico.Xperience.Twilio.SendGrid.Pages.BounceManagementMassAction"
    Theme="Default" MasterPageFile="~/CMSMasterPages/UI/Dialogs/ModalDialogPage.master" Title="Bounce management" %>

<%@ Register Src="~/CMSAdminControls/AsyncLogDialog.ascx" TagName="AsyncLog"
    TagPrefix="cms" %>
<asp:Content ContentPlaceHolderID="plcBeforeBody" runat="server" ID="cntBeforeBody">
    <asp:Panel runat="server" ID="pnlLog" Visible="False">
        <cms:AsyncLog ID="ctlAsyncLog" runat="server" ProvideLogContext="true" />
    </asp:Panel>
</asp:Content>
<asp:Content ID="plcContent" ContentPlaceHolderID="plcBeforeContent" runat="server"
    EnableViewState="false">
    <asp:Panel runat="server" ID="pnlContent" CssClass="PageContent" EnableViewState="false" Visible="True">
        <cms:MessagesPlaceHolder runat="server" ID="pnlMessagePlaceholder" IsLiveSite="false" />
        <asp:Panel ID="pnlDelete" runat="server" EnableViewState="false">
            <cms:LocalizedHeading runat="server" ID="headAnnouncement" Level="4" EnableViewState="false" />
            <asp:Panel ID="pnlItemList" runat="server" Visible="True" CssClass="form-control vertical-scrollable-list content-block-50"
                EnableViewState="false">
                <asp:Label ID="lblItems" runat="server" EnableViewState="true" />
            </asp:Panel>
        </asp:Panel>
    </asp:Panel>
</asp:Content>
<asp:Content ID="cntFooter" ContentPlaceHolderID="plcFooter" runat="server">
        <cms:LocalizedButton ID="btnCancel" runat="server" ButtonStyle="Default" OnClick="btnCancel_OnClick"
            ResourceString="general.cancel" EnableViewState="false" />
</asp:Content>
