using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sample.Client.Model
{

    public class CustomTypeModel : ICustomTypeProvider, INotifyPropertyChanged
    {
        public Guid ID { get; set; }
        public DateTime Date { get; set; }
        public int Order { get; set; }

        private CustomType? _CType;
        private static readonly object PropertiesLock = new object();
        private static readonly List<CustomPropertyInfoHelper<CustomTypeModel>> Properties = new List<CustomPropertyInfoHelper<CustomTypeModel>>();
        private readonly Dictionary<string, object?> PropertyValues;
        public event PropertyChangedEventHandler? PropertyChanged;

        public CustomTypeModel()
        {
            PropertyValues = new Dictionary<string, object?>();
            lock (PropertiesLock)
            {
                foreach (var property in Properties)
                {
                    PropertyValues[property.Name] = null;
                }
            }
        }

        public Type GetCustomType()
        {
            _CType ??= new CustomType(typeof(CustomTypeModel));
            return _CType;
        }

        public static void AddProperty(string name, Type propertyType, List<Attribute>? attributes)
        {
            lock (PropertiesLock)
            {
                if (CheckIfNameExists(name))
                    return;
                var CPI = new CustomPropertyInfoHelper<CustomTypeModel>(name, propertyType, attributes ?? new List<Attribute>());
                Properties.Add(CPI);
            }
        }

        public void SetPropertyValue(string propertyName, object? value)
        {
            CustomPropertyInfoHelper<CustomTypeModel>? propertyInfo;
            lock (PropertiesLock)
            {
                propertyInfo = Properties.FirstOrDefault(prop => prop.Name == propertyName);
            }

            // If propertyInfo is null it's not a dynamic property (or not defined) — do nothing.
            if (propertyInfo is null)
                return;

            // If dynamic property exists but PropertyValues doesn't yet contain it (added later), create the key.
            if (!PropertyValues.ContainsKey(propertyName))
            {
                PropertyValues[propertyName] = null;
            }

            if (ValidateValueType(value, propertyInfo.PropertyType))
            {
                var current = PropertyValues[propertyName];
                if (!Equals(current, value))
                {
                    PropertyValues[propertyName] = value;
                    NotifyPropertyChanged(propertyName);
                }
            }
            // else ignore invalid type
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private bool ValidateValueType(object? value, Type type)
        {
            if (value == null)
            {
                if (!type.IsValueType) return true;
                return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
            }
            else
            {
                try
                {
                    // try convert to the target type
                    var converted = Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type, System.Globalization.CultureInfo.InvariantCulture);
                    return converted != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static bool CheckIfNameExists(string name)
        {
            lock (PropertiesLock)
            {
                return Properties.Any(p => p.Name == name)
                    || typeof(CustomTypeModel).GetProperties().Any(p => p.Name == name);
            }
        }

        public object? GetPropertyValue(string propertyName)
        {
            if (PropertyValues.ContainsKey(propertyName))
                return PropertyValues[propertyName];
            else
                return null;
        }

        public static void RemoveProperty(string PropertyName)
        {
            lock (PropertiesLock)
            {
                var ExistsItem = Properties.FirstOrDefault(w => w.Name.Equals(PropertyName));
                if (ExistsItem is not null)
                    Properties.Remove(ExistsItem);
            }
        }

        public static IEnumerable<CustomTypeModel> GenerateItems(IEnumerable<WeatherForeacast> weathers,int count = 500) {
            var result = new List<CustomTypeModel>();
            CustomTypeModel.AddProperty("BoolProperty", typeof(bool), new() { new DisplayAttribute() { Name = "Boolean property" } });
            CustomTypeModel.AddProperty("DateTimeProperty", typeof(DateTime), new() { new DisplayAttribute() { Name = "Date time property" } });
            CustomTypeModel.AddProperty("NavigationProperty", typeof(WeatherForeacast), new() { new DisplayAttribute() { Name = "Navigation property" } });
            CustomTypeModel.AddProperty("IntProperty", typeof(int), new() { new DisplayAttribute() { Name = "Integer property" } });
          
            for (var i = 1; i < count; i++)
            { 
                var item = new CustomTypeModel() { ID = Guid.NewGuid(),Order = i, Date= DateTime.Now.AddDays(Random.Shared.Next(-500,500)) };
                item.SetPropertyValue("BoolProperty", Random.Shared.Next(0,1) != 0);
                item.SetPropertyValue("DateTimeProperty", DateTime.Now.AddDays(Random.Shared.Next(-500, 500)));
                item.SetPropertyValue("NavigationProperty", weathers.ElementAt(Random.Shared.Next(0, weathers.Count() -1)));
                item.SetPropertyValue("IntProperty", Random.Shared.Next(0, 5000));
                result.Add(item);
            }
            return result;
        }


        private class CustomType : Type
        {
            private readonly Type _baseType;
            public CustomType(Type delegatingType) => _baseType = delegatingType;

            public override Assembly Assembly => _baseType.Assembly;
            public override string? AssemblyQualifiedName => _baseType.AssemblyQualifiedName;
            public override Type? BaseType => _baseType.BaseType;
            public override string? FullName => _baseType.FullName;
            public override Guid GUID => _baseType.GUID;

            protected override TypeAttributes GetAttributeFlagsImpl() => _baseType.Attributes;

            protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers)
            {
                var ctors = _baseType.GetConstructors(bindingAttr);
                return ctors.FirstOrDefault(c =>
                {
                    var ps = c.GetParameters().Select(p => p.ParameterType).ToArray();
                    return types.SequenceEqual(ps);
                });
            }

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => _baseType.GetConstructors(bindingAttr);

            public override Type? GetElementType() => _baseType.GetElementType();

            public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => _baseType.GetEvent(name, bindingAttr);

            public override EventInfo[] GetEvents(BindingFlags bindingAttr) => _baseType.GetEvents(bindingAttr);

            public override FieldInfo? GetField(string name, BindingFlags bindingAttr) => _baseType.GetField(name, bindingAttr);

            public override FieldInfo[] GetFields(BindingFlags bindingAttr) => _baseType.GetFields(bindingAttr);

            public override Type? GetInterface(string name, bool ignoreCase) => _baseType.GetInterface(name, ignoreCase);

            public override Type[] GetInterfaces() => _baseType.GetInterfaces();

            public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => _baseType.GetMembers(bindingAttr);

            protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers)
            {
                var methods = _baseType.GetMethods(bindingAttr);
                foreach (var m in methods)
                {
                    if (m.Name != name) continue;
                    if (types == null) return m;
                    var parms = m.GetParameters().Select(p => p.ParameterType).ToArray();
                    if (types.SequenceEqual(parms)) return m;
                }
                return null;
            }

            public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => _baseType.GetMethods(bindingAttr);

            public override Type? GetNestedType(string name, BindingFlags bindingAttr) => _baseType.GetNestedType(name, bindingAttr);

            public override Type[] GetNestedTypes(BindingFlags bindingAttr) => _baseType.GetNestedTypes(bindingAttr);

            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                var clrProperties = _baseType.GetProperties(bindingAttr) ?? Array.Empty<PropertyInfo>();
                lock (PropertiesLock)
                {
                    return clrProperties.Concat(Properties.Cast<PropertyInfo>()).ToArray();
                }
            }

            protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers)
            {
                var props = GetProperties(bindingAttr);
                var prop = props.FirstOrDefault(p => p.Name == name && (returnType == null || p.PropertyType == returnType));
                if (prop != null) return prop;

                lock (PropertiesLock)
                {
                    return Properties.FirstOrDefault(p => p.Name == name);
                }
            }

            protected override bool HasElementTypeImpl() => _baseType.HasElementType;
            public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, System.Globalization.CultureInfo? culture, string[]? namedParameters)
                => _baseType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);

            protected override bool IsArrayImpl() => _baseType.IsArray;
            protected override bool IsByRefImpl() => _baseType.IsByRef;
            protected override bool IsCOMObjectImpl() => _baseType.IsCOMObject;
            protected override bool IsPointerImpl() => _baseType.IsPointer;
            protected override bool IsPrimitiveImpl() => _baseType.IsPrimitive;

            public override Module Module => _baseType.Module;
            public override string? Namespace => _baseType.Namespace;
            public override Type UnderlyingSystemType => _baseType.UnderlyingSystemType;

            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _baseType.GetCustomAttributes(attributeType, inherit);
            public override object[] GetCustomAttributes(bool inherit) => _baseType.GetCustomAttributes(inherit);
            public override bool IsDefined(Type attributeType, bool inherit) => _baseType.IsDefined(attributeType, inherit);
            public override string Name => _baseType.Name;
        }

        private class CustomPropertyInfoHelper<T> : PropertyInfo
        {
            private readonly string _name;
            private readonly Type _type;
            private readonly List<Attribute> _attributes;
            private MethodInfo? _setterValue;
            private MethodInfo? _getterValue;

            public CustomPropertyInfoHelper(string name, Type type)
                : this(name, type, new List<Attribute>())
            { }

            public CustomPropertyInfoHelper(string name, Type type, List<Attribute> attributes)
            {
                _name = name;
                _type = type;
                _attributes = attributes ?? new List<Attribute>();
            }

            public override PropertyAttributes Attributes => PropertyAttributes.None;

            public override bool CanRead => true;

            public override bool CanWrite => true;

            public override MethodInfo[] GetAccessors(bool nonPublic) => Array.Empty<MethodInfo>();

            public override MethodInfo? GetGetMethod(bool nonPublic) => null;

            public override ParameterInfo[] GetIndexParameters() => Array.Empty<ParameterInfo>();

            public override MethodInfo? GetSetMethod(bool nonPublic) => null;

            // Returns the value from the dictionary stored in the instance.
            public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, System.Globalization.CultureInfo? culture)
            {
                if (obj is null)
                    throw new NullReferenceException("Object is not a reference to an instance in GLB_HL_CustomTypeHelper");
                _getterValue ??= obj.GetType().GetMethod("GetPropertyValue", BindingFlags.Public | BindingFlags.Instance);
                if (_getterValue is not null)
                    return _getterValue.Invoke(obj, new object?[] { _name });
                return null;
            }

            public override Type PropertyType => _type;

            // Sets the value in the dictionary stored in the instance.
            public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, System.Globalization.CultureInfo? culture)
            {
                if (obj is null)
                    throw new NullReferenceException("Object is not a reference to an instance in GLB_HL_CustomTypeHelper");
                _setterValue ??= obj.GetType().GetMethod("SetPropertyValue", BindingFlags.Public | BindingFlags.Instance);
                if (_setterValue is not null)
                    _setterValue.Invoke(obj, new object?[] { _name, value });
            }

            public override Type DeclaringType => typeof(T);

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return _attributes.Where(a => attributeType.IsAssignableFrom(a.GetType())).ToArray();
            }

            public override object[] GetCustomAttributes(bool inherit) => _attributes.ToArray();

            public override bool IsDefined(Type attributeType, bool inherit) => _attributes.Any(a => attributeType.IsAssignableFrom(a.GetType()));

            public override string Name => _name;

            public override Type ReflectedType => typeof(T);

            internal List<Attribute> CustomAttributesInternal => _attributes;
        }

    }
}
