﻿using System;
using System.Linq;
using Should;
using Typewriter.CodeModel;
using System.Collections.Generic;
using Typewriter.Generation;
namespace Tests
{
    public class TestTests : TestBase
    {
        private readonly File fileInfo = GetFile(@"Tests\Render\RoutedApiController\BooksController.cs");

        public void Test()
        {
            var classInfo = fileInfo.Classes.First();

            GetValue(classInfo.Methods.First()).ShouldEqual("libraryId: number");
        }

        private string GetValue(Method method)
        {
            var parameters = method.Parameters.Select(p => new KeyValuePair<string, string>(p.Name, p.Type.ToString())).ToList();
            var matches = new System.Text.RegularExpressions.Regex(@"\{(\w+\:?\w+)\}").Matches(Extensions.Route(method));

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var values = match.Groups[1].Value.Split(':');
                if (parameters.Any(p => p.Key == values[0])) continue;
                var type = "any";
                if (new[] { "decimal", "double", "float", "int", "long" }.Contains(values[1])) type = "number";
                else if (new[] { "string", "guid", "datetime" }.Contains(values[1])) type = "string";
                parameters.Add(new KeyValuePair<string, string>(values[0], type));
            }
            return string.Join(", ", parameters.Select(p => string.Format("{0}: {1}", p.Key, p.Value)));
        }

        public void Test2()
        {
            var classInfo = fileInfo.Classes.First();

            DoStuff(classInfo.Methods.ToArray()[1]).ShouldEqual("\"api/library/\" + libraryId + \"/books/\" + id");
            DoStuff(classInfo.Methods.ToArray()[3])
                .ShouldEqual("\"api/library/\" + libraryId + \"/books/\" + \"?query1=\" + query1 + \"&query2=\" + query2");

            System.Text.RegularExpressions.Regex.IsMatch("api/library/{libraryId:int}/books/{id}", @"\{libraryId:?\w?\}");
        }

        private string DoStuff(Method method)
        {
            var route = Extensions.Route(method);
            var queryString = string.Join(" + \"&", method.Parameters.Where(p => p.Attributes.Any(a => a.Name == "FromBody") == false
            && System.Text.RegularExpressions.Regex.IsMatch(route, "\\{" + p.Name + ":?\\w?") == false)
                .Select(p => string.Format("{0}=\" + {0}", p.Name)));

            var routeExpression = "\"" + System.Text.RegularExpressions.Regex.Replace(route, @"\{(\w+):?\w*\}", delegate (System.Text.RegularExpressions.Match match)
            {
                return string.Format("\" + {0} + \"", match.Groups[1].Value);
            }) + "\"";
            if (routeExpression.EndsWith(" + \"\"")) routeExpression = routeExpression.Remove(routeExpression.Length - 5);
            return routeExpression + (string.IsNullOrEmpty(queryString) ? "" : " + \"?" + queryString);
        }

    }
    public class ClassTests : TestBase
    {
        private readonly File fileInfo = GetFile(@"Tests\CodeModel\ClassInfo.cs");

        public void Info()
        {
            var classInfo = fileInfo.Classes.First();

            classInfo.Name.ShouldEqual("Class1");
            classInfo.FullName.ShouldEqual("Tests.CodeModel.Class1");
            classInfo.Parent.ShouldEqual(fileInfo);
        }

        public void Attributes()
        {
            var classInfo = fileInfo.Classes.First();

            classInfo.Attributes.Count.ShouldEqual(1);

            classInfo.Attributes.First().Name.ShouldEqual("Test");
            classInfo.Attributes.First().FullName.ShouldEqual("Tests.CodeModel.TestAttribute");
            classInfo.Attributes.First().Value.ShouldEqual("classParameter");
        }

        #region Primitive properties

        public void BoolProperties()
        {
            TestPrimitiveProperty("Bool1", "Boolean");
        }

        public void CharProperties()
        {
            TestPrimitiveProperty("Char1", "Char");
        }

        public void StringProperties()
        {
            TestPrimitiveProperty("String1", "String");
        }

        public void ByteProperties()
        {
            TestPrimitiveProperty("Byte1", "Byte");
        }

        public void SbyteProperties()
        {
            TestPrimitiveProperty("Sbyte1", "SByte");
        }

        public void IntProperties()
        {
            TestPrimitiveProperty("Int1", "Int32");
        }

        public void UintProperties()
        {
            TestPrimitiveProperty("Uint1", "UInt32");
        }

        public void ShortProperties()
        {
            TestPrimitiveProperty("Short1", "Int16");
        }

        public void UshortProperties()
        {
            TestPrimitiveProperty("Ushort1", "UInt16");
        }

        public void LongProperties()
        {
            TestPrimitiveProperty("Long1", "Int64");
        }

        public void UlongProperties()
        {
            TestPrimitiveProperty("Ulong1", "UInt64");
        }

        public void FloatProperties()
        {
            TestPrimitiveProperty("Float1", "Single");
        }

        public void DoubleProperties()
        {
            TestPrimitiveProperty("Double1", "Double");
        }

        public void DecimalProperties()
        {
            TestPrimitiveProperty("Decimal1", "Decimal");
        }

        public void DateProperties()
        {
            TestPrimitiveProperty("DateTime1", "DateTime");
        }

        private void TestPrimitiveProperty(string name, string type)
        {
            var classInfo = fileInfo.Classes.First();
            var property = classInfo.Properties.First(p => p.Name == name);

            property.Name.ShouldEqual(name);
            property.FullName.ShouldEqual("Tests.CodeModel.Class1." + name);
            property.Parent.ShouldEqual(classInfo);

            property.HasGetter.ShouldBeTrue("HasGetter");
            property.HasSetter.ShouldBeTrue("HasSetter");

            property.IsEnum.ShouldBeFalse("IsEnum");
            property.IsEnumerable.ShouldBeFalse("IsEnumerable");
            property.IsGeneric.ShouldBeFalse("IsGeneric");
            property.IsNullable.ShouldBeFalse("IsNullable");
            property.IsPrimitive.ShouldBeTrue("IsPrimitive");

            property.Type.Name.ShouldEqual(type);
            property.Type.FullName.ShouldEqual("System." + type);

            property.Type.IsEnum.ShouldBeFalse("IsEnum");
            property.Type.IsEnumerable.ShouldBeFalse("IsEnumerable");
            property.Type.IsGeneric.ShouldBeFalse("IsGeneric");
            property.Type.IsNullable.ShouldBeFalse("IsNullable");
            property.Type.IsPrimitive.ShouldBeTrue("IsPrimitive");
            property.Type.GenericTypeArguments.Any().ShouldBeFalse("GenericTypeArguments");
            property.Type.Parent.ShouldEqual(property);
        }

        #endregion

        public void ObjectProperties()
        {
            var classInfo = fileInfo.Classes.First();
            var property = classInfo.Properties.First(p => p.Name == "Object1");

            property.Name.ShouldEqual("Object1");
            property.FullName.ShouldEqual("Tests.CodeModel.Class1.Object1");
            property.Parent.ShouldEqual(classInfo);

            property.HasGetter.ShouldBeTrue("HasGetter");
            property.HasSetter.ShouldBeTrue("HasSetter");

            property.IsEnum.ShouldBeFalse("IsEnum");
            property.IsEnumerable.ShouldBeFalse("IsEnumerable");
            property.IsGeneric.ShouldBeFalse("IsGeneric");
            property.IsNullable.ShouldBeFalse("IsNullable");
            property.IsPrimitive.ShouldBeFalse("IsPrimitive");

            property.Type.Name.ShouldEqual("Object");
            property.Type.FullName.ShouldEqual("System.Object");

            property.Type.IsEnum.ShouldBeFalse("IsEnum");
            property.Type.IsEnumerable.ShouldBeFalse("IsEnumerable");
            property.Type.IsGeneric.ShouldBeFalse("IsGeneric");
            property.Type.IsNullable.ShouldBeFalse("IsNullable");
            property.Type.IsPrimitive.ShouldBeFalse("IsPrimitive");
            property.Type.GenericTypeArguments.Any().ShouldBeFalse("GenericTypeArguments");
            property.Type.Parent.ShouldEqual(property);
        }

        public void DefinedClassProperties()
        {
            var classInfo = fileInfo.Classes.First();
            var property = classInfo.Properties.First(p => p.Name == "Class11");

            property.Name.ShouldEqual("Class11");
            property.FullName.ShouldEqual("Tests.CodeModel.Class1.Class11");
            property.Parent.ShouldEqual(classInfo);

            property.HasGetter.ShouldBeTrue("HasGetter");
            property.HasSetter.ShouldBeTrue("HasSetter");

            property.IsEnum.ShouldBeFalse("IsEnum");
            property.IsEnumerable.ShouldBeFalse("IsEnumerable");
            property.IsGeneric.ShouldBeFalse("IsGeneric");
            property.IsNullable.ShouldBeFalse("IsNullable");
            property.IsPrimitive.ShouldBeFalse("IsPrimitive");

            property.Type.Name.ShouldEqual("Class1");
            property.Type.FullName.ShouldEqual("Tests.CodeModel.Class1");

            property.Type.IsEnum.ShouldBeFalse("IsEnum");
            property.Type.IsEnumerable.ShouldBeFalse("IsEnumerable");
            property.Type.IsGeneric.ShouldBeFalse("IsGeneric");
            property.Type.IsNullable.ShouldBeFalse("IsNullable");
            property.Type.IsPrimitive.ShouldBeFalse("IsPrimitive");
            property.Type.GenericTypeArguments.Any().ShouldBeFalse("GenericTypeArguments");
            property.Type.Parent.ShouldEqual(property);

            property.Type.Attributes.Count.ShouldEqual(1);
            property.Type.Properties.Any().ShouldBeTrue();
            property.Type.Methods.Any().ShouldBeFalse();

            var typeProperty = property.Type.Properties.First(p => p.Name == "Class11");
            typeProperty.Name.ShouldEqual("Class11");
            typeProperty.FullName.ShouldEqual("Tests.CodeModel.Class1.Class11");
            typeProperty.Parent.ShouldEqual(property.Type);
        }

        public void DefinedEnumProperties()
        {
            var classInfo = fileInfo.Classes.First();
            var property = classInfo.Properties.First(p => p.Name == "Enum11");

            property.Name.ShouldEqual("Enum11");
            property.FullName.ShouldEqual("Tests.CodeModel.Class1.Enum11");
            property.Parent.ShouldEqual(classInfo);

            property.HasGetter.ShouldBeTrue("HasGetter");
            property.HasSetter.ShouldBeTrue("HasSetter");

            property.IsEnum.ShouldBeTrue("IsEnum");
            property.IsEnumerable.ShouldBeFalse("IsEnumerable");
            property.IsGeneric.ShouldBeFalse("IsGeneric");
            property.IsNullable.ShouldBeFalse("IsNullable");
            property.IsPrimitive.ShouldBeTrue("IsPrimitive");
        }

        public void EnumerableDefinedClassProperties()
        {
            var classInfo = fileInfo.Classes.First();
            var property = classInfo.Properties.First(p => p.Name == "IEnumerableClass11");

            property.Name.ShouldEqual("IEnumerableClass11");
            property.FullName.ShouldEqual("Tests.CodeModel.Class1.IEnumerableClass11");
            property.Parent.ShouldEqual(classInfo);

            property.HasGetter.ShouldBeTrue("HasGetter");
            property.HasSetter.ShouldBeTrue("HasSetter");

            property.IsEnum.ShouldBeFalse("IsEnum");
            property.IsEnumerable.ShouldBeTrue("IsEnumerable");
            property.IsGeneric.ShouldBeTrue("IsGeneric");
            property.IsNullable.ShouldBeFalse("IsNullable");
            property.IsPrimitive.ShouldBeFalse("IsPrimitive");

            property.Type.Name.ShouldEqual("IEnumerable");
            property.Type.FullName.ShouldEqual("System.Collections.Generic.IEnumerable<Tests.CodeModel.Class1>");

            property.Type.IsEnum.ShouldBeFalse("IsEnum");
            property.Type.IsEnumerable.ShouldBeTrue("IsEnumerable");
            property.Type.IsGeneric.ShouldBeTrue("IsGeneric");
            property.Type.IsNullable.ShouldBeFalse("IsNullable");
            property.Type.IsPrimitive.ShouldBeFalse("IsPrimitive");
            property.Type.GenericTypeArguments.Any().ShouldBeTrue("GenericTypeArguments");
            property.Type.Parent.ShouldEqual(property);

            var generic = property.Type.GenericTypeArguments.First();
            generic.Name.ShouldEqual("Class1");
            generic.FullName.ShouldEqual("Tests.CodeModel.Class1");
            generic.Parent.ShouldEqual(property.Type);

            generic.Attributes.Count.ShouldEqual(1);
            generic.Properties.Any().ShouldBeTrue();
            generic.Methods.Any().ShouldBeFalse();

            var typeProperty = generic.Properties.First(p => p.Name == "Class11");
            typeProperty.Name.ShouldEqual("Class11");
            typeProperty.FullName.ShouldEqual("Tests.CodeModel.Class1.Class11");
            typeProperty.Parent.ShouldEqual(generic);
        }
    }
}