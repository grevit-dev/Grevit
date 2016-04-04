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
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using Grevit.Types;

namespace Grevit.PDF
{
    public static class Extensions
    {
        public static double ToDouble(this PdfObject obj) { return double.Parse(obj.ToString()); }
    }

    public class ParsePDF
    {
        public static ComponentCollection ParseDocument(string filename, double conversion)
        {
            ComponentCollection collection = new ComponentCollection();
            collection.scale = conversion;
            collection.delete = false;

            using (var ms = new System.IO.MemoryStream())
            {
                PdfReader myPdfReader = new PdfReader(filename);
                PdfDictionary pageDict = myPdfReader.GetPageN(1);
                PdfArray annotArray = pageDict.GetAsArray(PdfName.ANNOTS);

                

                for (int i = 0; i < annotArray.Size; i++)
                {
                    PdfDictionary curAnnot = annotArray.GetAsDict(i);
                    PdfName subject = curAnnot.GetAsName(PdfName.SUBTYPE);
                    if (subject != null)
                    {
                        if (subject == PdfName.CIRCLE)
                        {
                            PdfArray arr = curAnnot.GetAsArray(PdfName.RECT);

                            Point a = new Point(arr[0].ToDouble(), arr[1].ToDouble(), 0);
                            Point b = new Point(arr[0].ToDouble(), arr[1].ToDouble(), 5);

                            Column column = new Column("","",null,a,b,"",false);
                            collection.Items.Add(column);
                        }

                        else if (subject == PdfName.LINE)
                        {
                            PdfArray arr2 = curAnnot.GetAsArray(PdfName.L);
                            Line line = new Line()
                            {
                                from = new Point(arr2[0].ToDouble(), arr2[1].ToDouble(), 0),
                                to = new Point(arr2[2].ToDouble(), arr2[3].ToDouble(), 0)
                            };

                            Wall wall = new Wall("","",null,line,"",5,false,false);
                            collection.Items.Add(wall);
                        }

                        else if (subject == PdfName.POLYGON)
                        {
                            PdfArray arr3 = curAnnot.GetAsArray(PdfName.VERTICES);
                            Profile p = new Profile();
                            Loop l = new Loop() { outline = new List<Component>() };
                            for (int j = 0; j < arr3.Size - 2; j = j + 2)
                            {
                                Point a = new Point(arr3[j].ToDouble(), arr3[j + 1].ToDouble(),0);
                                Point b = new Point(arr3[j + 2].ToDouble(), arr3[j + 3].ToDouble(),0);
                                l.outline.Add(new Line(){from = a, to = b});
                            }
                            p.profile = new List<Loop>() { l };
                            Slab s = new Slab()
                            {
                                surface = p
                            };
                            collection.Items.Add(s);
                        }
                    }
                }
            }

            return collection;
        }
    }

}
