
<%@ Page Title="Print Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Indexes.aspx.cs" Inherits="BarcodeConversion.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

     <script>
         // FADEOUT INDEX-SAVED MSG. FUNCTION
         //function FadeOut() {
         //    $("span[id$='indexSavedMsg']").fadeOut(3000);
         //}
        // function FadeOut2() {
          //   $("span[id$='indexSetPrintedMsg']").fadeOut(3000);
         //}
        // PRINTING INDEX SHEETS. FUNCTION
        function printing() {
            window.print();
        }

        // PRINT WINDOW LISTNER. FUNCTION: ALLOW STUFF BE DONE RIGHT BFR or AFTER PRINT PREVIEW WINDOW.
        (function () {
            var beforePrint = function () {
                // Do something before printing dialogue box appears
            };
            // After printing dialogue box disappears, back to unprinted indexes gridview
            var afterPrint = function () {
                var answer = confirm("IMPORTANT!\n\nAre you satisfied?\n" +
                    "Click OK If you did print and are satisfied with the Index Sheets.\n" +
                    "Click CANCEL if you did not print or are not satisfied with the Index Sheets.");
                if (answer == true) {
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=setAsPrinted.ClientID%>').click();                                    
                } else {
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=getUnprinted.ClientID%>').click(); 
                }
            };

            if (window.matchMedia) {
                var mediaQueryList = window.matchMedia('print');
                mediaQueryList.addListener(function (mql) {
                    if (mql.matches) {
                        beforePrint();
                    } else {
                        afterPrint();
                    }
                });
            }
            window.onbeforeprint = beforePrint;
            window.onafterprint = afterPrint;
         }());
    </script>


    <asp:Panel ID="unprintedIndexesPanel" runat="server">
        <div style="margin-top:45px; margin-bottom:40px; height:50px; border-bottom:solid 1px green;width:899px;">
            <h2 style="margin-top:35px;">Print Index Sheets</h2>   
        </div>
        <div style="display:inline-block;">           
            <table style="margin-top:15px; width:100%;" runat="server">
                <tr><td colspan="3" style="padding-bottom:40px;"><asp:Button ID="getUnprintedIndexes" Visible="true" runat="server" Text="Reset" onclick="getUnprintedIndexes_Click" /></td></tr>
                <tr style="background-color:aliceblue; height:40px;">
                    <td style="padding-left:5px;"><asp:Button ID="getBarcodeBtn" Width="105" Visible="false" runat="server" Text="Show Barcodes" onclick="getBarcode_Click" /></td>
                    <td style="text-align:center;">
                        <asp:Button ID="deleteBtn" Width="105" Visible="false" runat="server" Text="Delete Indexes" 
                            OnClientClick="return confirm('Selected Indexes will be permanently deleted. Delete anyway?');" 
                            OnClick="deleteIndexes_Click" />
                    </td>
                    <td style="text-align:right;padding-right:5px;"><asp:Button ID="printBarcodeBtn" Width="105" Visible="false" runat="server" Text="Print Barcodes" onclick="printBarcode_Click"/></td>
                </tr>
                <tr><td colspan="3" style="padding-top:30px;">
                        <h4 style="color:blue; display:inline"><asp:Label ID="description" Text="Your Unprinted Indexes" Visible="True" runat="server"></asp:Label></h4>
                    </td>
                </tr>
            </table>
            <table style="width:100%;">
                <tr >
                    <td style="padding-top:10px;">
                        <asp:Label ID="sortOrder" Text="Sorted By : CREATION_TIME ASC (Default)" runat="server"></asp:Label>
                    </td>
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
            <asp:GridView ID="indexesGridView" runat="server" style="margin-top:5px" CssClass="mydatagrid" PagerStyle-CssClass="pager" 
                          HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="True" OnPageIndexChanging="pageChange_Click"
                         OnRowDataBound="rowDataBound" OnSorting="gridView_Sorting" AllowSorting="True"> 
                <columns>
                    <asp:templatefield HeaderText="Select">
                        <HeaderTemplate>
                            &nbsp;<asp:CheckBox ID="selectAll" runat="server" AutoPostBack="true" OnCheckedChanged="selectAll_changed" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="cbSelect" runat="server" />
                        </ItemTemplate>
                    </asp:templatefield>
                    <asp:templatefield HeaderText="N°" ShowHeader="true">
                        <ItemTemplate>
                            <%# Container.DataItemIndex + 1 %>
                        </ItemTemplate>
                    </asp:templatefield>
                    <asp:TemplateField HeaderText="" ShowHeader="false">
                        <ItemTemplate>
                            <asp:Image ID="imgBarCode" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </columns>
                <HeaderStyle CssClass="header" />
                <PagerStyle CssClass="pager" />
                <RowStyle CssClass="rows" />
            </asp:GridView>
        </div>
         
    </asp:Panel>
    <div style="display:none; margin-top:15px;">
        <asp:Button ID="setAsPrinted" runat="server" Text="ShowPanel" onclick="setAsPrinted_Click"/>
    </div>
     <div style="display:none; margin-top:15px;">
        <asp:Button ID="getUnprinted" runat="server" Text="ShowPanel" onclick="getUnprinted_Click"/>
    </div>
  
</asp:Content>
