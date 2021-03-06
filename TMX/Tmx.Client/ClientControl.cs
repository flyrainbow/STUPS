﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 12/12/2014
 * Time: 5:55 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Client
{
    using System;
    using System.Linq;
    using PSTestLib.Helpers;
    
    /// <summary>
    /// Description of ClientControl.
    /// </summary>
    public class ClientControl
    {
        static bool _alreadyInitialized = false;
        
        public static void Init()
        {
            if (_alreadyInitialized) return;
            // 20141211
            // temporary
            // TODO: think about where to move it
            var tracingControl = new TracingControl("TmxClient_");
            
            loadPlugins();
            
            
            _alreadyInitialized = true;
        }

        static void loadPlugins()
        {
            var clientAssembly = (AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.FullName.Contains("Tmx.Client")));
            var clientFolderPath = clientAssembly.Location.Substring(0, clientAssembly.Location.LastIndexOf('\\'));
            
            var pluginsLoader = new PluginsLoader(clientFolderPath + @"\Plugins");
            pluginsLoader.Load();
        }
    }
}
