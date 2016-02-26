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
    /// Grevit Utility Methods
    /// </summary>
    public static class Utilities
    {

        /// <summary>
        /// Translate to Revit Rule
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="parameterId"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.FilterRule ToRevitRule(this Grevit.Types.Rule rule, ElementId parameterId)
        {
            string methodname = "Create" + rule.equalityComparer + "Rule";


            if (
                rule.equalityComparer == "Equals" ||
                rule.equalityComparer == "NotEquals" ||
                rule.equalityComparer == "Greater" ||
                rule.equalityComparer == "Less" ||
                rule.equalityComparer == "GreaterOrEqual" ||
                rule.equalityComparer == "LessOrEqual"
                )
            {
                if (rule.value.GetType() == typeof(int))
                {
                    System.Reflection.MethodInfo methodInfo = typeof(ParameterFilterRuleFactory).GetMethod(methodname, new[] { typeof(ElementId), typeof(int) });
                    if (methodInfo != null)
                        return (FilterRule)methodInfo.Invoke(null, new object[] { parameterId, (int)rule.value });
                }
                else if (rule.value.GetType() == typeof(double))
                {
                    System.Reflection.MethodInfo methodInfo = typeof(ParameterFilterRuleFactory).GetMethod(methodname, new[] { typeof(ElementId), typeof(double), typeof(double) });
                    if (methodInfo != null)
                        return (FilterRule)methodInfo.Invoke(null, new object[] { parameterId, (double)rule.value, 0 });
                }
                else if (rule.value.GetType() == typeof(string))
                {
                    System.Reflection.MethodInfo methodInfo = typeof(ParameterFilterRuleFactory).GetMethod(methodname, new[] { typeof(ElementId), typeof(string), typeof(bool) });
                    if (methodInfo != null)
                        return (FilterRule)methodInfo.Invoke(null, new object[] { parameterId, (string)rule.value, true });
                }
            }
            else
            {
                if (rule.value.GetType() == typeof(string))
                {
                    System.Reflection.MethodInfo methodInfo = typeof(ParameterFilterRuleFactory).GetMethod(methodname, new[] { typeof(ElementId), typeof(string), typeof(bool) });
                    if (methodInfo != null)
                        return (FilterRule)methodInfo.Invoke(null, new object[] { parameterId, (string)rule.value, true });
                }
            }
            

            return null;
        }

        /// <summary>
        /// Get Revit Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.Color ToRevitColor(this Grevit.Types.Color color)
        {
            return new Autodesk.Revit.DB.Color((byte)color.R, (byte)color.G, (byte)color.B);
        }


        /// <summary>
        /// Get Revit Families from the active Document
        /// </summary>
        public static RevitFamilyCollection GetFamilies(this Document document)
        {
            // create a new family collection
            Grevit.Types.RevitFamilyCollection familyCollection = new Grevit.Types.RevitFamilyCollection();
            
            // set up a new category List
            familyCollection.Categories = new List<Grevit.Types.RevitCategory>();

            // Get all WallTypes and add them to the system family category
            #region WallTypes
            
            FilteredElementCollector collector = new FilteredElementCollector(document).OfClass(typeof(WallType));
            
            RevitCategory systemFamilies = new RevitCategory()
            {
                Name = "System Families",
                Families = new List<RevitFamily>()
            };

            RevitFamily wallTypes = new RevitFamily()
            {
                Name = "Walls",
                Types = new List<string>()
            };

            foreach (WallType wallType in collector.ToElements())
                wallTypes.Types.Add(wallType.Name);

            systemFamilies.Families.Add(wallTypes);
            familyCollection.Categories.Add(systemFamilies);

            #endregion

            // Get all Family Instances and add them to the Grevit Family collection 
            #region familyInstances

            FilteredElementCollector familyInstanceCollector = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol));

            foreach (Autodesk.Revit.DB.FamilySymbol familySymbol in familyInstanceCollector.ToElements())
            {
                RevitFamily family = null;
                RevitCategory category = null;

                // Check if category already exists
                foreach (RevitCategory revitCategory in familyCollection.Categories)
                    if (revitCategory.Name == familySymbol.Category.Name) category = revitCategory;

                // Otherwise create a new category
                if (category == null)
                {
                    category = new RevitCategory()
                    { 
                        Name = familySymbol.Category.Name,
                        Families = new List<RevitFamily>()
                    };

                    familyCollection.Categories.Add(category);
                }

                // Check if Family already exists
                foreach (RevitFamily revitFamily in category.Families)
                    if (revitFamily.Name == familySymbol.FamilyName) family = revitFamily;

                // Otherwise create a new one
                if (family == null)
                {
                    family = new RevitFamily()
                    {
                        Name = familySymbol.FamilyName,
                        Types = new List<string>()
                    };

                    category.Families.Add(family);
                }

                family.Types.Add(familySymbol.Name);

            }

            #endregion

            return familyCollection;

        }

        /// <summary>
        /// Converts a List of Grevit Curves to Revit Curves
        /// </summary>
        /// <param name="document">Active Document</param>
        /// <param name="grevitCurves">List of Grevit Curves</param>
        /// <returns>List of Revit Curves</returns>
        public static List<Curve> GrevitCurvesToRevitCurves(Component component, CoordinatesOverride coords = null)
        {
            List<Curve> curvesOut = new List<Curve>();


            if (component.GetType() == typeof(Grevit.Types.Line))
            {
                Grevit.Types.Line line = (Grevit.Types.Line)component;
                curvesOut.Add(Autodesk.Revit.DB.Line.CreateBound(line.from.ToXYZ(coords), line.to.ToXYZ(coords)));
            }
            else if (component.GetType() == typeof(Grevit.Types.Arc))
            {
                Grevit.Types.Arc arc = (Grevit.Types.Arc)component;
                curvesOut.Add(Autodesk.Revit.DB.Arc.Create(arc.center.ToXYZ(coords), arc.radius, arc.start, arc.end, XYZ.BasisX, XYZ.BasisY));
            }
            else if (component.GetType() == typeof(Grevit.Types.Curve3Points))
            {
                Grevit.Types.Curve3Points curve3points = (Grevit.Types.Curve3Points)component;
                curvesOut.Add(Autodesk.Revit.DB.Arc.Create(curve3points.a.ToXYZ(coords), curve3points.c.ToXYZ(coords), curve3points.b.ToXYZ(coords)));
            }
            else if (component.GetType() == typeof(Grevit.Types.PLine))
            {
                Grevit.Types.PLine pline = (Grevit.Types.PLine)component;

                for (int i = 0; i < pline.points.Count - 1; i++)
                {
                    curvesOut.Add(Autodesk.Revit.DB.Line.CreateBound(pline.points[i].ToXYZ(coords), pline.points[i + 1].ToXYZ(coords)));
                }
            }
            else if (component.GetType() == typeof(Spline))
            {
                Spline spline = (Spline)component;
                IList<XYZ> points = new List<XYZ>();
                foreach (Grevit.Types.Point point in spline.controlPoints) points.Add(point.ToXYZ(coords));
                NurbSpline ns = NurbSpline.Create(points, spline.weights);
                ns.isClosed = spline.isClosed;
                curvesOut.Add(ns);
            }


            return curvesOut;

        }

        /// <summary>
        /// Creates a Reference Curve based on two Points
        /// </summary>
        /// <param name="doc">Active Document</param>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <returns>Reference Line</returns>
        public static Reference CurveFromXYZ(Document doc, XYZ point1, XYZ point2)
        {
            ReferencePoint referencePoint1 = doc.FamilyCreate.NewReferencePoint(point1);
            ReferencePoint referencePoint2 = doc.FamilyCreate.NewReferencePoint(point2);

            ReferencePointArray referencePoints = new ReferencePointArray();
            referencePoints.Append(referencePoint1);
            referencePoints.Append(referencePoint2);

            CurveByPoints curve = doc.FamilyCreate.NewCurveByPoints(referencePoints);

            return curve.GeometryCurve.Reference;
        }

        /// <summary>
        /// No Comment...
        /// </summary>
        const double _inch = 1.0 / 12.0;

        /// <summary>
        /// Also no Comment...
        /// </summary>
        const double _sixteenth = _inch / 16.0;

        static Curve CreateReversedCurve(Autodesk.Revit.Creation.Application creapp, Curve orig)
        {

            if (orig is Autodesk.Revit.DB.Line)
            {
                return Autodesk.Revit.DB.Line.CreateBound(
                  orig.GetEndPoint(1),
                  orig.GetEndPoint(0));
            }
            else if (orig is Autodesk.Revit.DB.Arc)
            {
                return Autodesk.Revit.DB.Arc.Create(orig.GetEndPoint(1),
                  orig.GetEndPoint(0),
                  orig.Evaluate(0.5, true));
            }
            else
            {
                throw new Exception(
                  "CreateReversedCurve - Unreachable");
            }
        }

        /// <summary>
        /// Sort a list of curves to make them correctly 
        /// ordered and oriented to form a closed loop.
        /// 
        /// This one comes directly from Jeremy Tamiks Revit API Blog.
        /// </summary>
        public static void SortCurvesContiguous(Autodesk.Revit.Creation.Application creapp, IList<Curve> curves)
        {
            int n = curves.Count;

            // Walk through each curve (after the first) 
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                Curve curve = curves[i];
                XYZ endPoint = curve.GetEndPoint(1);

                XYZ p;

                bool found = (i + 1 >= n);

                for (int j = i + 1; j < n; ++j)
                {
                    p = curves[j].GetEndPoint(0);

                    if (_sixteenth > p.DistanceTo(endPoint))
                    {


                        if (i + 1 != j)
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }

                    p = curves[j].GetEndPoint(1);

                    // If there is a match end->end, 
                    // reverse the next curve

                    if (_sixteenth > p.DistanceTo(endPoint))
                    {
                        if (i + 1 == j)
                        {


                            curves[i + 1] = CreateReversedCurve(
                              creapp, curves[j]);
                        }
                        else
                        {


                            Curve tmp = curves[i + 1];
                            curves[i + 1] = CreateReversedCurve(
                              creapp, curves[j]);
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("SortCurvesContiguous:"
                      + " non-contiguous input curves");
                }
            }
        }

        /// <summary>
        /// Activate Visibility of Masses
        /// </summary>
        /// <param name="doc"></param>
        public static void SetMassVisibilityOn(this Document doc)
        {
            // Get Mass Category
            Category category = doc.Settings.Categories.get_Item("Mass");

            // Set the Visibility for this Category true
            category.set_Visible(doc.ActiveView, true);
        }

        /// <summary>
        /// Creates a new Sketch Plane from a Curve
        /// </summary>
        /// <param name="document">Active Document</param>
        /// <param name="curve">Curve to get plane from</param>
        /// <returns>Plane of the curve</returns>
        public static SketchPlane NewSketchPlaneFromCurve(Document document, Autodesk.Revit.DB.Curve curve)
        {
            XYZ startPoint = curve.GetEndPoint(0);
            XYZ endPoint = curve.GetEndPoint(1);
            
            // If Start end Endpoint are the same check further points.
            int i = 2;
            while (startPoint == endPoint && endPoint != null)
            {
                endPoint = curve.GetEndPoint(i);
                i++;
            }




            // Plane to return
            Plane plane;


            // If Z Values are equal the Plane is XY
            if (startPoint.Z == endPoint.Z)
            {
                plane = document.Application.Create.NewPlane(XYZ.BasisZ, startPoint);
            }
            // If X Values are equal the Plane is YZ
            else if (startPoint.X == endPoint.X)
            {
                plane = document.Application.Create.NewPlane(XYZ.BasisX, startPoint);
            }
            // If Y Values are equal the Plane is XZ
            else if (startPoint.Y == endPoint.Y)
            {
                plane = document.Application.Create.NewPlane(XYZ.BasisY, startPoint);
            }
            // Otherwise the Planes Normal Vector is not X,Y or Z.
            // We draw lines from the Origin to each Point and use the Plane this one spans up.
            else
            {
                CurveArray curves = new CurveArray();
                curves.Append(curve);
                curves.Append(Autodesk.Revit.DB.Line.CreateBound(new XYZ(0, 0, 0), startPoint));
                curves.Append(Autodesk.Revit.DB.Line.CreateBound(endPoint, new XYZ(0, 0, 0)));
                plane = document.Application.Create.NewPlane(curves);
            }


            // return new Sketchplane
            return SketchPlane.Create(document, plane);
        }

        /// <summary>
        /// Returns a new Revit XYZ Point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static XYZ ToXYZ(this Grevit.Types.Point p)
        {
            return new XYZ(p.x * GrevitBuildModel.Scale, p.y * GrevitBuildModel.Scale, p.z * GrevitBuildModel.Scale);
        }

        public static XYZ ToXYZ(this Grevit.Types.Point p, CoordinatesOverride coordoverride)
        {
            if (coordoverride != null)
                return coordoverride.ApplyOverride(p);
            else
                return new XYZ(p.x * GrevitBuildModel.Scale, p.y * GrevitBuildModel.Scale, p.z * GrevitBuildModel.Scale);
        }


        /// <summary>
        /// Gets an Element by Class and Name
        /// </summary>
        /// <param name="document"></param>
        /// <param name="type">Element Class</param>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public static Element GetElementByName(this Document document, Type type, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document).OfClass(type);

            foreach (Element e in collector.ToElements())
            
                if (e.Name == name) return e;
            

            return collector.FirstElement();
        }


        public static Autodesk.Revit.DB.Level GetLevelByName(this Document document, string name, double elevation)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Level));
            
            Autodesk.Revit.DB.Level result = (Autodesk.Revit.DB.Level)collector.FirstElement();
            
            List<Autodesk.Revit.DB.Level> levels = new List<Autodesk.Revit.DB.Level>();

            foreach (Autodesk.Revit.DB.Level e in collector.ToElements())
            {
                if (e.Name == name) return e;
                levels.Add(e);
            }

            List<Autodesk.Revit.DB.Level> ordered = levels.OrderBy(e => e.Elevation).ToList();

            for (int i = 0; i < ordered.Count(); i++)
            {
                if (i < ordered.Count - 1)
                {
                    if (ordered[i].Elevation <= elevation && ordered[i + 1].Elevation > elevation)
                        result = ordered[i];
                }
                else
                    result = ordered[i];
            }


            return result;
        }

        /// <summary>
        /// Gets and Element by its Name only
        /// </summary>
        /// <param name="document"></param>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public static Element GetElementByName(this Document document, string name)
        {
            FilteredElementCollector collectorNotViewBased = new FilteredElementCollector(document).WhereElementIsElementType().OwnedByView(ElementId.InvalidElementId);

            foreach (Element e in collectorNotViewBased.ToElements())
            
                if (e.Name == name) return e;
            
            
            FilteredElementCollector collectorViewBased = new FilteredElementCollector(document).WhereElementIsNotElementType().OwnedByView(ElementId.InvalidElementId);
            
            foreach (Element e in collectorViewBased.ToElements())
            
                if (e.Name == name) return e;
            

            return null;
        }

        /// <summary>
        /// Gets a Family by its Type, Family name and Type Name
        /// </summary>
        /// <param name="document"></param>
        /// <param name="type">Element Class</param>
        /// <param name="family">Family Name</param>
        /// <param name="familyType">Type Name</param>
        /// <returns></returns>
        public static Element GetElementByName(this Document document, Type type, string family, string familyType, out bool found, BuiltInCategory category = BuiltInCategory.INVALID)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document).OfClass(type);

            if (category != BuiltInCategory.INVALID) collector.OfCategory(category);

            foreach (Element e in collector.ToElements())
            {
                string familyName = "";
                string typeName = "";

                if (e.GetType() == typeof(FamilyInstance))
                {
                    FamilyInstance fi = (FamilyInstance)e;
                    familyName = fi.Symbol.Family.Name;
                    typeName = fi.Symbol.Name;
                }
                else if (e.GetType() == typeof(FamilySymbol))
                {
                    FamilySymbol fs = (FamilySymbol)e;
                    familyName = fs.Family.Name;
                    typeName = fs.Name;
                }

                if (familyName == family && typeName == familyType) { found = true;  return e; }


            }

            found = false;
            return collector.FirstElement();
        }

        /// <summary>
        /// Gets a Family by its Category, Family name and Type Name
        /// </summary>
        /// <param name="document"></param>
        /// <param name="category">Category</param>
        /// <param name="family">Family Name</param>
        /// <param name="type">Type Name</param>
        /// <returns></returns>
        public static Element GetElementByName(this Document document, BuiltInCategory category, string family, string type, out bool found)
        {
            
            FilteredElementCollector collector = new FilteredElementCollector(document).OfCategory(category);
            foreach (Element e in collector.ToElements())
            {
                string familyName = "";
                string typeName = "";

                if (e.GetType() == typeof(FamilyInstance))
                {
                    FamilyInstance fi = (FamilyInstance)e;
                    familyName = fi.Symbol.Family.Name;
                    typeName = fi.Symbol.Name;
                }
                else if (e.GetType() == typeof(FamilySymbol))
                {
                    FamilySymbol fs = (FamilySymbol)e;
                    familyName = fs.Family.Name;
                    typeName = fs.Name;
                }

                if (familyName == family && typeName == type) { found = true; return e; }
            }

            found = false;
            return collector.FirstElement();
        }      

        /// <summary>
        /// Add Shared GID Parameter to active Document
        /// </summary>
        /// <param name="document"></param>
        /// <returns>returns if operation succeeded</returns>
        public static bool GrevitAddSharedParameter(this Document document)
        {
            try
            {
                string parameterName = "GID";

                ParameterType parameterType = ParameterType.Text;

                string sharedParameterFile = document.Application.SharedParametersFilename;

                string tempSharedParameterFile = System.IO.Path.GetTempFileName() + ".txt";
                using (System.IO.File.Create(tempSharedParameterFile)) { }

                document.Application.SharedParametersFilename = tempSharedParameterFile;

                // Create a new Category Set to apply the parameter to
                CategorySet categories = document.Application.Create.NewCategorySet();

                // Walk thru all categories and add them if they allow bound parameters
                foreach(Category cat in document.Settings.Categories)
                    if (cat.AllowsBoundParameters) categories.Insert(cat);
                
                // Create and External Definition in a Group called Grevit and a Parameter called GID
#if (Revit2016)
                ExternalDefinition def = document.Application.OpenSharedParameterFile().Groups.Create("Grevit").Definitions.Create(new ExternalDefinitionCreationOptions(parameterName, parameterType)) as ExternalDefinition;
#else
                ExternalDefinition def = document.Application.OpenSharedParameterFile().Groups.Create("Grevit").Definitions.Create(new ExternalDefinitonCreationOptions(parameterName, parameterType)) as ExternalDefinition;
#endif

                document.Application.SharedParametersFilename = sharedParameterFile;
                System.IO.File.Delete(tempSharedParameterFile);

                // Apply the Binding to almost all Categories
                Autodesk.Revit.DB.Binding bin = document.Application.Create.NewInstanceBinding(categories);
                BindingMap map = (new UIApplication(document.Application)).ActiveUIDocument.Document.ParameterBindings;
                map.Insert(def, bin,BuiltInParameterGroup.PG_DATA);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Cheacks if a Definition Group contains a Parameter Definition
        /// </summary>
        /// <param name="group"></param>
        /// <param name="parameterName">Name of the Parameter (case insensitive)</param>
        /// <returns></returns>
        public static bool ContainsDefinition(this DefinitionGroup group, string parameterName)
        {
            // Case Insensitive
            parameterName = parameterName.ToLower();

            // Check all definitions for this name
            foreach (Definition parameterDefinition in group.Definitions)
                if (string.Equals(parameterDefinition.Name.ToLower(), parameterName))
                    return true;

            return false;
        }

        /// <summary>
        /// Returns a Revit Structural Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.Structure.StructuralType ToRevitStructuralType(this Grevit.Types.StructuralType type)
        {
            switch (type)
            {
                case StructuralType.Beam: return Autodesk.Revit.DB.Structure.StructuralType.Beam;
                case StructuralType.Brace: return Autodesk.Revit.DB.Structure.StructuralType.Brace;
                case StructuralType.Column: return Autodesk.Revit.DB.Structure.StructuralType.Column;
                case StructuralType.Footing: return Autodesk.Revit.DB.Structure.StructuralType.Footing;
                case StructuralType.UnknownFraming: return Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming;
                default: return Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
            }
        }

        /// <summary>
        /// Gets all Elements from a List of Classes
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="classes">List of Classes</param>
        /// <returns>List of Elements</returns>
        public static List<Element> GetElementsFromClasses(this Document doc, List<Type> classes)
        {
            // Filtered Element List
            List<Element> myfilter = new List<Element>();

            // Get All Elements for each Class and add them to the List
            foreach (Type typ in classes)
            {
                FilteredElementCollector fc = new FilteredElementCollector(doc).OfClass(typ);
                foreach (Element em in fc.ToElements()) myfilter.Add(em);
            }

            return myfilter;
        }

        /// <summary>
        /// Checks if a Point is part of a List of points (by Coordinates)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="points">List to search in</param>
        /// <returns></returns>
        public static bool isInList(this XYZ point, List<XYZ> points)
        {
            foreach (XYZ pkt in points)
            {
                if (pkt.IsAlmostEqualTo(point))  return true; 
            }

            return false;
        }


        /// <summary>
        /// Get Existing Grevit Elements from Document using the GID
        /// </summary>
        /// <param name="document"></param>
        /// <param name="update">Grevit Client Update Value</param>
        /// <returns></returns>
        public static Dictionary<string, ElementId> GetExistingGrevitElements(this Document document, bool update)
        {
            if (!update) return new Dictionary<string, ElementId>();

            Dictionary<string, ElementId> elems = new Dictionary<string, ElementId>();

            List<Type> types = new List<Type>();
            types.Add(typeof(Autodesk.Revit.DB.Wall));
            types.Add(typeof(Autodesk.Revit.DB.FamilyInstance));

            List<Element> myfilter = document.GetElementsFromClasses(types);

            List<string> banned = new List<string>();

            foreach (Element em in myfilter)
            {
                Autodesk.Revit.DB.Parameter p = em.LookupParameter("GID");
                if (p != null && p.AsString() != null && p.AsString() != string.Empty && em.Id != ElementId.InvalidElementId)
                {
                    if (!banned.Contains(p.AsString()))
                    {
                        if (!elems.ContainsKey(p.AsString())) elems.Add(p.AsString(), em.Id);
                        else { elems.Remove(p.AsString()); banned.Add(p.AsString()); }
                    }
                }
            }

            return elems;
        }

    }



    public class CoordinatesOverride
    {
        public double Value;
        public Coordinate Coordinate;

        public static CoordinatesOverride OverrideX(double value) { return new CoordinatesOverride() { Value = value, Coordinate = Revit.Coordinate.X }; }
        public static CoordinatesOverride OverrideY(double value) { return new CoordinatesOverride() { Value = value, Coordinate = Revit.Coordinate.Y }; }
        public static CoordinatesOverride OverrideZ(double value) { return new CoordinatesOverride() { Value = value, Coordinate = Revit.Coordinate.Z }; }

        public XYZ ApplyOverride(Grevit.Types.Point point)
        {
            switch (Coordinate)
            {
                case Revit.Coordinate.X:
                    return new XYZ(Value, point.y, point.z);
                case Revit.Coordinate.Y:
                    return new XYZ(point.x, Value, point.z);
                case Revit.Coordinate.Z:
                    return new XYZ(point.x, point.y, Value);
                default:
                    return new XYZ(point.x, point.y, point.z);
            }

        }
    }

    public enum Coordinate
    { 
        X,
        Y,
        Z
    }
}
