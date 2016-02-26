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
    class GrevitSketchUpImporterUI : IExternalApplication
    {
        /// <summary>
        /// Grevit Assembly path
        /// </summary>
        static string path = typeof(GrevitSketchUpImporterUI).Assembly.Location;

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

            PushButton skpButton = grevitPanel.AddItem(new PushButtonData("ImportSketchUp", "Import SketchUp", path, "Grevit.Revit.ImportSketchUp")) as PushButton;
            skpButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Grevit.SketchUp.Properties.Resources.Skp.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));

            skpButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "http://grevit.net/"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

    }




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
                    catch (Exception e) { Grevit.Reporting.MessageBox.Show("Error", e.Message + e.StackTrace); }
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
                catch (Exception e) { Grevit.Reporting.MessageBox.Show("Error", e.Message + e.StackTrace); }
            }

            trans.Commit();
            trans.Dispose();

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
    /// Imports a SketchUp Model
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class ImportSketchUp : IExternalCommand
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

}


