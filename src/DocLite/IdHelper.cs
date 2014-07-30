using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace DocLite
{
    internal class IdHelper
    {
        private IDocLiteConfiguration _configuration;

        internal IdHelper(IDocLiteConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void EnsureAllGuidsIdsInObjectGraphAreNotEmpty(object graph)
        {
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

            var descriptor = _configuration.GetIdProperty(graph.GetType());

            if (descriptor == null)
            {
                return;
            }

            if (descriptor.PropertyType == typeof(Guid) && descriptor.CanWrite)
            {
                var value = (Guid)descriptor.GetValue(graph, new object[0]);
                if (value == Guid.Empty)
                    descriptor.SetValue(graph, NewSequentialGuid(), new object[0]);
            }
        }

        public object GetId(object target)
        {
            var descriptor = _configuration.GetIdProperty(target.GetType());

            if (descriptor == null) return null;
            return descriptor.GetValue(target, new object[0]);
        }

        public Type IdType(object target)
        {
            var descriptor = _configuration.GetIdProperty(target.GetType());

            if (descriptor == null)
            {
                return null;
            }

            return descriptor.PropertyType;
        }

        public void SetId(object target, object id)
        {
            var descriptor = _configuration.GetIdProperty(target.GetType());

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