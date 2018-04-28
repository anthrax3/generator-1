﻿namespace KY.Generator.Templates
{
    public class ParameterTemplate : AttributeableTempalte
    {
        public TypeTemplate Type { get; }
        public string Name { get; }
        public CodeFragment DefaultValue { get; }

        public ParameterTemplate(TypeTemplate type, string name, CodeFragment defaultValue = null)
        {
            this.Type = type;
            this.Name = name;
            this.DefaultValue = defaultValue;
        }

        public static ParameterTemplate Create(TypeTemplate type, string name, CodeFragment defaultValue = null)
        {
            return new ParameterTemplate(type, name, defaultValue);
        }
    }
}