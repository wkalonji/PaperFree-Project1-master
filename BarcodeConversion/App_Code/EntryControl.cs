using System;
using System.Web.UI.WebControls;

namespace BarcodeConversion.App_Code
{
    public class EntryControl
    {
        public Label label { get; set; }
        public TextBox textBox { get; set; }

        public EntryControl(Label label, TextBox textBox)
        {
            this.label = label;
            this.textBox = textBox;
        }
    }

    [Serializable()]
    public class EntryContent
    {
        public string labelText { get; set; }
        public string text { get; set; }

        public EntryContent(string label, string text)
        {
            this.labelText = label;
            this.text = text;
        }
    }

    [Serializable()]
    public class LabelControls
    {
        public string labelText { get; set; }
        public string regexText { get; set; }
        public string msgText { get; set; }
        
        public LabelControls(string label, string regex, string message) 
        {
            labelText = label;
            regexText = regex;
            msgText = message;
        }
    }
}