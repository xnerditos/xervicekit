using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace XKit.Lib.Common.Utility {

    public class CaseInsensitiveDynamicObject : DynamicObject, IDictionary<string, object> {

        private readonly IDictionary<string, object> Dictionary = 
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // =====================================================================
        // IDictionary<string, object>
        // =====================================================================

        ICollection<string> IDictionary<string, object>.Keys => Dictionary.Keys;
        ICollection<object> IDictionary<string, object>.Values => Dictionary.Values;
        int ICollection<KeyValuePair<string, object>>.Count => Dictionary.Count;
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => Dictionary.IsReadOnly;

        object IDictionary<string, object>.this [string key] { 
            get => Dictionary[key];
            set => Dictionary[key] = value;
        }

        void IDictionary<string, object>.Add(string key, object value) 
            => Dictionary.Add(key, value);

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) 
            => Dictionary.Add(item);

        void ICollection<KeyValuePair<string, object>>.Clear() 
            => Dictionary.Clear();

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) 
            => Dictionary.Contains(item);

        bool IDictionary<string, object>.ContainsKey(string key) 
            => Dictionary.ContainsKey(key);

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) 
            => Dictionary.CopyTo(array, arrayIndex);

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() 
            => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        bool IDictionary<string, object>.Remove(string key) 
            => Dictionary.Remove(key);

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) 
            => Dictionary.Remove(item);

        bool IDictionary<string, object>.TryGetValue(string key, out object value) 
            => Dictionary.TryGetValue(key, out value);

        // =====================================================================
        // DynamicObject overrides
        // =====================================================================

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (this.Dictionary.ContainsKey(binder.Name)) {
                result = this.Dictionary[binder.Name];
                return true;
            }
            result = null;
            return false;
            //return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            this.Dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            if (this.Dictionary.ContainsKey(binder.Name) && this.Dictionary[binder.Name] is Delegate) {
                Delegate del = this.Dictionary[binder.Name] as Delegate;
                if (del != null) {
                    result = del.DynamicInvoke(args);
                    return true;
                }
            }
            result = null;
            return false;
            //return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder) {
            if (this.Dictionary.ContainsKey(binder.Name)) {
                this.Dictionary.Remove(binder.Name);
                return true;
            }
            return false;
            //return base.TryDeleteMember(binder);
        }

        public override IEnumerable<string> GetDynamicMemberNames() => Dictionary.Keys;
    }
}