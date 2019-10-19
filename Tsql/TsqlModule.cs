﻿using KY.Core.Dependency;
using KY.Generator.Configuration;
using KY.Generator.Mappings;
using KY.Generator.Module;
using KY.Generator.Tsql.Configuration;
using KY.Generator.Tsql.Extensions;
using KY.Generator.Tsql.Readers;

namespace KY.Generator.Tsql
{
    public class TsqlModule : GeneratorModule
    {
        public TsqlModule(IDependencyResolver dependencyResolver)
            : base(dependencyResolver)
        { }

        public override void Initialize()
        {
            StaticResolver.TypeMapping = this.DependencyResolver.Get<ITypeMapping>().Initialize();
            this.DependencyResolver.Get<ReaderConfigurationMapping>().Map<TsqlReadConfiguration, TsqlReader>("tsql");
        }
    }
}