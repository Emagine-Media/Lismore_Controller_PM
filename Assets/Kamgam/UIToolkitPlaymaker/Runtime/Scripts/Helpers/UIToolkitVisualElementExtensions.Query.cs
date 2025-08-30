#if PLAYMAKER

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    public static partial class UIToolkitVisualElementExtensions
    {
        public static VisualElement QueryType(this UIDocument document, UIElementType type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null)
        {
            var sysType = UIElementTypeUtils.GetElementType(type);
            if (sysType != null)
            {
                return QueryType(document, sysType, name, className, predicate);
            }

            return null;
        }

        public static VisualElement QueryType(this VisualElement element, UIElementType type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null)
        {
            var sysType = UIElementTypeUtils.GetElementType(type);
            if (sysType != null)
            {
                return QueryType(element, sysType, name, className, predicate);
            }

            return null;
        }

        public static VisualElement QueryType(this UIDocument document, System.Type type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null)
        {
            if (document == null || document.rootVisualElement == null)
            {
                return null;
            }

            return QueryType(document.rootVisualElement, type, name, className, predicate);
        }

        public static VisualElement QueryType(this VisualElement element, System.Type type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null)
        {
            if (element == null)
            {
                return null;
            }

            var iter = element.Query<VisualElement>(name, className).Build();
            foreach (var ele in iter)
            {
                if (predicate != null && !predicate.Invoke(ele))
                    continue;

                var eleType = ele.GetType();
                if (eleType == type || eleType.IsSubclassOf(type))
                {
                    return ele;
                }
            }

            return null;
        }

        public static List<VisualElement> QueryTypes(this UIDocument document, UIElementType type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null, List<VisualElement> results = null)
        {
            prepareResults(ref results);

            var sysType = UIElementTypeUtils.GetElementType(type);
            if (sysType != null)
            {
                return QueryTypes(document, sysType, name, className, predicate, results);
            }

            return results;
        }

        public static List<VisualElement> QueryTypes(this UIDocument document, System.Type type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null, List<VisualElement> results = null)
        {
            prepareResults(ref results);

            if (document == null || document.rootVisualElement == null)
            {
                return results;
            }
            
            document.rootVisualElement.QueryTypes(type, name, className, predicate, results);

            return results;
        }

        public static List<VisualElement> QueryTypes(this VisualElement element, UIElementType type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null, List<VisualElement> results = null)
        {
            prepareResults(ref results);

            var sysType = UIElementTypeUtils.GetElementType(type);
            if (sysType != null)
            {
                return QueryTypes(element, sysType, name, className, predicate, results);
            }

            return results;
        }

        public static List<VisualElement> QueryTypes(this VisualElement element, System.Type type, string name = null, string className = null, System.Predicate<VisualElement> predicate = null, List<VisualElement> results = null)
        {
            prepareResults(ref results);

            if (element == null)
            {
                return results;
            }

            var iter = element.Query<VisualElement>(name, className).Build();
            foreach (var ele in iter)
            {
                if (predicate != null && !predicate.Invoke(ele))
                    continue;

                var eleType = ele.GetType();
                if (eleType == type || eleType.IsSubclassOf(type))
                {
                    results.Add(ele);
                }
            }

            return results;
        }

        static void prepareResults(ref List<VisualElement> results)
        {
            if (results == null)
                results = new List<VisualElement>();

            results.Clear();
        }
    }
}
#endif