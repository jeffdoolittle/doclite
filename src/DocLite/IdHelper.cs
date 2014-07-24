using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DocLite
{
    internal class IdHelper
    {
        private string _idPropertyName;
        private bool _initialized;

        public void Initialize(string idPropertyName)
        {
            _idPropertyName = idPropertyName;
            _initialized = true;
        }

        private void GuardInitialized()
        {
            if (!_initialized) throw new InvalidOperationException("IdHelper must be initialized");
        }

        public void EnsureAllGuidsIdsInObjectGraphAreNotEmpty(object graph)
        {
            GuardInitialized();

            var typeDescriptors = TypeDescriptor.GetProperties(graph).Cast<PropertyDescriptor>();
            foreach (var typeDescriptor in typeDescriptors)
            {
                if (typeDescriptor.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var enumerable = (IEnumerable)typeDescriptor.GetValue(graph);
                    if (enumerable == null) continue;
                    foreach (var document in enumerable)
                        EnsureAllGuidsIdsInObjectGraphAreNotEmpty(document);
                }
            }

            var descriptor = graph.GetType().GetProperty(_idPropertyName,
                                                          BindingFlags.Public | BindingFlags.NonPublic |
                                                          BindingFlags.Instance);
            if (descriptor == null) return;

            if (descriptor.PropertyType == typeof(Guid) && descriptor.CanWrite)
            {
                var value = (Guid)descriptor.GetValue(graph, new object[0]);
                if (value == Guid.Empty)
                    descriptor.SetValue(graph, NewSequentialGuid(), new object[0]);
            }
        }

        public void AutoIncrementIntegerIds(object document)
        {
            
        }

        public object GetId(object target)
        {
            GuardInitialized();

            var descriptor = target.GetType().GetProperty(_idPropertyName,
                                                          BindingFlags.Public | BindingFlags.NonPublic |
                                                          BindingFlags.Instance);
            if (descriptor == null) return null;
            return descriptor.GetValue(target, new object[0]);
        }

        public Type IdType(object target)
        {
            var descriptor = target.GetType().GetProperty(_idPropertyName,
                                              BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance);

            if (descriptor == null)
            {
                return null;
            }

            return descriptor.PropertyType;
        }

        public void SetId(object target, object id)
        {
            GuardInitialized();

            var descriptor = target.GetType().GetProperty(_idPropertyName,
                                                          BindingFlags.Public | BindingFlags.NonPublic |
                                                          BindingFlags.Instance);
            descriptor.SetValue(target, id, new object[0]);
        }

        private static class NativeMethods
        {
            [DllImport("rpcrt4.dll", SetLastError = true)]
            public static extern int UuidCreateSequential(out Guid guid);
        }

        private static Guid NewSequentialGuid()
        {
            const int rpcSOk = 0;
            Guid guid;
            var result = NativeMethods.UuidCreateSequential(out guid);
            return result == rpcSOk ? guid : Guid.NewGuid();
        }
    }
}