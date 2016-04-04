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

# Setup
require 'sketchup.rb'

module Grevit
	module SketchUp

		# Get Plugins menu
		@plugins_menu = UI.menu("Plugins")
		@grevit_menu = @plugins_menu.add_submenu("Grevit")

		# Add a menu item to launch Grevit
		@grevit_menu.add_item("Grevit Send") { 
			self.grevit_send
		}

		# Add a link to grevits website
		@grevit_menu.add_item("Grevit Info") {
			UI.openURL('http://grevit.net')
		}

		# Grevit Send Method
		def self.grevit_send

			# get active model
			@model = Sketchup.active_model  

			begin

				# save the current model as v2015 to temp
				@model.save_copy(Sketchup.temp_dir + "/GrevitTempFile.skp", Sketchup::Model::VERSION_2015)
			
			rescue ArgumentError => exception_object

				# report any errors to the user
				UI.messagebox(exception_object.message)

			end
  

			# get the grevit executable
			@executable = Sketchup.find_support_file("Grevit.SketchUp.exe","Plugins/grevit_send")
	
			
			if (File.exist?(@executable))

				# execute grevit if exe has been found
				UI.openURL('file:///'+@executable)

			else

				# report any errors
				UI.messagebox(@executable + " not found!")

			end
  
		end
	end
end