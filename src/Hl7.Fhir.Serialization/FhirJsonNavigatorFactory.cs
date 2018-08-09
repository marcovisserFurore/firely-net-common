﻿/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/ewoutkramer/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Hl7.Fhir.Serialization
{
    [Obsolete("Please use the equivalent functions on the FhirJsonNavigator factory class")]
    public struct JsonDomFhirNavigator
    {
        [Obsolete("Use FhirJsonNavigator.Untyped() instead")]
        public static IElementNavigator Create(JObject root, string rootName = null) => 
            FhirJsonNavigator.Untyped(root, rootName).ToElementNavigator();

        [Obsolete("Use FhirJsonNavigator.Untyped() instead")]
        public static IElementNavigator Create(JsonReader reader, string rootName = null) => 
            FhirJsonNavigator.Untyped(reader, rootName).ToElementNavigator();

        [Obsolete("Use FhirJsonNavigator.Untyped() instead")]
        public static IElementNavigator Create(string json, string rootName = null) =>
            FhirJsonNavigator.Untyped(json, rootName).ToElementNavigator();
    }

    public partial class FhirJsonNavigator
    {
        public static ISourceNode Untyped(JsonReader reader, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (reader == null) throw Error.ArgumentNull(nameof(reader));

            return createUntyped(reader, rootName, settings);
        }

        public static ISourceNode Untyped(string json, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (json == null) throw Error.ArgumentNull(nameof(json));

            using (var reader = SerializationUtil.JsonReaderFromJsonText(json))
            {
                return createUntyped(reader, rootName, settings);
            }
        }

        public static ISourceNode Untyped(JObject root, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (root == null) throw Error.ArgumentNull(nameof(root));

            return createUntyped(root, rootName, settings);
        }

        public static IElementNode ForResource(string json, IStructureDefinitionSummaryProvider provider, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (json == null) throw Error.ArgumentNull(nameof(json));
            if (provider == null) throw Error.ArgumentNull(nameof(provider));

            using (var reader = SerializationUtil.JsonReaderFromJsonText(json))
            {
                return createTyped(reader, null, rootName, provider, settings);
            }
        }

        public static IElementNode ForElement(string json, string type, IStructureDefinitionSummaryProvider provider, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (json == null) throw Error.ArgumentNull(nameof(json));
            if (type == null) throw Error.ArgumentNull(nameof(type));
            if (provider == null) throw Error.ArgumentNull(nameof(provider));

            using (var reader = SerializationUtil.JsonReaderFromJsonText(json))
            {
                return createTyped(reader, type, rootName, provider, settings);
            }
        }

        public static IElementNode ForResource(JsonReader reader, IStructureDefinitionSummaryProvider provider, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (reader == null) throw Error.ArgumentNull(nameof(reader));
            if (provider == null) throw Error.ArgumentNull(nameof(provider));

            return createTyped(reader, null, rootName, provider, settings);
        }

        public static IElementNode ForElement(JsonReader reader, string type, IStructureDefinitionSummaryProvider provider, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (reader == null) throw Error.ArgumentNull(nameof(reader));
            if (type == null) throw Error.ArgumentNull(nameof(type));
            if (provider == null) throw Error.ArgumentNull(nameof(provider));

            return createTyped(reader, type, rootName, provider, settings);
        }

        public static IElementNode ForResource(JObject root, IStructureDefinitionSummaryProvider provider, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (root == null) throw Error.ArgumentNull(nameof(root));
            if (provider == null) throw Error.ArgumentNull(nameof(provider));

            return createTyped(root, null, rootName, provider, settings);
        }

        public static IElementNode ForElement(JObject root, string type, IStructureDefinitionSummaryProvider provider, string rootName = null, FhirJsonNavigatorSettings settings = null)
        {
            if (root == null) throw Error.ArgumentNull(nameof(root));
            if (type == null) throw Error.ArgumentNull(nameof(type));
            if (provider == null) throw Error.ArgumentNull(nameof(provider));

            return createTyped(root, type, rootName, provider, settings);
        }


        private static ISourceNode createUntyped(JsonReader reader, string rootName, FhirJsonNavigatorSettings settings)
        {
            try
            {
                var doc = SerializationUtil.JObjectFromReader(reader);
                return createUntyped(doc, rootName, settings);
            }
            catch (FormatException fe)
            {
                return new ParseErrorStubNode(fe);
            }
        }

        private static ISourceNode createUntyped(JObject root, string rootName, FhirJsonNavigatorSettings settings)
        {
            var name = rootName ?? root.GetResourceTypeFromObject();

            if (name == null)
                throw Error.InvalidOperation("Root object has no type indication (resourceType) and therefore cannot be used to construct the navigator. Alternatively, specify a rootName using the parameter.");

            return new FhirJsonNode(root, name, settings);
        }

        private static IElementNode createTyped(JObject root, string type, string rootName, IStructureDefinitionSummaryProvider provider, FhirJsonNavigatorSettings settings) =>
            createUntyped(root, rootName ?? type?.ToLower(), settings).ToElementNode(provider, type);

        private static IElementNode createTyped(JsonReader reader, string type, string rootName, IStructureDefinitionSummaryProvider provider, FhirJsonNavigatorSettings settings) => 
            createUntyped(reader, rootName ?? type?.ToLower(), settings).ToElementNode(provider, type);
    }
}
