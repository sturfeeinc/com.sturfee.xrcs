using System;
using System.Linq;
using UnityEngine;

namespace Sturfee.XRCS.Utils
{
    public static class GameObjectExtensions
    {
        public static bool CanDestroy(this GameObject go, Type t)
        {
            return !go.GetComponents<Component>().Any(c => Requires(c.GetType(), t));
        }

        private static bool Requires(Type obj, Type requirement)
        {
            //also check for m_Type1 and m_Type2 if required
            return Attribute.IsDefined(obj, typeof(RequireComponent)) &&
                   Attribute.GetCustomAttributes(obj, typeof(RequireComponent)).OfType<RequireComponent>()
                   .Any(rc => rc.m_Type0.IsAssignableFrom(requirement));
        }

        public static Bounds CalculateBounds(this GameObject g)
        {
            return g.transform.CalculateBounds();
        }

        public static Bounds GetBoundsWithChildren(this GameObject gameObject)
        {
            // GetComponentsInChildren() also returns components on gameobject which you call it on
            // you don't need to get component specially on gameObject
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            // If renderers.Length = 0, you'll get OutOfRangeException
            // or null when using Linq's FirstOrDefault() and try to get bounds of null
            Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds();

            // Or if you like using Linq
            // Bounds bounds = renderers.Length > 0 ? renderers.FirstOrDefault().bounds : new Bounds();

            // Start from 1 because we've already encapsulated renderers[0]
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i].enabled)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return bounds;
        }

        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.GetComponent<T>() == null)
            {
                gameObject.AddComponent<T>();
            }

            return gameObject.GetComponent<T>();
        }
    }

    public static class MonoBehaviourExtensions
    {
        public static bool CanDestroy(this MonoBehaviour monob, Type t)
        {
            return !monob.GetComponents<Component>().Any(c => Requires(c.GetType(), t));
        }

        private static bool Requires(Type obj, Type requirement)
        {
            //also check for m_Type1 and m_Type2 if required
            return Attribute.IsDefined(obj, typeof(RequireComponent)) &&
                   Attribute.GetCustomAttributes(obj, typeof(RequireComponent)).OfType<RequireComponent>()
                   .Any(rc => rc.m_Type0.IsAssignableFrom(requirement));
        }
    }

    public static class NumberExtensions
    {

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

    }

    public static class TransformExtensions
    {
        public static Bounds CalculateBounds(this Transform t)
        {
            Renderer renderer = t.GetComponentInChildren<Renderer>(false);
            if (renderer)
            {
                Bounds bounds = renderer.bounds;
                if (bounds.size == Vector3.zero && bounds.center != renderer.transform.position)
                {
                    bounds = TransformBounds(renderer.transform.localToWorldMatrix, bounds);
                }
                CalculateBounds(t, ref bounds);
                if (bounds.extents == Vector3.zero)
                {
                    bounds.extents = new Vector3(0.5f, 0.5f, 0.5f);
                }
                return bounds;
            }

            return new Bounds(t.position, new Vector3(0.5f, 0.5f, 0.5f));
        }

        private static void CalculateBounds(Transform t, ref Bounds totalBounds)
        {
            foreach (Transform child in t)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer)
                {
                    Bounds bounds = renderer.bounds;
                    if (bounds.size == Vector3.zero && bounds.center != renderer.transform.position)
                    {
                        bounds = TransformBounds(renderer.transform.localToWorldMatrix, bounds);
                    }
                    totalBounds.Encapsulate(bounds.min);
                    totalBounds.Encapsulate(bounds.max);
                }

                CalculateBounds(child, ref totalBounds);
            }
        }
        public static Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
        {
            var center = matrix.MultiplyPoint(bounds.center);

            // transform the local extents' axes
            var extents = bounds.extents;
            var axisX = matrix.MultiplyVector(new Vector3(extents.x, 0, 0));
            var axisY = matrix.MultiplyVector(new Vector3(0, extents.y, 0));
            var axisZ = matrix.MultiplyVector(new Vector3(0, 0, extents.z));

            // sum their absolute value to get the world extents
            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds { center = center, extents = extents };
        }


        private static Vector3[] s_Corners = new Vector3[4];
        public static Bounds CalculateRelativeRectTransformBounds(Transform root, Transform child)
        {
            RectTransform component = child as RectTransform;
            if (component == null)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            Vector3 v1 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 v2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Matrix4x4 worldToLocalMatrix = root.worldToLocalMatrix;

            component.GetWorldCorners(s_Corners);
            for (int i = 0; i < 4; ++i)
            {
                Vector3 lhs = worldToLocalMatrix.MultiplyPoint3x4(s_Corners[i]);
                v1 = Vector3.Min(lhs, v1);
                v2 = Vector3.Max(lhs, v2);
            }

            Bounds bounds = new Bounds(v1, Vector3.zero);
            bounds.Encapsulate(v2);
            return bounds;
        }

        public static Bounds CalculateRelativeRectTransformBounds(this Transform trans)
        {
            return CalculateRelativeRectTransformBounds(trans, trans);
        }
    }

}

