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

    #region StructualFlags

    public class G_StructuralBeam : GH_Component
    {
        public G_StructuralBeam() : base("Grevit Revit Structural Beam", "Revit StructBeam", "Grevit Revit Structural Beam", "Grevit", "Revit Structural Types") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        { }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StructuralBeam", "StB", "StructuralBeam", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("StructuralBeam", Grevit.Types.StructuralType.Beam);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5eb1aa3d-d284-4a9f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Struct_Beam;
            }
        }
    
    }

    public class G_StructuralColumn : GH_Component
    {
        public G_StructuralColumn() : base("Grevit Revit Structural Column", "Revit StructColumn", "Grevit Revit Structural Column", "Grevit", "Revit Structural Types") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        { }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StructuralColumn", "StC", "StructuralColumn", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("StructuralColumn", Grevit.Types.StructuralType.Column);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5eb1aa3d-d284-4a9f-a777-4321beeb4b2a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Column;
            }
        }

    }

    public class G_SetCurtainPanel : GH_Component
    {
        public G_SetCurtainPanel() : base("Grevit SetCurtainPanel", "Revit SetCurtainPanel", "Grevit SetCurtainPanel", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("PanelID", "ID", "Panel ID", GH_ParamAccess.item);
            pManager.AddTextParameter("PanelType", "Type", "Panel Type Name", GH_ParamAccess.item);
            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("GrevitComponent", "C", "GrevitComponent", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String panelname = new GH_String("");
            GH_Integer id = new GH_Integer(0);
            DA.GetData<GH_Integer>("PanelID",ref id);
            DA.GetData<GH_String>("PanelType",ref panelname);
            List<Parameter> param = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", param)) param = new List<Parameter>();

            Grevit.Types.SetCurtainPanel scp = new SetCurtainPanel();
            scp.panelID = id.Value;
            scp.panelType = panelname.Value;
            scp.parameters = param;
            //scp.GID = this.InstanceGuid.ToString();
            DA.SetData("GrevitComponent", scp);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5eb1aa3d-d284-4a9f-a712-4378beab4b2a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.curtainpanel;
            }
        }

    }

    public class G_StructuralBrace : GH_Component
    {
        public G_StructuralBrace() : base("Grevit Revit Structural Brace", "Revit StructBrace", "Grevit Revit Structural Brace", "Grevit", "Revit Structural Types") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        { }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StructuralBrace", "StBr", "StructuralBrace", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("StructuralBrace", Grevit.Types.StructuralType.Brace);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5eb1aa3d-d284-4a9f-a777-4211baeb4b2a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Struct_Brace;
            }
        }

    }

    public class G_StructuralFooting : GH_Component
    {
        public G_StructuralFooting() : base("Grevit Revit Structural Footing", "Revit StructFooting", "Grevit Revit Structural Footing", "Grevit", "Revit Structural Types") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        { }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StructuralFooting", "StF", "StructuralFooting", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("StructuralFooting", Grevit.Types.StructuralType.Footing);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5eb1aa3d-d284-4a9f-a777-4214baeb4b2a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Struct_Footing;
            }
        }

    }

    public class G_StructuralUnknownFraming : GH_Component
    {
        public G_StructuralUnknownFraming() : base("Grevit Revit Structural UnknownFraming", "Revit StructUnknownFraming", "Grevit Revit Structural UnknownFraming", "Grevit", "Revit Structural Types") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        { }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StructuralUnknownFraming", "StU", "StructuralUnknownFraming", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("StructuralUnknownFraming", Grevit.Types.StructuralType.UnknownFraming);
        }

        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5eb1aa3d-d284-4a9f-a777-4114baab4b2a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Struct_Unkonwn;
            }
        }

    }

    #endregion

    #region GrevitParameter

    public class G_Parameter : GH_Component
    {
        public G_Parameter() : base("Grevit Revit Parameter", "Revit Parameter", "Grevit Revit Parameter", "Grevit", "Revit Parameters") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Parameter Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "Value", "Parameter Value", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter", "Param", "Parameter", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String();
            object value = null;

            DA.GetData<GH_String>("Name", ref name);
            DA.GetData<object>("Value", ref value);

            Parameter p = new Parameter(name.Value);

            if (value.GetType() == typeof(GH_Number))
            {
                GH_Number n = (GH_Number)value;
                p.value = n.Value;
            }
            else if (value.GetType() == typeof(GH_Integer))
            {
                GH_Integer n = (GH_Integer)value;
                p.value = n.Value;
            }
            else if (value.GetType() == typeof(GH_String))
            {
                GH_String n = (GH_String)value;
                p.value = n.Value;
            }
            else if (value.GetType() == typeof(GH_ObjectWrapper))
            {
                GH_ObjectWrapper obj = (GH_ObjectWrapper)value;

                if (obj.Value.GetType() == typeof(ElementID)) { p.value = obj.Value; }
                if (obj.Value.GetType() == typeof(SearchElementID)) { p.value = obj.Value; }
            }
            else
            {
                p.value = value.ToString();
            }


            DA.SetData("Parameter", p);
        }



        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d284-4a4f-a777-4191beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.tag_hash;
            }
        }


    }

    public class G_ElementID : GH_Component
    {
        public G_ElementID() : base("Grevit Revit ElementID", "Revit ElementID", "Grevit Revit ElementID", "Grevit", "Revit Parameters") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("ID", "ID", "Integer which will be converted to an ID", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ElementID", "ElementID", "ElementID", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Integer id = new GH_Integer();

            DA.GetData<GH_Integer>("ID", ref id);

            ElementID emid = new ElementID();
            emid.ID = id.Value;


            DA.SetData("ElementID", emid);
        }



        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d284-4a4f-a777-4191baeb2b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.tag_ID;
            }
        }


    }

    public class G_SearchElementID : GH_Component
    {
        public G_SearchElementID() : base("Grevit Revit Search ElementID", "Revit Search ElementID", "Grevit Revit Search ElementID", "Grevit", "Revit Parameters") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Element Name which will be searched", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ElementID", "ElementID", "ElementID", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String id = new GH_String();

            DA.GetData<GH_String>("Name", ref id);

            SearchElementID emid = new SearchElementID();
            emid.Name = id.Value;


            DA.SetData("ElementID", emid);
        }



        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7cc2d-d284-4c4f-a777-4191baeb2b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.tag_search;
            }
        }


    }

    #endregion

    #region RevitElements

    public class G_AdaptiveComponent : GrevitGrasshopperComponent
    {
        public G_AdaptiveComponent() : base("Grevit Revit Adaptive Component", "Revit Adaptive", "Grevit Revit Adaptive Component", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "Adaptive Points", GH_ParamAccess.list);
            pManager.AddTextParameter("Family", "Family", "Family name", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "Type", "Type name", GH_ParamAccess.item);
            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;

        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Point> points = new List<GH_Point>();

            DA.GetDataList<GH_Point>("Points", points);
            GH_String family = new GH_String();
            GH_String type = new GH_String();
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();


            DA.GetData<GH_String>("Family", ref family);
            DA.GetData<GH_String>("Type", ref type);

            Adaptive adaptiveComponent = new Adaptive();
            adaptiveComponent.FamilyOrStyle = family.Value;
            adaptiveComponent.TypeOrLayer = type.Value;
            adaptiveComponent.points = new Dictionary<int,Grevit.Types.Point>();
            adaptiveComponent.parameters = parameters;
            SetGID(adaptiveComponent);

            for(int i = 0; i < points.Count;i++)
            {
                GH_Point p = points[i];
                Rhino.Geometry.Surface srf = Rhino.Geometry.NurbsSurface.CreateFromSphere(new Rhino.Geometry.Sphere(p.Value, 0.5));
                SetPreview(adaptiveComponent.GID, srf.ToBrep());
                adaptiveComponent.points.Add(i,p.ToGrevitPoint());
            }



            
            DA.SetData("GrevitComponent", adaptiveComponent);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d284-4a4f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.adaptive;
            }
        }


    }

    public class G_Family : GrevitGrasshopperComponent
    {
        public G_Family() : base("Grevit Revit Family instance", "Revit Family", "Grevit Revit Family instance", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optionals = new List<int>();

            pManager.AddPointParameter("Points", "Points", "Insertion Points", GH_ParamAccess.list);
            optionals.Add(pManager.AddTextParameter("View", "View", "View name", GH_ParamAccess.item));
            optionals.Add(pManager.AddTextParameter("level", "level", "Level name", GH_ParamAccess.item));          
            pManager.AddTextParameter("Family", "Family", "Family name", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "Type", "Type name", GH_ParamAccess.item);
            optionals.Add(pManager.AddGenericParameter("Host", "Host", "Host", GH_ParamAccess.item));
            optionals.Add(pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list));
            optionals.Add(pManager.AddGenericParameter("StructuralType", "Struct", "Structural Type [Not Structural]", GH_ParamAccess.item));

            foreach (int a in optionals) pManager[a].Optional = true;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grevit.Types.Component host = null;
            DA.GetData<Grevit.Types.Component>("Host", ref host);
            List<GH_Point> points = new List<GH_Point>();
            DA.GetDataList<GH_Point>("Points", points);
            GH_String view = new GH_String();
            GH_String level = new GH_String();
            DA.GetData<GH_String>("level", ref level);
            DA.GetData<GH_String>("View", ref view);
            GH_String family = new GH_String();
            GH_String type = new GH_String();

            Grevit.Types.StructuralType stype = StructuralType.NonStructural;
            DA.GetData<StructuralType>("StructuralType", ref stype);

            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            DA.GetData<GH_String>("Family", ref family);
            DA.GetData<GH_String>("Type", ref type);

            Familyinstance faimlyInstance = new Familyinstance();
            faimlyInstance.FamilyOrStyle = family.Value;
            faimlyInstance.TypeOrLayer = type.Value;
            faimlyInstance.structuralType = stype;
            faimlyInstance.level = level.Value;
            faimlyInstance.points = new List<Grevit.Types.Point>();
            faimlyInstance.parameters = parameters;
            faimlyInstance.view = view.Value;
            if (host != null)
            {
                faimlyInstance.referenceGID = host.GID;
                faimlyInstance.stalledForReference = true;
            }


            SetGID(faimlyInstance);

            foreach (GH_Point p in points) faimlyInstance.points.Add(p.ToGrevitPoint());
           
            DA.SetData("GrevitComponent", faimlyInstance);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea4aa3d-d284-4a9f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Adaptive;
            }
        }


    }

    public class G_Room : GrevitGrasshopperComponent
    {
        public G_Room() : base("Grevit Revit Room", "Revit Room", "Grevit Revit Room", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Room Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Number", "Number", "Room Number", GH_ParamAccess.item);
            pManager.AddTextParameter("Phase", "Phase", "Phase", GH_ParamAccess.item);
            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String();
            GH_String phase = new GH_String();
            GH_String number = new GH_String();
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            DA.GetData<GH_String>("Name", ref name);
            DA.GetData<GH_String>("Number", ref number);
            DA.GetData<GH_String>("Phase", ref phase);

            Room room = new Room(name.Value,number.Value,phase.Value,parameters);

            room.GID = this.InstanceGuid.ToString();
           
            DA.SetData("GrevitComponent", room);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea4aa3d-d284-4a9f-a777-4321bfeb5b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Untitled_2;
            }
        }


    }

    public class G_Text : GrevitGrasshopperComponent
    {
        public G_Text() : base("Grevit Revit Text Note", "Revit Text", "Grevit Revit Text Note", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.item);
            pManager.AddPointParameter("Location", "Location", "Location", GH_ParamAccess.item);
            pManager.AddTextParameter("View", "View", "View name", GH_ParamAccess.item);
            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String content = new GH_String();
            GH_Point location = new GH_Point();
            GH_String view = new GH_String();
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            DA.GetData<GH_Point>("Location", ref location);
            DA.GetData<GH_String>("Text", ref content);
            DA.GetData<GH_String>("View", ref view);

            TextNote textnote = new TextNote();
            textnote.text = content.Value;
            textnote.view = view.Value;
            textnote.location = location.ToGrevitPoint();
            textnote.parameters = parameters;
            textnote.GID = this.InstanceGuid.ToString();

            DA.SetData("GrevitComponent", textnote);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ed4aa3d-d284-4a9f-a777-4321bfeb5b8d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Texttool;
            }
        }


    }

    public class G_Topo : GrevitGrasshopperComponent
    {
        public G_Topo() : base("Grevit Revit Topography", "Revit Topography", "Grevit Revit Topography", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "Topography Points", GH_ParamAccess.list);

            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Point> points = new List<GH_Point>();
            DA.GetDataList<GH_Point>("Points", points);


            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();



            Topography topography = new Topography();
            topography.points = new List<Grevit.Types.Point>();
            topography.parameters = parameters;
            topography.GID = this.InstanceGuid.ToString();
            foreach (GH_Point p in points) topography.points.Add(p.ToGrevitPoint());

            DA.SetData("GrevitComponent", topography);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca7e-d284-4a4f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Topo;
            }
        }


    }

    public class G_WallProfileBased : GrevitGrasshopperComponent
    {
        public G_WallProfileBased() : base("Grevit Revit Profile based wall", "Revit PbWall", "Grevit Revit Profile based wall", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Profile", "Profile", "Wall Profile", GH_ParamAccess.list);
            pManager.AddTextParameter("Type", "Type", "Type name", GH_ParamAccess.item);
            pManager.AddTextParameter("Level", "Level", "Level name", GH_ParamAccess.item);

            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            GH_String levelName = new GH_String();
            GH_String family = new GH_String("");
            GH_String type = new GH_String();

            List<Parameter> parameters = new List<Parameter>();
            List<GH_Curve> curves = new List<GH_Curve>();
            if (!DA.GetDataList<GH_Curve>("Profile", curves)) curves = new List<GH_Curve>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            DA.GetData<GH_String>("Type", ref type);
            DA.GetData<GH_String>("Level", ref levelName);


            List<Component> curveList = new List<Component>();
            foreach (GH_Curve curve in curves) curveList.Add(curve.ToGrevitCurve());
            
            WallProfileBased wall = new WallProfileBased(family.Value, type.Value,parameters, curveList, levelName.Value );

            SetGID(wall);

            DA.SetData("GrevitComponent", wall);
        }


        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a777-4321befc3b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Wall;
            }
        }


    }

    public class G_Wall : GrevitGrasshopperComponent
    {
        public G_Wall() : base("Grevit Revit Wall", "Revit Wall", "Grevit Revit Wall", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optional = new List<int>();
            pManager.AddCurveParameter("Baseline", "Baseline", "Baseline of the wall", GH_ParamAccess.item);
            optional.Add(pManager.AddTextParameter("Type", "Type", "Type name", GH_ParamAccess.item));
            optional.Add(pManager.AddTextParameter("Levelbottom", "Level", "Level name", GH_ParamAccess.item));
            pManager.AddNumberParameter("Height", "Height", "Wall height", GH_ParamAccess.item);
            optional.Add(pManager.AddBooleanParameter("Join", "Join", "Join walls after creating them", GH_ParamAccess.item));
            optional.Add(pManager.AddBooleanParameter("Flip", "Flip", "Flip wall after creating it", GH_ParamAccess.item));

            optional.Add(pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list));

            foreach(int a in optional) pManager[a].Optional = true;
            
            
            
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Curve baseline = new GH_Curve();
            GH_String level = new GH_String("none");
            GH_Number height = new GH_Number();
            GH_Number offset = new GH_Number();
            GH_Boolean join = new GH_Boolean(true);
            GH_Boolean flip = new GH_Boolean(false);
            GH_String family = new GH_String("");
            GH_String type = new GH_String("none");

            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            DA.GetData<GH_Boolean>("Join", ref join);
            DA.GetData<GH_String>("Type", ref type);
            DA.GetData<GH_String>("Levelbottom", ref level);
            DA.GetData<GH_Number>("Height", ref height);
            DA.GetData<GH_Curve>("Baseline", ref baseline);
            DA.GetData<GH_Boolean>("Flip", ref flip);


            Wall wall = new Wall(family.Value, type.Value, parameters, baseline.ToGrevitCurve(), level.Value,height.Value,join.Value,flip.Value );
            
            Rhino.Geometry.Surface srf = Rhino.Geometry.Surface.CreateExtrusion(baseline.Value, new Rhino.Geometry.Vector3d(0, 0, height.Value));

            SetGID(wall);
            SetPreview(wall.GID, srf.ToBrep());
            
            DA.SetData("GrevitComponent", wall);
        }


        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Wall;
            }
        }


    }

    public class G_Extrusion : GrevitGrasshopperComponent
    {
        public G_Extrusion() : base("Grevit Revit Extrusion", "Revit Extrusion", "Grevit Revit Extrusion", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Outline", "Outline", "Extrusion Outline", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector", "Vector", "Extrusion Vector", GH_ParamAccess.item);

            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Curve baseline = new GH_Curve();
            GH_Vector vector = new GH_Vector();
            GH_String family = new GH_String("");
            GH_String type = new GH_String("");
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            
            DA.GetData<GH_Vector>("Vector", ref vector);
            DA.GetData<GH_Curve>("Outline", ref baseline);

            if (baseline.Value.IsPolyline() && baseline.Value.IsClosed)
            {
                


                Rhino.Geometry.Polyline pline;
                if (baseline.Value.TryGetPolyline(out pline))
                {
                    PLine plineToExtrude = new PLine();
                    plineToExtrude.points = new List<Grevit.Types.Point>();
                    foreach (Rhino.Geometry.Point3d pkt in pline)
                    {
                        plineToExtrude.points.Add(pkt.ToGrevitPoint());
                    }
                    plineToExtrude.closed = pline.IsClosed;
                    plineToExtrude.GID = baseline.ReferenceID.ToString();


                    Grevit.Types.Point extrusionVector = new Grevit.Types.Point()
                    {
                        x = vector.Value.X,
                        y = vector.Value.Y,
                        z = vector.Value.Z
                    };

                    SimpleExtrusion extrusion = new SimpleExtrusion(family.Value, type.Value, parameters, plineToExtrude, extrusionVector);

                    Rhino.Geometry.Surface srf = Rhino.Geometry.Surface.CreateExtrusion(baseline.Value, vector.Value);
                    
                    SetGID(extrusion);
                    SetPreview(extrusion.GID, srf.ToBrep());

                    

                    DA.SetData("GrevitComponent",extrusion);
                }
            }
        }


        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a777-4321bfab7b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.massing;
            }
        }


    }

    public class G_Hatch : GrevitGrasshopperComponent
    {
        public G_Hatch() : base("Grevit Revit Hatch", "Revit Hatch", "Grevit Revit Hatch", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "Ouline Points", GH_ParamAccess.list);
            pManager.AddTextParameter("View", "View", "View name", GH_ParamAccess.item);

            int b = pManager.AddTextParameter("Pattern", "Pattern", "Pattern name", GH_ParamAccess.item);
            pManager[b].Optional = true;

            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Point> points = new List<GH_Point>();
            GH_String pattern = new GH_String("");
            GH_String view = new GH_String("");

            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();
            if (!DA.GetDataList<GH_Point>("Points", points)) points = new List<GH_Point>();

            DA.GetData<GH_String>("View", ref view);
            DA.GetData<GH_String>("Pattern", ref pattern);



            List<Grevit.Types.Point> outline = new List<Grevit.Types.Point>();
            IEnumerable<GH_Point> outlineWithoutDuplicates = points.Distinct(new GH_PointComparer());
            foreach (var point in outlineWithoutDuplicates)  outline.Add(point.ToGrevitPoint());
            
            Hatch hatch = new Hatch(parameters, outline,view.Value, pattern.Value);

            hatch.GID = this.InstanceGuid.ToString();


            DA.SetData("GrevitComponent", hatch);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a777-4111bfff4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Hatch;
            }
        }


    }

    public class G_FilterRule : GrevitGrasshopperComponent
    {
        public G_FilterRule() : base("Grevit Revit Filter Rule", "Revit Filter Rule", "Grevit Revit Filter Rule", "Grevit", "Revit Filter") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Parameter Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Operator", "Equality", "Equality [=,<,>]", GH_ParamAccess.item);
            pManager.AddTextParameter("Value", "Value", "Parameter Value", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String("");
            GH_String value = new GH_String("");
            GH_String eq = new GH_String("=");

            DA.GetData<GH_String>("Name", ref name);
            DA.GetData<GH_String>("Value", ref value);
            DA.GetData<GH_String>("Operator", ref eq);

            Grevit.Types.Rule rule = new Rule()
            {
                name = name.Value,
                value = value.Value,
                equalityComparer = eq.Value
            };

            DA.SetData("GrevitComponent", rule);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea1aa3d-d221-4a4f-a777-4111bfff4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.filter;
            }
        }


    }


    public class G_Filter : GrevitGrasshopperComponent
    {
        public G_Filter() : base("Grevit Revit Filter", "Revit Filter", "Grevit Revit Filter", "Grevit", "Revit Filter") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optionals = new List<int>();

            pManager.AddTextParameter("Name", "Name", "Parameter Name", GH_ParamAccess.item);
            pManager.AddTextParameter("View", "View", "View Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Rules", "Rules", "Rules", GH_ParamAccess.list);

            optionals.Add(pManager.AddTextParameter("Categories", "Categories", "Category Names", GH_ParamAccess.list));
            

            optionals.Add(pManager.AddColourParameter("CutFillColor", "CutFillColor", "CutFillColor", GH_ParamAccess.item));
            optionals.Add(pManager.AddColourParameter("CutLineColor", "CutLineColor", "CutLineColor", GH_ParamAccess.item));
            optionals.Add(pManager.AddColourParameter("ProjectionFillColor", "ProjectionFillColor", "ProjectionFillColor", GH_ParamAccess.item));
            optionals.Add(pManager.AddColourParameter("ProjectionLineColor", "ProjectionLineColor", "ProjectionLineColor", GH_ParamAccess.item));
            optionals.Add(pManager.AddIntegerParameter("CutLineWeight", "CutLineWeight", "CutLineWeight", GH_ParamAccess.item));
            optionals.Add(pManager.AddIntegerParameter("ProjectionLineWeight", "ProjectionLineWeight", "ProjectionLineWeight", GH_ParamAccess.item));
            optionals.Add(pManager.AddTextParameter("CutFillPattern", "CutFillPattern", "CutFillPattern", GH_ParamAccess.item));
            optionals.Add(pManager.AddTextParameter("CutLinePattern", "CutLinePattern", "CutLinePattern", GH_ParamAccess.item));
            optionals.Add(pManager.AddTextParameter("ProjectionFillPattern", "ProjectionFillPattern", "ProjectionFillPattern", GH_ParamAccess.item));
            optionals.Add(pManager.AddTextParameter("ProjectionLinePattern", "ProjectionLinePattern", "ProjectionLinePattern", GH_ParamAccess.item));

            foreach (int i in optionals) pManager[i].Optional = true;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String("");
            GH_String view = new GH_String("");
            List<Rule> rules = new List<Rule>();
            List<GH_String> cats = new List<GH_String>();
            List<string> categories = new List<string>();

            GH_Colour CutFillColor = null;
            GH_Colour CutLineColor = null;
            GH_Colour ProjectionFillColor = null;
            GH_Colour ProjectionLineColor = null;
            GH_Integer CutLineWeight = new GH_Integer(-1);
            GH_Integer ProjectionLineWeight = new GH_Integer(-1);
            GH_String CutFillPattern = null;
            GH_String CutLinePattern = null;
            GH_String ProjectionFillPattern = null;
            GH_String ProjectionLinePattern = null;

            DA.GetData<GH_Colour>("CutFillColor", ref CutFillColor);
            DA.GetData<GH_Colour>("CutLineColor", ref CutLineColor);
            DA.GetData<GH_Colour>("ProjectionFillColor", ref ProjectionFillColor);
            DA.GetData<GH_Colour>("ProjectionLineColor", ref ProjectionLineColor);
            DA.GetData<GH_Integer>("CutLineWeight", ref CutLineWeight);
            DA.GetData<GH_Integer>("ProjectionLineWeight", ref ProjectionLineWeight);
            DA.GetData<GH_String>("CutFillPattern", ref CutFillPattern);
            DA.GetData<GH_String>("CutLinePattern", ref CutLinePattern);
            DA.GetData<GH_String>("ProjectionFillPattern", ref ProjectionFillPattern);
            DA.GetData<GH_String>("ProjectionLinePattern", ref ProjectionLinePattern);

            DA.GetData<GH_String>("Name", ref name);
            DA.GetData<GH_String>("View", ref view);
            DA.GetDataList<Rule>("Rules",rules);
            DA.GetDataList<GH_String>("Categories",cats);

            foreach (GH_String cat in cats) categories.Add(cat.Value);

            Grevit.Types.Filter filter = new Filter()
            {
                name = name.Value,
                view = view.Value,
                categories = categories,
                Rules = rules
            };
            

            if (CutFillColor != null) filter.CutFillColor = CutFillColor.ToGrevitColor();
            if (CutLineColor != null) filter.CutLineColor = CutLineColor.ToGrevitColor();
            if (ProjectionFillColor != null) filter.ProjectionFillColor = ProjectionFillColor.ToGrevitColor();
            if (ProjectionLineColor != null) filter.ProjectionLineColor = ProjectionLineColor.ToGrevitColor();

            filter.CutLineWeight = CutLineWeight.Value;
            filter.ProjectionLineWeight = ProjectionLineWeight.Value;

            if (CutFillPattern != null) filter.CutFillPattern = CutFillPattern.Value;
            if (CutLinePattern != null) filter.CutLinePattern = CutLinePattern.Value;
            if (ProjectionFillPattern != null) filter.ProjectionFillPattern = ProjectionFillPattern.Value;
            if (ProjectionLinePattern != null) filter.ProjectionLinePattern = ProjectionLinePattern.Value;

            

            DA.SetData("GrevitComponent", filter);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea5aa3d-d221-4a4f-a771-4114bfff3b2d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.filter;
            }
        }


    }

    public class G_Slab : GrevitGrasshopperComponent
    {
        public G_Slab() : base("Grevit Revit Slab", "Revit Slab", "Grevit Revit Slab", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optional = new List<int>();

            pManager.AddSurfaceParameter("Surface", "S", "Slab Surface", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "Type", "Type name", GH_ParamAccess.item);
            pManager.AddTextParameter("Levelbottom", "Level", "Level name", GH_ParamAccess.item);
            //optional.Add(pManager.AddPointParameter("SlopeTopPoint", "STop", "Slab Slope Top Point", GH_ParamAccess.item));
            //optional.Add(pManager.AddPointParameter("SlopeBottomPoint", "SBottom", "Slab Slope Bottom Point", GH_ParamAccess.item));
            //optional.Add(pManager.AddNumberParameter("Slope", "Slope", "Slope", GH_ParamAccess.item));
            optional.Add(pManager.AddBooleanParameter("Structural", "Structural", "is Slab Structural? [false]", GH_ParamAccess.item));

            optional.Add(pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list));
            
            foreach(int a in optional) pManager[a].Optional = true;
            
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Surface surface = new GH_Surface();
            GH_String level = new GH_String();
            GH_String family = new GH_String("");
            GH_String type = new GH_String("");

            GH_Boolean structural = new GH_Boolean(false); 
            //GH_Number slope = new GH_Number(0.0);
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();
            DA.GetData<GH_Surface>("Surface", ref surface);

            //GH_Point slopeTopPoint = new GH_Point(surface.Value.Edges[0].PointAtStart);
            //GH_Point slopeBottomPoint = new GH_Point(surface.Value.Edges[0].PointAtEnd);

            //DA.GetData<GH_String>("Family", ref family);
            DA.GetData<GH_String>("Type", ref type);
            DA.GetData<GH_String>("Levelbottom", ref level);
            DA.GetData<GH_Boolean>("Structural", ref structural);
//            DA.GetData<GH_Point>("SlopeTopPoint", ref slopeTopPoint);
//            DA.GetData<GH_Point>("SlopeBottomPoint", ref slopeBottomPoint);
//            DA.GetData<GH_Number>("Slope", ref slope);

            

            Slab slab = new Slab();
            slab.FamilyOrStyle = family.Value;
            slab.TypeOrLayer = type.Value;
            slab.levelbottom = level.Value;
            slab.structural = structural.Value;
            slab.surface = new Profile();
            slab.surface.profile = new List<Loop>();
            
            Loop loop = new Loop();
            loop.outline = new List<Component>();

            foreach (Rhino.Geometry.BrepEdge be in surface.Value.Edges)
            {
                loop.outline.Add(be.ToNurbsCurve().ToGrevitCurve());
            }

            slab.surface.profile.Add(loop);

            slab.parameters = parameters;
            //slab.top = slopeTopPoint.ToGrevitPoint();
            //slab.bottom = slopeBottomPoint.ToGrevitPoint();
            //slab.slope = slope.Value;
            slab.GID = this.InstanceGuid.ToString();



            DA.SetData("GrevitComponent", slab);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a777-4111beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Slab;
            }
        }


    }

    public class G_Roof : GrevitGrasshopperComponent
    {
        public G_Roof() : base("Grevit Revit Roof", "Revit Roof", "Grevit Revit Roof", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optional = new List<int>();

            pManager.AddSurfaceParameter("Surface", "S", "Slab Surface", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "Type", "Type name", GH_ParamAccess.item);

            pManager.AddTextParameter("Level", "Level", "Level name", GH_ParamAccess.item);

            optional.Add(pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list));

            foreach (int a in optional) pManager[a].Optional = true;

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Surface surface = new GH_Surface();
            GH_String level = new GH_String();
            GH_String family = new GH_String("");
            GH_String type = new GH_String("");

            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();
            DA.GetData<GH_Surface>("Surface", ref surface);


            DA.GetData<GH_String>("Type", ref type);
            DA.GetData<GH_String>("Level", ref level);



            Roof roof = new Roof();
            roof.FamilyOrStyle = family.Value;
            roof.TypeOrLayer = type.Value;
            roof.levelbottom = level.Value;
            roof.surface = new Profile();
            roof.surface.profile = new List<Loop>();

            Loop loop = new Loop();
            loop.outline = new List<Component>();

            foreach (Rhino.Geometry.BrepEdge be in surface.Value.Edges)
            {
                loop.outline.Add(be.ToNurbsCurve().ToGrevitCurve());
            }

            roof.surface.profile.Add(loop);

            roof.parameters = parameters;
            roof.GID = this.InstanceGuid.ToString();


            DA.SetData("GrevitComponent", roof);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d211-4a4f-a777-4111beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Slab;
            }
        }


    }

    
    public class G_Level : GrevitGrasshopperComponent
    {
        public G_Level() : base("Grevit Revit Level", "Revit Level", "Grevit Revit Level", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Level Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Elevation", "Elevation", "Absolute Level Elevation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("View", "Create view", "View name", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String();
            GH_Number elevation = new GH_Number();
            GH_Boolean view = new GH_Boolean();

            DA.GetData<GH_String>("Name", ref name);
            DA.GetData<GH_Boolean>("View", ref view);
            DA.GetData<GH_Number>("Elevation", ref elevation);

            Level level = new Level(name.Value, elevation.Value, view.Value);
            level.GID = this.InstanceGuid.ToString();
            DA.SetData("GrevitComponent", level);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ef1ca3d-d271-4a4f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Level;
            }
        }


    }

    public class G_Column : GrevitGrasshopperComponent
    {
        public G_Column() : base("Grevit Revit Column", "Revit Column", "Grevit Revit Column", "Grevit", "Components Revit") { }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optional = new List<int>();
            optional.Add(pManager.AddTextParameter("Family", "Fam", "Family name", GH_ParamAccess.item));
            optional.Add(pManager.AddTextParameter("Type", "Type", "Type name", GH_ParamAccess.item));
            optional.Add(pManager.AddTextParameter("Level", "Level", "Level name", GH_ParamAccess.item));
            pManager.AddPointParameter("PointTop", "Top", "Column Top Point", GH_ParamAccess.item);
            pManager.AddPointParameter("PointBottom", "Bottom", "Column Bottom Point", GH_ParamAccess.item);

            optional.Add(pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list));
            optional.Add(pManager.AddTextParameter("GID", "GID", "Optional GID override", GH_ParamAccess.item));

            foreach(int a in optional) pManager[a].Optional = true;
            
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String level = new GH_String("none");
            GH_String family = new GH_String("none");
            GH_String type = new GH_String("none");
            GH_Point topPoint = new GH_Point();
            GH_Point bottomPoint = new GH_Point();
            GH_String gid = new GH_String("");
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();



            DA.GetData<GH_String>("Family", ref family);
            DA.GetData<GH_String>("Type", ref type);
            DA.GetData<GH_String>("Level", ref level);
            DA.GetData<GH_Point>("PointTop", ref topPoint);
            DA.GetData<GH_Point>("PointBottom", ref bottomPoint);
            DA.GetData<GH_String>("GID", ref gid);

            Column column = new Column(family.Value,type.Value,parameters, topPoint.ToGrevitPoint(), bottomPoint.ToGrevitPoint(),level.Value,true);


            SetGID(column);

            Rhino.Geometry.Circle circle = new Rhino.Geometry.Circle(bottomPoint.Value,0.5);
  
            Rhino.Geometry.Surface srf = Rhino.Geometry.NurbsSurface.CreateExtrusion(circle.ToNurbsCurve(), new Rhino.Geometry.Vector3d(new Rhino.Geometry.Point3d(topPoint.Value.X-bottomPoint.Value.X,topPoint.Value.Y-bottomPoint.Value.Y,topPoint.Value.Z-bottomPoint.Value.Z)));

            SetPreview(column.GID, srf.ToBrep());

            DA.SetData("GrevitComponent", column);
        }



        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d211-4f2f-a777-4321beeb4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Column;
            }
        }


    }

    public class G_Grid : GrevitGrasshopperComponent
    {
        public G_Grid() : base("Grevit Revit Grid", "Revit Grid", "Grevit Revit Grid", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Gridline", "Gridline", "Gridline", GH_ParamAccess.item);
            int b = pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
            pManager[b].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Line line = new GH_Line();

            DA.GetData<GH_Line>("Gridline", ref line);
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            GH_String name = new GH_String();
            DA.GetData<GH_String>("Name", ref name);

            Tuple<Grevit.Types.Point, Grevit.Types.Point> points = line.ToGrevitPoints();
            Grid gridLine = new Grid(parameters, points.Item1, points.Item2,name.Value);

            gridLine.GID = line.ReferenceID.ToString();
            DA.SetData("GrevitComponent", gridLine);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ef1ca3d-d271-4a4f-a777-4321befa4b1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.Grevit_Grid;
            }
        }


    }

    public class G_Line : GrevitGrasshopperComponent
    {
        public G_Line() : base("Grevit Revit Curve", "Revit Curve", "Grevit Revit Curve", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<int> optional = new List<int>();

            pManager.AddCurveParameter("Curve", "Curve", "Curve", GH_ParamAccess.item);
            optional.Add(pManager.AddBooleanParameter("isModelLine", "M", "isModelLine [false]", GH_ParamAccess.item));
            optional.Add(pManager.AddBooleanParameter("isDetailLine", "D", "isDetailLine [false]", GH_ParamAccess.item));
            optional.Add(pManager.AddBooleanParameter("isRoomBoundary", "B", "isRoomBoundary [false]", GH_ParamAccess.item));
            
            foreach (int a in optional) pManager[a].Optional = true;
            
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Curve line = new GH_Curve();
            DA.GetData<GH_Curve>("Curve", ref line);
            GH_Boolean isModelLine = new GH_Boolean(false);
            GH_Boolean isDetailLine = new GH_Boolean(false);
            GH_Boolean isRoomBoundary = new GH_Boolean(false);
            DA.GetData<GH_Boolean>("isModelLine", ref isModelLine);
            DA.GetData<GH_Boolean>("isDetailLine", ref isDetailLine);
            DA.GetData<GH_Boolean>("isRoomBoundary", ref isRoomBoundary);

            RevitLine revitLine = new RevitLine();
            revitLine.curve = line.Value.ToNurbsCurve().ToGrevitCurve();

            SetGID(revitLine);
            revitLine.isModelCurve = isModelLine.Value;
            revitLine.isDetailCurve = isDetailLine.Value;
            revitLine.isRoomBounding = isRoomBoundary.Value;

            DA.SetData("GrevitComponent", revitLine);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ef1ca3d-d271-4b4f-a171-4321befa4c1d}");
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

    public class G_ReferencePlane : GrevitGrasshopperComponent
    {
        public G_ReferencePlane() : base("Grevit Revit ReferencePlane", "Revit RefPlane", "Grevit Revit ReferencePlane", "Grevit", "Components Revit") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Plane", "Plane", "Plane", GH_ParamAccess.item);
            int b = pManager.AddTextParameter("View", "View", "View", GH_ParamAccess.item);
            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
            pManager[b].Optional = true;
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String name = new GH_String();
            DA.GetData<GH_String>("Name", ref name);
            GH_String view = new GH_String();
            DA.GetData<GH_String>("View", ref view);
            GH_Surface srf = new GH_Surface();
            DA.GetData<GH_Surface>("Plane", ref srf);
            List<Parameter> parameters = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", parameters)) parameters = new List<Parameter>();

            Rhino.Geometry.Point3d[] pkts = srf.Value.GetBoundingBox(true).GetCorners();

            Grevit.Types.ReferencePlane referencePlane = new ReferencePlane();
            referencePlane.Name = name.Value;
            referencePlane.EndA = pkts[0].ToGrevitPoint();
            referencePlane.EndB = pkts[1].ToGrevitPoint();
            referencePlane.cutVector = srf.Value.GetBoundingBox(true).Center.ToGrevitPoint();
            referencePlane.View = view.Value;
            SetGID(referencePlane);

            DA.SetData("GrevitComponent", referencePlane);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ef1ca3d-d271-4b4f-a171-4111befa2c1d}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.refplane;
            }
        }


    }

    #endregion

}



