﻿using System;
using System.Linq;
using KY.Core.Meta;
using KY.Core.Meta.Extensions;
using KY.Core.Meta.Templates;
using KY.Generator.Languages;
using KY.Generator.Templates;
using KY.Generator.Templates.Extensions;

namespace KY.Generator.Writers
{
    public class NamespaceWriter : ITemplateWriter
    {
        protected BaseLanguage Language { get; }

        public NamespaceWriter(BaseLanguage language)
        {
            this.Language = language;
        }

        public virtual void Write(IMetaElementList elements, CodeFragment fragment)
        {
            NamespaceTemplate template = (NamespaceTemplate)fragment;
            if (template.Children.Count == 0)
            {
                return;
            }

            IMetaElementList targetElements = elements;
            if (!string.IsNullOrEmpty(template.Name))
            {
                MetaBlock statement = elements.AddBlock();
                targetElements = statement.Elements;
                statement.Header.AddUnclosed().Code.Add($"{this.Language.NamespaceKeyword} {template.Name}");
            }
            targetElements.Add(template.Children.OfType<EnumTemplate>(), this.Language);
            targetElements.Add(template.Children.OfType<ClassTemplate>(), this.Language);
        }

        public virtual void Write(IMetaFragmentList fragments, CodeFragment fragment)
        {
            throw new InvalidOperationException();
        }
    }
}