#
# Grevit - Create Autodesk Revit (R) Models in McNeel's Rhino Grassopper 3D (R)
# For more Information visit grevit.net or food4rhino.com/project/grevit
# Copyright (C) 2015
# Authors: Maximilian Thumfart,
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program. If not, see <http:#www.gnu.org/licenses/>.
#
require 'sketchup.rb'
require 'extensions.rb'

grevit_extension = SketchupExtension.new("Grevit", "grevit_send/grevit_send_menu.rb")
grevit_extension.version = '1.0'
grevit_extension.description = 'Send SketchUp Geometries directly to Revit.'
Sketchup.register_extension(grevit_extension, true)
