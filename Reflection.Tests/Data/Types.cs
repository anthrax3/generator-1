﻿using System.Collections.Generic;

namespace KY.Generator.Reflection.Tests
{
    public class Types
    {
        // Fields
        public string StringField;
        public int IntField;
        public System.DateTime DateTimeField;

        // Types
        public string StringProperty { get; set; }
        public short ShortProperty { get; set; }
        public ushort UShortProperty { get; set; }
        public int IntProperty { get; set; }
        public uint UIntProperty { get; set; }
        public long LongProperty { get; set; }
        public ulong ULongProperty { get; set; }
        public float FloatProperty { get; set; }
        public double DoubleProperty { get; set; }
        public bool BoolProperty { get; set; }
        public object ObjectProperty { get; set; }

        // System Types
        public System.String SystemStringProperty { get; set; }
        public System.Int16 SystemInt16Property { get; set; }
        public System.Int32 SystemInt32Property { get; set; }
        public System.Int64 SystemInt64Property { get; set; }
        public System.Single SystemSingleProperty { get; set; }
        public System.Double SystemDoubleProperty { get; set; }
        public System.DateTime SystemDateTimeProperty { get; set; }
        public System.Array SystemArrayProperty { get; set; }
        public System.Byte SystemByteProperty { get; set; }
        public System.Char SystemCharProperty { get; set; }
        public System.Decimal SystemDecimalProperty { get; set; }
        public System.Guid SystemGuidProperty { get; set; }
        public System.Object SystemObjectProperty { get; set; }
        public System.TimeSpan SystemTimeSpanProperty { get; set; }
        public System.UInt16 SystemUInt16Property { get; set; }
        public System.UInt32 SystemUInt32Property { get; set; }
        public System.UInt64 SystemUInt64Property { get; set; }

        // Complex Types
        public SubType SubTypeProperty { get; set; }

        // Arrays
        public string[] StringArrayProperty { get; set; }
        public int[] IntArrayProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public System.DateTime[] SystemDateTimeArrayProperty { get; set; }
        public SubType[] SubTypeArrayProperty { get; set; }

        // Generics
        public List<string> StringList { get; set; }
        public List<SubType> SubTypeList { get; set; }
        public Dictionary<string, string> StringStringDictionary { get; set; }
        public Dictionary<int, string> IntStringDictionary { get; set; }
        public Dictionary<string, SubType> StringSubTypeDictionary { get; set; }
        public Dictionary<int, SubType> IntSubTypeDictionary { get; set; }
        public Dictionary<SubType, string> SubTypeStringDictionary { get; set; }

        // Accessors
        public string ReadonlyProperty => string.Empty;
        public string WriteonlyProperty { set {} }
        protected string ProtectedProperty { get; set; }
        private string PrivateProperty { get; set; }
        protected string ProtectedField;
        private string PrivateField;
    }

    public class SubType
    {
        public string Property { get; set; }
    }
}