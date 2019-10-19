﻿using KY.Core.Dependency;
using KY.Core.Module;
using KY.Generator.Configuration;
using KY.Generator.Mappings;
using KY.Generator.OpenApi.Configuration;
using KY.Generator.OpenApi.DataAccess;
using KY.Generator.OpenApi.Languages;
using KY.Generator.OpenApi.Readers;

namespace KY.Generator.OpenApi
{
    public class OpenApiModule : ModuleBase
    {
        public OpenApiModule(IDependencyResolver dependencyResolver)
            : base(dependencyResolver)
        { }

        public override void Initialize()
        {
            this.DependencyResolver.Bind<OpenApiDocumentReader>().ToSelf();
            this.DependencyResolver.Bind<OpenApiFileReader>().ToSelf();
            this.DependencyResolver.Bind<OpenApiUrlReader>().ToSelf();
            this.DependencyResolver.Get<ReaderConfigurationMapping>().Map<OpenApiReadConfiguration, OpenApiReader>("openApi");
            this.DependencyResolver.Get<ITypeMapping>().Initialize();
        }
    }
}