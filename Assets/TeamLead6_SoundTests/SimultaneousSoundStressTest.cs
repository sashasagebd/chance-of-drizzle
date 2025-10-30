using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace TeamLead6_SoundTests
{
    public class SimultaneousSoundStressTest
    {
        GameObject testObject;
        AudioClip testClip;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("StressTestObject");
            testClip = AudioClip.Create("StressClip", 22050, 1, 22050, false);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null) Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator MassiveSimultaneousTriggers()
        {
            int count = 100;
            var sources = new AudioSource[count];
            for (int i = 0; i < count; i++)
            {
                var s = testObject.AddComponent<AudioSource>();
                s.clip = testClip;
                s.volume = 0.01f;
                sources[i] = s;
            }
            for (int i = 0; i < count; i++) sources[i].Play();
            yield return null;

            int playing = 0;
            for (int i = 0; i < count; i++) if (sources[i].isPlaying) playing++;
            Assert.Greater(playing, count * 0.5f);
        }
    }
}
