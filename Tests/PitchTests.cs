using System;
using System.Collections.Generic;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class PitchTests
{
    [TestMethod]
    public void P()
    {
        var p = new Pitch(new Octave(4), Note.Parse("A"));
        var p2 = new Pitch(new Octave(5), Note.Parse("A"));
        var b = p2 > p;
    }
}