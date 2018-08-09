﻿/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/ewoutkramer/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Serialization
{
    public class ResourceTypeIndicator
    {
        public string ResourceType;
    }

    public static class SerializationNavigatorExtensions
    {
        public static string GetResourceType(this IAnnotated ia) => 
            ia.TryGetAnnotation<ResourceTypeIndicator>(out var rt) ? rt.ResourceType : null;
    }
}