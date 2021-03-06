﻿using KY.Generator.Models;
using KY.Generator.Output;
using KY.Generator.Templates;
using KY.Generator.Writers;

namespace KY.Generator.TypeScript.Writers
{
    public class TypeScriptFieldWriter : ITemplateWriter
    {
        public virtual void Write(ICodeFragment fragment, IOutputCache output)
        {
            FieldTemplate template = (FieldTemplate)fragment;
            output.If(template.Visibility != Visibility.None).Add(template.Visibility.ToString().ToLower()).Add(" ").EndIf()
                  .If(template.IsStatic).Add("static ").EndIf()
                  .If(template.IsConst).Add("const ").EndIf()
                  .If(template.IsReadonly).Add("readonly ").EndIf()
                  .Add(template.Name)
                  .Add(": ")
                  .Add(template.Type)
                  .If(template.DefaultValue != null).Add(" = ").Add(template.DefaultValue).EndIf()
                  .CloseLine();
        }
    }
}