﻿using KY.Core.Meta;
using KY.Core.Meta.Extensions;
using KY.Generator.Languages;
using KY.Generator.Templates;
using KY.Generator.Templates.Extensions;

namespace KY.Generator.Writers
{
    public class ConstraintWriter : ITemplateWriter
    {
        protected ILanguage Language { get; }

        public ConstraintWriter(ILanguage language)
        {
            this.Language = language;
        }

        public void Write(IMetaElementList elements, CodeFragment fragment)
        {
            this.Write(elements.AddUnclosed().Code, fragment);
        }

        public virtual void Write(IMetaFragmentList fragments, CodeFragment fragment)
        {
            ConstraintTemplate template = (ConstraintTemplate)fragment;
            if (template.Types.Count == 0)
            {
                return;
            }
            fragments.AddNewLine().WithSeparator(" ", x => x
                                                           .Add("where")
                                                           .Add(template.Name)
                                                           .Add(": ")
                                                           .Add(template.Types, this.Language, ", "));
        }
    }
}