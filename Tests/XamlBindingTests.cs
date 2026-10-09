using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using BadgerMvvm.Core;
using FluentAssertions;
using NUnit.Framework;
using Presentation;
using Presentation.Fretboard;

namespace Tests;

/// <summary>
/// The views cannot be run in a test, and WPF ignores a binding to a missing property without a word.
/// This reads every {Binding ...} in the XAML files and checks that the view model it binds to has the property.
/// </summary>
[TestFixture]
public class XamlBindingTests
{
    private static readonly Regex DesignInstance = new(@"d:DesignInstance\s+local:(?<type>\w+)");
    private static readonly Regex Binding = new(@"\{Binding\s+(?<path>[A-Za-z_]\w*)(?<value>\.Value)?[^}]*\}");
    private static readonly Regex Command = new(@"Command=""\{Binding\s+(?<path>[A-Za-z_]\w*)\s*\}""");

    private static string PresentationDirectory([CallerFilePath] string testFile = "")
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testFile)!, "..", "Presentation"));
    }

    public static IEnumerable<string> XamlFiles()
    {
        return Directory.GetFiles(PresentationDirectory(), "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("App.xaml"))
            .Select(f => Path.GetRelativePath(PresentationDirectory(), f));
    }

    private static Type ViewModelOf(string xaml)
    {
        var match = DesignInstance.Match(xaml);
        match.Success.Should().BeTrue("the view declares its view model with d:DesignInstance");

        return typeof(FretboardViewModel).Assembly.GetTypes()
            .Single(t => t.Name == match.Groups["type"].Value);
    }

    [Test]
    public void ThereAreViewsToCheck()
    {
        XamlFiles().Should().Contain(new[]
        {
            Path.Combine("Fretboard", "FretboardView.xaml"),
            Path.Combine("Fretboard", "StringView.xaml"),
            Path.Combine("Fretboard", "FretView.xaml"),
            "MainWindow.xaml",
        });
    }

    [Test]
    [TestCaseSource(nameof(XamlFiles))]
    public void EveryBindingIsAPropertyOfTheViewModel(string file)
    {
        var xaml = File.ReadAllText(Path.Combine(PresentationDirectory(), file));
        var viewModel = ViewModelOf(xaml);

        foreach (Match binding in Binding.Matches(xaml))
        {
            var name = binding.Groups["path"].Value;
            var property = viewModel.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

            property.Should().NotBeNull($"{file} binds to {name}, which {viewModel.Name} does not have");

            if (binding.Groups["value"].Success)
            {
                property!.PropertyType.IsGenericType.Should().BeTrue($"{name}.Value in {file}");
                property.PropertyType.GetGenericTypeDefinition().Should().Be(typeof(IBindable<>), $"{name}.Value in {file}");
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(XamlFiles))]
    public void EveryCommandBindingIsACommand(string file)
    {
        var xaml = File.ReadAllText(Path.Combine(PresentationDirectory(), file));
        var viewModel = ViewModelOf(xaml);

        foreach (Match command in Command.Matches(xaml))
        {
            var property = viewModel.GetProperty(command.Groups["path"].Value, BindingFlags.Public | BindingFlags.Instance);

            property.Should().NotBeNull();
            property!.PropertyType.Should().Be(typeof(ICommand), $"{property.Name} in {file}");
        }
    }

    [Test]
    public void TheFretboardViewBindsToEveryFeature()
    {
        var xaml = File.ReadAllText(Path.Combine(PresentationDirectory(), "Fretboard", "FretboardView.xaml"));
        var bound = Binding.Matches(xaml).Select(m => m.Groups["path"].Value).ToHashSet();

        bound.Should().Contain(new[]
        {
            "ChordNames", "Tunings", "SelectedTuning", "Capos", "SelectedCapo", "ShowScale", "Roots", "SelectedRoot", "Scales", "SelectedScale", "ScaleNotes", "ScaleChords",
            "TransposeUp", "TransposeDown", "ClearFrets", "ChordQuery", "ChordQueryStatus", "Shapes", "SelectedShape", "Strings",
        });
    }

    [Test]
    public void TheFretViewBindsToTheScaleMarks()
    {
        var xaml = File.ReadAllText(Path.Combine(PresentationDirectory(), "Fretboard", "FretView.xaml"));
        var bound = Binding.Matches(xaml).Select(m => m.Groups["path"].Value).ToHashSet();

        bound.Should().Contain(new[] { "IsChecked", "Caption", "IsInScale", "IsScaleRoot" });
    }

    [Test]
    public void TheWindowBindsToTheFretboard()
    {
        typeof(MainViewModel).GetProperty("FretboardViewModel")!.PropertyType.Should().Be(typeof(FretboardViewModel));
    }
}
