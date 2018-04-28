﻿using KY.Generator.Languages;
using KY.Generator.Meta;
using KY.Generator.Meta.Extensions;
using KY.Generator.Templates;

namespace KY.Generator.Writers
{
    public class CaseWriter : ITemplateWriter
    {
        protected ILanguage Language { get; }

        public CaseWriter(ILanguage language)
        {
            this.Language = language;
        }

        public virtual void Write(IMetaElementList elements, CodeFragment fragment)
        {
            CaseTemplate template = (CaseTemplate)fragment;
            MetaBlock block = elements.AddBlock().SkipStartAndEnd();
            this.Write(block.Header.AddUnclosed().Code, fragment);
            block.Elements.Add(template.Code, this.Language);
        }

        public virtual void Write(IMetaFragmentList fragments, CodeFragment fragment)
        {
            CaseTemplate template = (CaseTemplate)fragment;
            fragments.Add("case ")
                     .Add(template.Expression, this.Language)
                     .Add(":");
        }
    }
}