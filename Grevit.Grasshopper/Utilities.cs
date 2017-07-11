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
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using System.Net.Sockets;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;



namespace Grevit.GrassHopper
{
    /// <summary>
    /// Generic Grevit Grasshopper Component Wrapper
    /// </summary>
    public class GrevitGrasshopperComponent : GH_Component
    {

        public GrevitGrasshopperComponent(string name, string nickname, string description, string panel, string group) : base(name, nickname, description, panel, group) { }

        /// <summary>
        /// Dictionary for Element Previews
        /// </summary>
        public Dictionary<string, Rhino.Geometry.Brep> Preview = new Dictionary<string, Rhino.Geometry.Brep>();

        /// <summary>
        /// Set Preview using a Brep
        /// </summary>
        /// <param name="ID">Grevit ID</param>
        /// <param name="brep">Brep to show</param>
        public void SetPreview(string ID, Rhino.Geometry.Brep brep)
        {
            if (Preview.ContainsKey(ID))
                Preview[ID] = brep;
            else
                Preview.Add(ID, brep);

        }

        /// <summary>
        /// Draw Viewport Meshes override
        /// </summary>
        /// <param name="args"></param>
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // Draw all breps of the Preview Dictionary
            foreach (Rhino.Geometry.Brep brep in Preview.Values.ToList())
                args.Display.DrawBrepShaded(brep, args.ShadeMaterial);
        }

        /// <summary>
        /// Before Solve Override
        /// </summary>
        protected override void BeforeSolveInstance()
        {
            // Clear Preview Dictionary
            this.Preview = new Dictionary<string, Rhino.Geometry.Brep>();

            // Set Instance Counter to 0
            InstanceCounter = 0;
        }

        /// <summary>
        /// Input Parameters
        /// </summary>
        /// <param name="pManager"></param>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) { }

        /// <summary>
        /// Output Parameters
        /// </summary>
        /// <param name="pManager"></param>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Register Standard Output Component
            pManager.AddGenericParameter("GrevitComponent", "C", "GrevitComponent", GH_ParamAccess.item);
        }

        /// <summary>
        /// Instance counter counts how ofthen the component was executed
        /// It is being used for creating individual GIDs
        /// </summary>
        public static int InstanceCounter;

        /// <summary>
        /// Solve Instance
        /// </summary>
        /// <param name="DA"></param>
        protected override void SolveInstance(IGH_DataAccess DA) { }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("");
            }
        }

        /// <summary>
        /// Set if GIDs are stored in the component
        /// </summary>
        public static bool storeGIDs = true;

        /// <summary>
        /// Indicator to avoid searching for the configuration component multiple times
        /// </summary>
        public static bool checkedForConfig = false;

        /// <summary>
        /// Creates a new GID or retrieves an existing one
        /// </summary>
        /// <param name="component">Grevit Component</param>
        public void SetGID(Component component)
        {
            // If we have not checked for the configuration component yet
            if (!checkedForConfig)
            {
                // Walk thru all existing components on the canvas
                foreach (IGH_DocumentObject obj in Grasshopper.Instances.ActiveCanvas.Document.Objects)
                {
                    // Check if the GrevitConfig Component exists. If yes, dont store GIDs anymore
                    if (obj.GetType() == typeof(GrevitConfig)) storeGIDs = false; 
                }

                // Set indicator to true
                checkedForConfig = true;
            }

            // get gid for this instance counter value
            string mygid = this.GetValue(InstanceCounter.ToString(), string.Empty);

            // If the gid is valid set it to the component
            if (mygid != string.Empty)
                component.GID = mygid;

            // Otherwise create a new GID and save it
            else
            {
                component.GID = Component.NewGID();
                if (storeGIDs) this.SetValue(InstanceCounter.ToString(), component.GID);
            }

            // Increment the instance counter
            InstanceCounter++;
        }
    }

    /// <summary>
    /// Send Data Component
    /// </summary>
    public class Senddata : GH_Component
    {
        public Senddata() : base("Grevit Send", "Send", "Send Grevit data to target application", "Grevit", "Assemble") { }
        
        /// <summary>
        /// Airoplane Icon
        /// </summary>
        private Bitmap Icon = Properties.Resources.paper_airplane_green; 

        /// <summary>
        /// Timout for connection
        /// </summary>
        public GH_Integer Timeout;

        /// <summary>
        /// IP for Connection
        /// </summary>
        public GH_String Ip;

        /// <summary>
        /// Port for connection
        /// </summary>
        public GH_Integer Port;

        /// <summary>
        /// List of Components as GHGoo
        /// </summary>
        public List<object> Components;

        /// <summary>
        /// Components which are being sent
        /// </summary>
        public ComponentCollection componentsToSend;

        /// <summary>
        /// Update Setting
        /// </summary>
        public bool Update = true;

        /// <summary>
        /// Erase Setting
        /// </summary>
        public bool Erase = false;

        public double Scale;

        /// <summary>
        /// FamilyCollection from the Target Document
        /// </summary>
        private static RevitFamilyCollection revitFamilyCollection = new RevitFamilyCollection();

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optional = new List<int>();

            pManager.AddGenericParameter("Components", "C", "Connect all your components here and don't forget to flatten.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Send", "Send", "Add a button to send all components by one click.", GH_ParamAccess.item);

            optional.Add(pManager.AddTextParameter("IP", "IP", "IP Adress [127.0.0.1]", GH_ParamAccess.item));
            optional.Add(pManager.AddIntegerParameter("Port", "Port", "Port [8002]", GH_ParamAccess.item));
            optional.Add(pManager.AddIntegerParameter("TimeOut", "Tout", "TimeOut [10000 ms]", GH_ParamAccess.item)); 
            optional.Add(pManager.AddBooleanParameter("Update", "Update", "Updatemode [true]", GH_ParamAccess.item)); 
            optional.Add(pManager.AddBooleanParameter("Erase", "Erase", "Erase not updated Elements [false]", GH_ParamAccess.item));
            optional.Add(pManager.AddNumberParameter("Scale", "Scale", "Scale [3.28084]", GH_ParamAccess.item));
            foreach(int i in optional) pManager[i].Optional = true;
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Categories", "C", "Response Categories", GH_ParamAccess.list);
           
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            this.Components = new List<object>();
            DA.GetDataList<object>("Components", this.Components);
            GH_Boolean update = new GH_Boolean(true);
            GH_Boolean erase = new GH_Boolean(false);

            this.Ip = new GH_String("127.0.0.1");
            this.Port = new GH_Integer(8002);
            this.Timeout = new GH_Integer(10000);
            DA.GetData<GH_Boolean>("Update", ref update);
            DA.GetData<GH_String>("IP", ref this.Ip);
            DA.GetData<GH_Integer>("Port", ref this.Port);
            DA.GetData<GH_Integer>("TimeOut", ref this.Timeout);
            GH_Boolean send = new GH_Boolean();
            DA.GetData<GH_Boolean>("Send", ref send);
            DA.GetData<GH_Boolean>("Erase", ref erase);
            this.Erase = erase.Value;
            this.Update = update.Value;
            GH_Number scale = new GH_Number(3.28084);
            DA.GetData<GH_Number>("Scale", ref scale);
            this.Scale = scale.Value;

            if (send.Value)
            {
                this.Icon = Properties.Resources.paper_airplane_red;
                this.DestroyIconCache();
                Grasshopper.Instances.RedrawAll();

                bool retry = true;
                string responseData = "";
                
                try
                {
                    while (retry)
                    {
                        using (TcpClient tcpClient = new TcpClient())
                        {

                            tcpClient.Connect(IPAddress.Parse(Ip.Value), Port.Value);
                            tcpClient.NoDelay = true;
                            tcpClient.ReceiveTimeout = Timeout.Value;
                            tcpClient.SendTimeout = Timeout.Value;

                            using (NetworkStream stream = tcpClient.GetStream())
                            {
                                using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
                                {
                                    writer.AutoFlush = true;
                                    using (StreamReader reader = new StreamReader(stream))
                                    {
                                        string line = ComponentsToString(this.Components);
                                        writer.WriteLine(line);

                                        string response = reader.ReadLine();
                                        if (response == line) retry = false;
                                        responseData = reader.ReadLine();
                                    }
                                }
                            }

                        }
                    }

                }
                catch (Exception ex)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                }

                try
                {
                    if (responseData != "")
                    {
                        revitFamilyCollection = (RevitFamilyCollection)Grevit.Serialization.Utilities.Deserialize(responseData, typeof(RevitFamilyCollection));
                    }

                }
                catch (Exception ex)
                {

                }

                this.Icon = Properties.Resources.paper_airplane_green;
                this.DestroyIconCache();
                Grasshopper.Instances.RedrawAll();

               
            }

            DA.SetDataList("Categories", revitFamilyCollection.Categories);

 
        }


        void bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
           
            System.ComponentModel.BackgroundWorker worker = sender as System.ComponentModel.BackgroundWorker;
            

                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true; 
                }
                else
                {


                    using (TcpClient tcpClient = new TcpClient())
                    {
                        try
                        {
                            tcpClient.Connect(Ip.Value, Port.Value);
                            tcpClient.ReceiveTimeout = this.Timeout.Value;
                            tcpClient.SendTimeout = this.Timeout.Value;



                            StreamWriter writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                            writer.AutoFlush = true;
                            writer.WriteLine(ComponentsToString(this.Components));


                        }
                        catch (Exception ex)
                        {
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                            e.Cancel = true;
                        }
                    }

                }
            



        }


        /// <summary>
        /// GHGoo Components to string
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        private string ComponentsToString(List<object> components)
        {
            Grevit.Types.ComponentCollection componentCollection = new ComponentCollection();
            componentCollection.Items = new List<Component>();
            componentCollection.update = this.Update;
            componentCollection.delete = this.Erase;
            componentCollection.scale = this.Scale;
            foreach (GH_Goo<object> component in components)
            {
                Component cmp = (Component)component.Value;
                componentCollection.Items.Add(cmp);
            }

            return Grevit.Serialization.Utilities.Serialize(componentCollection);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d284-4a4f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return this.Icon;
            }
        }


    }
    
    /// <summary>
    /// Bakes a bunch of Elements
    /// </summary>
    public class GrevitBakery : GH_Component
    {
        public GrevitBakery() : base("Grevit Bakery", "Bakery", "Grevit Bakery", "Grevit", "Assemble") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("dough", "D", "geometry to bake", GH_ParamAccess.list);
            int a = pManager.AddTextParameter("layer", "L", "layer to bake on [0]", GH_ParamAccess.item);
            int b = pManager.AddBooleanParameter("bake", "b", "toggle baking on/off [false]", GH_ParamAccess.item);
            pManager[a].Optional = true;
            pManager[b].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("cake", "C", "baked geometry", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_ObjectWrapper> lines = new List<GH_ObjectWrapper>();
            List<GH_Guid> cakes = new List<GH_Guid>();
            GH_String layername = new GH_String("");
            GH_Boolean bake = new GH_Boolean(false);

            DA.GetData<GH_String>("layer", ref layername);
            DA.GetData<GH_Boolean>("bake", ref bake);



            if (!DA.GetDataList<GH_ObjectWrapper>("dough", lines)) lines = new List<GH_ObjectWrapper>();


            if (lines.Count > 0 && bake.Value)
            {
                foreach (GH_ObjectWrapper line in lines)
                {
                    int layerindex = 0;
                    Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();



                    foreach (Rhino.DocObjects.Layer layer in Rhino.RhinoDoc.ActiveDoc.Layers)
                    {
                        if (layer.Name == layername.Value) layerindex = layer.LayerIndex;
                    }
                    att.LayerIndex = layerindex;

                    Guid g = Guid.Empty;
                    line.BakeGeometry(Rhino.RhinoDoc.ActiveDoc, att, ref g);
                    if (g != Guid.Empty) cakes.Add(new GH_Guid(g));
                }

                DA.SetDataList("cake", cakes);
            }

        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d284-4d7f-a777-4583beeb4b5d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.baked;
            }
        }


    }


    public class GrevitID : GH_Component
    {
        public GrevitID() : base("Grevit ID", "GID", "Grevit ID", "Grevit", "Assemble") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GrevitComponent", "C", "Grevit Component", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ID", "ID", "ID", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grevit.Types.Component component = null;

            DA.GetData<Grevit.Types.Component>("GrevitComponent", ref component);

            if (component == null)
                DA.SetData("ID", new GH_String(""));
            else
                DA.SetData("ID", new GH_String(component.GID));

        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d114-4d5f-a777-4583beeb5b5d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.id;
            }
        }


    }

    /// <summary>
    /// Gets The Layer of a Rhino Element
    /// </summary>
    public class GrevitLayer : GH_Component
    {
        public GrevitLayer() : base("Grevit Layer", "Layer", "Grevit Layer", "Grevit", "Assemble") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "E", "Element", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Layer", "L", "Layer", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Brep brep = new GH_Brep();
            DA.GetData<GH_Brep>("Element", ref brep);
            string layername = "";

            Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
            if (brep.ReferenceID != null && brep.ReferenceID != Guid.Empty)
            {
                Rhino.DocObjects.RhinoObject obj = doc.Objects.Find(brep.ReferenceID);
                layername = doc.Layers[obj.Attributes.LayerIndex].Name;
            }

            DA.SetData("Layer", layername);           

        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d284-4d7f-a777-4583beaa4a5d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.layer_stack_arrange;
            }
        }


    }

    /// <summary>
    /// CurveLoop
    /// </summary>
    public class GrevitCurveLoop : GH_Component
    {
        public GrevitCurveLoop() : base("Grevit Curve Loop", "Curve Loop", "Grevit Curve Loop", "Grevit", "Info") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Loop", "L", "Loop", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Curve> curves = new List<GH_Curve>();
            DA.GetDataList<GH_Curve>(0, curves);

            Loop loop = new Loop();
            loop.outline = new List<Component>();
            foreach (GH_Curve curve in curves) loop.outline.Add(curve.Value.ToGrevitCurve());

            DA.SetData(0,loop);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d245-4d7f-a137-4583beeb4b5d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.modelline;
            }
        }


    }

    /// <summary>
    /// Grevit Category Component
    /// </summary>
    public class GrevitRevitCategory : GH_Component
    {
        public GrevitRevitCategory() : base("Grevit RevitCategory", "RevitCategory", "Grevit RevitCategory", "Grevit", "Info") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RevitCategory", "RC", "RevitCategory", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Families", "F", "Families", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RevitCategory cat = new RevitCategory();
            DA.GetData<RevitCategory>("RevitCategory", ref cat);
            DA.SetDataList("Families", cat.Families);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d266-4d7f-a737-4583beeb4b5d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.document_tree;
            }
        }


    }

    /// <summary>
    /// Grevit Revit Family Component
    /// </summary>
    public class GrevitRevitFamily : GH_Component
    {
        public GrevitRevitFamily() : base("Grevit RevitFamily", "RevitFamily", "Grevit RevitFamily", "Grevit", "Info") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RevitFamily", "RF", "RevitFamily", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Types", "T", "Types", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RevitFamily cat = new RevitFamily();
            DA.GetData<RevitFamily>("RevitFamily", ref cat);
            DA.SetDataList("Types", cat.Types);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d266-4d7f-a733-1583beeb4b5d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.document_tree;
            }
        }


    }

    /// <summary>
    /// Grevit Config just needs to exist to avoid GID storage
    /// </summary>
    public class GrevitConfig : GH_Component
    {
        public GrevitConfig() : base("Grevit Config", "Grevit Config", "Grevit Config no GIDs", "Grevit", "Assemble") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {     
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ce4d-d282-2d1f-a717-4583beeb4b2d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.cog;
            }
        }
    }

    /// <summary>
    /// Compares GH Points
    /// </summary>
    class GH_PointComparer : IEqualityComparer<GH_Point>
    {
        public bool Equals(GH_Point x, GH_Point y)
        {
            if (Object.ReferenceEquals(x.Value,y.Value)) return true;
            if (Object.ReferenceEquals(x.Value, null) || Object.ReferenceEquals(y.Value, null)) return false;

            return x.Value.EpsilonEquals(y.Value, Rhino.RhinoMath.SqrtEpsilon);
        }

        public int GetHashCode(GH_Point p)
        { 
            if (Object.ReferenceEquals(p, null)) return 0;
            return p.Value.GetHashCode();
        }
    }



}



