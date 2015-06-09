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
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;


namespace Grevit.Types
{
    // Set Known Types for serialization
    [DataContract]
    [XmlSerializerFormat]
    [KnownType(typeof(Component))]
    [KnownType(typeof(Adaptive))]
    [KnownType(typeof(Wall))]
    [KnownType(typeof(Slab))]
    [KnownType(typeof(Column))]
    [KnownType(typeof(Level))]
    [KnownType(typeof(TextNote))]
    [KnownType(typeof(Grid))]
    [KnownType(typeof(SpotCoordinate))]
    [KnownType(typeof(Topography))]
    [KnownType(typeof(Familyinstance))]
    [KnownType(typeof(Line))]
    [KnownType(typeof(Room))]
    [KnownType(typeof(ReferencePlane))]
    [KnownType(typeof(Hatch))]
    [KnownType(typeof(Extrusion))]
    [KnownType(typeof(Door))]
    [KnownType(typeof(WallProfileBased))]
    [KnownType(typeof(Arc))]
    [KnownType(typeof(Curve3Points))]
    [KnownType(typeof(PLine))]
    [KnownType(typeof(DrawingPoint))]
    [KnownType(typeof(ElementID))]
    [KnownType(typeof(SearchElementID))]
    [KnownType(typeof(Faces))]
    [KnownType(typeof(RevitFamilyCollection))]
    [KnownType(typeof(RevitFamily))]
    [KnownType(typeof(RevitCategory))]
    [KnownType(typeof(SimpleExtrusion))]
    [KnownType(typeof(StructuralType))]
    [KnownType(typeof(RevitLine))]
    [KnownType(typeof(Spline))]
    [KnownType(typeof(Stair))]
    [KnownType(typeof(SetCurtainPanel))]
    [KnownType(typeof(CurtainGridLine))]
    [KnownType(typeof(Surface))]
    [KnownType(typeof(Parameter))]

    /// <summary>
    /// Component Collection, container for components
    /// </summary>
    public class ComponentCollection
    {
        [DataMember]
        public List<Component> Items { get; set; }
        [DataMember]
        public bool update { get; set; }
        [DataMember]
        public bool delete { get; set; }

        public ComponentCollection()
        {
            this.Items = new List<Grevit.Types.Component>();
        }
    }

    /// <summary>
    /// Component base class
    /// </summary>
    [DataContract]
    public abstract class Component
    {
        /// <summary>
        /// Return a new Grevit GID
        /// </summary>
        /// <returns></returns>
        public static string NewGID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Reference Grevit ID as String
        /// </summary>
        [DataMember]
        public string GID { get; set; }

        /// <summary>
        /// Family Name (Revit) or Style Name (ACA)
        /// </summary>
        [DataMember]
        public string FamilyOrStyle { get; set; }

        /// <summary>
        /// Type Name (Revit) or Layer Name (ACA)
        /// </summary>
        [DataMember]
        public string TypeOrLayer { get; set; }

        /// <summary>
        /// Create this Component last because of dependencies.
        /// </summary>
        [DataMember]
        public bool stalledForReference { get; set; }

        /// <summary>
        /// General Parameter (Revit) or Property (ACA) List 
        /// </summary>
        [DataMember]
        public List<Parameter> parameters { get; set; }

        /// <summary>
        /// ToString override for debugging in Grasshoppper
        /// </summary>
        public override string ToString()
        {
            string data = this.GID + " ";
            if (this.FamilyOrStyle != null) data += "(" + this.FamilyOrStyle + ") ";
            if (this.TypeOrLayer != null) data += "(" + this.TypeOrLayer + ")";
            data += "\n";
            return data;
        }


    }

    /// <summary>
    /// Wall component curve based
    /// </summary>
    [DataContract]
    public class Wall : Component
    {
        [DataMember]
        public Component curve { get; set; }
        [DataMember]
        public string levelbottom { get; set; }
        [DataMember]
        public double height { get; set; }
        [DataMember]
        public bool join { get; set; }
        [DataMember]
        public bool flip { get; set; }

        public Wall(string familyOrStyle, string typeOrLayer, List<Parameter> parameters, Component curve, string levelbottom, double height, bool join, bool flip)
        {
            this.FamilyOrStyle = familyOrStyle;
            this.TypeOrLayer = typeOrLayer;
            this.parameters = parameters;
            this.curve = curve;
            this.levelbottom = levelbottom;
            this.height = height;
            this.join = join;
            this.flip = flip;
        }
    }

    /// <summary>
    /// Extruded polyline
    /// </summary>
    [DataContract]
    public class SimpleExtrusion : Component
    {
        [DataMember]
        public PLine polyline { get; set; }
        [DataMember]
        public Point vector { get; set; }

        public SimpleExtrusion(string familyOrStyle, string typeOrLayer, List<Parameter> parameters, PLine outline, Point extrusionVector)
        {
            this.FamilyOrStyle = familyOrStyle;
            this.TypeOrLayer = typeOrLayer;
            this.parameters = parameters;
            this.polyline = outline;
            this.vector = extrusionVector;
        }
    }

    /// <summary>
    /// Face component
    /// </summary>
    [DataContract]
    public class Faces : Component
    {
        [DataMember]
        public List<Face> faces { get; set; }
    }

    /// <summary>
    /// Profile based wall
    /// </summary>
    [DataContract]
    public class WallProfileBased : Component
    {
        [DataMember]
        public List<Component> curves { get; set; }
        [DataMember]
        public string level { get; set; }

        public WallProfileBased(string familyOrStyle, string typeOrLayer, List<Parameter> parameters, List<Component> profile, string levelName)
        {
            this.FamilyOrStyle = familyOrStyle;
            this.TypeOrLayer = typeOrLayer;
            this.parameters = parameters;
            this.curves = profile;
            this.level = levelName;
        }
    }

    /// <summary>
    /// Outline based hatch
    /// </summary>
    [DataContract]
    public class Hatch : Component
    {
        [DataMember]
        public List<Point> outline { get; set; }
        [DataMember]
        public string view { get; set; }
        [DataMember]
        public string pattern { get; set; }

        public Hatch(List<Parameter> parameters, List<Point> outline, string viewName, string patternName)
        {
            this.parameters = parameters;
            this.outline = outline;
            this.view = viewName;
            this.pattern = patternName;
        }
    }



    /// <summary>
    /// Slab Component
    /// </summary>
    [DataContract]
    public class Slab : Component
    {
        [DataMember]
        public Surface surface { get; set; }
        [DataMember]
        public string levelbottom { get; set; }
        [DataMember]
        public Point bottom { get; set; }
        [DataMember]
        public Point top { get; set; }
        [DataMember]
        public double slope { get; set; }
        [DataMember]
        public double height { get; set; }
        [DataMember]
        public bool structural { get; set; }


    }

    /// <summary>
    /// Surface, outline based
    /// </summary>
    [DataContract]
    public class Surface : Component
    {
        [DataMember]
        public List<Component> outline { get; set; }
    }

    /// <summary>
    /// General Extrusion with outline height and slanting value
    /// </summary>
    [DataContract]
    public class Extrusion : Component
    {
        [DataMember]
        public List<Point> outline { get; set; }
        [DataMember]
        public double height { get; set; }
        [DataMember]
        public double slanted { get; set; }

        public Extrusion(string familyOrStyle, string typeOrLayer, List<Parameter> parameters, List<Point> profile, double height, double slanted)
        {
            this.FamilyOrStyle = familyOrStyle;
            this.TypeOrLayer = typeOrLayer;
            this.parameters = parameters;
            this.outline = profile;
            this.height = height;
            this.slanted = slanted;
        }
    }

    /// <summary>
    /// Column with top and base point
    /// </summary>
    [DataContract]
    public class Column : Component
    {
        [DataMember]
        public Point location { get; set; }
        [DataMember]
        public string levelbottom { get; set; }
        [DataMember]
        public Point locationTop { get; set; }
        [DataMember]
        public bool structural { get; set; }

        public Column(string familyOrStyle, string typeOrLayer, List<Parameter> parameters, Point bottomPoint, Point topPoint, string levelName, bool isStructural)
        {
            this.FamilyOrStyle = familyOrStyle;
            this.TypeOrLayer = typeOrLayer;
            this.parameters = parameters;
            this.location = bottomPoint;
            this.locationTop = topPoint;
            this.levelbottom = levelName;
            this.structural = isStructural;
        }
    }

    /// <summary>
    /// General Parameter (Revit) or Property (ACA) 
    /// </summary>
    [DataContract]
    public class Parameter
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public object value { get; set; }

        public override string ToString()
        {
            return this.name + "= " + this.value.ToString() + "\n";
        }

        public Parameter(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// Adaptive Component (Point based)
    /// </summary>
    [DataContract]
    public class Adaptive : Component
    {
        [DataMember]
        public Dictionary<int, Point> points { get; set; }


    }

    /// <summary>
    /// Point based Family
    /// </summary>
    [DataContract]
    public class Familyinstance : Component
    {
        [DataMember]
        public List<Point> points { get; set; }
        [DataMember]
        public string view { get; set; }
        [DataMember]
        public string level { get; set; }
        [DataMember]
        public StructuralType structuralType { get; set; }
        [DataMember]
        public string referenceGID { get; set; }
    }

    /// <summary>
    /// Topography consiting of a list of points
    /// </summary>
    [DataContract]
    public class Topography : Component
    {
        [DataMember]
        public List<Point> points { get; set; }
    }

    /// <summary>
    /// CurtainGridLine for curtain walls
    /// </summary>
    [DataContract]
    public class CurtainGridLine : Component
    {
        [DataMember]
        public Point point { get; set; }
        [DataMember]
        public string referenceGID { get; set; }
        [DataMember]
        public bool horizontal { get; set; }
        [DataMember]
        public bool single { get; set; }
    }

    /// <summary>
    /// Curtain Panel control component
    /// </summary>
    [DataContract]
    public class SetCurtainPanel : Component
    {
        [DataMember]
        public int panelID { get; set; }
        [DataMember]
        public string panelType { get; set; }
        [DataMember]
        public string verticalGID { get; set; }
        [DataMember]
        public string horizontalGID { get; set; }
    }

    /// <summary>
    /// Linear Gridline based on two reference points.
    /// </summary>
    [DataContract]
    public class Grid : Component
    {
        [DataMember]
        public Point from { get; set; }
        [DataMember]
        public Point to { get; set; }
        [DataMember]
        public string Name { get; set; }

        public Grid(List<Parameter> parameters, Point from, Point to, string name)
        {
            this.parameters = parameters;
            this.from = from;
            this.to = to;
            this.Name = name;
        }
    }

    /// <summary>
    /// Spot coordinate component (RVT)
    /// </summary>
    [DataContract]
    public class SpotCoordinate : Component
    {
        [DataMember]
        public Point locationPoint { get; set; }
        [DataMember]
        public string referenceGID { get; set; }
        [DataMember]
        public string view { get; set; }
        [DataMember]
        public Point bendPoint { get; set; }
        [DataMember]
        public Point endPoint { get; set; }
        [DataMember]
        public Point refPoint { get; set; }
        [DataMember]
        public bool hasLeader { get; set; }
        [DataMember]
        public bool isElevation { get; set; }
    }

    /// <summary>
    /// Door Component for ACA
    /// </summary>
    [DataContract]
    public class Door : Component
    {
        [DataMember]
        public Point locationPoint { get; set; }
        [DataMember]
        public string referenceGID { get; set; }
    }

    /// <summary>
    /// Simple Line with from and to locations 
    /// </summary>
    [DataContract]
    public class Line : Component
    {
        [DataMember]
        public Point from { get; set; }
        [DataMember]
        public Point to { get; set; }
        [DataMember]
        public string view { get; set; }
    }

    [DataContract]
    public class Stair : Component
    {
        [DataMember]
        public Component front { get; set; }
        [DataMember]
        public Component back { get; set; }
        [DataMember]
        public Component left { get; set; }
        [DataMember]
        public Component right { get; set; }
    }

    /// <summary>
    /// Arc with center point, start, end and radius value
    /// </summary>
    [DataContract]
    public class Arc : Component
    {
        [DataMember]
        public Point center { get; set; }
        [DataMember]
        public double start { get; set; }
        [DataMember]
        public double end { get; set; }
        [DataMember]
        public double radius { get; set; }
    }



    /// <summary>
    /// Textnote Component
    /// </summary>
    [DataContract]
    public class TextNote : Component
    {
        [DataMember]
        public Point location { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public string view { get; set; }
    }

    /// <summary>
    /// Polyline with a list of reference points
    /// </summary>
    [DataContract]
    public class PLine : Component
    {
        [DataMember]
        public List<Point> points { get; set; }
        [DataMember]
        public bool closed { get; set; }
    }

    /// <summary>
    /// Spline from control points
    /// </summary>
    [DataContract]
    public class Spline : Component
    {
        [DataMember]
        public List<Point> controlPoints { get; set; }
        [DataMember]
        public List<double> weights { get; set; }
        [DataMember]
        public List<double> knots { get; set; }
        [DataMember]
        public int degree { get; set; }
        [DataMember]
        public bool isPeriodic { get; set; }
        [DataMember]
        public bool isClosed { get; set; }
        [DataMember]
        public bool isRational { get; set; }
    }

    /// <summary>
    /// Curve with 3 reference points
    /// </summary>
    [DataContract]
    public class Curve3Points : Component
    {
        [DataMember]
        public Point a { get; set; }
        [DataMember]
        public Point b { get; set; }
        [DataMember]
        public Point c { get; set; }
    }

    /// <summary>
    /// Model point pointing to a reference point
    /// </summary>
    [DataContract]
    public class DrawingPoint : Component
    {
        [DataMember]
        public Point point { get; set; }
    }

    /// <summary>
    /// Simple reference point with x,y,z and referenceID
    /// </summary>
    [DataContract]
    public class Point
    {
        [DataMember]
        public double x { get; set; }
        [DataMember]
        public double y { get; set; }
        [DataMember]
        public double z { get; set; }
        [DataMember]
        public string id { get; set; }

        public Point(double X, double Y, double Z)
        {
            this.x = X; this.y = Y; this.z = Z; this.id = "";
        }

        public Point() { }
    }

    /// <summary>
    /// Simple face
    /// </summary>
    [DataContract]
    public class Face
    {
        [DataMember]
        public Point A { get; set; }
        [DataMember]
        public Point B { get; set; }
        [DataMember]
        public Point C { get; set; }
        [DataMember]
        public Point D { get; set; }
        [DataMember]
        public bool isTriangle { get; set; }
    }

    /// <summary>
    /// Family Container
    /// </summary>
    [DataContract]
    public class RevitFamilyCollection
    {
        [DataMember]
        public List<RevitCategory> Categories;
    }

    /// <summary>
    /// Revit Family Identifier
    /// </summary>
    [DataContract]
    public class RevitFamily
    {
        [DataMember]
        public string Name;
        [DataMember]
        public List<string> Types;

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Revit Category Identifier
    /// </summary>
    [DataContract]
    public class RevitCategory
    {
        [DataMember]
        public string Name;
        [DataMember]
        public List<RevitFamily> Families;

        public override string ToString()
        {
            return Name;
        }
    }


    /// <summary>
    /// Reference Plane component
    /// </summary>
    [DataContract]
    public class ReferencePlane : Component
    {
        [DataMember]
        public Point EndB { get; set; }
        [DataMember]
        public Point EndA { get; set; }
        [DataMember]
        public Point cutVector { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string View { get; set; }
    }

    /// <summary>
    /// Revit Line component
    /// </summary>
    [DataContract]
    public class RevitLine : Component
    {
        [DataMember]
        public Component curve { get; set; }
        [DataMember]
        public bool isDetailCurve { get; set; }
        [DataMember]
        public bool isModelCurve { get; set; }
        [DataMember]
        public bool isRoomBounding { get; set; }
    }

    /// <summary>
    /// Level Component with Name and Height.
    /// </summary>
    [DataContract]
    public class Level : Component
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public double height { get; set; }
        [DataMember]
        public bool addView { get; set; }

        public Level(string name, double height, bool addView)
        {
            this.name = name;
            this.height = height;
            this.addView = addView;
        }
    }

    /// <summary>
    /// Room component with location Points (ACA only) and associative flag (ACA)
    /// In Revit the Room will be created without geometry, it will appear as an unplaced room.
    /// </summary>
    [DataContract]
    public class Room : Component
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string number { get; set; }
        [DataMember]
        public string phase { get; set; }
        [DataMember]
        public bool associative { get; set; }
        [DataMember]
        public List<Point> points { get; set; }

        public Room(string name, string number, string phase, List<Parameter> parameters) 
        {
            this.name = name;
            this.number = number;
            this.phase = phase;
            this.parameters = parameters;

        }
    }

    /// <summary>
    /// Element ID as int
    /// </summary>
    [DataContract]
    public class ElementID
    {
        [DataMember]
        public int ID { get; set; }
    }

    /// <summary>
    /// Search for ElementID by name
    /// </summary>
    [DataContract]
    public class SearchElementID
    {
        [DataMember]
        public string Name { get; set; }
    }

    /// <summary>
    /// Structural types
    /// </summary>
    [DataContract]
    public enum StructuralType
    {
        Beam,
        Brace,
        Column,
        Footing,
        NonStructural,
        UnknownFraming
    }
}

