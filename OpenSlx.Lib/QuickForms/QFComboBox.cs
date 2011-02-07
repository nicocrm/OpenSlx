using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.QuickForms.Controls;
using OpenSlx.Lib.QuickForms.Interfaces;
using System.ComponentModel;
using OpenSlx.Lib.QuickForms.Editors;
using System.Drawing.Design;
using System.Drawing;
using System.Windows.Forms.VisualStyles;


/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/ngaller/OpenSlx)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/


namespace OpenSlx.Lib.QuickForms
{
    /// <summary>
    /// Combo box (wrapper for an Ext JS ComboBox)
    /// 
    /// TODO: add support for hard-coding the items in the QuickForm
    /// </summary>
    [DisplayName("OpenSlx ComboBox")]
    [Description("Combination of dropdown list and textbox")]
    public class QFComboBox : QuickFormsControlBase
        // Remove the interface for now because it causes a compilation error
        //, IQFComboBox
    {
        [Bindable(true)]
        public String Text { get; set; }

        [DisplayName("Data Source")]
        [Description("Data Source providing the items.  This needs to be an OpenSlx Entity Data Source or another ASP.NET compatible datasource, NOT a stock Sage datasource")]
        [Editor(typeof(DataSourceSelectEditor), typeof(UITypeEditor))]
        public String DataSourceID { get; set; }

        /// <summary>
        /// Draw method shamelessly stolen from Sage's QFListBox
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="clientRectangle"></param>
        public override void Draw(Graphics graphics, Rectangle clientRectangle)
        {
            // (commented out the dropdown display as it was quite ugly)
            //if (VisualStyleRenderer.IsSupported)
            //{
            //    graphics.FillRectangle(SystemBrushes.ButtonFace, clientRectangle);
            //    new Rectangle(clientRectangle.Left, clientRectangle.Top, clientRectangle.Width, clientRectangle.Height).Inflate(-2, -2);
            //    if (this._renderer == null)
            //    {
            //        this._renderer = new VisualStyleRenderer(VisualStyleElement.ComboBox.DropDownButton.Normal);
            //    }
            //    this._renderer.SetParameters(VisualStyleElement.ComboBox.DropDownButton.Normal);
            //    Size size = new Size(0x11, 0x15);
            //    Rectangle bounds = new Rectangle(clientRectangle.Left + 4, clientRectangle.Top + ((clientRectangle.Height - size.Height) / 2), clientRectangle.Width - 4, size.Height);
            //    this._renderer.SetParameters(VisualStyleElement.TextBox.TextEdit.Normal);
            //    this._renderer.DrawBackground(graphics, bounds);
            //    this._renderer.DrawEdge(graphics, bounds, Edges.Bottom | Edges.Right | Edges.Top | Edges.Left, EdgeStyle.Bump, EdgeEffects.Soft);
            //    SizeF ef = graphics.MeasureString(this.Caption, SystemFonts.DefaultFont);
            //    int x = bounds.Left + ((bounds.Width - ((int)ef.Width)) / 2);
            //    int y = bounds.Top + ((bounds.Height - ((int)ef.Height)) / 2);
            //    graphics.DrawString(this.Caption, SystemFonts.DefaultFont, SystemBrushes.WindowText, (PointF)new Point(x, y));
            //    this._renderer.SetParameters(VisualStyleElement.ComboBox.DropDownButton.Normal);
            //    Rectangle rectangle3 = new Rectangle((bounds.Left + bounds.Width) - size.Width, bounds.Top, size.Width, size.Height);
            //    rectangle3.Inflate(0, -1);
            //    this._renderer.DrawBackground(graphics, rectangle3);
            //}
            //else
            //{
                this.DrawLikeEdit(graphics, clientRectangle, false);
            //}
        }
        //private VisualStyleRenderer _renderer;
 

    }
}
