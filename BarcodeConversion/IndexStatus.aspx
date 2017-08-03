<%@ Page Title="Index Status" Language="C#"  MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IndexStatus.aspx.cs" Inherits="BarcodeConversion.IndexStatus" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
<script src="Scripts/jquery.dynDateTime.min.js" type="text/javascript"></script>
<script src="Scripts/calendar-en.min.js" type="text/javascript"></script>
<link href="Content/calendar-blue.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    $(document).ready(function () {
        $("#<%=from.ClientID %>").dynDateTime({
            showsTime: true,
            ifFormat: "%Y/%m/%d %H:%M",
            daFormat: "%l;%M %p, %e %m, %Y",
            align: "BR",
            electric: false,
            singleClick: false,
            displayArea: ".siblings('.dtcDisplayArea')",
            button: ".next()"
        });
        $("#<%=to.ClientID %>").dynDateTime({
            showsTime: true,
            ifFormat: "%Y/%m/%d %H:%M",
            daFormat: "%l;%M %p, %e %m, %Y",
            align: "BR",
            electric: false,
            singleClick: false,
            displayArea: ".siblings('.dtcDisplayArea')",
            button: ".next()"
        });
    });
</script>


     <asp:Panel ID="indexStatusPanel" runat="server">
        <div style="margin-top:45px; margin-bottom:40px; height:50px; border-bottom:solid 1px green;width:899px;">
            <h2 style="margin-top:35px;">Index Status Report</h2>
        </div>   
        <div>           
            <table class = "table">
                <tr> 
                    <td style="padding-bottom:15px;"><asp:Button ID="reset" runat="server" Text="Reset" onclick="reset_Click" /></td>
                </tr>
                <tr>
                    <td><asp:Label ID="filterLabel" runat="server"><h4>Filter :</h4></asp:Label></td>
                    <td style="padding-top:14px;"> 
                        <asp:DropDownList ID="jobsFilter" OnSelectedIndexChanged="onSelectedChange" runat="server">
                            <asp:ListItem Value="allJobs">All Jobs</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td style="padding-top:14px;"> 
                        <asp:DropDownList ID="whoFilter" OnSelectedIndexChanged="onSelectedChange" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="meOnly">Your Indexes Only</asp:ListItem>
                            <asp:ListItem Value="everyone">Indexes for all Operators</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                     <td style="padding-top:14px;"> 
                        <asp:DropDownList ID="whenFilter" OnSelectedIndexChanged="onSelectWhen" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="allTime">For All Time</asp:ListItem>
                            <asp:ListItem Value="pickRange">Select Date/Time Range</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td style="padding-top:14px;"> 
                        <asp:DropDownList ID="whatFilter" OnSelectedIndexChanged="onSelectedChange" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="allSheets">All Sheets</asp:ListItem>
                            <asp:ListItem Value="printed">Printed Only</asp:ListItem>
                            <asp:ListItem Value="notPrinted">Not Printed</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
            </table> 
            <asp:Panel ID="timePanel" Visible="false" runat="server">
                <table class = "table" style="width:548px;">
                    <tr>
                        <td><asp:label runat="server">From:&nbsp;&nbsp;&nbsp;</asp:label>
                            <asp:TextBox ID="from" runat="server" ></asp:TextBox>
                            <img style="margin-left:2px;" src="Content/calender.png" /> 
                        </td>
                        <td style="padding-left:15px;"><asp:label runat="server">To:&nbsp;&nbsp;</asp:label>
                            <asp:TextBox ID="to" runat="server"></asp:TextBox>
                            <img style="margin-left:2px;" src="Content/calender.png" /> 
                        </td>
                        <td style="padding-left:15px;">
                            <asp:Button ID="dates" Text="Submit" runat="server" onclick="submit_Click" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <div style="display:inline-block;">
                <table id="gridHeader" style="width:100%;margin-top:25px;margin-bottom:-10px;" runat="server">
                    <tr><td colspan="2"><h4 style="color:blue"><asp:Label ID="description" Text="" runat="server"></asp:Label></h4> </td></tr>
                    <tr>
                        <td><asp:Label ID="sortOrder" Text="Sorted By : CREATION_TIME ASC (Default)" runat="server"></asp:Label></td>
                        <td style="text-align:right;">
                            <asp:Label ID="recordsPerPageLabel" Text="Records per page" runat="server"></asp:Label>
                            <asp:DropDownList ID="recordsPerPage" OnSelectedIndexChanged="onSelectedRecordsPerPage" runat="server" AutoPostBack="true">
                                <asp:ListItem Value="5">5</asp:ListItem>
                                <asp:ListItem Value="10" Selected="true">10</asp:ListItem>
                                <asp:ListItem Value="15">15</asp:ListItem>
                                <asp:ListItem Value="20">20</asp:ListItem>
                                <asp:ListItem Value="30">30</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                </table>
            
                <asp:GridView ID="indexeStatusGridView" runat="server" style="margin-top:15px" CssClass="mydatagrid" PagerStyle-CssClass="pager"
                            PageSize="10" HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="true" OnPageIndexChanging="pageChange_Click"
                            OnRowDataBound="rowDataBound" OnSorting="gridView_Sorting" AllowSorting="True"> 
                    <columns>
                        <asp:templatefield HeaderText ="N°" ShowHeader="true">
                            <ItemTemplate >
                                <%# Container.DataItemIndex + 1 %>
                            </ItemTemplate>
                        </asp:templatefield>
                    </columns>       
                </asp:GridView>
            </div>
        
        </div>   
    </asp:Panel>
</asp:Content>
