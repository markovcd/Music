using System.Collections.Generic;
using NUnit.Framework;
using Presentation.Fretboard;

namespace Tests;

[TestFixture]
public class FretboardViewModelTests
{
    [Test]
    public void XX()
    {
        var x = new FretViewModel();
        var l = new List<string>();
        x.PropertyChanged += (_, p) => l.Add(p.PropertyName!);

        x.IsChecked.Value = true;
    }
}
