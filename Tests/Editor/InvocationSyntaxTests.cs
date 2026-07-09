using System.Collections.Generic;
using System.Linq;

using Ibralogue.Localization;
using Ibralogue.Parser;

using NUnit.Framework;

using UnityEngine;

namespace Ibralogue.Editor.Tests
{
    public class InvocationSyntaxTests
    {
        private DialogueAsset dialogueAsset;

        [SetUp]
        public void Setup()
        {
            dialogueAsset = ScriptableObject.CreateInstance<DialogueAsset>();
            VariableStore.ClearAll();
            DialogueParser.ClearCache();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(dialogueAsset);
            VariableStore.ClearAll();
            DialogueParser.ClearCache();
        }

        private Line GetLine(Conversation conversation, int index)
        {
            List<RuntimeLine> lines = LineResolver.CollectLines(conversation.Content);
            return LineResolver.Resolve(lines[index], null);
        }

        private Invocation GetFirstInvocation(string content)
        {
            dialogueAsset.Content = content;
            List<Conversation> result = DialogueParser.ParseDialogue(dialogueAsset);
            Line line = GetLine(result[0], 0);
            return line.LineContent.Invocations.FirstOrDefault();
        }

        [Test]
        public void SplitArguments_NoArguments_ReturnsEmptyList()
        {
            Invocation invocation = GetFirstInvocation("[NPC]\n{{Log()}}\n");

            Assert.That(invocation, Is.Not.Null);
            Assert.That(invocation.Name, Is.EqualTo("Log"));
            Assert.That(invocation.Arguments, Is.Empty);
        }

        [Test]
        public void SplitArguments_SingleArgument_ReturnsArgument()
        {
            Invocation invocation = GetFirstInvocation("[NPC]\n{{Log(Hello)}}\n");

            Assert.That(invocation, Is.Not.Null);
            Assert.That(invocation.Name, Is.EqualTo("Log"));
            Assert.That(invocation.Arguments, Has.Count.EqualTo(1));
            Assert.That(invocation.Arguments[0], Is.EqualTo("Hello"));
        }

        [Test]
        public void SplitArguments_MultipleSimpleArguments_SplitsCorrectly()
        {
            Invocation invocation = GetFirstInvocation("[NPC]\n{{Add(3, 4)}}\n");

            Assert.That(invocation, Is.Not.Null);
            Assert.That(invocation.Name, Is.EqualTo("Add"));
            Assert.That(invocation.Arguments, Has.Count.EqualTo(2));
            Assert.That(invocation.Arguments[0], Is.EqualTo("3"));
            Assert.That(invocation.Arguments[1], Is.EqualTo("4"));
        }

        [Test]
        public void SplitArguments_CommasInsideQuotes_KeepsCommasIntact()
        {
            Invocation invocation = GetFirstInvocation("[NPC]\n{{Log(\"Wait, no!\", \"Second arg\")}}\n");

            Assert.That(invocation, Is.Not.Null);
            Assert.That(invocation.Name, Is.EqualTo("Log"));
            Assert.That(invocation.Arguments, Has.Count.EqualTo(2));
            Assert.That(invocation.Arguments[0], Is.EqualTo("Wait, no!"));
            Assert.That(invocation.Arguments[1], Is.EqualTo("Second arg"));
        }

        [Test]
        public void SplitArguments_NestedParentheses_TreatsAsSingleArgument()
        {
            Invocation invocation = GetFirstInvocation("[NPC]\n{{Format(Clamp(5, 0, 10))}}\n");

            Assert.That(invocation, Is.Not.Null);
            Assert.That(invocation.Name, Is.EqualTo("Format"));
            Assert.That(invocation.Arguments, Has.Count.EqualTo(1));
            Assert.That(invocation.Arguments[0], Is.EqualTo("Clamp(5, 0, 10)"));
        }

        [Test]
        public void SplitArguments_EscapedQuotes_IgnoresEscapedQuotes()
        {
            Invocation invocation = GetFirstInvocation("[NPC]\n{{Log(\"She said \\\"hello\\\"\")}}\n");

            Assert.That(invocation, Is.Not.Null);
            Assert.That(invocation.Name, Is.EqualTo("Log"));
            Assert.That(invocation.Arguments, Has.Count.EqualTo(1));
            Assert.That(invocation.Arguments[0], Is.EqualTo("She said \\\"hello\\\""));
        }
    }
}
