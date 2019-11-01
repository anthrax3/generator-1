﻿using System;
using System.Collections.Generic;
using System.Linq;
using KY.Core;
using KY.Generator.Configuration;

namespace KY.Generator
{
    public static class GeneratorTypeLoader
    {
        public static Type Get(ConfigurationBase configuration, string assemblyName, string nameSpace, string typeName, params string[] locations)
        {
            List<string> list = locations.ToList();
            list.Add(configuration.Environment.ConfigurationFilePath);
            list.Add(configuration.Environment.OutputPath);
            Version defaultVersion = typeof(CoreModule).Assembly.GetName().Version;
            return NugetPackageTypeLoader.Get(assemblyName, nameSpace, typeName, defaultVersion, list.ToArray());
        }
    }
}
