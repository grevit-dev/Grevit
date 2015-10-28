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

    #region GrevitAutoCadComponents

    public class ACA_Wall : GrevitGrasshopperComponent
    {
        public ACA_Wall() : base("Grevit Autocad Wall", "Acad Wall", "Grevit linebased Autocad Wall", "Grevit", "Components Acad") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Baseline", "Line", "Baseline of the wall", GH_ParamAccess.item);
            int b = pManager.AddTextParameter("Layer", "Layer", "Layer name", GH_ParamAccess.item);
            int c = pManager.AddTextParameter("Style", "Style", "Wallstyle name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Height", "Wall height", GH_ParamAccess.item);

            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
            pManager[b].Optional = true;
            pManager[c].Optional = true;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Curve baseline = new GH_Curve();
            GH_String layer = new GH_String("");
            GH_Number height = new GH_Number();
            GH_String style = new GH_String("");
            List<Parameter> param = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", param)) param = new List<Parameter>();

            //DA.GetData<GH_String>("Family", ref family);
            DA.GetData<GH_String>("Layer", ref layer);
            DA.GetData<GH_String>("Style", ref style);
            DA.GetData<GH_Number>("Height", ref height);
            DA.GetData<GH_Curve>("Baseline", ref baseline);


            Wall w = new Wall(style.Value,layer.Value,param, baseline.ToGrevitCurve(),"",height.Value,true,false);

            SetGID(w);
            Rhino.Geometry.Surface srf = Rhino.Geometry.Surface.CreateExtrusion(baseline.Value, new Rhino.Geometry.Vector3d(0, 0, height.Value));
            SetPreview(w.GID, srf.ToBrep());
            DA.SetData("GrevitComponent",w);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a777-4321beeb4b1a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.ACA_Wall;
            }
        }


    }
   
    public class ACA_Column : GrevitGrasshopperComponent
    {
        public ACA_Column() : base("Grevit Autocad Column", "Acad Column", "Grevit pointbased Autocad Column", "Grevit", "Components Acad") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            int c = pManager.AddTextParameter("Style", "Style", "Columnstyle name", GH_ParamAccess.item);
            int b = pManager.AddTextParameter("Layer", "Layer", "Layer name", GH_ParamAccess.item);

            pManager.AddPointParameter("PointTop", "Top", "Top point of the column", GH_ParamAccess.item);
            pManager.AddPointParameter("PointBottom", "Bottom", "Bottom point of the column", GH_ParamAccess.item);

            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
            pManager[b].Optional = true;
            pManager[c].Optional = true;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String style = new GH_String("");
            GH_String layer = new GH_String("");
            GH_Point stop = new GH_Point();
            GH_Point sbtm = new GH_Point();
            List<Parameter> param = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", param)) param = new List<Parameter>();

            DA.GetData<GH_String>("Style", ref style);
            DA.GetData<GH_String>("Layer", ref layer);

            DA.GetData<GH_Point>("PointTop", ref stop);
            DA.GetData<GH_Point>("PointBottom", ref sbtm);

            Column s = new Column(style.Value,layer.Value,param,sbtm.ToGrevitPoint(),stop.ToGrevitPoint(),"",true );
            SetGID(s);

            Rhino.Geometry.Circle c = new Rhino.Geometry.Circle(sbtm.Value, 0.5);
            Rhino.Geometry.Surface srf = Rhino.Geometry.NurbsSurface.CreateExtrusion(c.ToNurbsCurve(), new Rhino.Geometry.Vector3d(new Rhino.Geometry.Point3d(stop.Value.X - sbtm.Value.X, stop.Value.Y - sbtm.Value.Y, stop.Value.Z - sbtm.Value.Z)));
            SetPreview(s.GID, srf.ToBrep());
            DA.SetData("GrevitComponent", s);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d211-4f2f-a777-4321beeb4b1a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.ACA_Column;
            }
        }


    }

    public class ACA_Room : GrevitGrasshopperComponent
    {
        public ACA_Room() : base("Grevit Autocad Room", "Acad Room", "Grevit Autocad Room", "Grevit", "Components Acad") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Room Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Number", "Number", "Room Number", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "Points", "Points describing the outline of the room", GH_ParamAccess.list);
            int d = pManager.AddBooleanParameter("Assoc", "Assoc", "Set Room boundary associative", GH_ParamAccess.item);

            int b = pManager.AddTextParameter("Style", "Style", "Spacestyle name", GH_ParamAccess.item);
            int c = pManager.AddTextParameter("Layer", "Layer", "Layer name", GH_ParamAccess.item);
            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
            pManager[b].Optional = true;
            pManager[c].Optional = true;
            pManager[d].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Boolean assoc = new GH_Boolean(false);
            GH_String name = new GH_String();
            GH_String layer = new GH_String("");
            GH_String style = new GH_String("");
            GH_String number = new GH_String();
            List<Parameter> param = new List<Parameter>();
            List<GH_Point> points = new List<GH_Point>();
            if (!DA.GetDataList<Parameter>("Parameters", param)) param = new List<Parameter>();

            if (!DA.GetDataList<GH_Point>("Points", points)) points = new List<GH_Point>();

            DA.GetData<GH_String>("Name", ref name);
            DA.GetData<GH_Boolean>("Assoc", ref assoc);
            DA.GetData<GH_String>("Number", ref number);
            DA.GetData<GH_String>("Style", ref style);
            DA.GetData<GH_String>("Layer", ref layer);


            Room ac = new Room(name.Value,number.Value,"",param);

            IEnumerable<GH_Point> nodups = points.Distinct(new GH_PointComparer());
            ac.points = new List<Grevit.Types.Point>();
            foreach (var p in nodups)
            {
                ac.points.Add(p.ToGrevitPoint());
            }

            ac.associative = assoc.Value;
            ac.FamilyOrStyle = style.Value;
            ac.TypeOrLayer = layer.Value;
            ac.GID = this.InstanceGuid.ToString();

            DA.SetData("GrevitComponent", ac);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea4aa3d-d284-4a9f-a777-4321bfeb5b1a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.ACA_Room;
            }
        }


    }

    public class ACA_Curve : GrevitGrasshopperComponent
    {
        public ACA_Curve() : base("Grevit Autocad Curve", "Acad Curve", "Grevit Autocad Curve", "Grevit", "Components Acad") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Curve", "Curve", GH_ParamAccess.item);
            int a = pManager.AddTextParameter("Layer", "Layer", "Layer name", GH_ParamAccess.item);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Curve curve = new GH_Curve();
            DA.GetData<GH_Curve>("Curve", ref curve);
            GH_String layer = new GH_String(string.Empty);
            DA.GetData<GH_String>("Layer", ref layer);

            Component c = curve.Value.ToNurbsCurve().ToGrevitCurve();
            c.TypeOrLayer = layer.ToString();
            SetGID(c);
            DA.SetData("GrevitComponent", c);          
            
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ef1ca3d-d271-4b4f-a171-4321befa211a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.ACA_Curve;
            }
        }


    }

    public class ACA_Point : GrevitGrasshopperComponent
    {
        public ACA_Point() : base("Grevit Autocad Point", "Acad Point", "Grevit Autocad Point", "Grevit", "Components Acad") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Point", GH_ParamAccess.item);
            int a = pManager.AddTextParameter("Layer", "Layer", "Layer name", GH_ParamAccess.item);
            pManager[a].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Point curve = new GH_Point();
            DA.GetData<GH_Point>("Point", ref curve);
            GH_String layer = new GH_String(string.Empty);
            DA.GetData<GH_String>("Layer", ref layer);


            DrawingPoint arc = new DrawingPoint();
            arc.point = curve.ToGrevitPoint();
                    arc.TypeOrLayer = layer.Value;
                    arc.GID = this.InstanceGuid.ToString();
                    DA.SetData("GrevitComponent", arc);

        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ef1ca3d-d271-4b4f-a222-4321befa211a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.ACA_Point;
            }
        }


    }

    public class ACA_Slab : GrevitGrasshopperComponent
    {
        public ACA_Slab() : base("Grevit Autocad Slab", "Acad Slab", "Grevit Autocad Slab", "Grevit", "Components Acad") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface", GH_ParamAccess.item);
            //pManager.AddTextParameter("Style", "Style", "Style name", GH_ParamAccess.item);
            pManager.AddTextParameter("Layer", "Layer", "Layer name", GH_ParamAccess.item);
            pManager.AddNumberParameter("height", "height", "Slab height", GH_ParamAccess.item);
            int c = pManager.AddNumberParameter("taperAngle", "taperAngle", "Taper angle", GH_ParamAccess.item);

            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
            pManager[c].Optional = true;
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Surface surface = new GH_Surface();
            GH_String lvlbtm = new GH_String("");
            GH_String style = new GH_String("");
            GH_String layer = new GH_String("");
            GH_Number taperAng = new GH_Number(0);
            GH_Number height = new GH_Number();
            GH_Point stop = new GH_Point();
            GH_Point sbtm = new GH_Point();
            GH_Boolean structural = new GH_Boolean(true);
          
            List<Parameter> param = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", param)) param = new List<Parameter>();
            DA.GetData<GH_Surface>("Surface", ref surface);

            DA.GetData<GH_String>("Layer", ref layer);
            //DA.GetData<GH_String>("Style", ref style);
            DA.GetData<GH_Number>("taperAngle", ref taperAng);
            DA.GetData<GH_Number>("height", ref height);

            Slab s = new Slab();
            s.FamilyOrStyle = style.Value;
            s.TypeOrLayer = layer.Value;
            s.levelbottom = lvlbtm.Value;
            s.structural = structural.Value;
            s.surface = new Profile();
            s.surface.profile = new List<Loop>();
            Loop loop = new Loop() { outline = new List<Component>() };

            foreach (Rhino.Geometry.BrepEdge be in surface.Value.Edges)
            {
                loop.outline.Add(be.ToNurbsCurve().ToGrevitCurve());
            }
            s.surface.profile.Add(loop);


            s.height = height.Value;
            s.parameters = param;
            //s.top = ComponentUtilities.GHPoint2Point(stop);
            //s.bottom = ComponentUtilities.GHPoint2Point(sbtm);
            s.slope = taperAng.Value;
            s.GID = this.InstanceGuid.ToString();


            //SetPreview(s.GID, surface.Value);
            DA.SetData("GrevitComponent", s);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a777-4111beea4b1a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.ACA_Slab;
            }
        }


    }

    public class ACA_Door : GrevitGrasshopperComponent
    {
        public ACA_Door() : base("Grevit Autocad Door", "Acad Door", "Grevit Autocad Door", "Grevit", "Components Acad") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Location of the door", GH_ParamAccess.item);
            int b = pManager.AddTextParameter("Style", "Style", "Style name", GH_ParamAccess.item);
            int c = pManager.AddTextParameter("Layer", "Layer", "Layer name", GH_ParamAccess.item);
            pManager.AddGenericParameter("wall", "wall", "Host wall of the door", GH_ParamAccess.item);


            int a = pManager.AddGenericParameter("Parameters", "Param", "Parameters", GH_ParamAccess.list);
            pManager[a].Optional = true;
            pManager[b].Optional = true;
            pManager[c].Optional = true;

        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Point point = new GH_Point();
            Grevit.Types.Wall wall = null;


            GH_String layer = new GH_String("");
            GH_String style = new GH_String("");

            List<Parameter> param = new List<Parameter>();
            if (!DA.GetDataList<Parameter>("Parameters", param)) param = new List<Parameter>();

            DA.GetData<GH_Point>("Point", ref point);
            DA.GetData<GH_String>("Layer", ref layer);
            DA.GetData<GH_String>("Style", ref style);
            DA.GetData<Wall>("wall", ref wall);


            Door d = new Door();
            SetGID(d);
            d.stalledForReference = true;
            d.TypeOrLayer = layer.Value;
            d.FamilyOrStyle = style.Value;
            d.locationPoint = point.ToGrevitPoint();
            d.parameters = param;
            SetGID(d);
        
            

           Rhino.Geometry.Circle c = new Rhino.Geometry.Circle(point.Value, 0.5);
           Rhino.Geometry.Surface srf = Rhino.Geometry.NurbsSurface.CreateExtrusion(c.ToNurbsCurve(), new Rhino.Geometry.Vector3d(0,0, 2));


            
            SetPreview(d.GID, srf.ToBrep());


            GH_Surface ghb = new GH_Surface(srf);
            
            DA.SetData("GrevitComponent", d);
        }


        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5ea7ca3d-d271-4a4f-a727-4121beea4b2a}");
            }
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Properties.Resources.ACA_Door;
            }
        }


    }

    #endregion
}



