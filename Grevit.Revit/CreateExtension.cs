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
using Grevit.Revit;

namespace Grevit.Revit
{
    /// <summary>
    /// Extends all Grevit Elements by Revit specific Create Methods
    /// </summary>
    public static class CreateExtension
    {

        public static Element Create(this Grevit.Types.Filter filter)
        {
            List<ElementId> categories = new List<ElementId>();

            Dictionary<string,ElementId> parameters = new Dictionary<string,ElementId>();

            foreach (Category category in GrevitBuildModel.document.Settings.Categories)
            {
                if (filter.categories.Contains(category.Name) || filter.categories.Count == 0) categories.Add(category.Id);

                FilteredElementCollector collector = new FilteredElementCollector(GrevitBuildModel.document).OfCategoryId(category.Id);
                if (collector.Count() > 0)
                {
                    foreach (Autodesk.Revit.DB.Parameter parameter in collector.FirstElement().Parameters)
                        if (!parameters.ContainsKey(parameter.Definition.Name)) parameters.Add(parameter.Definition.Name, parameter.Id);
                }
            }




            ParameterFilterElement parameterFilter = null;

            FilteredElementCollector collect = new FilteredElementCollector(GrevitBuildModel.document).OfClass(typeof(ParameterFilterElement));
            foreach (ParameterFilterElement existingFilter in collect.ToElements())
            {
                if (existingFilter.Name == filter.name)
                {
                    existingFilter.ClearRules();
                    parameterFilter = existingFilter;
                }
            }

            if (parameterFilter == null) parameterFilter = ParameterFilterElement.Create(GrevitBuildModel.document, filter.name, categories);


            View view = (View)Utilities.GetElementByName(GrevitBuildModel.document, typeof(View), filter.view);
            view.AddFilter(parameterFilter.Id);
           

            #region Apply Rules

            List<FilterRule> filterRules = new List<FilterRule>();

            foreach (Grevit.Types.Rule rule in filter.Rules)
            {
                if (parameters.ContainsKey(rule.name))
                {
                    FilterRule filterRule = rule.ToRevitRule(parameters[rule.name]);
                    if (filterRule != null) filterRules.Add(filterRule);
                }
            }

            parameterFilter.SetRules(filterRules);

            #endregion

            #region Apply Overrides

            OverrideGraphicSettings filterSettings = new OverrideGraphicSettings();

            // Apply Colors
            if (filter.CutFillColor != null) filterSettings.SetCutFillColor(filter.CutFillColor.ToRevitColor());
            if (filter.ProjectionFillColor != null) filterSettings.SetProjectionFillColor(filter.ProjectionFillColor.ToRevitColor());
            if (filter.CutLineColor != null) filterSettings.SetCutLineColor(filter.CutLineColor.ToRevitColor());
            if (filter.ProjectionLineColor != null) filterSettings.SetProjectionLineColor(filter.ProjectionLineColor.ToRevitColor());

            // Apply Lineweight
            if (filter.CutLineWeight != -1) filterSettings.SetCutLineWeight(filter.CutLineWeight);
            if (filter.ProjectionLineWeight != -1) filterSettings.SetProjectionLineWeight(filter.ProjectionLineWeight);

            // Apply Patterns          
            if (filter.CutFillPattern != null)
            {
                FillPatternElement pattern = (FillPatternElement)Utilities.GetElementByName(GrevitBuildModel.document, typeof(FillPatternElement), filter.CutFillPattern);
                filterSettings.SetCutFillPatternId(pattern.Id);
            }

            if (filter.ProjectionFillPattern != null)
            {
                FillPatternElement pattern = (FillPatternElement)Utilities.GetElementByName(GrevitBuildModel.document, typeof(FillPatternElement), filter.ProjectionFillPattern);
                filterSettings.SetProjectionFillPatternId(pattern.Id);
            }

            if (filter.CutLinePattern != null)
            {
                LinePatternElement pattern = (LinePatternElement)Utilities.GetElementByName(GrevitBuildModel.document, typeof(LinePatternElement), filter.CutLinePattern);
                filterSettings.SetCutLinePatternId(pattern.Id);
            }

            if (filter.ProjectionLinePattern != null)
            {
                LinePatternElement pattern = (LinePatternElement)Utilities.GetElementByName(GrevitBuildModel.document, typeof(LinePatternElement), filter.ProjectionLinePattern);
                filterSettings.SetProjectionLinePatternId(pattern.Id);
            }

            view.SetFilterOverrides(parameterFilter.Id, filterSettings);

            #endregion

            return parameterFilter;
        }




        /// <summary>
        /// Set a Revit Curtain Panel
        /// </summary>
        /// <param name="setCurtainPanel"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.SetCurtainPanel setCurtainPanel)
        {
            // If there arent any valid properties return null
            if (setCurtainPanel.panelID == 0 || setCurtainPanel.panelType == "") return null;

            // Get the panel to change
            Panel panel = (Panel)GrevitBuildModel.document.GetElement(new ElementId(setCurtainPanel.panelID));
            
            // get its host wall
            Element wallElement = panel.Host;

            if (wallElement.GetType() == typeof(Autodesk.Revit.DB.Wall))
            {
                // Cast the Wall
                Autodesk.Revit.DB.Wall wall = (Autodesk.Revit.DB.Wall)wallElement;

                // Try to get the curtain panel type
                FilteredElementCollector collector = new FilteredElementCollector(GrevitBuildModel.document).OfClass(typeof(PanelType));
                Element paneltype = collector.FirstElement();
                foreach (Element em in collector.ToElements()) if (em.Name == setCurtainPanel.panelType) paneltype = em;
                
                // Cast the Element type
                ElementType type = (ElementType)paneltype;

                // Change the panel type
                wall.CurtainGrid.ChangePanelType(panel, type);
            }

            return panel;
        }

        /// <summary>
        /// Create a new Family Instance
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        public static Element Create(this Familyinstance familyInstance)
        {
            // Get the FamilySymbol
            bool found = false;
            Element familySymbolElement = GrevitBuildModel.document.GetElementByName(typeof(FamilySymbol), familyInstance.FamilyOrStyle, familyInstance.TypeOrLayer, out found);

            // Setup a new Family Instance
            Autodesk.Revit.DB.FamilyInstance newFamilyInstance = null;

            // If the familySymbol is valid create a new instance
            if (familySymbolElement != null)
            {
                // Get the structural type 
                Autodesk.Revit.DB.Structure.StructuralType stype = familyInstance.structuralType.ToRevitStructuralType();

                // Cast the familySymbolElement
                FamilySymbol familySymbol = (FamilySymbol)familySymbolElement;

                if (!familySymbol.IsActive) familySymbol.Activate();

                // Create a reference Element
                Element referenceElement = null;

                double elevation = familyInstance.points[0].z;

                // If the view property is not set, the reference should be a level
                // Otherwise the family is view dependent
                if (familyInstance.view == null || familyInstance.view == string.Empty)                
                    referenceElement = GrevitBuildModel.document.GetLevelByName(familyInstance.level,elevation);                
                else                
                    referenceElement = GrevitBuildModel.document.GetElementByName(typeof(Autodesk.Revit.DB.View), familyInstance.view);
                              

                // If there is an element with the same GID existing, update it
                // Otherwise create a new one
                if (GrevitBuildModel.existing_Elements.ContainsKey(familyInstance.GID))
                {
                    newFamilyInstance = (Autodesk.Revit.DB.FamilyInstance)GrevitBuildModel.document.GetElement(GrevitBuildModel.existing_Elements[familyInstance.GID]);
                    if (newFamilyInstance.Location.GetType() == typeof(LocationPoint))
                    {
                        LocationPoint lp = (LocationPoint)newFamilyInstance.Location;
                        lp.Point = familyInstance.points[0].ToXYZ();

                    }
                    else if (newFamilyInstance.Location.GetType() == typeof(LocationCurve))
                    {
                        LocationCurve lp = (LocationCurve)newFamilyInstance.Location;
                        lp.Curve = Autodesk.Revit.DB.Line.CreateBound(familyInstance.points[0].ToXYZ(), familyInstance.points[1].ToXYZ());
                    }
                }
                else
                {
                    // If there is no reference element just create the family without
                    if (referenceElement == null)
                    { 
                        if (familyInstance.points.Count == 1) newFamilyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(familyInstance.points[0].ToXYZ(), familySymbol, stype);
                    }
                    else
                    {
                        if (familyInstance.points.Count == 1)
                        {
                            if (referenceElement.GetType() == typeof(Autodesk.Revit.DB.View))
                                newFamilyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(familyInstance.points[0].ToXYZ(), familySymbol, (Autodesk.Revit.DB.View)referenceElement);
                            else
                                newFamilyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(familyInstance.points[0].ToXYZ(), familySymbol, (Autodesk.Revit.DB.Level)referenceElement, stype);
                        }
                        else if (familyInstance.points.Count == 2)
                        {
                            Autodesk.Revit.DB.Line c = Autodesk.Revit.DB.Line.CreateBound(familyInstance.points[0].ToXYZ(), familyInstance.points[1].ToXYZ());

                            if (referenceElement.GetType() == typeof(Autodesk.Revit.DB.View))
                                newFamilyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(c, familySymbol, (Autodesk.Revit.DB.View)referenceElement);
                            else
                                newFamilyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(c, familySymbol, (Autodesk.Revit.DB.Level)referenceElement, stype);
                        }
                    }
                }
            }

            return newFamilyInstance;

        }

        /// <summary>
        /// Create a hosted family instance
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="hostElement"></param>
        /// <returns></returns>
        public static Element Create(this Familyinstance familyInstance, Element hostElement)
        {
            // Get the Family Symbol 
            bool found = false;
            Element familySymbolElement = GrevitBuildModel.document.GetElementByName(typeof(FamilySymbol), familyInstance.FamilyOrStyle, familyInstance.TypeOrLayer, out found);

            

            // Set up a new family Instance
            Autodesk.Revit.DB.FamilyInstance newFamilyInstance = null;

            // If the symbol and the host element are valid create the family instance
            if (familySymbolElement != null && hostElement != null)
            {
                // get the structural type
                Autodesk.Revit.DB.Structure.StructuralType structuralType = familyInstance.structuralType.ToRevitStructuralType();

                // Cast the family Symbol
                FamilySymbol familySymbol = (FamilySymbol)familySymbolElement;
                if (!familySymbol.IsActive) familySymbol.Activate();

                double elevation = familyInstance.points[0].z;
                // Get the placement level
                Autodesk.Revit.DB.Level level = (Autodesk.Revit.DB.Level)GrevitBuildModel.document.GetLevelByName(familyInstance.level,elevation);

                // If the element already exists just update it
                // Otherwise create a new one
                if (GrevitBuildModel.existing_Elements.ContainsKey(familyInstance.GID))
                {
                    newFamilyInstance = (Autodesk.Revit.DB.FamilyInstance)GrevitBuildModel.document.GetElement(GrevitBuildModel.existing_Elements[familyInstance.GID]);
                    if (newFamilyInstance.Location.GetType() == typeof(LocationPoint))
                    {
                        LocationPoint lp = (LocationPoint)newFamilyInstance.Location;
                        lp.Point = familyInstance.points[0].ToXYZ();
                    }
                }
                else
                {
                    if (familyInstance.points.Count == 1)
                        newFamilyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(familyInstance.points[0].ToXYZ(), familySymbol, hostElement, level, structuralType);
                }

            }
            return newFamilyInstance;
        }

        /// <summary>
        /// Create a Line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.RevitLine line)
        {
            // get revit curves from grevit curve
            foreach (Curve c in Utilities.GrevitCurvesToRevitCurves(line.curve))
            {               
                if (line.isModelCurve)
                    GrevitBuildModel.document.Create.NewModelCurve(c, Utilities.NewSketchPlaneFromCurve(GrevitBuildModel.document, c));
                
                if (line.isDetailCurve)            
                    GrevitBuildModel.document.Create.NewDetailCurve(GrevitBuildModel.document.ActiveView, c);
                
                if (line.isRoomBounding)
                {
                    CurveArray tmpca = new CurveArray();
                    tmpca.Append(c);
                    GrevitBuildModel.document.Create.NewRoomBoundaryLines(Utilities.NewSketchPlaneFromCurve(GrevitBuildModel.document, c), tmpca, GrevitBuildModel.document.ActiveView);
                }

            }
            return null;
        }


        public static string ConceptualMassTemplatePath = GrevitBuildModel.RevitTemplateFolder + @"\Conceptual Mass\Metric Mass.rft";

        /// <summary>
        /// Create Extrusion
        /// </summary>
        /// <param name="extrusion"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.SimpleExtrusion extrusion)
        {
            // If the file doesnt exist ask the user for a new template
            if (!File.Exists(ConceptualMassTemplatePath))
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Multiselect = false;
                ofd.Title = ConceptualMassTemplatePath + " not found.";
                System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
                if (dr != System.Windows.Forms.DialogResult.OK) return null;
                ConceptualMassTemplatePath = ofd.FileName;
            }

            // Create a new family Document
            Document familyDocument = GrevitBuildModel.document.Application.NewFamilyDocument(ConceptualMassTemplatePath);

            // Create a new family transaction
            Transaction familyTransaction = new Transaction(familyDocument, "Transaction in Family");
            familyTransaction.Start();

            // get family categories
            Categories categories = familyDocument.Settings.Categories;

            // Get the mass category
            Category catMassing = categories.get_Item("Mass");

            // Create a new subcategory in Mass
            Category subcategory = familyDocument.Settings.Categories.NewSubcategory(catMassing, "GrevitExtrusion");

            // Create a reference Array for the Profile
            ReferenceArray referenceArrayProfile = new ReferenceArray();

            // Translate all Points to the reference Array
            for (int i = 0; i < extrusion.polyline.points.Count - 1; i++)
            {
                Grevit.Types.Point pkt1 = extrusion.polyline.points[i];
                Grevit.Types.Point pkt2 = extrusion.polyline.points[i + 1];
                referenceArrayProfile.Append(Utilities.CurveFromXYZ(familyDocument, pkt1.ToXYZ(), pkt2.ToXYZ()));
            }

            // Create a new Extrusion
            Form extrudedMass = familyDocument.FamilyCreate.NewExtrusionForm(true, referenceArrayProfile, new XYZ(extrusion.vector.x, extrusion.vector.y, extrusion.vector.z));
            
            // Apply the subcategory
            extrudedMass.Subcategory = subcategory;

            // Commit the Family Transaction
            familyTransaction.Commit();

            // Assemble a filename to save the family to
            string filename = Path.Combine(Path.GetTempPath(), "GrevitMass-" + extrusion.GID + ".rfa");

            // Save Family to Temp path and close it
            SaveAsOptions opt = new SaveAsOptions();
            opt.OverwriteExistingFile = true;
            familyDocument.SaveAs(filename, opt);
            familyDocument.Close(false);

            // Load the created family to the document
            Family family = null;
            GrevitBuildModel.document.LoadFamily(filename, out family);

            // Get the first family symbol
            FamilySymbol symbol = null;
            foreach (ElementId s in family.GetFamilySymbolIds())
            {
                symbol = (FamilySymbol)GrevitBuildModel.document.GetElement(s);
                break;
            }

            if (!symbol.IsActive) symbol.Activate();

            // Create a new Family Instance origin based
            FamilyInstance familyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(new XYZ(0, 0, 0), symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            // Activate Mass visibility
            GrevitBuildModel.document.SetMassVisibilityOn();

            return familyInstance;

        }

        /// <summary>
        /// Create a Level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.Level level)
        {
            // Get all Levels from the document
            FilteredElementCollector sollector = new FilteredElementCollector(GrevitBuildModel.document).OfClass(typeof(Autodesk.Revit.DB.Level));

            // If there is any level with the same name that we are going to create return null because that level already exists
            foreach (Element e in sollector.ToElements()) if (e.Name == level.name) return null;

            // Create the new Level
            Autodesk.Revit.DB.Level newLevel = GrevitBuildModel.document.Create.NewLevel(level.height);
            
            // Set the Levels name
            newLevel.Name = level.name;

            // If we should create a view with it
            if (level.addView)
            {
                // Get all View Family Types of familyType Floor plan
                IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new FilteredElementCollector(GrevitBuildModel.document).OfClass(typeof(ViewFamilyType))
                                                              let type = elem as ViewFamilyType
                                                              where type.ViewFamily == ViewFamily.FloorPlan
                                                              select type;

                // Create a new view
                ViewPlan.Create(GrevitBuildModel.document, viewFamilyTypes.First().Id, newLevel.Id);
            }

            return newLevel;
        }

        /// <summary>
        /// Create Column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.Column column)
        {
            // Get the Family, Type and Level
            bool found = false;

            XYZ location = column.location.ToXYZ();
            XYZ top = column.locationTop.ToXYZ();

            XYZ lower = (location.Z < top.Z) ? location : top;
            XYZ upper = (location.Z < top.Z) ? top : location;


            Element familyElement = GrevitBuildModel.document.GetElementByName(typeof(FamilySymbol), column.FamilyOrStyle, column.TypeOrLayer, out found);
            Element levelElement = GrevitBuildModel.document.GetLevelByName(column.levelbottom,lower.Z);

            Autodesk.Revit.DB.FamilyInstance familyInstance = null;



            if (familyElement != null && levelElement != null && familyElement != null)
            {
                // Cast the FamilySymbol and the Level
                FamilySymbol sym = (FamilySymbol)familyElement;
                if (!sym.IsActive) sym.Activate();



                Autodesk.Revit.DB.Level level = (Autodesk.Revit.DB.Level)levelElement;



                // If the column already exists update it
                // Otherwise create a new one
                if (GrevitBuildModel.existing_Elements.ContainsKey(column.GID))         
                    familyInstance = (Autodesk.Revit.DB.FamilyInstance)GrevitBuildModel.document.GetElement(GrevitBuildModel.existing_Elements[column.GID]);            
                else
                    familyInstance = GrevitBuildModel.document.Create.NewFamilyInstance(lower, sym, level, Autodesk.Revit.DB.Structure.StructuralType.Column);

                #region slantedColumn

                // Get the Slanted Column parameter
                Autodesk.Revit.DB.Parameter param = familyInstance.get_Parameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM);

                // Set the slanted option to EndPoint which means we can set a top and a bottom point for the column
                param.Set((double)SlantedOrVerticalColumnType.CT_EndPoint);

                // get the locationcurve of the column
                LocationCurve elementCurve = familyInstance.Location as LocationCurve;

                if (elementCurve != null)
                {
                    // Create a new line brom bottom to top
                    Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(lower, upper);
                    
                    // Apply this line to the location curve
                    elementCurve.Curve = line;
                }

                #endregion

            }
            return familyInstance;

        }

        /// <summary>
        /// Create a reference Plane
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.ReferencePlane plane)
        {
            // Get the supposed view element
            Element view = GrevitBuildModel.document.GetElementByName(typeof(Autodesk.Revit.DB.View), plane.View);

            // If its valid
            if (view != null)
            {
                // Cast the View
                View v = (View)view;

                // Create a new plane
                Autodesk.Revit.DB.ReferencePlane newPlane = GrevitBuildModel.document.Create.NewReferencePlane2(plane.EndA.ToXYZ(), plane.EndB.ToXYZ(), plane.cutVector.ToXYZ(), v);
                
                // Set its name
                plane.Name = plane.Name;

                return newPlane;

            }
            return null;

        }

        /// <summary>
        /// Create a gridline
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.Grid grid)
        {
            // Create a new gridline
            Autodesk.Revit.DB.Grid gridline = GrevitBuildModel.document.Create.NewGrid(Autodesk.Revit.DB.Line.CreateBound(grid.from.ToXYZ(), grid.to.ToXYZ()));
            
            // If a name is supplied, set the name
            if (grid.Name != null && grid.Name != "") gridline.Name = grid.Name;

            return gridline;
        }

        /// <summary>
        /// Create Hatch
        /// </summary>
        /// <param name="hatch"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.Hatch hatch)
        {
            // Get all Filled region types
            FilteredElementCollector collector = new FilteredElementCollector(GrevitBuildModel.document).OfClass(typeof(Autodesk.Revit.DB.FilledRegionType));
            
            // Get the View to place the hatch on
            Element viewElement = GrevitBuildModel.document.GetElementByName(typeof(Autodesk.Revit.DB.View), hatch.view);

            // Get the hatch pattern name and set it to solid if the hatch pattern name is invalid
            string patternname = (hatch.pattern == null || hatch.pattern == string.Empty) ? patternname = "Solid fill" : hatch.pattern;

            // Get the fill pattern element and filled region type
            FillPatternElement fillPatternElement = FillPatternElement.GetFillPatternElementByName(GrevitBuildModel.document, FillPatternTarget.Drafting, patternname);
            FilledRegionType filledRegionType = collector.FirstElement() as FilledRegionType;

            // Setup a new curveloop for the outline
            CurveLoop curveLoop = new CurveLoop();
            List<CurveLoop> listOfCurves = new List<CurveLoop>();

            // Get a closed loop from the grevit points
            for (int i = 0; i < hatch.outline.Count; i++)
            {
                int j = i + 1;
                Grevit.Types.Point p1 = hatch.outline[i];
                if (j == hatch.outline.Count) j = 0;
                Grevit.Types.Point p2 = hatch.outline[j];

                Curve cn = Autodesk.Revit.DB.Line.CreateBound(p1.ToXYZ(), p2.ToXYZ());
                curveLoop.Append(cn);
            }

            listOfCurves.Add(curveLoop);
            
            // Create a filled region from the loop
            return FilledRegion.Create(GrevitBuildModel.document, filledRegionType.Id, viewElement.Id, listOfCurves);

        }

        /// <summary>
        /// Create spot Coordinate
        /// </summary>
        /// <param name="spotCoordinate"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.SpotCoordinate spotCoordinate, Element reference)
        {
            // get View to place the spot coordinate on
            Element viewElement = GrevitBuildModel.document.GetElementByName(typeof(Autodesk.Revit.DB.View), spotCoordinate.view);
            if (viewElement != null)
            {
                View view = (View)viewElement;

                // Get the reference point for the coordinate
                XYZ pointref = spotCoordinate.refPoint.ToXYZ();

                // If the reference element has a locationcurve or locationpoint use
                // this point instead of the reference point
                if (reference.Location.GetType() == typeof(LocationPoint))
                {
                    LocationPoint lp = (LocationPoint)reference.Location;
                    pointref = lp.Point;
                }
                if (reference.Location.GetType() == typeof(LocationCurve))
                {
                    LocationCurve lp = (LocationCurve)reference.Location;
                    pointref = lp.Curve.GetEndPoint(0);
                }

                // Create Spot Elevation as elevation or standard
                if (spotCoordinate.isElevation)
                    return GrevitBuildModel.document.Create.NewSpotElevation(view, new Reference(reference), spotCoordinate.locationPoint.ToXYZ(), spotCoordinate.bendPoint.ToXYZ(), spotCoordinate.endPoint.ToXYZ(), pointref, spotCoordinate.hasLeader);
                else
                    return GrevitBuildModel.document.Create.NewSpotCoordinate(view, new Reference(reference), spotCoordinate.locationPoint.ToXYZ(), spotCoordinate.bendPoint.ToXYZ(), spotCoordinate.endPoint.ToXYZ(), pointref, spotCoordinate.hasLeader);
            }
            return null;
        }

        /// <summary>
        /// Create Topography
        /// </summary>
        /// <param name="topography"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.Topography topography)
        {
            // Translate the Grevit Points to Revit Points
            List<XYZ> points = new List<XYZ>();
            foreach (Grevit.Types.Point point in topography.points)  points.Add(point.ToXYZ());
            
            // Create a new Topography based on this
            return Autodesk.Revit.DB.Architecture.TopographySurface.Create(GrevitBuildModel.document, points);        

        }

        /// <summary>
        /// Creates a new Revit Texnote
        /// </summary>
        /// <param name="textnote"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.TextNote textnote)
        {
            // Get the View Element to place the note on
            Element viewElement = GrevitBuildModel.document.GetElementByName(typeof(Autodesk.Revit.DB.View), textnote.view);
            if (viewElement != null)
            {
                // Cast to view
                View view = (View)viewElement;

                // Set a reasonable width
                double width = textnote.text.Length * 2;

                // return a new Textnote
                return GrevitBuildModel.document.Create.NewTextNote(view, textnote.location.ToXYZ(), XYZ.BasisX, XYZ.BasisY, width, TextAlignFlags.TEF_ALIGN_LEFT, textnote.text);
            }
            return null;
        }

        /// <summary>
        /// Creates a new Revit Slab
        /// </summary>
        /// <param name="slab"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.Slab slab)
        {
            // Create a List of Curves for the ouline
            List<Curve> curves = new List<Curve>();

            // Translate Grevit Curves to Revit Curves
            for (int i = 0; i < slab.surface.profile[0].outline.Count; i++)
            {
                foreach (Curve curve in Utilities.GrevitCurvesToRevitCurves(slab.surface.profile[0].outline[i])) curves.Add(curve);
            }

            // Get the two slope points
            XYZ slopePointBottom = slab.bottom.ToXYZ();
            XYZ slopeTopPoint = slab.top.ToXYZ();

            // get a Z Value from an outline point to check if the slope points are in this plane
            double outlineZCheckValue = curves[0].GetEndPoint(0).Z;

            // If one of the points is not in the same Z plane
            // Create new points replacing the Z value
            if (!slopePointBottom.Z.Equals(outlineZCheckValue) || !slopeTopPoint.Z.Equals(outlineZCheckValue))
            {
                slopePointBottom = new XYZ(slopePointBottom.X, slopePointBottom.Y, outlineZCheckValue);
                slopeTopPoint = new XYZ(slopeTopPoint.X, slopeTopPoint.Y, outlineZCheckValue);
            }

            // Create a new slope line between the points
            Autodesk.Revit.DB.Line slopeLine = Autodesk.Revit.DB.Line.CreateBound(slopePointBottom, slopeTopPoint);

            // Sort the outline curves contiguous
            Utilities.SortCurvesContiguous(GrevitBuildModel.document.Application.Create, curves);

            // Create a new surve array for creating the slab
            CurveArray outlineCurveArray = new CurveArray();
            foreach (Curve c in curves) outlineCurveArray.Append(c);

            // get the supposed level
            Element levelElement = GrevitBuildModel.document.GetLevelByName(slab.levelbottom,slopePointBottom.Z);
            if (levelElement != null)
            {
                // Create a new slab
                return GrevitBuildModel.document.Create.NewSlab(outlineCurveArray, (Autodesk.Revit.DB.Level)levelElement, slopeLine, slab.slope, slab.structural);
            }

            return null;

        }

        /// <summary>
        /// Create Profile based wall
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.WallProfileBased w)
        {
            double elevation = 90000000;
            // Translate Profile Curves
            List<Curve> curves = new List<Curve>();
            foreach (Component component in w.curves)
            {
                foreach (Curve curve in Utilities.GrevitCurvesToRevitCurves(component))
                {
                    curves.Add(curve);
                    if (curve.GetEndPoint(0).Z < elevation) elevation = curve.GetEndPoint(0).Z;
                }
            }

            // Get Wall Type
            Element wallTypeElement = GrevitBuildModel.document.GetElementByName(typeof(Autodesk.Revit.DB.WallType), w.TypeOrLayer);

            // Get Level
            Element levelElement = GrevitBuildModel.document.GetLevelByName(w.level,elevation);

            if (wallTypeElement != null && levelElement != null)
            {
                Autodesk.Revit.DB.Wall wall;

                // If the wall exists update it otherwise create a new one
                if (GrevitBuildModel.existing_Elements.ContainsKey(w.GID))    
                    wall = (Autodesk.Revit.DB.Wall)GrevitBuildModel.document.GetElement(GrevitBuildModel.existing_Elements[w.GID]); 
                else 
                    wall = Autodesk.Revit.DB.Wall.Create(GrevitBuildModel.document, curves, wallTypeElement.Id, levelElement.Id, true);

                return wall;
            }

            return null;
        }

        /// <summary>
        /// Create curtain Gridline
        /// </summary>
        /// <param name="curtainGridLine"></param>
        /// <param name="hostElement"></param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.CurtainGridLine curtainGridLine, Element hostElement)
        {
            // Check host if it is a wall
            if (hostElement.GetType() == typeof(Autodesk.Revit.DB.Wall))
            {
                // Cast the wall
                Autodesk.Revit.DB.Wall wall = (Autodesk.Revit.DB.Wall)hostElement;

                // Create a new Gridline on the wall
                Autodesk.Revit.DB.CurtainGridLine gridline = wall.CurtainGrid.AddGridLine(curtainGridLine.horizontal, curtainGridLine.point.ToXYZ(), curtainGridLine.single);
                
                return gridline;
            }
            return null;
        }

        /// <summary>
        /// Creates a new Revit Room
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static Element Create(this Room room)
        {
            // Get the rooms phase
            Phase phase = null;
            foreach (Phase p in GrevitBuildModel.document.Phases) if (p.Name == room.phase) phase = p;

            if (phase != null)
            {
                // Create a new Room
                Autodesk.Revit.DB.Architecture.Room newRoom = GrevitBuildModel.document.Create.NewRoom(phase);
                
                // Set Name and Number
                newRoom.Name = room.name;
                newRoom.Number = room.number;

                return newRoom;
            }

            return null;
        }

        /// <summary>
        /// Create Adaptive Component
        /// </summary>
        /// <param name="adaptive"></param>
        /// <returns></returns>
        public static Element Create(this Adaptive adaptive)
        {
            // Get the family Symbol
            bool found = false;
            Element faimlyElement = GrevitBuildModel.document.GetElementByName(typeof(FamilySymbol), adaptive.FamilyOrStyle, adaptive.TypeOrLayer, out found);
            FamilySymbol faimlySymbol = (FamilySymbol)faimlyElement;
          
            if (faimlySymbol != null)
            {
                if (!faimlySymbol.IsActive) faimlySymbol.Activate();

                FamilyInstance adaptiveComponent = null;

                // If the adaptive component already exists get it
                // Otherwise create a new one
                if (GrevitBuildModel.existing_Elements.ContainsKey(adaptive.GID)) 
                    adaptiveComponent = (FamilyInstance)GrevitBuildModel.document.GetElement(GrevitBuildModel.existing_Elements[adaptive.GID]); 
                else
                    adaptiveComponent = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(GrevitBuildModel.document, faimlySymbol);

                // Get the Placement points of the adaptive component
                IList<ElementId> ids = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(adaptiveComponent);

                // Walk thru the points and set them to the grevit points coordinates

                if (adaptive.points.Count != ids.Count) return null;

                for (int i = 0; i < ids.Count; i++)
                {
                    // Get the Reference Point
                    ElementId id = ids[i];
                    Element em = GrevitBuildModel.document.GetElement(id);
                    ReferencePoint referencePoint = (ReferencePoint)em;

                    // Set the reference Point to the Grevit Point
                    referencePoint.Position = adaptive.points[i].ToXYZ();
                }

                return adaptiveComponent;

            }
            return null;
        }

        /// <summary>
        /// Create Revit Wall
        /// </summary>
        /// <param name="grevitWall"></param>
        /// <param name="from">Optional Point From</param>
        /// <param name="to">Optional Point To</param>
        /// <returns></returns>
        public static Element Create(this Grevit.Types.Wall grevitWall, Grevit.Types.Point from = null, Grevit.Types.Point to = null)
        {
            #region baseLineCurve

            // The Baseline curve for the wall
            Autodesk.Revit.DB.Curve baselineCurve = null;

            // If the from and the to point have been set
            // Draw a line using those points (linear wall)
            if (from != null && to != null)
            {
                XYZ a = from.ToXYZ();
                XYZ b = to.ToXYZ();
                if (a.DistanceTo(b) > 0.01)
                {
                    baselineCurve = Autodesk.Revit.DB.Line.CreateBound(a, b);
                }
            }
            // Otherwise check the curve type
            else
            {
                // If the curve is a polyline (linear) split it into segments and create walls of them
                if (grevitWall.curve.GetType() == typeof(Grevit.Types.PLine))
                {
                    Grevit.Types.PLine pline = (Grevit.Types.PLine)grevitWall.curve;

                    // Walk thru all points and create segments
                    for (int i = 0; i < pline.points.Count; i++)
                    {
                        if (i == pline.points.Count - 1)
                        {
                            if (pline.closed) grevitWall.Create(pline.points[i], pline.points[0]);
                        }
                        else
                            grevitWall.Create(pline.points[i], pline.points[i + 1]);
                        
                    }
                }

                // If the curve is a line just create a line
                else if (grevitWall.curve.GetType() == typeof(Grevit.Types.Line))
                {
                    Grevit.Types.Line baseline = (Grevit.Types.Line)grevitWall.curve;
                    baselineCurve = Autodesk.Revit.DB.Line.CreateBound(baseline.from.ToXYZ(), baseline.to.ToXYZ());
                }

                // If the curve is an Arc create an Arc with Centerpoint, start, radius and end
                else if (grevitWall.curve.GetType() == typeof(Grevit.Types.Arc))
                {
                    Grevit.Types.Arc baseline = (Grevit.Types.Arc)grevitWall.curve;
                    baselineCurve = Autodesk.Revit.DB.Arc.Create(baseline.center.ToXYZ(), baseline.radius, baseline.start, baseline.end, XYZ.BasisX, XYZ.BasisY);
                }

                // If the curve is a 3Point Arc, create a new 3 Point based arc.
                else if (grevitWall.curve.GetType() == typeof(Grevit.Types.Curve3Points))
                {
                    Grevit.Types.Curve3Points baseline = (Grevit.Types.Curve3Points)grevitWall.curve;
                    baselineCurve = Autodesk.Revit.DB.Arc.Create(baseline.a.ToXYZ(), baseline.c.ToXYZ(), baseline.b.ToXYZ());

                }
            }

            #endregion

            // Get the Wall type and the level
            Element wallTypeElement = GrevitBuildModel.document.GetElementByName(typeof(Autodesk.Revit.DB.WallType), grevitWall.TypeOrLayer);
            

            if (wallTypeElement != null && baselineCurve != null)
            {
                Element levelElement = GrevitBuildModel.document.GetLevelByName(grevitWall.levelbottom, baselineCurve.GetEndPoint(0).Z);

                if (levelElement == null) return null;

                Autodesk.Revit.DB.Wall wall;

                // If the wall already exists update the baseline curve
                // Otherwise create a new wall
                if (GrevitBuildModel.existing_Elements.ContainsKey(grevitWall.GID))
                {
                    wall = (Autodesk.Revit.DB.Wall)GrevitBuildModel.document.GetElement(GrevitBuildModel.existing_Elements[grevitWall.GID]);
                    LocationCurve locationCurve = (LocationCurve)wall.Location;
                    locationCurve.Curve = baselineCurve;
                }
                else     
                    wall = Autodesk.Revit.DB.Wall.Create(GrevitBuildModel.document, baselineCurve, wallTypeElement.Id, levelElement.Id, grevitWall.height, 0, false, true);
     
                // Apply the automatic join setting
                if (!grevitWall.join)
                {
                    WallUtils.DisallowWallJoinAtEnd(wall, 0);
                    WallUtils.DisallowWallJoinAtEnd(wall, 1);
                }

                // Apply Flipped status
                if (grevitWall.flip) wall.Flip();

                // This method is applying the parameters and GID settings
                // itself because it might run recursively (see Polyline)
                grevitWall.SetParameters(wall);
                grevitWall.StoreGID(wall.Id);
            }

            // Always returns null as it handles
            // parameters and GIDs within this method
            return null;

        }

    }
}
