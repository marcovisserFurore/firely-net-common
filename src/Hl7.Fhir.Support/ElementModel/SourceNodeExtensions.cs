﻿using System.Linq;
using System.Collections.Generic;
using System;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.ElementModel.Adapters;

namespace Hl7.Fhir.ElementModel
{

    public static class SourceNodeExtensions
    {
        public static IEnumerable<ISourceNode> Children(this IEnumerable<ISourceNode> navigators, string name = null) =>
            navigators.SelectMany(n => n.Children(name));


        public static IEnumerable<ISourceNode> Descendants(this ISourceNode navigator)
        {
            foreach (var child in navigator.Children())
            {
                yield return child;
                foreach (var grandchild in child.Descendants()) yield return grandchild;
            }
        }

        public static IEnumerable<ISourceNode> DescendantsAndSelf(this ISourceNode navigator)
        {
            return (new[] { navigator }).Concat(navigator.Descendants());
        }

        public static IEnumerable<ISourceNode> DescendantsAndSelf(this IEnumerable<ISourceNode> navigators)
        {
            return navigators.SelectMany(n => n.DescendantsAndSelf());
        }

        public static void Visit(this ISourceNode navigator, Action<ISourceNode> visitor)
        {
            visitor(navigator);
            foreach (var child in navigator.Children())
            {
                Visit(child, visitor);
            }
        }

        private static void visit(this ISourceNode navigator, Action<int, ISourceNode> visitor, int depth = 0)
        {
            visitor(depth, navigator);
            foreach (var child in navigator.Children())
            {
                visit(child, visitor, depth + 1);
            }
        }

        public static void Visit(this ISourceNode navigator, Action<int, ISourceNode> visitor) => navigator.visit(visitor, 0);

        public static IDisposable Catch(this ISourceNode source, ExceptionNotificationHandler handler) =>
            source is IExceptionSource s ? s.Catch(handler) : throw new NotImplementedException("source does not implement IExceptionSource");

        public static IEnumerable<object> Annotations(this ISourceNode nav, Type type) =>
            nav is IAnnotated ann ? ann.Annotations(type) : Enumerable.Empty<object>();

        public static bool InPipeline(this ISourceNode navigator, Type componentType) =>
                    navigator is IAnnotated ia ? ia.Annotation(componentType) != null : false;

        public static bool InPipeline<T>(this ISourceNode navigator) =>
                    navigator.InPipeline(navigator.GetType());

        public static ElementDefinitionSummary GetElementDefinitionSummary(this ISourceNode navigator) =>
                navigator is IAnnotated ia ? ia.GetElementDefinitionSummary() : null;

        public static string GetResourceType(this ISourceNode node) => 
            node is IAnnotated ia ? ia.GetResourceType() : null;

        public static List<ExceptionNotification> VisitAndCatch(this ISourceNode nav)
        {
            var errors = new List<ExceptionNotification>();

            using (nav.Catch((o, arg) => errors.Add(arg)))
            {
                nav.VisitAll();
            }

            return errors;
        }


        public static void VisitAll(this ISourceNode nav) => nav.Visit(n => { var dummy = n.Text; });

        [Obsolete("IElementNavigator should be replaced by the IElementNode interface, which is returned by the parsers")]
        public static IElementNavigator ToElementNavigator(this ISourceNode sourceNav, IStructureDefinitionSummaryProvider provider, string type = null) =>
            sourceNav.ToElementNode(provider, type).ToElementNavigator();

        [Obsolete("WARNING! For internal API use only. Turning an untyped SourceNode into a typed ElementNavigator without providing" +
            "type information (see other overload) will cause side-effects with components in the API that are not prepared to deal with" +
            "missing type information. Please don't use this overload unless you know what you are doing.")]
        public static IElementNavigator ToElementNavigator(this ISourceNode sourceNav, string type = null) =>
            new SourceNodeToElementNavAdapter(sourceNav);

        public static IElementNode ToElementNode(this ISourceNode sourceNav, IStructureDefinitionSummaryProvider provider, string type = null)
        {
            if (provider == null) throw Error.ArgumentNull(nameof(provider));
                return new TypedNode(sourceNav, type, provider);
        }

        [Obsolete("WARNING! For internal API use only. Turning an untyped SourceNode into a typed ElementNode without providing" +
    "type information (see other overload) will cause side-effects with components in the API that are not prepared to deal with" +
    "missing type information. Please don't use this overload unless you know what you are doing.")]
        public static IElementNode ToElementNode(this ISourceNode sourceNav) =>
                new SourceNodeToElementNodeAdapter(sourceNav);

    }
}