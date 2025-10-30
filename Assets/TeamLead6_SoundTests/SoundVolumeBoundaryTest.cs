using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace TeamLead6_SoundTests
{
    public class SoundVolumeBoundaryTest
    {
        GameObject testObject;
        AudioSource audioSource;
        AudioClip testClip;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("SoundTestObject");
            audioSource = testObject.AddComponent<AudioSource>();
            testClip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
            audioSource.clip = testClip;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null) Object.DestroyImmediate(testObject);
        }

        [Test]
        public void MinVolume()
        {
            audioSource.volume = 0.0f;
            Assert.AreEqual(0.0f, audioSource.volume);
        }

        [Test]
        public void MaxVolume()
        {
            audioSource.volume = 1.0f;
            Assert.AreEqual(1.0f, audioSource.volume);
        }

        [UnityTest]
        public IEnumerator ChangeDuringPlayback()
        {
            audioSource.volume = 0.5f;
            audioSource.Play();
            audioSource.volume = 0.8f;
            Assert.AreEqual(0.8f, audioSource.volume);
            yield return null;
            Assert.IsTrue(audioSource.isPlaying);
        }
    }
}
