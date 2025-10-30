using UnityEngine;
using NUnit.Framework;

namespace TeamLead6_SoundTests
{
    public class AudioChannelBoundaryTest
    {
        GameObject testObject;
        AudioClip testClip;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("ChannelTestObject");
            testClip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null) Object.DestroyImmediate(testObject);
        }

        [Test]
        public void SpatialBlendClamps()
        {
            var src = testObject.AddComponent<AudioSource>();
            src.clip = testClip;

            src.spatialBlend = -1f;
            Assert.AreEqual(0f, src.spatialBlend, 0.0001f);

            src.spatialBlend = 0.5f;
            Assert.AreEqual(0.5f, src.spatialBlend, 0.0001f);

            src.spatialBlend = 2f;
            Assert.AreEqual(1f, src.spatialBlend, 0.0001f);
        }

        [Test]
        public void CanCreateManySources()
        {
            int count = 50;
            int enabledCount = 0;
            for (int i = 0; i < count; i++)
            {
                var s = testObject.AddComponent<AudioSource>();
                s.clip = testClip;
                if (s.enabled) enabledCount++;
            }
            Assert.AreEqual(count, testObject.GetComponents<AudioSource>().Length);
            Assert.Greater(enabledCount, 0);
        }
    }
}
