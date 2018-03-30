﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class TsMetadata : StringBuilderFileGenerator<ReflectionRepository>
    {
        public TsMetadata(GeneratorServices services) : base(services)
        {
        }

        public override Task<string> BuildOutputAsync()
        {
            var b = new TypeScriptCodeBuilder(indentSize: 2);

            b.Line("import { Domain, getEnumMeta, ModelType, ObjectType, PrimitiveProperty, ModelProperty, ForeignKeyProperty, PrimaryKeyProperty } from 'coalesce-vue/lib/metadata' ");
            b.Line();

            // Assigning each property as a member of domain ensures we don't break type contracts.
            // Exporting each model individually offers easier usage in imports.
            b.Line("const domain: Domain = { types: {}, enums: {} }");



            foreach (var model in Model.ClientEnums.OrderBy(e => e.Name))
            {
                WriteEnumMetadata(b, model);
            }

            foreach (var model in Model.ApiBackedClasses.OrderBy(e => e.Name))
            {
                WriteApiBackedTypeMetadata(b, model);
            }

            foreach (var model in Model.ExternalTypes.OrderBy(e => e.Name))
            {
                WriteExternalTypeMetadata(b, model);
            }

            // Create an enhanced Domain definition for deep intellisense.
            b.Line();
            using (b.Block("interface AppDomain extends Domain"))
            {
                using (b.Block("enums:"))
                {
                    foreach (var model in Model.ClientEnums.OrderBy(e => e.Name))
                    {
                        b.Line($"{model.Name}: typeof {model.Name}");
                    }
                }
                using (b.Block("types:"))
                {
                    foreach (var model in Model.ClientClasses.OrderBy(e => e.Name))
                    {
                        b.Line($"{model.ViewModelClassName}: typeof {model.ViewModelClassName}");
                    }
                }
            }
            
            b.Line();
            b.Line("export default domain as AppDomain");


            return Task.FromResult(b.ToString());
        }

        void WriteCommonClassMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            b.StringProp("name", model.Name.ToCamelCase());
            b.StringProp("displayName", model.DisplayName);
            if (model.ListTextProperty != null)
            {
                // This might not be defined for external types, because sometimes it just doesn't make sense. We'll accommodate on the client.
                b.Line($"get displayProp() {{ return this.props.{model.ListTextProperty.JsVariable} }}, ");
            }
        }

        private void WriteExternalTypeMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block($"export const {model.ViewModelClassName} = domain.types.{model.ViewModelClassName} ="))
            {
                WriteCommonClassMetadata(b, model);
                b.StringProp("type", "object");

                WriteClassPropertiesMetadata(b, model);
            }
        }

        private void WriteApiBackedTypeMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block($"export const {model.ViewModelClassName} = domain.types.{model.ViewModelClassName} ="))
            {
                WriteCommonClassMetadata(b, model);
                b.StringProp("type", "model");
                b.StringProp("controllerRoute", model.ApiRouteControllerPart);
                b.Line($"get keyProp() {{ return this.props.{model.PrimaryKey.JsVariable} }}, ");

                WriteClassPropertiesMetadata(b, model);

                WriteClassMethodMetadata(b, model);
            }
        }

        private static void WriteEnumMetadata(TypeScriptCodeBuilder b, TypeViewModel model)
        {
            using (b.Block($"export const {model.Name} = domain.enums.{model.Name} ="))
            {
                b.StringProp("name", model.Name.ToCamelCase());
                b.StringProp("displayName", model.DisplayName);
                b.StringProp("type", "enum");

                string enumShape = string.Join("|", model.EnumValues.Select(ev => $"\"{ev.Value}\""));
                b.Line($"...getEnumMeta<{enumShape}>([");
                foreach (var value in model.EnumValues)
                {
                    // TODO: allow for localization of displayName
                    b.Indented($"{{ value: {value.Key}, strValue: '{value.Value}', displayName: '{value.Value}' }},");
                }
                b.Line("]),");
            }
        }

        /// <summary>
        /// For a given type, if that type is a primitive type,
        /// return the string type discriminator
        /// </summary>
        /// <param name="type"></param>
        /// <returns>"number", "boolean", "string", "enum", or "date"</returns>
        private string GetPrimitiveTypeDiscriminator(TypeViewModel type)
        {
            if (type.IsNumber
                || type.IsBool
                || type.IsString
                || type.IsByteArray
            ) return type.TsType;

            if (type.IsEnum) return "enum";
            if (type.IsDate) return "date";
            return null;
        }

        private void WriteClassPropertiesMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block("props:", ','))
            {
                foreach (var prop in model.ClientProperties)
                {
                    WriteClassPropertyMetadata(b, model, prop);
                }
            }
        }

        string GetClassMetadataRef(ClassViewModel obj = null)
        {
            // We need to qualify with "domain." instead of the exported const
            // because in the case of a self-referential property, TypeScript can't handle recursive implicit type definitions.

            return $"(domain.types.{obj.ViewModelClassName} as {(obj.IsDbMappedType ? "ModelType" : "ObjectType")})";
        }

        private void WriteClassPropertyMetadata(TypeScriptCodeBuilder b, ClassViewModel model, PropertyViewModel prop)
        {
            using (b.Block($"{prop.JsVariable}:", ','))
            {
                WriteValueCommonMetadata(b, prop);

                if (prop.Object != null)
                {
                    if (prop.Object.IsDbMappedType)
                    {
                        if (prop.Type.IsPOCO && prop.ObjectIdProperty != null)
                        {
                            // Reference navigations
                            // TS Type: "ModelProperty"
                            b.StringProp("role", "referenceNavigation");
                            b.Line($"get foreignKey() {{ return {GetClassMetadataRef(model)}.props.{prop.ObjectIdProperty.JsVariable} as ForeignKeyProperty }},");
                            b.Line($"get principalKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.Object.PrimaryKey.JsVariable} as PrimaryKeyProperty }},");
                        }
                        else if (prop.Type.IsCollection && prop.Object.PrimaryKey != null)
                        {
                            // Collection navigations
                            // TS Type: "ModelCollectionProperty"
                            b.StringProp("role", "collectionNavigation");
                            b.Line($"get foreignKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.InverseProperty.ObjectIdProperty.JsVariable} as ForeignKeyProperty }},");
                        }
                    }
                    else
                    {
                        // External types, and collections of such
                        b.StringProp("role", "value");
                    }
                }
                else if (prop.Type.IsCollection)
                {
                    // Primitive collections
                    b.StringProp("role", "value");
                }
                else
                {
                    // All non-object/collection properties:
                    if (prop.IsPrimaryKey)
                    {
                        b.StringProp("role", "primaryKey");
                    }
                    else if (prop.IsForeignKey && prop.IdPropertyObjectProperty.PureTypeOnContext)
                    {
                        var principalProp = prop.IdPropertyObjectProperty;
                        b.StringProp("role", "foreignKey");
                        b.Line($"get principalKey() {{ return {GetClassMetadataRef(principalProp.Object)}.props.{principalProp.Object.PrimaryKey.JsVariable} as PrimaryKeyProperty }},");
                        b.Line($"get principalType() {{ return {GetClassMetadataRef(principalProp.Object)} }},");
                        b.Line($"get navigationProp() {{ return {GetClassMetadataRef(model)}.props.{prop.IdPropertyObjectProperty.JsVariable} as ModelProperty }},");
                    }
                    else
                    {
                        b.StringProp("role", "value");
                    }
                }

            }
        }

        /// <summary>
        /// Write the metadata for all methods of a class
        /// </summary>
        private void WriteClassMethodMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block("methods:", ','))
            {
                foreach (var method in model.ClientMethods)
                {
                    WriteClassMethodMetadata(b, model, method);
                }
            }
        }

        /// <summary>
        /// Write the metadata for an entire method
        /// </summary>
        private void WriteClassMethodMetadata(TypeScriptCodeBuilder b, ClassViewModel model, MethodViewModel method)
        {
            using (b.Block($"{method.JsVariable}:", ','))
            {

                b.StringProp("name", method.JsVariable);
                b.StringProp("displayName", method.DisplayName);

                using (b.Block("params:", ','))
                {
                    foreach (var param in method.ClientParameters)
                    {
                        WriteMethodParameterMetadata(b, method, param);
                    }
                }

                using (b.Block("return:", ','))
                {
                    b.StringProp("name", "$return");
                    b.StringProp("displayName", "Result"); // TODO: i18n
                    b.StringProp("role", "value");
                    WriteTypeCommonMetadata(b, method.ResultType);
                }
            }
        }

        /// <summary>
        /// Write the metadata for a specific parameter to a specific method
        /// </summary>
        private void WriteMethodParameterMetadata(TypeScriptCodeBuilder b, MethodViewModel method, ParameterViewModel parameter)
        {
            using (b.Block($"{parameter.JsVariable}:", ','))
            {
                WriteValueCommonMetadata(b, parameter);
                b.StringProp("role", "value");
            }
        }


        /// <summary>
        /// Write metadata common to all value representations, like properties and method parameters.
        /// </summary>
        private void WriteValueCommonMetadata(TypeScriptCodeBuilder b, IValueViewModel value)
        {
            b.StringProp("name", value.JsVariable);
            b.StringProp("displayName", value.DisplayName);

            WriteTypeCommonMetadata(b, value.Type);
        }

        /// <summary>
        /// Write metadata common to all type representations, 
        /// like properties, method parameters, method returns, etc.
        /// </summary>
        private void WriteTypeCommonMetadata(TypeScriptCodeBuilder b, TypeViewModel type)
        {
            void WriteTypeDiscriminator(string propName, TypeViewModel t)
            {
                var kind = t.TsTypeKind;
                switch (kind)
                {
                    case TypeDiscriminator.Unknown:
                        // We assume any unknown props are strings.
                        b.Line("// Type not supported natively by Coalesce - falling back to string.");
                        b.StringProp(propName, "string");
                        break;

                    default:
                        b.StringProp(propName, kind.ToString().ToLowerInvariant());
                        break;
                }
            }

            void WriteTypeDef(string propName, TypeViewModel t)
            {
                var kind = t.TsTypeKind;
                switch (kind)
                {
                    case TypeDiscriminator.Enum:
                        b.Line($"get {propName}() {{ return domain.enums.{t.Name} }},");
                        break;

                    case TypeDiscriminator.Model:
                    case TypeDiscriminator.Object:
                        b.Line($"get {propName}() {{ return {GetClassMetadataRef(t.ClassViewModel)} }},");
                        break;
                }
            }


            WriteTypeDiscriminator("type", type);
            WriteTypeDef("typeDef", type);

            // For collections, write the references to the underlying type.
            if (type.TsTypeKind == TypeDiscriminator.Collection)
            {
                if (type.PureType.TsTypeKind == TypeDiscriminator.Collection)
                {
                    throw new InvalidOperationException("Collections of collections aren't supported by Coalesce as exposed types");
                }

                using (b.Block($"itemType:", ','))
                {
                    b.StringProp("name", "$collectionItem");
                    b.StringProp("displayName", "");
                    b.StringProp("role", "value");
                    WriteTypeCommonMetadata(b, type.PureType);
                }
            }
        }
    }
}
