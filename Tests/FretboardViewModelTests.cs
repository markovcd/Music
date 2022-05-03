using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation.Fretboard;

namespace Tests;

[TestClass]
public class FretboardViewModelTests
{
    [TestMethod]
    public void XX()
    {
        var x = new FretViewModel();
        var l = new List<string>();
        x.PropertyChanged += (_, p) => l.Add(p.PropertyName!);

        x.IsChecked.Value = true;
    }
}
