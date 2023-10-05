using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SoT_Helper.Extensions
{
    public class Nameof<T>
    {
        public static string Property<TProp>(Expression<Func<T, TProp>> expression)
        {
            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("'expression' should be a member expression");
            return body.Member.Name;
        }
    }

    /// <summary>
    /// Extensions methos for using reflection to get / set member values
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets the public or private member using reflection.
        /// </summary>
        /// <param name="obj">The source target.</param>
        /// <param name="memberName">Name of the field or property.</param>
        /// <returns>the value of member</returns>
        public static object GetMemberValue(this object obj, string memberName)
        {
            var memInf = GetMemberInfo(obj, memberName);

            if (memInf == null)
                throw new Exception("memberName");

            if (memInf is PropertyInfo)
                return memInf.As<PropertyInfo>().GetValue(obj, null);

            if (memInf is FieldInfo)
                return memInf.As<FieldInfo>().GetValue(obj);

            throw new Exception();
        }

        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }

        /// <summary>
        /// Gets the public or private member using reflection.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="memberName">Name of the field or property.</param>
        /// <returns>Old Value</returns>
        public static object SetMemberValue(this object obj, string memberName, object newValue)
        {
            var memInf = GetMemberInfo(obj, memberName);

            if (memInf == null)
                throw new Exception("memberName");

            var oldValue = obj.GetMemberValue(memberName);

            if (memInf is PropertyInfo)
                memInf.As<PropertyInfo>().SetValue(obj, newValue, null);
            else if (memInf is FieldInfo)
                memInf.As<FieldInfo>().SetValue(obj, newValue);
            else
                throw new Exception();

            return oldValue;
        }

        /// <summary>
        /// Gets the member info
        /// </summary>
        /// <param name="obj">source object</param>
        /// <param name="memberName">name of member</param>
        /// <returns>instanse of MemberInfo corresponsing to member</returns>
        private static MemberInfo GetMemberInfo(object obj, string memberName)
        {
            var prps = new List<PropertyInfo>();

            prps.Add(obj.GetType().GetProperty(memberName,
                                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                                BindingFlags.FlattenHierarchy));
            prps = prps.Where(i => !ReferenceEquals(i, null)).ToList();
            if (prps.Count != 0)
                return prps[0];

            var flds = new List<FieldInfo>();

            flds.Add(obj.GetType().GetField(memberName,
                                            BindingFlags.NonPublic | BindingFlags.Instance |
                                            BindingFlags.FlattenHierarchy));

            //to add more types of properties

            flds = flds.Where(i => !ReferenceEquals(i, null)).ToList();

            if (flds.Count != 0)
                return flds[0];

            return null;
        }

        /// <summary>
        /// Gets the method info
        /// </summary>
        /// <param name="obj">source object</param>
        /// <param name="methodName">name of method</param>
        /// <returns>instanse of MethodInfo corresponsing to method</returns>
        public static MethodInfo GetMethodInfo(this object obj, string methodName)
        {
            var prps = new List<MethodInfo>();

            prps.Add(obj.GetType().GetMethod(methodName,
                                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                                BindingFlags.FlattenHierarchy | BindingFlags.Static));
            prps = prps.Where(i => !ReferenceEquals(i, null)).ToList();
            if (prps.Count != 0)
                return prps[0];

            return null;
        }

        [System.Diagnostics.DebuggerHidden]
        private static T As<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
