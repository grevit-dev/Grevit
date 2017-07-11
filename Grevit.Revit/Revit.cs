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
        /// Create UI on StartUp
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel grevitPanel = null;

            foreach (RibbonPanel rpanel in application.GetRibbonPanels())
                if (rpanel.Name == "Grevit") grevitPanel = rpanel;

            if (grevitPanel == null) grevitPanel = application.CreateRibbonPanel("Grevit");
                
            PushButton commandButton = grevitPanel.AddItem(new PushButtonData("GrevitCommand", "Grevit", path, "Grevit.Revit.GrevitCommand")) as PushButton;
            commandButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.paper_airplane.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));

            commandButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "http://grevit.net/"));

            PushButton parameterButton = grevitPanel.AddItem(new PushButtonData("ParameterNames", "Parameter names", path, "Grevit.Revit.ParameterNames")) as PushButton;
            parameterButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.tag_hash.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));

            parameterButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "http://grevit.net/"));

            PushButton getFaceRefButton = grevitPanel.AddItem(new PushButtonData("GetFaceReference", "Face Reference", path, "Grevit.Revit.GrevitFaceReference")) as PushButton;
            getFaceRefButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.radio_button.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));

            getFaceRefButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "http://grevit.net/"));

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

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Environment Variables
            UIApplication uiApp = commandData.Application;
            GrevitBuildModel model = new GrevitBuildModel(uiApp.ActiveUIDocument.Document);
            return model.BuildModel(null);
        }

    }


    [Transaction(TransactionMode.Manual)]
    public class GrevitFaceReference : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Environment Variables
            UIApplication uiApp = commandData.Application;
            Reference reference = uiApp.ActiveUIDocument.Selection.PickObject(ObjectType.Face, "Select Face");
            if (reference != null)
            {
                string stable = reference.ConvertToStableRepresentation(uiApp.ActiveUIDocument.Document);
                Prompt.ShowDialog("Stable Reference:", stable);
            }

            return Result.Succeeded;
        }

    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            System.Windows.Forms.Form prompt = new System.Windows.Forms.Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
               // Text = caption,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                Text = ""
            };
            System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label() { Left = 50, Top = 20, Text = text };
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 50, Top = 50, Width = 400 , ReadOnly = true, Text = caption};
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = System.Windows.Forms.DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == System.Windows.Forms.DialogResult.OK ? textBox.Text : "";
        }
    }

    /// <summary>
    /// The actual Revit Grevit Command
    /// </summary>

    public class GrevitBuildModel 
    {
        public GrevitBuildModel(Autodesk.Revit.DB.Document doc)
        {
            document = doc;
        }

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
        /// List for roof shape points to apply
        /// </summary>
        public static List<Tuple<ElementId, CurveArray>> RoofShapePoints;

        public static double Scale;

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

        public Result BuildModel(Grevit.Types.ComponentCollection components)
        {
            bool delete = false;

            if (components == null)
            {
                // Create new Grevit Client sending existing Families 
                Grevit.Client.ClientWindow grevitClientDialog = new Grevit.Client.ClientWindow(document.GetFamilies());
                //Grevit.Serialization.Client grevitClientDialog = new Grevit.Serialization.Client(document.GetFamilies());

                // Show Client Dialog
                grevitClientDialog.ShowWindow();
                //if (grevitClientDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return Result.Cancelled;

                // Set the received component collection
                components = grevitClientDialog.componentCollection;

                delete = grevitClientDialog.componentCollection.delete;

                Scale = grevitClientDialog.componentCollection.scale;
            }

            RoofShapePoints = new List<Tuple<ElementId,CurveArray>>();


            // Set up a List for stalled components (with References)
            List<Component> componentsWithReferences = new List<Component>();

            // Get all existing Grevit Elements from the Document
            // If Update is false this will just be an empty List
            existing_Elements = document.GetExistingGrevitElements(components.update);

            // Set up an empty List for created Elements
            created_Elements = new Dictionary<string, ElementId>();


            #region createComponents

            Transaction trans = new Transaction(GrevitBuildModel.document, "GrevitCreate");
            trans.Start();

            // Walk thru all received components
            foreach (Component component in components.Items)
            {
                // If they are not reference dependent, create them directly
                // Otherwise add the component to a List of stalled elements
                if (!component.stalledForReference)
                {
                    try
                    {
                        component.Build(false);
                    }
                    catch (Exception e) { Grevit.Reporting.MessageBox.Show(component.GetType().Name + " Error", e.InnerException.Message); }
                }
                else
                    componentsWithReferences.Add(component);
            }

            // Walk thru all elements which are stalled because they are depending on
            // an Element which needed to be created first


            foreach (Component component in componentsWithReferences)
            { 
                try
                {
                    component.Build(true); 
                }
                catch (Exception e) { Grevit.Reporting.MessageBox.Show(component.GetType().Name + " Error", e.InnerException.Message); }
            }

            trans.Commit();
            trans.Dispose();



            foreach (Tuple<ElementId,CurveArray> rsp in RoofShapePoints)
            {
                if (rsp.Item1 != ElementId.InvalidElementId)
                {
                    Autodesk.Revit.DB.RoofBase roof = (Autodesk.Revit.DB.RoofBase)document.GetElement(rsp.Item1);
                    if (roof != null)
                    {
                        if (roof.SlabShapeEditor != null)
                        {
                            if (roof.SlabShapeEditor.IsEnabled)
                            {
                                Transaction pp = new Transaction(GrevitBuildModel.document, "GrevitPostProcessing");
                                pp.Start();
                                roof.SlabShapeEditor.Enable(); 
                                pp.Commit();
                                pp.Dispose();
                            }

                            List<XYZ> points = new List<XYZ>();
                            foreach (Curve c in rsp.Item2)
                                points.Add(c.GetEndPoint(0));

                            Transaction ppx = new Transaction(GrevitBuildModel.document, "GrevitPostProcessing");
                            ppx.Start();

                            foreach (SlabShapeVertex v in roof.SlabShapeEditor.SlabShapeVertices)
                            { 
                                double Zdiff = 0;

                                foreach (XYZ pt in points)
                                {
                                    if (Math.Abs(v.Position.X - pt.X) < double.Epsilon
                                        && Math.Abs(v.Position.Y - pt.Y) < double.Epsilon
                                        && Math.Abs(v.Position.Z - pt.Z) > double.Epsilon)
                                        Zdiff = pt.Z;
                                }

                                if (Zdiff != 0)
                                    roof.SlabShapeEditor.ModifySubElement(v, Zdiff);
                            }

                            ppx.Commit();
                            ppx.Dispose();
                        
                        }
                    
                    }
                
                }
            }



            #endregion


            // If Delete Setting is activated
            if (delete)
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
        private UIApplication uiApp;

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // if the request is coming from us
            if ((args.RequestingAssembly != null) && (args.RequestingAssembly == this.GetType().Assembly))
            {
                if ((args.Name != null) && (args.Name.Contains(","))) // ignore resources and such
                {
                    string asmName = args.Name.Split(',')[0];
                    string targetFilename = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, asmName + ".dll");
                    uiApp.Application.WriteJournalComment("Assembly Resolve issue. Looking for: " + args.Name, false);
                    uiApp.Application.WriteJournalComment("Looking for " + targetFilename, false);
                    if (File.Exists(targetFilename))
                    {
                        uiApp.Application.WriteJournalComment("Found, and loading...", false);
                        return System.Reflection.Assembly.LoadFrom(targetFilename);
                    }
                }
            }
            return null;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Revit Environment
            uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

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



    /*
    /// <summary>
    /// Imports a SketchUp Model
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class ImportSketchUp : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Revit Environment
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            GrevitBuildModel c = new GrevitBuildModel(doc);
            GrevitBuildModel.Scale = 3.28084;

            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.Multiselect = false;
            if (filedialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SketchUpNET.SketchUp skp = new SketchUpNET.SketchUp();
                if (skp.LoadModel(filedialog.FileName))
                {
                    Grevit.Types.ComponentCollection components = new ComponentCollection() { Items = new List<Component>() };

                    foreach (SketchUpNET.Instance instance in skp.Instances)
                    {
                        if (instance.Name.ToLower().Contains("wall"))
                        {
                            foreach (SketchUpNET.Surface surface in instance.Parent.Surfaces)
                            {
                                components.Items.Add(new WallProfileBased(instance.Parent.Name, instance.Parent.Name, new List<Types.Parameter>(), surface.ToGrevitOutline(instance.Transformation), "") { GID = instance.Guid });
                            }
                        }


                        if (instance.Name.ToLower().Contains("floor"))
                        {
                            foreach (SketchUpNET.Surface surface in instance.Parent.Surfaces)
                            {
                                Types.Point bottom = instance.Transformation.GetTransformed(surface.Vertices[0]).ToGrevitPoint();
                                int ctr = surface.Vertices.Count / 2;
                                Types.Point top = instance.Transformation.GetTransformed(surface.Vertices[ctr]).ToGrevitPoint();



                                components.Items.Add(new Slab()
                                {
                                    FamilyOrStyle = instance.Parent.Name,
                                    TypeOrLayer = instance.Parent.Name,
                                    parameters = new List<Types.Parameter>(),
                                    structural = true,
                                    height = 1,
                                    surface =
                                        surface.ToGrevitProfile(instance.Transformation),
                                    bottom = bottom,
                                    top = top,
                                    slope = top.z - bottom.z,
                                    GID = instance.Guid,
                                    levelbottom = "",
                                });
                            }
                        }

                        if (instance.Name.ToLower().Contains("column"))
                        {
                            Grevit.Types.Profile profile = null;
                            Grevit.Types.Point top = null;
                            Grevit.Types.Point btm = new Types.Point(instance.Transformation.X, instance.Transformation.Y, instance.Transformation.Z);

                            foreach (SketchUpNET.Surface surface in instance.Parent.Surfaces)
                            {

                                if (surface.Normal.Z == 1)
                                {
                                    top = new Types.Point(instance.Transformation.X, instance.Transformation.Y,
                                        surface.Vertices[0].ToGrevitPoint(instance.Transformation).z);
                                }

                            }

                            components.Items.Add(new Grevit.Types.Column(instance.Parent.Name, instance.Parent.Name, new List<Types.Parameter>(), btm, top, "", true)
                            {
                                GID = instance.Guid
                            });
                        }



                    }

                    c.BuildModel(components);

                }

            }



            // Return Success
            return Result.Succeeded;
        }
    }

    public static class Geometry
    {

        public static Grevit.Types.Profile ToGrevitProfile(this SketchUpNET.Surface surface, SketchUpNET.Transform t = null)
        {
            Types.Profile profile = new Types.Profile();
            profile.profile = new List<Loop>();

            Loop outerloop = new Loop();

            outerloop.outline = new List<Types.Component>();
            foreach (SketchUpNET.Edge corner in surface.OuterEdges.Edges)
                outerloop.outline.Add(corner.ToGrevitLine(t));

            profile.profile.Add(outerloop);


            foreach (SketchUpNET.Loop skploop in surface.InnerEdges)
            {
                Loop innerloop = new Loop();

                innerloop.outline = new List<Types.Component>();
                foreach (SketchUpNET.Edge corner in skploop.Edges)
                    innerloop.outline.Add(corner.ToGrevitLine(t));

                profile.profile.Add(innerloop);
            }

            return profile;
        }

        public static List<Grevit.Types.Component> ToGrevitOutline(this SketchUpNET.Surface surface, SketchUpNET.Transform t = null)
        {
            List<Grevit.Types.Component> lines = new List<Types.Component>();
            foreach (SketchUpNET.Edge corner in surface.OuterEdges.Edges)
            {
                lines.Add(corner.ToGrevitLine(t));
            }
            return lines;
        }

        public static Grevit.Types.Line ToGrevitLine(this SketchUpNET.Edge corner, SketchUpNET.Transform t = null)
        {
            return new Grevit.Types.Line()
            {
                from = corner.Start.ToGrevitPoint(t),
                to = corner.End.ToGrevitPoint(t)
            };
        }

        public static Grevit.Types.Point ToGrevitPoint(this SketchUpNET.Vertex v, SketchUpNET.Transform t = null)
        {
            
            if (t != null)
            {
                SketchUpNET.Vertex vertex = t.GetTransformed(v);
                return new Grevit.Types.Point(vertex.X, vertex.Y, vertex.Z);
            }
            else
                return new Grevit.Types.Point(v.X, v.Y , v.Z );
        }
    }
    */
}


