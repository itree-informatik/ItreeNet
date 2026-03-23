using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace ItreeNet.Shared.Form
{
    public partial class ItreeFormProperty<TModel>
    {
        private readonly ItreeForm<TModel> _form;

        public PropertyInfo Property { get; }
        public string? DropdownName { get; }
        public List<ItreeFormDropdownItem>? DropdownData { get; }
        public List<ItreeFormPropertyType>? ItreeFormPropertyTypes { get; }

        private ItreeFormProperty(ItreeForm<TModel> form, PropertyInfo propertyInfo, List<ItreeFormDropdownItem>? dropdownData, string? dropdownName, List<ItreeFormPropertyType>? itreeFormPropertyTypes)
        {
            _form = form;
            Property = propertyInfo;
            DropdownName = dropdownName;
            DropdownData = dropdownData;
            ItreeFormPropertyTypes = itreeFormPropertyTypes;
        }

        internal static List<ItreeFormProperty<TModel>> Create(ItreeForm<TModel> form, string? ignoreProperties, List<ItreeFormDropdown>? dropdownList, List<ItreeFormPropertyType>? customTypes)
        {
            var ignorePropteriesList = new List<string>();
            if (ignoreProperties != null)
                ignorePropteriesList = ignoreProperties.Split(',').ToList();

            var result = new List<ItreeFormProperty<TModel>>();
            var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            var ignoreList = properties.Where(p => p.Name.ToLower().Contains("translatedtext"));
            foreach (var ignore in ignoreList)
                ignorePropteriesList.Add(ignore.Name);

            foreach (var prop in properties)
            {
                if (!ignorePropteriesList.Contains(prop.Name) && prop.SetMethod != null)
                {
                    List<ItreeFormDropdownItem>? data = null;
                    string? label = null;

                    if (dropdownList != null && dropdownList.Any(d => d.Field == prop.Name))
                    {
                        var dropdown = dropdownList.Single(d => d.Field == prop.Name);
                        data = dropdown.Data;
                        label = dropdown.Label;
                    }

                    if (prop.PropertyType.FullName != null && prop.PropertyType.IsClass && !prop.PropertyType.FullName.StartsWith("System"))
                        continue;

                    result.Add(new ItreeFormProperty<TModel>(form, prop, data, label, customTypes));
                }
            }

            return result;
        }
    }

    public partial class ItreeFormProperty<TModel>
    {
        public Type PropertyType => Property.PropertyType;

        public string EditorId
        {
            get
            {
                var typeName = GetTypeName(Property.PropertyType).Replace("?", "nullable");
                var propertyName = Property.Name.ToLower();
                return $"{typeName}_{propertyName}";
            }
        }

        public TModel Owner => _form.Model!;

        public string DisplayName
        {
            get
            {
                var displayName = Property.GetCustomAttribute<DisplayAttribute>()?.GetName();
                if (!string.IsNullOrEmpty(displayName))
                    return displayName;
                return Property.Name;
            }
        }

        public object? Value
        {
            get => Property.GetValue(Owner);
            set => Property.SetValue(Owner, value);
        }

        public string TypeString => GetTypeName(Property.PropertyType);
    }

    public partial class ItreeFormProperty<TModel>
    {
        public readonly MethodInfo EventCallbackFactoryCreate = GetEventCallbackFactoryCreate();

        public LambdaExpression ExpressionHandler()
        {
            var access = Expression.Property(Expression.Constant(Owner, typeof(TModel)), Property);
            var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(PropertyType), access);
            return lambda;
        }

        public object? ChangeHandler()
        {
            var changeHandlerParameter = Expression.Parameter(PropertyType);
            var body = Expression.Assign(Expression.Property(Expression.Constant(this), nameof(Value)), Expression.Convert(changeHandlerParameter, typeof(object)));
            var changeHandlerLambda = Expression.Lambda(typeof(Action<>).MakeGenericType(PropertyType), body, changeHandlerParameter);

            var method = EventCallbackFactoryCreate.MakeGenericMethod(PropertyType);
            var changeHandler = method.Invoke(EventCallback.Factory, new object[] { this, changeHandlerLambda.Compile() });
            return changeHandler;
        }

        public RenderFragment EditorTemplate
        {
            get
            {
                return builder =>
                {
                    var typeName = GetTypeName(Property.PropertyType);

                    // Custom type override
                    var customType = ItreeFormPropertyTypes?.SingleOrDefault(s => s.Field == Property.Name);
                    if (customType != null)
                    {
                        builder.OpenComponent(0, customType.Type!);
                        builder.AddAttribute(1, "Value", Value);
                        builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                        builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                        builder.AddAttribute(4, "Id", EditorId);
                        builder.CloseComponent();
                        return;
                    }

                    // Guid / Guid? → MudSelect with DropdownData
                    if (typeName is "guid" or "guid?")
                    {
                        if (typeName == "guid?")
                        {
                            builder.OpenComponent<MudSelect<Guid?>>(0);
                            builder.AddAttribute(1, "Value", (Guid?)Value);
                            builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                            builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                            builder.AddAttribute(4, "Id", EditorId);
                            builder.AddAttribute(5, "ChildContent", (RenderFragment)(b =>
                            {
                                b.OpenComponent<MudSelectItem<Guid?>>(0);
                                b.AddAttribute(1, "Value", (Guid?)null);
                                b.AddContent(2, "— Auswählen —");
                                b.CloseComponent();
                                if (DropdownData != null)
                                    foreach (var item in DropdownData)
                                    {
                                        b.OpenComponent<MudSelectItem<Guid?>>(0);
                                        b.AddAttribute(1, "Value", (Guid?)item.Value);
                                        b.AddContent(2, item.Text);
                                        b.CloseComponent();
                                    }
                            }));
                            builder.CloseComponent();
                        }
                        else
                        {
                            builder.OpenComponent<MudSelect<Guid>>(0);
                            builder.AddAttribute(1, "Value", (Guid)(Value ?? Guid.Empty));
                            builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                            builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                            builder.AddAttribute(4, "Id", EditorId);
                            builder.AddAttribute(5, "ChildContent", (RenderFragment)(b =>
                            {
                                if (DropdownData != null)
                                    foreach (var item in DropdownData)
                                    {
                                        b.OpenComponent<MudSelectItem<Guid>>(0);
                                        b.AddAttribute(1, "Value", item.Value);
                                        b.AddContent(2, item.Text);
                                        b.CloseComponent();
                                    }
                            }));
                            builder.CloseComponent();
                        }
                        return;
                    }

                    // DateTime / DateTime? → MudDatePicker
                    if (typeName is "datetime" or "datetime?")
                    {
                        var dateVal = Value == null ? (DateTime?)null : (DateTime?)Convert.ToDateTime(Value);
                        var dateChangedCallback = EventCallback.Factory.Create<DateTime?>(this, (DateTime? d) =>
                        {
                            Value = typeName == "datetime" ? (object?)(d ?? DateTime.MinValue) : d;
                            _form.HasChanges();
                        });
                        builder.OpenComponent<MudDatePicker>(0);
                        builder.AddAttribute(1, "Date", dateVal);
                        builder.AddAttribute(2, "DateChanged", dateChangedCallback);
                        builder.AddAttribute(3, "Id", EditorId);
                        builder.AddAttribute(4, "Editable", true);
                        builder.CloseComponent();
                        return;
                    }

                    // DateOnly / DateOnly? → MudDatePicker (convert via DateTime)
                    if (typeName is "dateonly" or "dateonly?")
                    {
                        DateTime? dateTimeVal = null;
                        if (Value != null)
                        {
                            var dateOnly = (DateOnly)Value;
                            dateTimeVal = dateOnly.ToDateTime(TimeOnly.MinValue);
                        }
                        var dateChangedCallback = EventCallback.Factory.Create<DateTime?>(this, (DateTime? d) =>
                        {
                            if (typeName == "dateonly?")
                                Value = d.HasValue ? (object?)DateOnly.FromDateTime(d.Value) : null;
                            else
                                Value = d.HasValue ? DateOnly.FromDateTime(d.Value) : DateOnly.MinValue;
                            _form.HasChanges();
                        });
                        builder.OpenComponent<MudDatePicker>(0);
                        builder.AddAttribute(1, "Date", dateTimeVal);
                        builder.AddAttribute(2, "DateChanged", dateChangedCallback);
                        builder.AddAttribute(3, "Id", EditorId);
                        builder.AddAttribute(4, "Editable", true);
                        builder.CloseComponent();
                        return;
                    }

                    // bool / bool? → MudCheckBox
                    if (typeName is "boolean" or "boolean?")
                    {
                        if (typeName == "boolean?")
                        {
                            builder.OpenComponent<MudCheckBox<bool?>>(0);
                            builder.AddAttribute(1, "Value", (bool?)Value);
                            builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                            builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                            builder.AddAttribute(4, "Id", EditorId);
                            builder.CloseComponent();
                        }
                        else
                        {
                            builder.OpenComponent<MudCheckBox<bool>>(0);
                            builder.AddAttribute(1, "Value", (bool)(Value ?? false));
                            builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                            builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                            builder.AddAttribute(4, "Id", EditorId);
                            builder.CloseComponent();
                        }
                        return;
                    }

                    // int / int? → MudNumericField
                    if (typeName is "int32" or "int32?")
                    {
                        if (typeName == "int32?")
                        {
                            builder.OpenComponent<MudNumericField<int?>>(0);
                            builder.AddAttribute(1, "Value", (int?)Value);
                        }
                        else
                        {
                            builder.OpenComponent<MudNumericField<int>>(0);
                            builder.AddAttribute(1, "Value", (int)(Value ?? 0));
                        }
                        builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                        builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                        builder.AddAttribute(4, "Id", EditorId);
                        builder.CloseComponent();
                        return;
                    }

                    // decimal / decimal? → MudNumericField
                    if (typeName is "decimal" or "decimal?")
                    {
                        if (typeName == "decimal?")
                        {
                            builder.OpenComponent<MudNumericField<decimal?>>(0);
                            builder.AddAttribute(1, "Value", (decimal?)Value);
                        }
                        else
                        {
                            builder.OpenComponent<MudNumericField<decimal>>(0);
                            builder.AddAttribute(1, "Value", (decimal)(Value ?? 0m));
                        }
                        builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                        builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                        builder.AddAttribute(4, "Id", EditorId);
                        builder.AddAttribute(5, "Format", "N2");
                        builder.CloseComponent();
                        return;
                    }

                    // string → MudTextField (with optional mask for phone)
                    if (typeName is "string" or "string?")
                    {
                        builder.OpenComponent<MudTextField<string>>(0);
                        builder.AddAttribute(1, "Value", (string?)Value);
                        builder.AddAttribute(2, "ValueChanged", ChangeHandler());
                        builder.AddAttribute(3, "ValueExpression", ExpressionHandler());
                        builder.AddAttribute(4, "Id", EditorId);
                        if (Property.Name.ToLower() == "telefon")
                            builder.AddAttribute(5, "Mask", new PatternMask("000 000 00 00"));
                        builder.CloseComponent();
                        return;
                    }

                    if (Property.PropertyType.FullName != null && Property.PropertyType.IsClass && Property.PropertyType.FullName.StartsWith("System"))
                        throw new InvalidOperationException($"ItreeFormProperty: PropertyType({typeName}) of {Property.Name} is not supported");
                };
            }
        }

        public RenderFragment FieldValidationTemplate
        {
            get
            {
                return builder =>
                {
                    builder.OpenComponent(0, typeof(ValidationMessage<>).MakeGenericType(PropertyType));
                    builder.AddAttribute(1, "For", ExpressionHandler());
                    builder.CloseComponent();
                };
            }
        }

        private static string GetTypeName(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                return $"{nullableType.Name.ToLower()}?";
            return type.Name.ToLower();
        }

        private static MethodInfo GetEventCallbackFactoryCreate()
        {
            return typeof(EventCallbackFactory).GetMethods()
                .Single(m =>
                {
                    if (m.Name != "Create" || !m.IsPublic || m.IsStatic || !m.IsGenericMethod)
                        return false;
                    var generic = m.GetGenericArguments();
                    if (generic.Length != 1) return false;
                    var args = m.GetParameters();
                    return args.Length == 2 && args[0].ParameterType == typeof(object) && args[1].ParameterType.IsGenericType && args[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>);
                });
        }
    }
}
