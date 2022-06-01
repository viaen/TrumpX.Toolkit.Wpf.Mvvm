使用教程：

[ObservableObject · Trump-X/TrumpX.Toolkit.Wpf.Mvvm Wiki (github.com)](https://github.com/Trump-X/TrumpX.Toolkit.Wpf.Mvvm/wiki/ObservableObject)

源码分析

# ObservableObject

## 检查数据源是否存在属性名称对应的属性

```c#
private void VerifyPropertyName(string propertyName)
{
    if (EnableVerifyPropertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return;
        PropertyInfo info = GetType().GetProperty(propertyName);
        if (info != null) return;
        throw new ArgumentException("Property not found.", nameof(propertyName));
    }
}
```

`propertyName`是值发生改变的属性的名称.`string.Empty`和`(string)null`是合法的参数,因为PropertyChangedEventArgs(string.Empty)和PropertyChangedEventArgs((string)null)表示向外通知数据源的所有属性的值已发生了变化.
