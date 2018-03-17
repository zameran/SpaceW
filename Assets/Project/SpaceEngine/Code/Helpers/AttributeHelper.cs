#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: 2017.05.02
// Creation Time: 5:45 PM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Extension class for working with <see cref="Attribute"/>.
/// Contains some helpful methods to working with attributes.
/// 
/// <see cref="AttributeHelper"/> is a collection of methods developed 
/// to help retrieve <see cref="Attribute"/> information for Types and methods in C# through Reflection.
/// 
/// <see cref="AttributeHelper"/> uses caching to rise up speed of access.
/// </summary>
public static class AttributeHelper
{
    private static readonly Dictionary<object, List<Attribute>> _attributeCache = new Dictionary<object, List<Attribute>>();

    /// <summary>
    /// <see cref="Attribute"/> cache. Readonly.
    /// </summary>
    public static Dictionary<object, List<Attribute>> AttributeCache { get { return _attributeCache; } }

    /// <summary>
    /// Gets all attributes of type.
    /// </summary>
    /// <typeparam name="TType">Type.</typeparam>
    /// <returns>Returns list, that contains all attributes of defined type.</returns>
    public static List<Attribute> GetTypeAttributes<TType>()
    {
        return GetTypeAttributes(typeof(TType));
    }

    /// <summary>
    /// Gets all attributes of type.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <returns>Returns list, that contains all attributes of defined type.</returns>
    public static List<Attribute> GetTypeAttributes(Type type)
    {
        return LockAndGetAttributes(type, tp => ((Type)tp).GetCustomAttributes(true));
    }

    public static List<TAttributeType> GetTypeAttributes<TAttributeType>(Type type, Func<TAttributeType, bool> predicate = null)
    {
        return GetTypeAttributes(type).Where<Attribute, TAttributeType>().Where(attr => predicate == null || predicate(attr)).ToList();
    }

    public static List<TAttributeType> GetTypeAttributes<TType, TAttributeType>(Func<TAttributeType, bool> predicate = null)
    {
        return GetTypeAttributes(typeof(TType), predicate);
    }

    public static TAttributeType GetTypeAttribute<TType, TAttributeType>(Func<TAttributeType, bool> predicate = null)
    {
        return GetTypeAttribute(typeof(TType), predicate);
    }

    public static TAttributeType GetTypeAttribute<TAttributeType>(Type type, Func<TAttributeType, bool> predicate = null)
    {
        return GetTypeAttributes<TAttributeType>(type, predicate).FirstOrDefault();
    }

    public static bool HasTypeAttribute<TType, TAttributeType>(Func<TAttributeType, bool> predicate = null)
    {
        return HasTypeAttribute<TAttributeType>(typeof(TType), predicate);
    }

    public static bool HasTypeAttribute<TAttributeType>(Type type, Func<TAttributeType, bool> predicate = null)
    {
        return GetTypeAttribute<TAttributeType>(type, predicate) != null;
    }

    public static List<Attribute> GetMemberAttributes<TType>(Expression<Func<TType, object>> action)
    {
        return GetMemberAttributes(GetMember(action));
    }

    public static List<TAttributeType> GetMemberAttributes<TType, TAttributeType>(Expression<Func<TType, object>> action, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
    {
        return GetMemberAttributes<TAttributeType>(GetMember(action), predicate);
    }

    public static TAttributeType GetMemberAttribute<TType, TAttributeType>(Expression<Func<TType, object>> action, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
    {
        return GetMemberAttribute<TAttributeType>(GetMember(action), predicate);
    }

    public static bool HasMemberAttribute<TType, TAttributeType>(Expression<Func<TType, object>> action, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
    {
        return GetMemberAttribute(GetMember(action), predicate) != null;
    }

    public static List<Attribute> GetMemberAttributes(this MemberInfo memberInfo)
    {
        return LockAndGetAttributes(memberInfo, mi => ((MemberInfo)mi).GetCustomAttributes(true));
    }

    public static List<TAttributeType> GetMemberAttributes<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
    {
        return GetMemberAttributes(memberInfo).Where<Attribute, TAttributeType>().Where(attr => predicate == null || predicate(attr)).ToList();
    }

    public static TAttributeType GetMemberAttribute<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
    {
        return GetMemberAttributes<TAttributeType>(memberInfo, predicate).FirstOrDefault();
    }

    public static bool HasMemberAttribute<TAttributeType>(this MemberInfo memberInfo, Func<TAttributeType, bool> predicate = null) where TAttributeType : Attribute
    {
        return memberInfo.GetMemberAttribute<TAttributeType>(predicate) != null;
    }

    private static IEnumerable<TType> Where<X, TType>(this IEnumerable<X> list)
    {
        return list.Where(item => item is TType).Cast<TType>();
    }

    private static TType FirstOrDefault<X, TType>(this IEnumerable<X> list)
    {
        return list.Where<X, TType>().FirstOrDefault();
    }

    private static List<Attribute> LockAndGetAttributes(object key, Func<object, object[]> retrieveValue)
    {
        return LockAndGet<object, List<Attribute>>(_attributeCache, key, mi => retrieveValue(mi).Cast<Attribute>().ToList());
    }

    private static TValue LockAndGet<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> retrieveValue)
    {
        var value = default(TValue);

        lock (dictionary)
        {
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
        }

        value = retrieveValue(key);

        lock (dictionary)
        {
            if (dictionary.ContainsKey(key) == false)
            {
                dictionary.Add(key, value);
            }

            return value;
        }
    }

    private static MemberInfo GetMember<T>(Expression<Func<T, object>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;

        if (memberExpression != null)
        {
            return memberExpression.Member;
        }

        var unaryExpression = expression.Body as UnaryExpression;

        if (unaryExpression != null)
        {
            memberExpression = unaryExpression.Operand as MemberExpression;

            if (memberExpression != null)
            {
                return memberExpression.Member;
            }

            var methodCall = unaryExpression.Operand as MethodCallExpression;

            if (methodCall != null)
            {
                return methodCall.Method;
            }
        }

        return null;
    }
}