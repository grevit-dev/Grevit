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
using System.Diagnostics;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;

namespace Grevit.Client
{
    public static class Utilities
    {
        public static void Send(Grevit.Types.ComponentCollection components, string host = "127.0.0.1", int port = 8002, int timeout = 10000)
        {
            bool retry = true;
            string responseData = "";

            try
            {
                while (retry)
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {

                        tcpClient.Connect(IPAddress.Parse(host), port);
                        tcpClient.NoDelay = true;
                        tcpClient.ReceiveTimeout = timeout;
                        tcpClient.SendTimeout = timeout;

                        using (NetworkStream stream = tcpClient.GetStream())
                        {
                            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
                            {
                                writer.AutoFlush = true;
                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    string line = Grevit.Serialization.Utilities.Serialize(components);
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
                System.Console.Write(ex.Message);
            }
        }
    }
}
