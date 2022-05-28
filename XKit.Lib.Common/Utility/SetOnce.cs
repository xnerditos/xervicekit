using System;

namespace XKit.Lib.Common.Utility {
    public class SetOnceOrThrow<T> {
        private bool set;
        private T value;

        public SetOnceOrThrow(T defaultValue = default(T)) {
            value = defaultValue;
        }

        public T Value {
            get { return value; }
            set {
                if (set) { throw new Exception("Value already set"); }
                set = true;
                this.value = value;
            }
        }

        public bool HasValue => set;

        public static implicit operator T(SetOnceOrThrow<T> toConvert) {
            return toConvert.value;
        }
    }

    public class SetOnceOrIgnore<T> {
        private bool set;
        private T value;

        public SetOnceOrIgnore(T defaultValue = default(T)) {
            value = defaultValue;
        }

        public T Value {
            get { return value; }
            set {
                if (set) { return; }
                set = true;
                this.value = value;
            }
        }

        public bool HasValue => set;

        public static implicit operator T(SetOnceOrIgnore<T> toConvert) {
            return toConvert.value;
        }
    }
}