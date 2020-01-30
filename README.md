# Grevit
## Build your BIM Model directly in Grasshopper.

Grevit allows you to define BIM Elements in Grasshopper or SketchUp and translate them directly to Autodesk Revit or AutoCad Architecture. Grevit follows a one way process so your design model remains the geometrical source of truth: send geometry and attributes from Rhino/Grassopper or SketchUp to Autodesk Revit or ACA. Don't worry if your design changes, Grevit can even update existing geometries. Grevit supports a lot of BIM elements, check out the 
[documentation](https://grevit.gitbooks.io/grevit-documentation/content/)

Grevit is Free and Open Source and you can help to make it better: contribute on GitHub.

### Why not just exporting and importing?

Exporting static geometries is working fine from Rhino or SketchUp but when it comes to a BIM workflow exporting isn't enough. Static geometries won't intersect with native elements and are difficult to enhance with parameters. Redrawing geometry with native BIM elements often seems to be the only solution. But then, any design change is forcing you to repeat the process all over again. Grevit does not only create BIM elements from Rhino or SketchUp, it also allows you to update BIM elements later according to your latest design changes while all parameter values remain in place.

### Why is Grevit sending data through network?

This way you could run Revit or ACA on a different machine than the Design Model. A designer working in Grasshopper and another one working in SketchUp: both can send their geometries to one Revit instance using Grevit.

### Stack

Grevit is built in C#.NET making use of the following .NET APIs:
Autodesk (R) Revit, Autodesk (R) AutoCAD Architecture, SketchUp and McNeel (R) Grasshopper
