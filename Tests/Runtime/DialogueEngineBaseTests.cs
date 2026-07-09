using System.Collections;
using System.Collections.Generic;

using Ibralogue.Parser;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace Ibralogue.Tests
{
    public class DialogueEngineBaseTests
    {
        private GameObject _go;
        private SimpleDialogueEngine _engine;
        private DialogueAsset _asset;
        private MockFunctions _mocks;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestEngine");
            _engine = _go.AddComponent<SimpleDialogueEngine>();
            _mocks = _go.AddComponent<MockFunctions>();
            _asset = ScriptableObject.CreateInstance<DialogueAsset>();

            VariableStore.ClearAll();
            VisitTracker.Clear();
            DialogueParser.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.Destroy(_go);

            if (_asset != null)
                Object.DestroyImmediate(_asset);
                
            VariableStore.ClearAll();
            VisitTracker.Clear();
            DialogueParser.ClearCache();
        }

        public class MockFunctions : MonoBehaviour
        {
            public bool voidFunctionCalled = false;
            
            [DialogueInvocation]
            public string GetPlayerName() { return "Hero"; }
            
            [DialogueInvocation]
            public string GetDay() { return "Tuesday"; }
            
            [DialogueInvocation]
            public void PlaySound() { voidFunctionCalled = true; }
        }

        [UnityTest]
        public IEnumerator InvokeTextProducingFunctions_SingleFunction_InsertsTextCorrectly()
        {
            _asset.Content = "[NPC]\nHello {{GetPlayerName}}.\n";

            string displayedText = null;
            _engine.OnLineDisplayed.AddListener(line => displayedText = line.LineContent.Text);

            _engine.StartConversation(_asset);
            yield return null;

            Assert.That(displayedText, Is.EqualTo("Hello Hero."));
        }

        [UnityTest]
        public IEnumerator InvokeTextProducingFunctions_VoidFunction_LeavesTextUnchanged()
        {
            _asset.Content = "[NPC]\nHello. {{PlaySound}}\n";

            string displayedText = null;
            _engine.OnLineDisplayed.AddListener(line => displayedText = line.LineContent.Text);

            _engine.StartConversation(_asset);
            yield return null;

            Assert.That(displayedText, Is.EqualTo("Hello. "));
            Assert.That(_mocks.voidFunctionCalled, Is.True);
        }

        [UnityTest]
        public IEnumerator InvokeTextProducingFunctions_MultipleFunctionsInSameLine_DoesNotOverwrite()
        {
            _asset.Content = "[NPC]\nHello {{GetPlayerName}}, today is {{GetDay}}!\n";

            string displayedText = null;
            _engine.OnLineDisplayed.AddListener(line => displayedText = line.LineContent.Text);

            _engine.StartConversation(_asset);
            yield return null;

            Assert.That(displayedText, Is.EqualTo("Hello Hero, today is Tuesday!"));
        }

        [UnityTest]
        public IEnumerator InvokeTextProducingFunctions_WithVoidFunctions_ShiftsIndicesCorrectly()
        {
            _asset.Content = "[NPC]\nHello {{GetPlayerName}}!{{PlaySound}}\n";

            Line displayedLine = null;
            _engine.OnLineDisplayed.AddListener(line => displayedLine = line);

            _engine.StartConversation(_asset);
            yield return null;

            Assert.That(displayedLine.LineContent.Text, Is.EqualTo("Hello Hero!"));
            Assert.That(_mocks.voidFunctionCalled, Is.True);
            
            List<Invocation> invocations = displayedLine.LineContent.Invocations;
            Assert.That(invocations, Has.Count.EqualTo(2));

            Invocation playSoundInv = invocations.Find(i => i.Name == "PlaySound");
            Assert.That(playSoundInv.Name, Is.EqualTo("PlaySound"));
            Assert.That(_mocks.voidFunctionCalled, Is.True);
        }
    }
}
