﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListExtensions.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf.SharpDX.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class CollectionExtensions
    {
        static class ArrayAccessor<T>
        {
            public static readonly Func<List<T>, T[]> Getter;

            static ArrayAccessor()
            {
                var dm = new DynamicMethod(
                    "get",
                    MethodAttributes.Static | MethodAttributes.Public,
                    CallingConventions.Standard,
                    typeof(T[]),
                    new Type[] { typeof(List<T>) },
                    typeof(ArrayAccessor<T>),
                    true);
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
                il.Emit(OpCodes.Ldfld,
                    typeof(List<T>).GetField("_items",
                    BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
                il.Emit(OpCodes.Ret); // Return field
                Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
            }
        }

        /// <summary>
        /// Gets the internal array of a <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="list">The respective list.</param>
        /// <returns>The internal array of the list.</returns>
        public static T[] GetInternalArray<T>(this List<T> list)
        {
            return ArrayAccessor<T>.Getter(list);
        }

        /// <summary>
        /// Tries to get a value from a <see cref="IDictionary{K,V}"/>.
        /// </summary>
        /// <typeparam name="K">The type of the key.</typeparam>
        /// <typeparam name="V">The type of the value.</typeparam>
        /// <param name="dict">The respective dictionary.</param>
        /// <param name="key">The respective key.</param>
        /// <returns>The value if exists, else <c>null</c>.</returns>
        public static V Get<K, V>(this IDictionary<K, V> dict, K key)
        {
            V val;
            if (dict.TryGetValue(key, out val))
            {
                return val;
            }

            return default(V);
        }
    }
}
