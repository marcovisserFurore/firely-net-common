﻿using System.Linq;
using System.Collections.Generic;
using System;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.ElementModel
{

    public static class ElementNavigatorExtensions
    {
        public static IEnumerable<IElementNavigator> Children(this IElementNavigator navigator, string name = null)
        {
            var nav = navigator.Clone();
            if (nav.MoveToFirstChild(name))
            {
                do
                {
                    yield return nav.Clone();
                }
                while (nav.MoveToNext(name));
            }
        }


        public static IEnumerable<IElementNavigator> Children(this IEnumerable<IElementNavigator> navigators, string name = null) =>
            navigators.SelectMany(n => n.Children(name));

        //Since moving to the first child can be very expensive on stacked navigators, we should not offer this
        //functionality, unless by adding it to IElementNavigator
        [Obsolete("This method can be prohibitively expensive, and should not be used anymore.")]
        public static bool HasChildren(this IEnumerable<IElementNavigator> navigators, string name = null) => navigators.Children(name).Any();

        [Obsolete("This method can be prohibitively expensive, and should not be used anymore.")]
        public static bool HasChildren(this IElementNavigator navigator, string name = null) => navigator.Children(name).Any();

        public static IEnumerable<IElementNavigator> Descendants(this IElementNavigator navigator)
        {
            foreach (var child in navigator.Children())
            {
                yield return child;
                foreach (var grandchild in child.Descendants()) yield return grandchild;
            }
        }

        public static IEnumerable<IElementNavigator> DescendantsAndSelf(this IElementNavigator navigator)
        {
            return (new[] { navigator }).Concat(navigator.Descendants());
        }

        public static IEnumerable<IElementNavigator> DescendantsAndSelf(this IEnumerable<IElementNavigator> navigators)
        {
            return navigators.SelectMany(n => n.DescendantsAndSelf());
        }

        public static void Visit(this IElementNavigator navigator, Action<IElementNavigator> visitor)
        {
            visitor(navigator);
            foreach (var child in navigator.Children())
            {
                Visit(child, visitor);
            }
        }

        private static void visit(this IElementNavigator navigator, Action<int, IElementNavigator> visitor, int depth = 0)
        {
            visitor(depth, navigator);
            foreach (var child in navigator.Children())
            {
                visit(child, visitor, depth + 1);
            }
        }

        public static void Visit(this IElementNavigator navigator, Action<int, IElementNavigator> visitor) => navigator.visit(visitor, 0);

        public static IEnumerable<object> Annotations(this IElementNavigator nav, Type type) =>
                nav is IAnnotated ann ? ann.Annotations(type) : Enumerable.Empty<object>();
        public static T Annotation<T>(this IElementNavigator nav) where T:class =>
            nav is IAnnotated ann ? ann.Annotation<T>() : null;


        public static bool InPipeline(this IElementNavigator navigator, Type componentType) =>
            navigator is IAnnotated ia ? ia.Annotation(componentType) != null : false;
        public static bool InPipeline<T>(this IElementNavigator navigator) =>
            navigator.InPipeline(navigator.GetType());

        public static string GetResourceType(this IElementNavigator navigator) => 
            navigator is IAnnotated ia ? ia.GetResourceType() : null;

        public static List<ExceptionNotification> VisitAndCatch(this IElementNavigator nav)
        {
            var errors = new List<ExceptionNotification>();

            using (nav.Catch((o, arg) => errors.Add(arg)))
            {
                nav.Visit(n => { var dummy = n.Value; });
            }

            return errors;
        }

        public static ElementDefinitionSummary GetElementDefinitionSummary(this IElementNavigator navigator) =>
            navigator is IAnnotated ia ? ia.GetElementDefinitionSummary() : null;

        public static IElementNode ToElementNode(this IElementNavigator nav) => 
            new ElementNavToElementNodeAdapter(nav);
    }
}