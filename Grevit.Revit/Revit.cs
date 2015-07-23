//
//  Grevit - Create Autodesk Revit (R) Models in McNeel's Rhino Grassopper 3D (R)
//  For more Information visit grevit.net or food4rhino.com/project/grevit
//  Copyright (C) 2015
//  Authors: Maximilian Thumfart,
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;
using System.Windows.Media.Imaging;


namespace Grevit.Revit
{
    /// <summary>
    /// Create Grevit UI
    /// </summary>    
    class GrevitUI : IExternalApplication
    {
        /// <summary>
        /// Grevit Assembly path
        /// </summary>
        static string path = typeof(GrevitUI).Assembly.Location;

        /// <summary>
        /// Singleton external application class instance.
        /// </summary>
        internal static GrevitUI grevitUI = null;

        /// <summary>
        /// Provide access to singleton class instance.
        /// </summary>
        public static GrevitUI Instance
        {
            get { return grevitUI; }
        }

        /// <summary>
        /// Create UI on StartUp
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication application)
        {
            grevitUI = this;

            RibbonPanel grevitPanel = application.CreateRibbonPanel("Grevit");
            
            PushButton commandButton = grevitPanel.AddItem(new PushButtonData("GrevitCommand", "Grevit", @"C:\ProgramData\Autodesk\Revit\Addins\" + GrevitCommand.Version + @"\Grevit.Revit.dll", "Grevit.Revit.GrevitCommand")) as PushButton;
            commandButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.paper_airplane.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));


            PushButton parameterButton = grevitPanel.AddItem(new PushButtonData("ParameterNames", "Parameter names", @"C:\ProgramData\Autodesk\Revit\Addins\" + GrevitCommand.Version + @"\Grevit.Revit.dll", "Grevit.Revit.ParameterNames")) as PushButton;
            parameterButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.tag_hash.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));
            

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

    }


    /// <summary>
    /// The actual Revit Grevit Command
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class GrevitCommand : IExternalCommand
    {
        /// <summary>
        /// Current Revit document
        /// </summary>
        public static Document document;
        
        /// <summary>
        /// Elements newly created by Grevit
        /// </summary>
        public static Dictionary<string, ElementId> created_Elements;
        
        /// <summary>
        /// Existing Grevit Elements 
        /// </summary>
        public static Dictionary<string, ElementId> existing_Elements;

        /// <summary>
        /// Version of the API being used
        /// </summary>
#if (Revit2016)
        public static string Version = "2016";
#else
        public static string Version = "2015";
#endif
        /// <summary>
        /// Revit Template Folder for creating template based family instances
        /// </summary>
        public static string RevitTemplateFolder = String.Format(@"C:\ProgramData\Autodesk\RAC {0}\Family Templates\English",Version);

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Environment Variables
            UIApplication uiApp = commandData.Application;
            document = uiApp.ActiveUIDocument.Document;

            // Create new Grevit Client sending existing Families 
            Grevit.Serialization.Client grevitClientDialog = new Grevit.Serialization.Client(document.GetFamilies());

            // Show Client Dialog
            if (grevitClientDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return Result.Cancelled;

            // Set the received component collection
            Grevit.Types.ComponentCollection components = grevitClientDialog.componentCollection;
          
            // Set up a List for stalled components (with References)
            List<Component> componentsWithReferences = new List<Component>();

            // Get all existing Grevit Elements from the Document
            // If Update is false this will just be an empty List
            existing_Elements = document.GetExistingGrevitElements(components.update);

            // Set up an empty List for created Elements
            created_Elements = new Dictionary<string, ElementId>();


            #region createComponents

            // Walk thru all received components
            foreach (Component component in components.Items)
            {
                // If they are not reference dependent, create them directly
                // Otherwise add the component to a List of stalled elements
                if (!component.stalledForReference)
                    component.Build(false);       
                else
                    componentsWithReferences.Add(component);
            }

            // Walk thru all elements which are stalled because they are depending on
            // an Element which needed to be created first
            foreach (Component component in componentsWithReferences) component.Build(true);

            #endregion


            // If Delete Setting is activated
            if (grevitClientDialog.componentCollection.delete)
            {
                // Create a new transaction
                Transaction transaction = new Transaction(document, "GrevitDelete");
                transaction.Start();

                // get the Difference between existing and new elements to erase them
                IEnumerable<KeyValuePair<string, ElementId>> unused = 
                    existing_Elements.Except(created_Elements).Concat(created_Elements.Except(existing_Elements));

                // Delete those elements from the document
                foreach (KeyValuePair<string, ElementId> element in unused) document.Delete(element.Value);

                // commit and dispose the transaction
                transaction.Commit();
                transaction.Dispose();
            }



            return Result.Succeeded;
        }


    }



    /// <summary>
    /// Provides a parameter overview on selected elements
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class ParameterNames : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Revit Environment
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            // Initialize a new Dictionary containing 4 string fields for parameter values
            Dictionary<ElementId, Tuple<string, string, string, string>> Parameters = 
                new Dictionary<ElementId, Tuple<string, string, string, string>>();

            // Walk thru selected elements
            foreach (ElementId elementid in uidoc.Selection.GetElementIds())
            {
                // Get the element
                Element element = doc.GetElement(elementid);

                // Walk thru all parameters
                foreach (Autodesk.Revit.DB.Parameter param in element.Parameters)
                {
                    // If the parameter hasn't been added yet, 
                    // Add a new entry to the dictionary
                    if (!Parameters.ContainsKey(param.Id))
                        Parameters.Add(param.Id, new Tuple<string, string, string, string>
                        (
                            param.Definition.Name,
                            param.StorageType.ToString(),
                            param.AsValueString(),
                            param.IsReadOnly.ToString())
                        );
                }  
            }

            // Create a new instance of the parameter List Dialog
            ParameterList parameterListDialog = new ParameterList();
            
            // Walk thru the sorted (by name) dictionary
            foreach (KeyValuePair<ElementId, Tuple<string, string, string, string>> kvp in Parameters.OrderBy(val => val.Value.Item1))
            { 
                // Create a ListViewItem containing all four values as subitems
                System.Windows.Forms.ListViewItem lvi = new System.Windows.Forms.ListViewItem();
                lvi.Text = kvp.Key.IntegerValue.ToString();
                lvi.SubItems.Add(kvp.Value.Item1);
                lvi.SubItems.Add(kvp.Value.Item2);
                lvi.SubItems.Add(kvp.Value.Item3);
                lvi.SubItems.Add(kvp.Value.Item4);
                
                // Add the ListViewItem to the List View
                parameterListDialog.parameters.Items.Add(lvi);           
            }
            
            // Show the Dialog
            parameterListDialog.ShowDialog();

            // Return Success
            return Result.Succeeded;
        }
    }
}


