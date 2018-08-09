﻿/* 
 * Copyright (c) 2017, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using System;
using System.Linq;

namespace Hl7.Fhir.ElementModel
{
    public static class ElementNavigatorComparator
    {
        public struct ComparisonResult
        {
            public bool Success;
            public string FailureLocation;
            public string Details;

            public static ComparisonResult Fail(string location, string details = null) =>
                new ComparisonResult { Success = false, FailureLocation = location, Details = details };

            public static readonly ComparisonResult OK = new ComparisonResult() { Success = true };
        }


        public static ComparisonResult IsEqualTo(this IElementNavigator expected, IElementNavigator actual)
        {
            if (expected.Name != actual.Name)
                return ComparisonResult.Fail(actual.Location, $"name: was '{actual.Name}', expected '{expected.Name}'");
            if (!Object.Equals(expected.Value,actual.Value))
                return ComparisonResult.Fail(actual.Location, $"value: was '{actual.Value}', expected '{expected.Value}'");
            if (expected.Type != actual.Type && actual.Type != null) return ComparisonResult.Fail(actual.Location, $"type: was '{actual.Type}', expected '{expected.Type}'");
            if (expected.Location != actual.Location) ComparisonResult.Fail(actual.Location, $"location: was '{actual.Location}', expected '{expected.Location}'");

            // Ignore ordering (only relevant to xml)
            var childrenExp = expected.Children().OrderBy(e => e.Name);
            var childrenActual = actual.Children().OrderBy(e => e.Name).GetEnumerator();

            // Don't compare lengths, as this would require complete enumeration of both collections
            // just enumerate through the lists, comparing each item as they go.
            // first fail (or list end) will drop out.
            foreach (var exp in childrenExp)
            {
                if (!childrenActual.MoveNext())
                    ComparisonResult.Fail(actual.Location, $"number of children was different");

                var result = exp.IsEqualTo(childrenActual.Current);
                if (!result.Success)
                    return result;
            }
            if (childrenActual.MoveNext())
                ComparisonResult.Fail(actual.Location, $"number of children was different");

            return ComparisonResult.OK;
        }
    }
}
