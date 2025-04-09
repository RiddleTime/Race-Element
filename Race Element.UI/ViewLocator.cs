using System;
using RaceElement.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace RaceElement.UI;

public class ViewLocator : IDataTemplate
{
  public Control? Build(object? param)
  {
    if (param is null)
      return null;

    var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
    var type = Type.GetType(name);

    if (type != null)
    {
      return (Control)Activator.CreateInstance(type)!;
    }

    var pageName = param.GetType().FullName!.Replace("ViewModel", "PageView", StringComparison.Ordinal);
    pageName =pageName.Replace("PageViews", "Views.Pages", StringComparison.Ordinal);
    var pageType = Type.GetType(pageName);

    if (pageType != null)
    {
      return (Control)Activator.CreateInstance(pageType)!;
    }
    return new TextBlock { Text = "Not Found: " + name + " or " + pageName};
  }

  public bool Match(object? data)
  {
    return data is ViewModelBase;
  }
}
